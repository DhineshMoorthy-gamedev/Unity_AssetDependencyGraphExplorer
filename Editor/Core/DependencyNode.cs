using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetDependencyGraph.Editor.Core
{
    /// <summary>
    /// Represents a node in the dependency graph.
    /// Each node corresponds to a Unity asset.
    /// </summary>
    [Serializable]
    public class DependencyNode
    {
        public string AssetPath { get; set; }
        public string AssetGuid { get; set; }
        public string AssetName { get; set; }
        public AssetCategory Category { get; set; }
        public Type AssetType { get; set; }
        
        // Graph layout
        public Vector2 Position { get; set; }
        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }
        public int Depth { get; set; }
        
        // Dependencies
        public List<DependencyEdge> Dependencies { get; } = new List<DependencyEdge>();
        public List<DependencyEdge> Dependents { get; } = new List<DependencyEdge>();
        
        // Health indicators
        public bool IsMissing { get; set; }
        public bool IsEditorOnly { get; set; }
        public bool IsInResources { get; set; }
        public bool IsAddressable { get; set; }
        public long FileSizeBytes { get; set; }
        
        public DependencyNode(string assetPath, string guid)
        {
            AssetPath = assetPath;
            AssetGuid = guid;
            AssetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            IsExpanded = false;
            IsSelected = false;
        }
        
        public bool HasDependencies => Dependencies.Count > 0;
        public bool HasDependents => Dependents.Count > 0;
        
        public override bool Equals(object obj)
        {
            if (obj is DependencyNode other)
                return AssetGuid == other.AssetGuid;
            return false;
        }
        
        public override int GetHashCode()
        {
            return AssetGuid?.GetHashCode() ?? 0;
        }
    }
    
    /// <summary>
    /// Represents an edge (connection) between two nodes in the dependency graph.
    /// </summary>
    [Serializable]
    public class DependencyEdge
    {
        public DependencyNode FromNode { get; set; }
        public DependencyNode ToNode { get; set; }
        public DependencyType Type { get; set; }
        public string PropertyName { get; set; }
        
        public DependencyEdge(DependencyNode from, DependencyNode to, DependencyType type, string propertyName = null)
        {
            FromNode = from;
            ToNode = to;
            Type = type;
            PropertyName = propertyName;
        }
    }
    
    /// <summary>
    /// Categories of Unity assets for visual differentiation.
    /// </summary>
    public enum AssetCategory
    {
        Unknown,
        Prefab,
        Scene,
        Script,
        Material,
        Texture,
        Shader,
        Animation,
        Audio,
        Model,
        ScriptableObject,
        Font,
        VideoClip,
        Addressable,
        SubAsset,
        Folder
    }
    
    /// <summary>
    /// Types of dependency relationships.
    /// </summary>
    public enum DependencyType
    {
        Direct,           // Standard serialized reference
        SerializedField,  // [SerializeField] reference
        MaterialProperty, // Texture/shader in material
        ShaderInclude,    // Shader #include
        AnimatorState,    // Animation controller reference
        SceneHierarchy,   // Prefab in scene
        Resources,        // Resources.Load reference
        Addressable       // Addressable reference
    }
}
