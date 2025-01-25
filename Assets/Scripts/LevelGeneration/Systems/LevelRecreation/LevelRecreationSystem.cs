using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine.UIElements;

using static UnityEditor.UIElements.ToolbarMenu;

[UpdateInGroup(typeof(LevelRecreationSystemGroup))]
[BurstCompile]
public partial struct LevelRecreationSystem : ISystem {
    private Random _random;
    private NativeArray<Tile> _matrix;
    private float _levelScale;
    private float _variantProbability;
    private NativeHashMap<int, quaternion> _angles;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {

        state.RequireForUpdate<LevelGenerationData>();
        state.RequireForUpdate<MatrixData>();
        state.RequireForUpdate<GenerationRandomData>();
        state.RequireForUpdate<TilePrefabsData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] LevelRecreationSystem starts");

        state.CompleteDependency();
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        var prefabs = SystemAPI.GetSingleton<TilePrefabsData>();
        _random = SystemAPI.GetSingletonRW<GenerationRandomData>().ValueRW.Value;
        var levelGenerationData = SystemAPI.GetSingleton<LevelGenerationData>();
        _levelScale = levelGenerationData.LevelScale;
        _variantProbability = math.clamp((levelGenerationData.VariationChance / 100f), 0, 1f);
        _matrix = SystemAPI.GetSingleton<MatrixData>().Matrix;
        _angles = GetAngles(Allocator.Temp);

        NativeList<Tile> roomsFloor = new(Allocator.Temp);
        NativeList<Tile> roomsWalls = new(Allocator.Temp);
        NativeList<Tile> corridorsFloor = new(Allocator.Temp);
        NativeList<Tile> corridorsWalls = new(Allocator.Temp);

        foreach (Tile tile in _matrix) {
            switch (tile.Type) {
                case RoomType.Space:
                    break;
                case RoomType.Corridor:
                    switch (tile.Element) {
                        case RoomElement.Floor:
                            corridorsFloor.Add(tile);
                            break;
                        default:
                            corridorsWalls.Add(tile);
                            break;
                    }
                    break;
                default:
                    switch (tile.Element) {
                        case RoomElement.Floor:
                            roomsFloor.Add(tile);
                            break;
                        default:
                            roomsWalls.Add(tile);
                            break;

                    }
                    break;
            }



            //UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] LevelRecreationSystem ends");
        }

        GenerateFloor(roomsFloor, prefabs.BaseRoomFloorTiles, prefabs.VariantRoomFloorTiles, ecb);
        GenerateFloor(corridorsFloor, prefabs.BaseCorridorFloorTiles, prefabs.VariantCorridorFloorTiles, ecb);
        GenerateWalls(roomsWalls, prefabs.BaseRoomWallTiles, prefabs.VariantRoomWallTiles, ecb);
        GenerateWalls(corridorsWalls, prefabs.BaseCorridorWallTiles, prefabs.VariantCorridorWallTiles, ecb);

        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] LevelRecreationSystem ends");
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) {
        foreach (var prefabsData in SystemAPI.Query<TilePrefabsData>()) {
            prefabsData.BaseCorridorFloorTiles.Dispose();
            prefabsData.VariantCorridorFloorTiles.Dispose();
            prefabsData.BaseCorridorWallTiles.Dispose();
            prefabsData.VariantCorridorWallTiles.Dispose();
            prefabsData.BaseRoomFloorTiles.Dispose();
            prefabsData.VariantRoomFloorTiles.Dispose();
            prefabsData.BaseRoomWallTiles.Dispose();
            prefabsData.VariantRoomWallTiles.Dispose();
        }
    }

    private void GenerateFloor(
        NativeList<Tile> source,
        NativeArray<Entity> basic,
        NativeArray<Entity> variant,
        EntityCommandBuffer ecb) {

        foreach (var tile in source) {
            var rotation = quaternion.RotateY(math.radians(0 + 90f * _random.NextInt(0, 3)));
            GenerateTile(tile, ecb, basic, variant, rotation, false);
        }
    }

    private void GenerateWalls(NativeList<Tile> source,
        NativeArray<Entity> basic,
        NativeArray<Entity> variant,
        EntityCommandBuffer ecb) {

        foreach (var tile in source) {
            var mask = tile.Element;
            bool isFirstElement = true;
            foreach (var kvp in _angles) {
                if (((int)mask & kvp.Key) != 0) {
                    GenerateTile(tile, ecb, basic, variant, kvp.Value, !isFirstElement);
                    isFirstElement = false;
                }
            }
        }
    }


    private void GenerateTile(
        Tile tile,
        EntityCommandBuffer ecb,
        NativeArray<Entity> basic,
        NativeArray<Entity> variant,
        quaternion rotation,
        bool basicOnly) {
        var prefab = GetRandomPrefab(basic, variant, basicOnly);
        var e = ecb.Instantiate(prefab);

        ecb.SetComponent(e, new LocalTransform {
            Position = new(tile.Position.x * _levelScale, 0, tile.Position.y * _levelScale),
            Rotation = rotation,
            Scale = _levelScale
        });
        ecb.AddComponent(e, new LevelEntity());
    }

    private Entity GetRandomPrefab(NativeArray<Entity> basic, NativeArray<Entity> variant, bool basicOnly) {
        if (!basicOnly && _random.NextFloat() < _variantProbability) {
            return variant[_random.NextInt(0, variant.Length)];
        }
        return basic[_random.NextInt(0, basic.Length)];
    }
    private NativeHashMap<int, quaternion> GetAngles(Allocator allocator) {
        var angles = new NativeHashMap<int, quaternion>(4, allocator);
        angles.Add(16, quaternion.RotateY(math.radians(0)));
        angles.Add(8, quaternion.RotateY(math.radians(180)));
        angles.Add(4, quaternion.RotateY(math.radians(270)));
        angles.Add(2, quaternion.RotateY(math.radians(90)));

        return angles;
    }
}

