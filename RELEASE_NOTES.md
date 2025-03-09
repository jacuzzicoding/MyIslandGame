# MyIslandGame Release Notes

## v0.0.4-alpha (March 8, 2025)

### New Features

#### Crafting System
- Implemented `CraftingSystem` for transforming resources into items
- Added `Recipe` and `RecipeManager` for defining and managing crafting recipes
- Created `CraftingGrid` (2x2 and 3x3) for different crafting station types
- Integrated basic recipes for common items (wood planks, basic tools)
- Added `CraftingComponent` for tracking crafting capabilities
- Implemented toggling of crafting UI with 'C' key

#### Item System Expansion
- Extended `Item` class with crafting-related properties
- Added support for different stack sizes based on item type
- Implemented improved item tooltips with crafting information
- Added quantity tracking for crafting materials and results

#### Backend Improvements
- Enhanced ECS with specialized components for crafting
- Improved inventory integration with crafting system
- Added event-based notifications for crafting actions
- Implemented specialized tool handling for resource gathering
- Increased tree harvesting range to improve usability

### Technical Improvements
- Extended resource management system to handle crafting ingredients
- Added support for crafting stations with different grid sizes
- Improved input management for UI interactions
- Enhanced player spawn logic to ensure valid starting location
- Added more debug logging to diagnose rendering issues

### Known Issues
- Crafting UI may not render visually despite being functionally toggled
- Inventory item movement can be buggy with inconsistent stacking behavior
- Tree harvesting requires close proximity and may not consistently collect resources
- Breaking rocks has inventory stacking issues
- Resource breaking is proximity-based rather than cursor-targeted
- UI system has SpriteBatch management and rendering issues
- No initial crafting station exists in the world (must craft one first)

### Controls
- **WASD/Arrows**: Move player
- **E**: Toggle inventory
- **C**: Toggle crafting interface
- **1-9**: Select hotbar slot
- **Mouse Wheel**: Cycle through hotbar
- **Left Click**: Gather resources / Use selected item / Interact with crafting
- **+/-**: Zoom camera
- **T**: Fast-forward time
- **R**: Reset time to 8:00 AM
- **ESC**: Close crafting / Quit game

## v0.0.3-alpha (March 2, 2025)

### New Features

#### Resource System
- Added `Resource` class with ID, name, description, category, and properties
- Implemented `ResourceManager` for managing resource definitions
- Created resource categories: Organic, Mineral, and Environmental
- Added 12 initial resource types including wood, stone, berries, seeds, etc.
- Created placeholder textures for all resources

#### Environmental Objects
- Added `EnvironmentalObjectComponent` for interactive world objects
- Created object types: trees, rocks, and bushes with unique behaviors
- Implemented growth stages and visual changes based on harvesting
- Added resource regrowth over time based on in-game time
- Integrated environmental objects with world generation

#### Inventory System
- Implemented `Inventory` and `InventorySlot` classes for item storage
- Added `InventoryComponent` for player inventory management
- Created hotbar selection system with number keys and scrolling
- Implemented item stacking and management
- Added basic item types (Resource, Tool) with inheritance

#### Resource Gathering
- Created `GatheringSystem` for handling resource collection
- Implemented tool-specific gathering (axe for trees, pickaxe for rocks)
- Added tool durability system
- Created visual feedback when resources are gathered
- Integrated gathering with the inventory system

### UI Improvements
- Added inventory UI with hotbar display
- Implemented inventory toggle (E key)
- Created item display with quantity indicators
- Added selection highlighting for current hotbar slot

### Technical Improvements
- Extended ECS with new components and systems
- Added `PlayerComponent` to identify the player entity
- Created `EnvironmentalObjectFactory` for easily creating world objects
- Improved input handling for inventory management
- Added integration between multiple systems

### Known Issues
- Tool acquisition is limited (will be addressed in v0.0.4)
- No crafting system yet to utilize gathered resources
- Art assets are simple placeholder shapes
- No visual confirmation when something can be interacted with
- Limited feedback when inventory is full
- Some environmental objects might spawn in inaccessible locations

### Controls
- **WASD/Arrows**: Move player
- **E**: Toggle inventory
- **1-9**: Select hotbar slot
- **Mouse Wheel**: Cycle through hotbar
- **Left-Click**: Gather resources / Use selected item
- **+/-**: Zoom camera
- **T**: Fast-forward time
- **R**: Reset time to 8:00 AM
- **ESC**: Quit game

## v0.0.2-alpha (February 15, 2025)

### New Features

#### Camera System
- Implemented camera following with smooth tracking
- Added zoom functionality using +/- keys
- Added screen clamping to world boundaries
- Created camera transformation for proper rendering

#### World Generation
- Added tile-based map system
- Implemented procedural generation for maps
- Created different tile types (grass, water, sand, stone)
- Added properties for passable/impassable tiles

#### Day/Night Cycle
- Created time management system
- Implemented day/night visual transitions
- Added ambient light color changes
- Created time display and controls

### Technical Improvements
- Fixed collision detection precision
- Added boundary checks for player movement
- Improved entity rendering with depth sorting
- Fixed Vector2 modification issues

### Controls
- **WASD/Arrows**: Move player
- **+/-**: Zoom camera in/out
- **T**: Speed up time (5x)
- **R**: Reset time to 8:00 AM
- **ESC**: Quit game

## v0.0.1-alpha (January 20, 2025)

### Initial Features

#### Technical Foundation
- Set up MonoGame with .NET 8.0
- Created basic game loop
- Implemented window and graphics initialization

#### Core Systems
- Entity Component System (ECS) architecture
- Game state management
- Input handling system with action mapping

#### Basic Gameplay
- Simple player movement with WASD/arrow keys
- Collision detection
- Obstacle creation for testing

### Technical Notes
- Requires MonoGame 3.8.1 or later
- Built with .NET 8.0
- Mac users need to install freetype6 library