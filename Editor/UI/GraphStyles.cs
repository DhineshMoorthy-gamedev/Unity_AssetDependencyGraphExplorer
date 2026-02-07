using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetDependencyGraph.Editor.UI
{
    /// <summary>
    /// Centralized styling for the dependency graph visualization.
    /// </summary>
    public static class GraphStyles
    {
        // Node dimensions
        public const float NodeWidth = 200f;
        public const float NodeHeight = 60f;
        public const float NodeCornerRadius = 8f;
        public const float IconSize = 24f;
        
        // Colors by asset category
        private static readonly Dictionary<Core.AssetCategory, Color> CategoryColors = new Dictionary<Core.AssetCategory, Color>
        {
            { Core.AssetCategory.Prefab, new Color(0.4f, 0.7f, 1f) },      // Blue
            { Core.AssetCategory.Scene, new Color(0.5f, 0.8f, 0.5f) },     // Green
            { Core.AssetCategory.Script, new Color(0.9f, 0.7f, 0.4f) },    // Orange
            { Core.AssetCategory.Material, new Color(0.8f, 0.5f, 0.8f) },  // Purple
            { Core.AssetCategory.Texture, new Color(0.9f, 0.6f, 0.6f) },   // Red/Pink
            { Core.AssetCategory.Shader, new Color(0.6f, 0.9f, 0.9f) },    // Cyan
            { Core.AssetCategory.Animation, new Color(0.9f, 0.9f, 0.5f) }, // Yellow
            { Core.AssetCategory.Audio, new Color(0.7f, 0.5f, 0.9f) },     // Violet
            { Core.AssetCategory.Model, new Color(0.6f, 0.8f, 0.7f) },     // Teal
            { Core.AssetCategory.ScriptableObject, new Color(0.8f, 0.6f, 0.5f) }, // Brown
            { Core.AssetCategory.Font, new Color(0.7f, 0.7f, 0.7f) },      // Gray
            { Core.AssetCategory.VideoClip, new Color(0.9f, 0.5f, 0.7f) }, // Pink
            { Core.AssetCategory.Addressable, new Color(0.5f, 0.9f, 0.7f) }, // Mint
            { Core.AssetCategory.SubAsset, new Color(0.6f, 0.6f, 0.8f) },  // Lavender
            { Core.AssetCategory.Unknown, new Color(0.5f, 0.5f, 0.5f) },   // Gray
            { Core.AssetCategory.Folder, new Color(0.7f, 0.7f, 0.5f) }     // Khaki
        };
        
        // Edge colors
        public static readonly Color EdgeColorNormal = new Color(0.6f, 0.6f, 0.6f, 0.8f);
        public static readonly Color EdgeColorHighlight = new Color(1f, 0.8f, 0.2f, 1f);
        public static readonly Color EdgeColorSelected = new Color(0.3f, 0.7f, 1f, 1f);
        
        // Status colors
        public static readonly Color StatusMissing = new Color(1f, 0.3f, 0.3f);
        public static readonly Color StatusEditorOnly = new Color(0.7f, 0.7f, 0.3f);
        public static readonly Color StatusResources = new Color(0.9f, 0.6f, 0.2f);
        public static readonly Color StatusHeavyAsset = new Color(1f, 0.5f, 0.5f);
        
        // UI colors
        public static readonly Color BackgroundColor = new Color(0.2f, 0.2f, 0.2f);
        public static readonly Color GridColor = new Color(0.25f, 0.25f, 0.25f);
        public static readonly Color SelectionColor = new Color(0.3f, 0.6f, 1f, 0.3f);
        
        // Cached styles
        private static GUIStyle _nodeStyle;
        private static GUIStyle _nodeLabelStyle;
        private static GUIStyle _nodePathStyle;
        private static GUIStyle _expandButtonStyle;
        
        public static Color GetCategoryColor(Core.AssetCategory category)
        {
            return CategoryColors.TryGetValue(category, out var color) ? color : CategoryColors[Core.AssetCategory.Unknown];
        }
        
        public static Color GetCategoryColorDark(Core.AssetCategory category)
        {
            var baseColor = GetCategoryColor(category);
            return new Color(baseColor.r * 0.6f, baseColor.g * 0.6f, baseColor.b * 0.6f, baseColor.a);
        }
        
        public static GUIStyle NodeStyle
        {
            get
            {
                if (_nodeStyle == null)
                {
                    _nodeStyle = new GUIStyle(GUI.skin.box)
                    {
                        padding = new RectOffset(8, 8, 8, 8),
                        margin = new RectOffset(0, 0, 0, 0),
                        border = new RectOffset(12, 12, 12, 12)
                    };
                }
                return _nodeStyle;
            }
        }
        
        public static GUIStyle NodeLabelStyle
        {
            get
            {
                if (_nodeLabelStyle == null)
                {
                    _nodeLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 12,
                        normal = { textColor = Color.white },
                        clipping = TextClipping.Overflow
                    };
                }
                return _nodeLabelStyle;
            }
        }
        
        public static GUIStyle NodePathStyle
        {
            get
            {
                if (_nodePathStyle == null)
                {
                    _nodePathStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 9,
                        normal = { textColor = new Color(0.7f, 0.7f, 0.7f) },
                        clipping = TextClipping.Clip
                    };
                }
                return _nodePathStyle;
            }
        }
        
        public static GUIStyle ExpandButtonStyle
        {
            get
            {
                if (_expandButtonStyle == null)
                {
                    _expandButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        padding = new RectOffset(2, 2, 2, 2),
                        margin = new RectOffset(0, 0, 0, 0),
                        fontSize = 10,
                        alignment = TextAnchor.MiddleCenter
                    };
                }
                return _expandButtonStyle;
            }
        }
        
        /// <summary>
        /// Gets the appropriate icon for an asset category.
        /// </summary>
        public static Texture2D GetCategoryIcon(Core.AssetCategory category)
        {
            string iconName = category switch
            {
                Core.AssetCategory.Prefab => "Prefab Icon",
                Core.AssetCategory.Scene => "SceneAsset Icon",
                Core.AssetCategory.Script => "cs Script Icon",
                Core.AssetCategory.Material => "Material Icon",
                Core.AssetCategory.Texture => "Texture Icon",
                Core.AssetCategory.Shader => "Shader Icon",
                Core.AssetCategory.Animation => "AnimationClip Icon",
                Core.AssetCategory.Audio => "AudioClip Icon",
                Core.AssetCategory.Model => "PrefabModel Icon",
                Core.AssetCategory.ScriptableObject => "ScriptableObject Icon",
                Core.AssetCategory.Font => "Font Icon",
                Core.AssetCategory.VideoClip => "VideoClip Icon",
                Core.AssetCategory.Folder => "Folder Icon",
                _ => "DefaultAsset Icon"
            };
            
            return EditorGUIUtility.IconContent(iconName)?.image as Texture2D;
        }
        
        /// <summary>
        /// Formats file size for display.
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int suffixIndex = 0;
            double size = bytes;
            
            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }
            
            return $"{size:F1} {suffixes[suffixIndex]}";
        }
    }
}
