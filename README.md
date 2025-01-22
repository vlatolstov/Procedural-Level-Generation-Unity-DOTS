# Dungeon Generation

This module is developed in Unity 6, using Unity.Entities, Unity.Burst, Unity.Mathematics.

This prototype presents an implementation of procedural level generation as a system of rooms, corridors and halls, with the ability to influence the generation parameters at runtime.

The approach realizes the construction of a matrix, the construction of a graph with the application of parameters, the graph is processed using Kruskal's algorithm to construct a minimum spanning tree in order to obtain a unique reproducible result.

Video: https://youtu.be/Ds1vyH7LHmw
