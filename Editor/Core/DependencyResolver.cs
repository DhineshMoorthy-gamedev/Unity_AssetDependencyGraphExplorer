using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AssetDependencyGraph.Editor.Core
{
    /// <summary>
    /// Resolves asset dependencies using Unity's AssetDatabase.
    /// Supports both direct and transitive dependency resolution.
    /// </summary>
    public class DependencyResolver
    {
        private readonly Dictionary<string, DependencyNode> _nodeCache = new Dictionary<string, DependencyNode>();
        private readonly HashSet<string> _visitedPaths = new HashSet<string>();
        
        /// <summary>
        /// Resolves all dependencies for the given asset path.
        /// </summary>
        /// <param name="assetPath">Path to the root asset</param>
        /// <param name="maxDepth">Maximum depth to traverse (0 = root only, -1 = unlimited)</param>
        /// <returns>The root dependency node with all resolved dependencies</returns>
        public DependencyNode ResolveDependencies(string assetPath, int maxDepth = -1)
        {
            _nodeCache.Clear();
            _visitedPaths.Clear();
            
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
            {
                Debug.LogWarning($"Asset not found: {assetPath}");
                return null;
            }
            
            var rootNode = GetOrCreateNode(assetPath);
            rootNode.Depth = 0;
            
            ResolveDependenciesRecursive(rootNode, maxDepth);
            
            return rootNode;
        }
        
        private void ResolveDependenciesRecursive(DependencyNode node, int maxDepth, int currentDepth = 0)
        {
            if (maxDepth >= 0 && currentDepth >= maxDepth)
                return;
                
            if (_visitedPaths.Contains(node.AssetPath))
                return;
                
            _visitedPaths.Add(node.AssetPath);
            
            // Get direct dependencies using AssetDatabase
            string[] dependencyPaths = AssetDatabase.GetDependencies(node.AssetPath, false);
            
            foreach (string depPath in dependencyPaths)
            {
                // Skip self-reference
                if (depPath == node.AssetPath)
                    continue;
                    
                // Skip built-in resources
                if (depPath.StartsWith("Resources/") || depPath.StartsWith("Library/"))
                    continue;
                
                var depNode = GetOrCreateNode(depPath);
                depNode.Depth = currentDepth + 1;
                
                // Determine dependency type
                var depType = DetermineDependencyType(node, depNode);
                
                var edge = new DependencyEdge(node, depNode, depType);
                node.Dependencies.Add(edge);
                depNode.Dependents.Add(new DependencyEdge(depNode, node, depType));
                
                // Recurse into dependencies
                ResolveDependenciesRecursive(depNode, maxDepth, currentDepth + 1);
            }
        }
        
        /// <summary>
        /// Resolves reverse dependencies (what assets depend on this one).
        /// </summary>
        public List<DependencyNode> ResolveReverseDependencies(string assetPath)
        {
            var dependents = new List<DependencyNode>();
            
            // Find all assets in the project
            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            
            foreach (string asset in allAssets)
            {
                if (asset == assetPath)
                    continue;
                    
                // Check if this asset depends on our target
                string[] deps = AssetDatabase.GetDependencies(asset, false);
                if (deps.Contains(assetPath))
                {
                    var node = GetOrCreateNode(asset);
                    dependents.Add(node);
                }
            }
            
            return dependents;
        }
        
        private DependencyNode GetOrCreateNode(string assetPath)
        {
            if (_nodeCache.TryGetValue(assetPath, out var existingNode))
                return existingNode;
                
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            var node = new DependencyNode(assetPath, guid);
            
            // Determine asset category
            node.Category = DetermineAssetCategory(assetPath);
            node.AssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            
            // Check health indicators
            node.IsMissing = node.AssetType == null;
            node.IsEditorOnly = assetPath.Contains("/Editor/") || assetPath.StartsWith("Editor/");
            node.IsInResources = assetPath.Contains("/Resources/");
            
            // Get file size
            if (File.Exists(assetPath))
            {
                var fileInfo = new FileInfo(assetPath);
                node.FileSizeBytes = fileInfo.Length;
            }
            
            _nodeCache[assetPath] = node;
            return node;
        }
        
        private AssetCategory DetermineAssetCategory(string assetPath)
        {
            string extension = Path.GetExtension(assetPath).ToLowerInvariant();
            
            return extension switch
            {
                ".prefab" => AssetCategory.Prefab,
                ".unity" => AssetCategory.Scene,
                ".cs" => AssetCategory.Script,
                ".mat" => AssetCategory.Material,
                ".png" or ".jpg" or ".jpeg" or ".tga" or ".psd" or ".exr" or ".hdr" => AssetCategory.Texture,
                ".shader" or ".shadergraph" or ".shadersubgraph" => AssetCategory.Shader,
                ".anim" or ".controller" or ".overrideController" => AssetCategory.Animation,
                ".wav" or ".mp3" or ".ogg" or ".aiff" => AssetCategory.Audio,
                ".fbx" or ".obj" or ".blend" or ".3ds" or ".dae" => AssetCategory.Model,
                ".asset" => AssetCategory.ScriptableObject,
                ".ttf" or ".otf" or ".fontsettings" => AssetCategory.Font,
                ".mp4" or ".mov" or ".webm" or ".avi" => AssetCategory.VideoClip,
                _ => AssetCategory.Unknown
            };
        }
        
        private DependencyType DetermineDependencyType(DependencyNode from, DependencyNode to)
        {
            // Determine relationship type based on asset categories
            if (from.Category == AssetCategory.Material && to.Category == AssetCategory.Texture)
                return DependencyType.MaterialProperty;
                
            if (from.Category == AssetCategory.Material && to.Category == AssetCategory.Shader)
                return DependencyType.MaterialProperty;
                
            if (from.Category == AssetCategory.Shader)
                return DependencyType.ShaderInclude;
                
            if (from.Category == AssetCategory.Animation)
                return DependencyType.AnimatorState;
                
            if (from.Category == AssetCategory.Scene && to.Category == AssetCategory.Prefab)
                return DependencyType.SceneHierarchy;
                
            if (to.IsInResources)
                return DependencyType.Resources;
                
            return DependencyType.Direct;
        }
        
        /// <summary>
        /// Gets all cached nodes.
        /// </summary>
        public IEnumerable<DependencyNode> GetAllNodes() => _nodeCache.Values;
        
        /// <summary>
        /// Clears the node cache.
        /// </summary>
        public void ClearCache()
        {
            _nodeCache.Clear();
            _visitedPaths.Clear();
        }
    }
}
