using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetDependencyGraph.Editor.Core
{
    /// <summary>
    /// Settings for filtering which nodes are displayed in the graph.
    /// </summary>
    [Serializable]
    public class FilterSettings
    {
        // Type filters
        public bool ShowPrefabs = true;
        public bool ShowScenes = true;
        public bool ShowScripts = true;
        public bool ShowMaterials = true;
        public bool ShowTextures = true;
        public bool ShowShaders = true;
        public bool ShowAnimations = true;
        public bool ShowAudio = true;
        public bool ShowModels = true;
        public bool ShowScriptableObjects = true;
        public bool ShowOther = true;
        
        // Special filters
        public bool ShowEditorOnly = true;
        public bool ShowResourcesAssets = true;
        public bool ShowMissingReferences = true;
        
        // Depth control
        public int MaxDepth = -1; // -1 = unlimited
        
        // Size filter (in bytes, 0 = no filter)
        public long MinFileSizeBytes = 0;
        
        /// <summary>
        /// Checks if a node should be visible based on current filter settings.
        /// </summary>
        public bool ShouldShowNode(DependencyNode node)
        {
            if (node == null)
                return false;
                
            // Check category filters
            bool categoryAllowed = node.Category switch
            {
                AssetCategory.Prefab => ShowPrefabs,
                AssetCategory.Scene => ShowScenes,
                AssetCategory.Script => ShowScripts,
                AssetCategory.Material => ShowMaterials,
                AssetCategory.Texture => ShowTextures,
                AssetCategory.Shader => ShowShaders,
                AssetCategory.Animation => ShowAnimations,
                AssetCategory.Audio => ShowAudio,
                AssetCategory.Model => ShowModels,
                AssetCategory.ScriptableObject => ShowScriptableObjects,
                _ => ShowOther
            };
            
            if (!categoryAllowed)
                return false;
                
            // Check special filters
            if (!ShowEditorOnly && node.IsEditorOnly)
                return false;
                
            if (!ShowResourcesAssets && node.IsInResources)
                return false;
                
            if (!ShowMissingReferences && node.IsMissing)
                return false;
                
            // Check depth filter
            if (MaxDepth >= 0 && node.Depth > MaxDepth)
                return false;
                
            // Check size filter
            if (MinFileSizeBytes > 0 && node.FileSizeBytes < MinFileSizeBytes)
                return false;
                
            return true;
        }
        
        /// <summary>
        /// Filters a list of nodes based on current settings.
        /// </summary>
        public List<DependencyNode> FilterNodes(IEnumerable<DependencyNode> nodes)
        {
            var filtered = new List<DependencyNode>();
            
            foreach (var node in nodes)
            {
                if (ShouldShowNode(node))
                    filtered.Add(node);
            }
            
            return filtered;
        }
        
        /// <summary>
        /// Resets all filters to show everything.
        /// </summary>
        public void ShowAll()
        {
            ShowPrefabs = true;
            ShowScenes = true;
            ShowScripts = true;
            ShowMaterials = true;
            ShowTextures = true;
            ShowShaders = true;
            ShowAnimations = true;
            ShowAudio = true;
            ShowModels = true;
            ShowScriptableObjects = true;
            ShowOther = true;
            ShowEditorOnly = true;
            ShowResourcesAssets = true;
            ShowMissingReferences = true;
            MaxDepth = -1;
            MinFileSizeBytes = 0;
        }
        
        /// <summary>
        /// Hides all asset types.
        /// </summary>
        public void HideAll()
        {
            ShowPrefabs = false;
            ShowScenes = false;
            ShowScripts = false;
            ShowMaterials = false;
            ShowTextures = false;
            ShowShaders = false;
            ShowAnimations = false;
            ShowAudio = false;
            ShowModels = false;
            ShowScriptableObjects = false;
            ShowOther = false;
        }
    }
}
