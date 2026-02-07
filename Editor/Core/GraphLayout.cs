using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AssetDependencyGraph.Editor.Core
{
    /// <summary>
    /// Handles the layout calculation for dependency graph nodes.
    /// Uses a hierarchical tree layout algorithm.
    /// </summary>
    public class GraphLayout
    {
        private const float NodeWidth = 200f;
        private const float NodeHeight = 60f;
        private const float HorizontalSpacing = 80f;
        private const float VerticalSpacing = 40f;
        
        /// <summary>
        /// Calculates positions for all nodes in the graph using a tree layout.
        /// </summary>
        public void CalculateLayout(DependencyNode rootNode)
        {
            if (rootNode == null)
                return;
                
            // Reset all positions
            var allNodes = CollectAllNodes(rootNode);
            foreach (var node in allNodes)
            {
                node.Position = Vector2.zero;
            }
            
            // Group nodes by depth
            var nodesByDepth = allNodes
                .GroupBy(n => n.Depth)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            // Calculate positions for each depth level
            float currentX = 0;
            
            foreach (var kvp in nodesByDepth)
            {
                int depth = kvp.Key;
                var nodesAtDepth = kvp.Value;
                
                float totalHeight = nodesAtDepth.Count * (NodeHeight + VerticalSpacing) - VerticalSpacing;
                float startY = -totalHeight / 2f;
                
                for (int i = 0; i < nodesAtDepth.Count; i++)
                {
                    var node = nodesAtDepth[i];
                    node.Position = new Vector2(
                        currentX,
                        startY + i * (NodeHeight + VerticalSpacing)
                    );
                }
                
                currentX += NodeWidth + HorizontalSpacing;
            }
        }
        
        /// <summary>
        /// Calculates a radial layout with the root at the center.
        /// </summary>
        public void CalculateRadialLayout(DependencyNode rootNode, float radius = 200f)
        {
            if (rootNode == null)
                return;
                
            var allNodes = CollectAllNodes(rootNode);
            var nodesByDepth = allNodes
                .GroupBy(n => n.Depth)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            // Root at center
            rootNode.Position = Vector2.zero;
            
            // Arrange other nodes in concentric circles
            foreach (var kvp in nodesByDepth)
            {
                if (kvp.Key == 0)
                    continue;
                    
                var nodesAtDepth = kvp.Value;
                float currentRadius = radius * kvp.Key;
                float angleStep = 360f / nodesAtDepth.Count;
                
                for (int i = 0; i < nodesAtDepth.Count; i++)
                {
                    float angle = i * angleStep * Mathf.Deg2Rad;
                    nodesAtDepth[i].Position = new Vector2(
                        Mathf.Cos(angle) * currentRadius,
                        Mathf.Sin(angle) * currentRadius
                    );
                }
            }
        }
        
        /// <summary>
        /// Collects all nodes reachable from the root.
        /// </summary>
        public List<DependencyNode> CollectAllNodes(DependencyNode rootNode)
        {
            var visited = new HashSet<string>();
            var result = new List<DependencyNode>();
            
            CollectNodesRecursive(rootNode, visited, result);
            
            return result;
        }
        
        private void CollectNodesRecursive(DependencyNode node, HashSet<string> visited, List<DependencyNode> result)
        {
            if (node == null || visited.Contains(node.AssetGuid))
                return;
                
            visited.Add(node.AssetGuid);
            result.Add(node);
            
            foreach (var edge in node.Dependencies)
            {
                CollectNodesRecursive(edge.ToNode, visited, result);
            }
        }
        
        /// <summary>
        /// Gets only the visible nodes based on expansion state.
        /// </summary>
        public List<DependencyNode> GetVisibleNodes(DependencyNode rootNode)
        {
            var visible = new List<DependencyNode>();
            var visited = new HashSet<string>();
            
            GetVisibleNodesRecursive(rootNode, visited, visible, true);
            
            return visible;
        }
        
        private void GetVisibleNodesRecursive(DependencyNode node, HashSet<string> visited, 
            List<DependencyNode> visible, bool parentExpanded)
        {
            if (node == null || visited.Contains(node.AssetGuid))
                return;
                
            if (!parentExpanded)
                return;
                
            visited.Add(node.AssetGuid);
            visible.Add(node);
            
            foreach (var edge in node.Dependencies)
            {
                GetVisibleNodesRecursive(edge.ToNode, visited, visible, node.IsExpanded);
            }
        }
        
        /// <summary>
        /// Recalculates layout for only visible nodes.
        /// </summary>
        public void CalculateVisibleLayout(DependencyNode rootNode)
        {
            if (rootNode == null)
                return;
                
            var visibleNodes = GetVisibleNodes(rootNode);
            
            // Rebuild depth for visible nodes only
            var depthMap = new Dictionary<string, int>();
            CalculateVisibleDepth(rootNode, depthMap, 0);
            
            // Group by calculated depth
            var nodesByDepth = visibleNodes
                .GroupBy(n => depthMap.GetValueOrDefault(n.AssetGuid, 0))
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            float currentX = 0;
            
            foreach (var kvp in nodesByDepth)
            {
                var nodesAtDepth = kvp.Value;
                
                float totalHeight = nodesAtDepth.Count * (NodeHeight + VerticalSpacing) - VerticalSpacing;
                float startY = -totalHeight / 2f;
                
                for (int i = 0; i < nodesAtDepth.Count; i++)
                {
                    var node = nodesAtDepth[i];
                    node.Position = new Vector2(
                        currentX,
                        startY + i * (NodeHeight + VerticalSpacing)
                    );
                }
                
                currentX += NodeWidth + HorizontalSpacing;
            }
        }
        
        private void CalculateVisibleDepth(DependencyNode node, Dictionary<string, int> depthMap, int currentDepth)
        {
            if (node == null || depthMap.ContainsKey(node.AssetGuid))
                return;
                
            depthMap[node.AssetGuid] = currentDepth;
            
            if (!node.IsExpanded)
                return;
                
            foreach (var edge in node.Dependencies)
            {
                CalculateVisibleDepth(edge.ToNode, depthMap, currentDepth + 1);
            }
        }
    }
}
