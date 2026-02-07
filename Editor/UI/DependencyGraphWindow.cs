using AssetDependencyGraph.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace AssetDependencyGraph.Editor.UI
{
    /// <summary>
    /// Main editor window for the Asset Dependency Graph Explorer.
    /// </summary>
    public class DependencyGraphWindow : EditorWindow
    {
        // Core components
        private DependencyResolver _resolver;
        private GraphLayout _layout;
        private NodeRenderer _nodeRenderer;
        private EdgeRenderer _edgeRenderer;
        private FilterSettings _filterSettings;
        
        // Graph state
        private DependencyNode _rootNode;
        private DependencyNode _selectedNode;
        private DependencyNode _hoveredNode;
        
        // View state
        private Vector2 _panOffset = Vector2.zero;
        private float _zoom = 1f;
        private const float MinZoom = 0.25f;
        private const float MaxZoom = 2f;
        
        // Dragging state
        private bool _isPanning;
        private Vector2 _lastMousePos;
        
        // UI state
        private bool _showFilters = true;
        private bool _showInfo = true;
        private Vector2 _filterScrollPos;
        
        // Drop zone
        private Rect _dropZoneRect;
        
        [MenuItem("Window/Asset Dependency Graph Explorer")]
        public static void ShowWindow()
        {
            var window = GetWindow<DependencyGraphWindow>();
            window.titleContent = new GUIContent("Dependency Graph", EditorGUIUtility.IconContent("d_SceneViewFx").image);
            window.minSize = new Vector2(600, 400);
            window.Show();
        }
        
        private void OnEnable()
        {
            _resolver = new DependencyResolver();
            _layout = new GraphLayout();
            _nodeRenderer = new NodeRenderer();
            _edgeRenderer = new EdgeRenderer();
            _filterSettings = new FilterSettings();
            
            wantsMouseMove = true;
        }
        
        private void OnGUI()
        {
            // Handle events
            HandleEvents();
            
            // Draw background
            DrawBackground();
            
            // Draw graph content
            if (_rootNode != null)
            {
                DrawGraph();
            }
            else
            {
                DrawDropZone();
            }
            
            // Draw toolbar
            DrawToolbar();
            
            // Draw side panels
            if (_showFilters)
                DrawFilterPanel();
                
            if (_showInfo && _selectedNode != null)
                DrawInfoPanel();
            
            // Handle drag and drop
            HandleDragAndDrop();
            
            // Repaint on mouse move for hover effects
            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }
        
        private void HandleEvents()
        {
            Event e = Event.current;
            
            // Pan with middle mouse or Alt+left mouse
            if (e.type == EventType.MouseDown && (e.button == 2 || (e.button == 0 && e.alt)))
            {
                _isPanning = true;
                _lastMousePos = e.mousePosition;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && (e.button == 2 || e.button == 0))
            {
                _isPanning = false;
            }
            else if (e.type == EventType.MouseDrag && _isPanning)
            {
                _panOffset += (e.mousePosition - _lastMousePos) / _zoom;
                _lastMousePos = e.mousePosition;
                Repaint();
                e.Use();
            }
            
            // Zoom with scroll wheel
            if (e.type == EventType.ScrollWheel)
            {
                float zoomDelta = -e.delta.y * 0.05f;
                float oldZoom = _zoom;
                _zoom = Mathf.Clamp(_zoom + zoomDelta, MinZoom, MaxZoom);
                
                // Zoom towards mouse position
                if (oldZoom != _zoom)
                {
                    Vector2 mousePos = e.mousePosition;
                    Vector2 graphPos = (mousePos / oldZoom) - _panOffset;
                    _panOffset = (mousePos / _zoom) - graphPos;
                }
                
                Repaint();
                e.Use();
            }
            
            // Select node on click
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt && _rootNode != null)
            {
                var clickedNode = GetNodeAtPosition(e.mousePosition);
                if (clickedNode != null)
                {
                    if (_selectedNode != null)
                        _selectedNode.IsSelected = false;
                    
                    clickedNode.IsSelected = true;
                    _selectedNode = clickedNode;
                    
                    // Ping asset in project window
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(clickedNode.AssetPath);
                    if (asset != null)
                        EditorGUIUtility.PingObject(asset);
                    
                    Repaint();
                    e.Use();
                }
                else if (!IsMouseOverUI(e.mousePosition))
                {
                    // Deselect
                    if (_selectedNode != null)
                    {
                        _selectedNode.IsSelected = false;
                        _selectedNode = null;
                    }
                    Repaint();
                }
            }
            
            // Double-click to open asset
            if (e.type == EventType.MouseDown && e.button == 0 && e.clickCount == 2 && _rootNode != null)
            {
                var clickedNode = GetNodeAtPosition(e.mousePosition);
                if (clickedNode != null)
                {
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(clickedNode.AssetPath));
                    e.Use();
                }
            }
            
            // Context menu on right-click
            if (e.type == EventType.ContextClick && _rootNode != null)
            {
                var clickedNode = GetNodeAtPosition(e.mousePosition);
                if (clickedNode != null)
                {
                    ShowNodeContextMenu(clickedNode);
                    e.Use();
                }
            }
        }
        
        private void DrawBackground()
        {
            // Fill background
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), GraphStyles.BackgroundColor);
            
            // Draw grid
            DrawGrid(20f * _zoom, 0.2f);
            DrawGrid(100f * _zoom, 0.4f);
        }
        
        private void DrawGrid(float gridSpacing, float gridOpacity)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);
            
            Handles.BeginGUI();
            Handles.color = new Color(GraphStyles.GridColor.r, GraphStyles.GridColor.g, GraphStyles.GridColor.b, gridOpacity);
            
            Vector2 offset = new Vector2(_panOffset.x * _zoom % gridSpacing, _panOffset.y * _zoom % gridSpacing);
            
            for (int i = 0; i <= widthDivs; i++)
            {
                Handles.DrawLine(
                    new Vector3(gridSpacing * i + offset.x, 0, 0),
                    new Vector3(gridSpacing * i + offset.x, position.height, 0)
                );
            }
            
            for (int j = 0; j <= heightDivs; j++)
            {
                Handles.DrawLine(
                    new Vector3(0, gridSpacing * j + offset.y, 0),
                    new Vector3(position.width, gridSpacing * j + offset.y, 0)
                );
            }
            
            Handles.EndGUI();
        }
        
        private void DrawGraph()
        {
            // Offset to center the graph in the view
            Vector2 viewCenter = new Vector2(position.width / 2f, position.height / 2f) / _zoom;
            Vector2 totalOffset = _panOffset + viewCenter;
            
            // Draw edges first (behind nodes)
            _edgeRenderer.DrawAllEdges(_rootNode, totalOffset, _zoom, _hoveredNode);
            
            // Draw nodes
            DrawNodesRecursive(_rootNode, totalOffset, new System.Collections.Generic.HashSet<string>());
            
            // Update hovered node
            _hoveredNode = GetNodeAtPosition(Event.current.mousePosition);
        }
        
        private void DrawNodesRecursive(DependencyNode node, Vector2 offset, System.Collections.Generic.HashSet<string> visited)
        {
            if (node == null || visited.Contains(node.AssetGuid))
                return;
                
            if (!_filterSettings.ShouldShowNode(node))
                return;
                
            visited.Add(node.AssetGuid);
            
            // Draw this node
            if (_nodeRenderer.DrawNode(node, offset, _zoom))
            {
                // Node expand/collapse was clicked, recalculate layout
                _layout.CalculateVisibleLayout(_rootNode);
                Repaint();
            }
            
            // Draw child nodes if expanded
            if (node.IsExpanded)
            {
                foreach (var edge in node.Dependencies)
                {
                    DrawNodesRecursive(edge.ToNode, offset, visited);
                }
            }
        }
        
        private void DrawDropZone()
        {
            float zoneWidth = 400f;
            float zoneHeight = 200f;
            
            _dropZoneRect = new Rect(
                (position.width - zoneWidth) / 2f,
                (position.height - zoneHeight) / 2f,
                zoneWidth,
                zoneHeight
            );
            
            // Draw drop zone background
            Color bgColor = DragAndDrop.visualMode == DragAndDropVisualMode.Copy
                ? new Color(0.3f, 0.5f, 0.3f, 0.5f)
                : new Color(0.3f, 0.3f, 0.3f, 0.5f);
            
            EditorGUI.DrawRect(_dropZoneRect, bgColor);
            
            // Draw border
            Handles.BeginGUI();
            Handles.color = new Color(0.5f, 0.5f, 0.5f);
            Handles.DrawWireCube(
                new Vector3(_dropZoneRect.center.x, _dropZoneRect.center.y, 0),
                new Vector3(_dropZoneRect.width, _dropZoneRect.height, 0)
            );
            Handles.EndGUI();
            
            // Draw text
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };
            
            GUI.Label(_dropZoneRect, "Drag & Drop an Asset Here\n\nSupported: Prefabs, Scenes, Materials,\nScripts, Textures, Shaders, etc.", style);
        }
        
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // Reset view button
            if (GUILayout.Button("Reset View", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                _panOffset = Vector2.zero;
                _zoom = 1f;
            }
            
            // Fit to view button
            if (_rootNode != null && GUILayout.Button("Fit", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                FitGraphToView();
            }
            
            GUILayout.Space(10);
            
            // Zoom slider
            GUILayout.Label("Zoom:", GUILayout.Width(40));
            _zoom = GUILayout.HorizontalSlider(_zoom, MinZoom, MaxZoom, GUILayout.Width(100));
            GUILayout.Label($"{_zoom:P0}", GUILayout.Width(40));
            
            GUILayout.FlexibleSpace();
            
            // Toggle panels
            _showFilters = GUILayout.Toggle(_showFilters, "Filters", EditorStyles.toolbarButton, GUILayout.Width(60));
            _showInfo = GUILayout.Toggle(_showInfo, "Info", EditorStyles.toolbarButton, GUILayout.Width(40));
            
            GUILayout.Space(10);
            
            // Clear button
            if (_rootNode != null && GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                ClearGraph();
            }
            
            GUILayout.EndHorizontal();
        }
        
        private void DrawFilterPanel()
        {
            float panelWidth = 180f;
            Rect panelRect = new Rect(0, 20, panelWidth, position.height - 20);
            
            // Panel background
            EditorGUI.DrawRect(panelRect, new Color(0.25f, 0.25f, 0.25f, 0.95f));
            
            GUILayout.BeginArea(panelRect);
            _filterScrollPos = GUILayout.BeginScrollView(_filterScrollPos);
            
            GUILayout.Label("Asset Types", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            
            _filterSettings.ShowPrefabs = GUILayout.Toggle(_filterSettings.ShowPrefabs, "Prefabs");
            _filterSettings.ShowScenes = GUILayout.Toggle(_filterSettings.ShowScenes, "Scenes");
            _filterSettings.ShowScripts = GUILayout.Toggle(_filterSettings.ShowScripts, "Scripts");
            _filterSettings.ShowMaterials = GUILayout.Toggle(_filterSettings.ShowMaterials, "Materials");
            _filterSettings.ShowTextures = GUILayout.Toggle(_filterSettings.ShowTextures, "Textures");
            _filterSettings.ShowShaders = GUILayout.Toggle(_filterSettings.ShowShaders, "Shaders");
            _filterSettings.ShowAnimations = GUILayout.Toggle(_filterSettings.ShowAnimations, "Animations");
            _filterSettings.ShowAudio = GUILayout.Toggle(_filterSettings.ShowAudio, "Audio");
            _filterSettings.ShowModels = GUILayout.Toggle(_filterSettings.ShowModels, "Models");
            _filterSettings.ShowScriptableObjects = GUILayout.Toggle(_filterSettings.ShowScriptableObjects, "ScriptableObjects");
            _filterSettings.ShowOther = GUILayout.Toggle(_filterSettings.ShowOther, "Other");
            
            GUILayout.Space(10);
            GUILayout.Label("Special", EditorStyles.boldLabel);
            
            _filterSettings.ShowEditorOnly = GUILayout.Toggle(_filterSettings.ShowEditorOnly, "Editor Only");
            _filterSettings.ShowResourcesAssets = GUILayout.Toggle(_filterSettings.ShowResourcesAssets, "Resources");
            _filterSettings.ShowMissingReferences = GUILayout.Toggle(_filterSettings.ShowMissingReferences, "Missing");
            
            GUILayout.Space(10);
            GUILayout.Label("Depth", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max:", GUILayout.Width(35));
            string depthText = _filterSettings.MaxDepth < 0 ? "∞" : _filterSettings.MaxDepth.ToString();
            if (GUILayout.Button("-", GUILayout.Width(25)))
                _filterSettings.MaxDepth = Mathf.Max(-1, _filterSettings.MaxDepth - 1);
            GUILayout.Label(depthText, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(30));
            if (GUILayout.Button("+", GUILayout.Width(25)))
                _filterSettings.MaxDepth++;
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("All"))
                _filterSettings.ShowAll();
            if (GUILayout.Button("None"))
                _filterSettings.HideAll();
            GUILayout.EndHorizontal();
            
            if (EditorGUI.EndChangeCheck() && _rootNode != null)
            {
                _layout.CalculateVisibleLayout(_rootNode);
                Repaint();
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        
        private void DrawInfoPanel()
        {
            if (_selectedNode == null)
                return;
                
            float panelWidth = 250f;
            float panelHeight = 200f;
            Rect panelRect = new Rect(position.width - panelWidth, position.height - panelHeight, panelWidth, panelHeight);
            
            // Panel background
            EditorGUI.DrawRect(panelRect, new Color(0.25f, 0.25f, 0.25f, 0.95f));
            
            GUILayout.BeginArea(panelRect);
            GUILayout.Space(5);
            
            GUILayout.Label("Selected Asset", EditorStyles.boldLabel);
            GUILayout.Label(_selectedNode.AssetName, EditorStyles.largeLabel);
            
            GUILayout.Space(5);
            
            EditorGUILayout.LabelField("Type", _selectedNode.Category.ToString());
            EditorGUILayout.LabelField("Path", _selectedNode.AssetPath, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("Size", GraphStyles.FormatFileSize(_selectedNode.FileSizeBytes));
            EditorGUILayout.LabelField("Dependencies", _selectedNode.Dependencies.Count.ToString());
            EditorGUILayout.LabelField("Dependents", _selectedNode.Dependents.Count.ToString());
            
            GUILayout.Space(5);
            
            // Status indicators
            if (_selectedNode.IsMissing)
                EditorGUILayout.HelpBox("Missing Reference", MessageType.Error);
            if (_selectedNode.IsEditorOnly)
                EditorGUILayout.HelpBox("Editor Only", MessageType.Info);
            if (_selectedNode.IsInResources)
                EditorGUILayout.HelpBox("In Resources Folder", MessageType.Warning);
            
            GUILayout.EndArea();
        }
        
        private void HandleDragAndDrop()
        {
            Event e = Event.current;
            
            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (DragAndDrop.objectReferences.Length > 0)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        
                        if (e.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            
                            Object droppedAsset = DragAndDrop.objectReferences[0];
                            string assetPath = AssetDatabase.GetAssetPath(droppedAsset);
                            
                            if (!string.IsNullOrEmpty(assetPath))
                            {
                                LoadAsset(assetPath);
                            }
                        }
                        
                        e.Use();
                    }
                    break;
            }
        }
        
        private void LoadAsset(string assetPath)
        {
            _resolver.ClearCache();
            _rootNode = _resolver.ResolveDependencies(assetPath);
            
            if (_rootNode != null)
            {
                _rootNode.IsExpanded = true; // Auto-expand root
                _layout.CalculateVisibleLayout(_rootNode);
                
                // Reset view
                _panOffset = Vector2.zero;
                _zoom = 1f;
                
                // Select root
                _selectedNode = _rootNode;
                _rootNode.IsSelected = true;
            }
            
            Repaint();
        }
        
        private void ClearGraph()
        {
            _rootNode = null;
            _selectedNode = null;
            _hoveredNode = null;
            _resolver.ClearCache();
            Repaint();
        }
        
        private void FitGraphToView()
        {
            if (_rootNode == null)
                return;
                
            var allNodes = _layout.GetVisibleNodes(_rootNode);
            if (allNodes.Count == 0)
                return;
                
            // Find bounds
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            
            foreach (var node in allNodes)
            {
                minX = Mathf.Min(minX, node.Position.x);
                minY = Mathf.Min(minY, node.Position.y);
                maxX = Mathf.Max(maxX, node.Position.x + GraphStyles.NodeWidth);
                maxY = Mathf.Max(maxY, node.Position.y + GraphStyles.NodeHeight);
            }
            
            float graphWidth = maxX - minX;
            float graphHeight = maxY - minY;
            
            // Calculate zoom to fit
            float viewWidth = position.width - (_showFilters ? 180f : 0f) - 40f;
            float viewHeight = position.height - 60f;
            
            float zoomX = viewWidth / graphWidth;
            float zoomY = viewHeight / graphHeight;
            
            _zoom = Mathf.Clamp(Mathf.Min(zoomX, zoomY), MinZoom, MaxZoom);
            
            // Center the graph
            Vector2 graphCenter = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
            _panOffset = -graphCenter;
        }
        
        private DependencyNode GetNodeAtPosition(Vector2 mousePos)
        {
            if (_rootNode == null)
                return null;
                
            Vector2 viewCenter = new Vector2(position.width / 2f, position.height / 2f) / _zoom;
            Vector2 totalOffset = _panOffset + viewCenter;
            
            return GetNodeAtPositionRecursive(_rootNode, mousePos, totalOffset, new System.Collections.Generic.HashSet<string>());
        }
        
        private DependencyNode GetNodeAtPositionRecursive(DependencyNode node, Vector2 mousePos, Vector2 offset, System.Collections.Generic.HashSet<string> visited)
        {
            if (node == null || visited.Contains(node.AssetGuid))
                return null;
                
            if (!_filterSettings.ShouldShowNode(node))
                return null;
                
            visited.Add(node.AssetGuid);
            
            Rect nodeRect = _nodeRenderer.GetNodeRect(node, offset, _zoom);
            if (nodeRect.Contains(mousePos))
                return node;
                
            if (node.IsExpanded)
            {
                foreach (var edge in node.Dependencies)
                {
                    var found = GetNodeAtPositionRecursive(edge.ToNode, mousePos, offset, visited);
                    if (found != null)
                        return found;
                }
            }
            
            return null;
        }
        
        private bool IsMouseOverUI(Vector2 mousePos)
        {
            // Check if mouse is over filter panel
            if (_showFilters && mousePos.x < 180f)
                return true;
                
            // Check if mouse is over toolbar
            if (mousePos.y < 20f)
                return true;
                
            // Check if mouse is over info panel
            if (_showInfo && _selectedNode != null)
            {
                Rect infoRect = new Rect(position.width - 250f, position.height - 200f, 250f, 200f);
                if (infoRect.Contains(mousePos))
                    return true;
            }
            
            return false;
        }
        
        private void ShowNodeContextMenu(DependencyNode node)
        {
            GenericMenu menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Ping in Project"), false, () =>
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(node.AssetPath);
                if (asset != null)
                    EditorGUIUtility.PingObject(asset);
            });
            
            menu.AddItem(new GUIContent("Open Asset"), false, () =>
            {
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(node.AssetPath));
            });
            
            menu.AddItem(new GUIContent("Show in Explorer"), false, () =>
            {
                EditorUtility.RevealInFinder(node.AssetPath);
            });
            
            menu.AddSeparator("");
            
            if (node.HasDependencies)
            {
                menu.AddItem(new GUIContent("Expand All"), false, () =>
                {
                    ExpandNodeRecursive(node, true);
                    _layout.CalculateVisibleLayout(_rootNode);
                    Repaint();
                });
                
                menu.AddItem(new GUIContent("Collapse All"), false, () =>
                {
                    ExpandNodeRecursive(node, false);
                    _layout.CalculateVisibleLayout(_rootNode);
                    Repaint();
                });
            }
            
            menu.AddSeparator("");
            
            menu.AddItem(new GUIContent("Set as Root"), false, () =>
            {
                LoadAsset(node.AssetPath);
            });
            
            menu.ShowAsContext();
        }
        
        private void ExpandNodeRecursive(DependencyNode node, bool expand)
        {
            node.IsExpanded = expand;
            foreach (var edge in node.Dependencies)
            {
                ExpandNodeRecursive(edge.ToNode, expand);
            }
        }
    }
}
