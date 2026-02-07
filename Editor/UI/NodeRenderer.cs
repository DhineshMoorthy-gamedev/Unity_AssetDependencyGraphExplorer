using AssetDependencyGraph.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace AssetDependencyGraph.Editor.UI
{
    /// <summary>
    /// Handles rendering of individual dependency nodes.
    /// </summary>
    public class NodeRenderer
    {
        private const float ExpandButtonSize = 20f;
        private const float StatusIndicatorSize = 8f;
        
        /// <summary>
        /// Draws a dependency node at its position.
        /// </summary>
        /// <returns>True if the node was clicked</returns>
        public bool DrawNode(DependencyNode node, Vector2 offset, float zoom)
        {
            if (node == null)
                return false;
                
            Vector2 screenPos = (node.Position + offset) * zoom;
            Rect nodeRect = new Rect(
                screenPos.x,
                screenPos.y,
                GraphStyles.NodeWidth * zoom,
                GraphStyles.NodeHeight * zoom
            );
            
            // Background
            Color bgColor = node.IsSelected 
                ? GraphStyles.GetCategoryColor(node.Category)
                : GraphStyles.GetCategoryColorDark(node.Category);
            
            EditorGUI.DrawRect(nodeRect, bgColor);
            
            // Border
            Color borderColor = node.IsSelected 
                ? Color.white 
                : new Color(0.1f, 0.1f, 0.1f);
            DrawRectBorder(nodeRect, borderColor, 2f);
            
            // Content area
            float padding = 8f * zoom;
            Rect contentRect = new Rect(
                nodeRect.x + padding,
                nodeRect.y + padding,
                nodeRect.width - padding * 2,
                nodeRect.height - padding * 2
            );
            
            // Icon
            Texture2D icon = GraphStyles.GetCategoryIcon(node.Category);
            if (icon != null)
            {
                float iconSize = GraphStyles.IconSize * zoom;
                Rect iconRect = new Rect(contentRect.x, contentRect.y, iconSize, iconSize);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                contentRect.x += iconSize + 4f * zoom;
                contentRect.width -= iconSize + 4f * zoom;
            }
            
            // Asset name
            var labelStyle = new GUIStyle(GraphStyles.NodeLabelStyle)
            {
                fontSize = Mathf.RoundToInt(12 * zoom)
            };
            
            string displayName = node.AssetName;
            if (displayName.Length > 20)
                displayName = displayName.Substring(0, 17) + "...";
                
            Rect nameRect = new Rect(contentRect.x, contentRect.y, contentRect.width - ExpandButtonSize * zoom, 20f * zoom);
            GUI.Label(nameRect, displayName, labelStyle);
            
            // Path (smaller text below name)
            var pathStyle = new GUIStyle(GraphStyles.NodePathStyle)
            {
                fontSize = Mathf.RoundToInt(9 * zoom)
            };
            
            string displayPath = node.AssetPath;
            if (displayPath.Length > 30)
                displayPath = "..." + displayPath.Substring(displayPath.Length - 27);
                
            Rect pathRect = new Rect(contentRect.x, contentRect.y + 18f * zoom, contentRect.width, 14f * zoom);
            GUI.Label(pathRect, displayPath, pathStyle);
            
            // Status indicators
            DrawStatusIndicators(node, nodeRect, zoom);
            
            // Expand/collapse button (if has dependencies)
            bool clicked = false;
            if (node.HasDependencies)
            {
                clicked = DrawExpandButton(node, nodeRect, zoom);
            }
            
            // Dependency count badge
            if (node.Dependencies.Count > 0)
            {
                DrawDependencyBadge(node, nodeRect, zoom);
            }
            
            return clicked;
        }
        
        private void DrawStatusIndicators(DependencyNode node, Rect nodeRect, float zoom)
        {
            float indicatorY = nodeRect.y + 4f * zoom;
            float indicatorX = nodeRect.xMax - 4f * zoom;
            float size = StatusIndicatorSize * zoom;
            float spacing = 2f * zoom;
            
            // Missing reference indicator
            if (node.IsMissing)
            {
                indicatorX -= size + spacing;
                Rect indicatorRect = new Rect(indicatorX, indicatorY, size, size);
                EditorGUI.DrawRect(indicatorRect, GraphStyles.StatusMissing);
            }
            
            // Editor-only indicator
            if (node.IsEditorOnly)
            {
                indicatorX -= size + spacing;
                Rect indicatorRect = new Rect(indicatorX, indicatorY, size, size);
                EditorGUI.DrawRect(indicatorRect, GraphStyles.StatusEditorOnly);
            }
            
            // Resources folder indicator
            if (node.IsInResources)
            {
                indicatorX -= size + spacing;
                Rect indicatorRect = new Rect(indicatorX, indicatorY, size, size);
                EditorGUI.DrawRect(indicatorRect, GraphStyles.StatusResources);
            }
            
            // Heavy asset indicator (> 1MB)
            if (node.FileSizeBytes > 1024 * 1024)
            {
                indicatorX -= size + spacing;
                Rect indicatorRect = new Rect(indicatorX, indicatorY, size, size);
                EditorGUI.DrawRect(indicatorRect, GraphStyles.StatusHeavyAsset);
            }
        }
        
        private bool DrawExpandButton(DependencyNode node, Rect nodeRect, float zoom)
        {
            float buttonSize = ExpandButtonSize * zoom;
            Rect buttonRect = new Rect(
                nodeRect.xMax - buttonSize - 4f * zoom,
                nodeRect.y + (nodeRect.height - buttonSize) / 2f,
                buttonSize,
                buttonSize
            );
            
            string buttonText = node.IsExpanded ? "−" : "+";
            
            if (GUI.Button(buttonRect, buttonText, GraphStyles.ExpandButtonStyle))
            {
                node.IsExpanded = !node.IsExpanded;
                return true;
            }
            
            return false;
        }
        
        private void DrawDependencyBadge(DependencyNode node, Rect nodeRect, float zoom)
        {
            string countText = node.Dependencies.Count.ToString();
            Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(countText));
            
            float badgeWidth = Mathf.Max(textSize.x + 8f * zoom, 20f * zoom);
            float badgeHeight = 16f * zoom;
            
            Rect badgeRect = new Rect(
                nodeRect.xMax - badgeWidth - 4f * zoom,
                nodeRect.yMax - badgeHeight - 4f * zoom,
                badgeWidth,
                badgeHeight
            );
            
            // Badge background
            EditorGUI.DrawRect(badgeRect, new Color(0.2f, 0.2f, 0.2f, 0.8f));
            
            // Badge text
            var badgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(10 * zoom),
                normal = { textColor = Color.white }
            };
            
            GUI.Label(badgeRect, countText, badgeStyle);
        }
        
        private void DrawRectBorder(Rect rect, Color color, float thickness)
        {
            // Top
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            // Bottom
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            // Left
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            // Right
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }
        
        /// <summary>
        /// Gets the rect for a node at its current position.
        /// </summary>
        public Rect GetNodeRect(DependencyNode node, Vector2 offset, float zoom)
        {
            Vector2 screenPos = (node.Position + offset) * zoom;
            return new Rect(
                screenPos.x,
                screenPos.y,
                GraphStyles.NodeWidth * zoom,
                GraphStyles.NodeHeight * zoom
            );
        }
    }
}
