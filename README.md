# Asset Dependency Graph Explorer

A visual, interactive graph tool for Unity that represents all dependencies of a selected asset.

## Features

- **Drag & Drop Interface**: Simply drag any asset into the tool window
- **Visual Dependency Graph**: Node-based visualization of asset relationships
- **Depth Control**: Expand/collapse nodes to manage complexity
- **Type Filtering**: Filter by textures, scripts, shaders, audio, etc.
- **Asset Health Signals**: Identify missing references, circular dependencies, and heavy assets
- **Build Relevance**: See which assets are included in builds

## Installation

### Via Package Manager (Git URL)

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click `+` > `Add package from git URL`
3. Enter: `https://github.com/DHINESH/Unity_AssetDependencyGraphExplorer.git`

### Manual Installation

1. Clone or download this repository
2. Copy the folder into your Unity project's `Packages/` directory

## Usage

1. Open the tool: `Window > Asset Dependency Graph Explorer`
2. Drag any asset (Prefab, Scene, Material, Script, etc.) into the window
3. Explore the dependency graph:
   - Click nodes to expand/collapse
   - Use filters to show/hide asset types
   - Hover for detailed information

## Supported Asset Types

- Prefabs
- Scenes
- Scripts
- Materials
- Textures
- Shaders
- Animations
- Audio
- Addressables
- Sub-assets (e.g., AnimationClip inside FBX)

## Requirements

- Unity 2021.3 or later

## License

MIT License - see [LICENSE](LICENSE.md) for details
