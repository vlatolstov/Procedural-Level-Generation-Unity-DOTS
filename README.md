# Procedural Level Generation Unity DOTS
This project showcases a procedural level generation approach built with Unity's Entity Component System (ECS). The system efficiently generates dynamic levels with rooms, corridors, walls, making it appropriate for games requiring large, procedurally generated maps with the ability to influence the generation parameters at runtime.


# 🚀 High Performance

Built with Unity ECS for massive scalability and parallel processing.

Uses Unity's Burst compiler and NativeContainer structures for optimized memory usage and speed. 

Supports runtime level regeneration for dynamic gameplay scenarios.

# 🏗 Modular Design

Separation of logic into distinct systems for rooms, corridors, walls, and spawn points.

Fully customizable generation parameters via ScriptableObjects. 

Additionally, supports the placement of any prefabs for tiles, allowing you to use the system for personal or unique purposes.

# 🔄 Procedural Diversity

Implements graph-based algorithms (e.g., Kruskal’s algorithm) for optimal connectivity.

Adds randomness to corridor widths and non-essential connections for variety.

# 📦 ECS Integration

Fully integrated into Unity's ECS framework, ensuring compatibility with DOTS workflows.




# How It Works

The level generation system is divided into several ECS systems, each responsible for a specific aspect of the level:

1. Graph Generation

Constructs a graph representing node connections.
Ensures that rooms are connected via a Minimum Spanning Tree (MST) while allowing additional connections for variety.

2. Corridor Generation

Uses graph edges to determine corridor paths and randomizes corridor widths.
Implements Kruskal's algorithm for MST generation.

3. Room Placement

Places rooms based on configuration settings, ensuring no overlap.
Supports various room shapes and sizes through prefab modifications.

4. Wall Placement

Fills in gaps around rooms and corridors with walls.
Ensures seamless transitions between connected spaces.

5. Spawn Point Placement

Marks key areas for player and enemy spawns.
Configurable via ScriptableObjects.

# Getting Started
Prerequisites:
Unity 6 (60000.0.23f1) with Universal Render Pipeline (URP) configured.
ECS-related packages:
Entities 1.3.9
Entities Graphics 1.3.2
Unity Physics 1.3.9
Mathematics 1.3.2
Collections 2.5.2
Burst 18.18

# How can i try it?
1. Open GenerationScene.
2. Enter play mode.
3. Navigate to the Settings folder.
4. Open a LevelGenerationSettings ScriptableObject for level configuration.
5. Make settings changes and push Generate Level button.
6. Explore the level through the Game or Scene windows.
7. Repeat from step number 5.

# Use of third-party assets for visualisation

FREE Low Poly Human - RPG Character: https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/free-low-poly-human-rpg-character-219979

Low Poly Dungeons Lite: https://assetstore.unity.com/packages/p/low-poly-dungeons-lite-177937

Lowpoly Medieval Skeleton - Free - MEDIEVAL FANTASY SERIES: https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/lowpoly-medieval-skeleton-free-medieval-fantasy-series-181883

Lowpoly Medieval Weapon Pack: https://assetstore.unity.com/packages/3d/props/weapons/lowpoly-medieval-weapon-pack-291374

Poly Halloween: https://assetstore.unity.com/packages/3d/props/poly-halloween-236625Ultimate 

Low Poly Dungeon: https://assetstore.unity.com/packages/3d/environments/dungeons/ultimate-low-poly-dungeon-143535

Contact
For questions or feedback, feel free to reach out via vlatolstov.it@gmail.com.



Happy Generating! 🎮
