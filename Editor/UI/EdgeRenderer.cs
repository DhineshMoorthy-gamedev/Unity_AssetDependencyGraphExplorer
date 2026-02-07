using AssetDependencyGraph.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace AssetDependencyGraph.Editor.UI
{
    /// <summary>
    /// Handles rendering of edges (connections) between dependency nodes.
    /// </summary>
    public class EdgeRenderer
    {
        private const float ArrowSize = 8f;
        private const float EdgeThickness = 2f;
        
        /// <summary>
        /// Draws an edge between two nodes using a bezier curve.
        /// </summary>
        public void DrawEdge(DependencyEdge edge, Vector2 offset, float zoom, bool isHighlighted = false)
        {
            if (edge?.FromNode == null || edge.ToNode == null)
                return;
                
            // Calculate connection points
            Vector2 startPos = GetNodeRightCenter(edge.FromNode, offset, zoom);
            Vector2 endPos = GetNodeLeftCenter(edge.ToNode, offset, zoom);
            
            // Determine edge color
            Color edgeColor = isHighlighted 
                ? GraphStyles.EdgeColorHighlight 
                : GraphStyles.EdgeColorNormal;
            
            if (edge.FromNode.IsSelected || edge.ToNode.IsSelected)
                edgeColor = GraphStyles.EdgeColorSelected;
            
            // Draw bezier curve
            DrawBezierEdge(startPos, endPos, edgeColor, zoom);
            
            // Draw arrow at end
            DrawArrow(endPos, Vector2.left, edgeColor, zoom);
        }
        
        /// <summary>
        /// Draws a bezier curve between two points.
        /// </summary>
        private void DrawBezierEdge(Vector2 start, Vector2 end, Color color, float zoom)
        {
            float tangentLength = Mathf.Abs(end.x - start.x) * 0.5f;
            tangentLength = Mathf.Max(tangentLength, 50f * zoom);
            
            Vector3 startTangent = new Vector3(start.x + tangentLength, start.y, 0);
            Vector3 endTangent = new Vector3(end.x - tangentLength, end.y, 0);
            
            Handles.BeginGUI();
            
            Color oldColor = Handles.color;
            Handles.color = color;
            
            Handles.DrawBezier(
                new Vector3(start.x, start.y, 0),
                new Vector3(end.x, end.y, 0),
                startTangent,
                endTangent,
                color,
                null,
                EdgeThickness * zoom
            );
            
            Handles.color = oldColor;
            Handles.EndGUI();
        }
        
        /// <summary>
        /// Draws a simple straight line edge.
        /// </summary>
        public void DrawStraightEdge(Vector2 start, Vector2 end, Color color, float zoom)
        {
            Handles.BeginGUI();
            
            Color oldColor = Handles.color;
            Handles.color = color;
            
            Handles.DrawLine(
                new Vector3(start.x, start.y, 0),
                new Vector3(end.x, end.y, 0)
            );
            
            Handles.color = oldColor;
            Handles.EndGUI();
        }
        
        /// <summary>
        /// Draws an arrow head at the specified position.
        /// </summary>
        private void DrawArrow(Vector2 position, Vector2 direction, Color color, float zoom)
        {
            float size = ArrowSize * zoom;
            direction = direction.normalized;
            
            // Calculate arrow points
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            
            Vector2 tip = position;
            Vector2 back = position - direction * size;
            Vector2 left = back + perpendicular * size * 0.5f;
            Vector2 right = back - perpendicular * size * 0.5f;
            
            // Draw filled triangle
            Handles.BeginGUI();
            
            Color oldColor = Handles.color;
            Handles.color = color;
            
            Handles.DrawAAConvexPolygon(
                new Vector3(tip.x, tip.y, 0),
                new Vector3(left.x, left.y, 0),
                new Vector3(right.x, right.y, 0)
            );
            
            Handles.color = oldColor;
            Handles.EndGUI();
        }
        
        /// <summary>
        /// Draws all edges for visible nodes.
        /// </summary>
        public void DrawAllEdges(DependencyNode rootNode, Vector2 offset, float zoom, 
            DependencyNode highlightedNode = null)
        {
            if (rootNode == null)
                return;
                
            var visited = new System.Collections.Generic.HashSet<string>();
            DrawEdgesRecursive(rootNode, offset, zoom, visited, highlightedNode);
        }
        
        private void DrawEdgesRecursive(DependencyNode node, Vector2 offset, float zoom,
            System.Collections.Generic.HashSet<string> visited, DependencyNode highlightedNode)
        {
            if (node == null || visited.Contains(node.AssetGuid))
                return;
                
            visited.Add(node.AssetGuid);
            
            // Only draw edges if node is expanded
            if (node.IsExpanded)
            {
                foreach (var edge in node.Dependencies)
                {
                    bool isHighlighted = highlightedNode != null && 
                        (edge.FromNode == highlightedNode || edge.ToNode == highlightedNode);
                    
                    DrawEdge(edge, offset, zoom, isHighlighted);
                    
                    // Recurse to children
                    DrawEdgesRecursive(edge.ToNode, offset, zoom, visited, highlightedNode);
                }
            }
        }
        
        /// <summary>
        /// Gets the center-right point of a node for edge connections.
        /// </summary>
        private Vector2 GetNodeRightCenter(DependencyNode node, Vector2 offset, float zoom)
        {
            Vector2 pos = (node.Position + offset) * zoom;
            return new Vector2(
                pos.x + GraphStyles.NodeWidth * zoom,
                pos.y + GraphStyles.NodeHeight * zoom * 0.5f
            );
        }
        
        /// <summary>
        /// Gets the center-left point of a node for edge connections.
        /// </summary>
        private Vector2 GetNodeLeftCenter(DependencyNode node, Vector2 offset, float zoom)
        {
            Vector2 pos = (node.Position + offset) * zoom;
            return new Vector2(
                pos.x,
                pos.y + GraphStyles.NodeHeight * zoom * 0.5f
            );
        }
    }
}
