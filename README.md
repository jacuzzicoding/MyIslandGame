# 🏝️ MyIslandGame

An island-based survival and ecosystem simulation game where players explore procedurally generated islands, interact with evolving wildlife, gather resources, and build structures. Built with MonoGame and C#.

![Current Version](https://img.shields.io/badge/version-0.0.4--alpha-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Status](https://img.shields.io/badge/status-early%20development-orange)

## About

MyIslandGame combines resource management with deep ecosystem simulation. Your actions have real consequences on the environment and its inhabitants.

### Key Features (Planned)
- **Living Ecosystem**: Animals and plants form a complex food web with interdependent species
- **Entity Evolution**: Creatures evolve over generations, adapting to your actions and the environment
- **Resource Extraction**: Everything you craft comes from the world with visible environmental impact
- **Survival Elements**: Navigate day/night cycles, weather, and hunger systems
- **Procedural Generation**: Explore unique islands with their own resources and challenges

## Current State

The game is in early development (v0.0.4-alpha). Current features include:
- Entity Component System architecture
- Basic movement and collision detection
- Simple rendering of placeholder graphics
- Input management system
- Game state management
- Camera system with zoom and player following
- Tile-based procedural map generation
- Day/night cycle with visual effects
- Resource system with different resource types
- Environmental objects (trees, rocks, bushes) with harvesting mechanics
- Inventory system with hotbar
- Resource gathering with tools
- Initial crafting system implementation (with some UI rendering issues)

See the [RELEASE_NOTES.md](RELEASE_NOTES.md) for detailed information about the current version.

## Screenshot

*Coming soon - currently just colored rectangles for development!*

## Development Roadmap

### v0.0.2 (Released)
- Camera system with zoom and player tracking
- Tile-based procedural world generation
- Day/night cycle with lighting effects
- Basic UI framework with debug information
- Player movement and world boundaries

### v0.0.3 (Released)
- Resource system with different resource types
- Environmental objects (trees, rocks, bushes) with regrowth
- Inventory system with hotbar and UI
- Resource gathering mechanics with tools

### v0.0.4 (Released)
- Initial crafting system implementation
- Recipe and recipe manager system
- Basic item transformation
- CraftingStationType support for different crafting stations
- Crafting UI toggle functionality (though rendering has issues)

### v0.0.5-alpha (In Development)
- Complete UI system overhaul
- Standardized UI component interfaces
- Fixed crafting UI rendering
- Improved resource management
- Initial architecture improvements

### v0.0.6-alpha (Planned)
- Fixed resource gathering mechanics
- Improved inventory management
- Basic crafting without stations
- Clear crafting progression
- Building mechanics for player structures
- Continued architecture improvements

### v0.0.7-alpha (Planned)
- First NPC entities (deer)
- Grass implementation for animal grazing
- Basic ecosystem interactions
- Final architecture improvements
- Enhanced environmental interactions

## Development Setup

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or newer
- [MonoGame](https://www.monogame.net/)

### Installation and Running
```bash
# Clone the repository
git clone https://github.com/jacuzzicoding/MyIslandGame.git
cd MyIslandGame

# Install dependencies (Mac users)
# Mac users may need to install freetype6 library
brew install freetype

# Build and run
dotnet build
dotnet run
```

### Controls
- **WASD/Arrow Keys**: Move player
- **E**: Toggle inventory
- **C**: Toggle crafting interface
- **1-9**: Select hotbar slots
- **Mouse Wheel**: Cycle through hotbar
- **Left Click**: Gather resources / Use selected item / Interact with crafting
- **+/-**: Zoom in/out camera
- **T**: Speed up time (5x)
- **R**: Reset time to 8:00 AM
- **ESC**: Close crafting interface / Quit game

## Architecture

The game uses an Entity Component System (ECS) architecture:
- **Entities**: Simple containers with unique IDs
- **Components**: Data containers attached to entities (Transform, Sprite, etc.)
- **Systems**: Logic that processes entities with specific component combinations

### Current Development Focus
We're currently focusing on a complete UI system overhaul that will address rendering issues and provide a more consistent UI architecture. This will be followed by improvements to the core gameplay systems and then world enrichment with the first NPC entities.

See our [GitHub issues](https://github.com/jacuzzicoding/MyIslandGame/issues) for detailed development tasks.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- MonoGame community
- Inspiration from games like Stardew Valley, Animal Crossing, and Vintage Story

---

*More details as development progresses*