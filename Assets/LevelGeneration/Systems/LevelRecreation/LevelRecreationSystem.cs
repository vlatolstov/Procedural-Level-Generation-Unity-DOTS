using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(LevelRecreationSystemGroup))]
[BurstCompile]
public partial struct LevelRecreationSystem : ISystem {
    private EntityQuery _query;
    private Random _random;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<TilePositionComponent, RoomElementComponent, RoomTypeComponent>()
            ;

        _query = state.GetEntityQuery(builder);
        state.RequireForUpdate(_query);
        state.RequireForUpdate<LevelGenerationData>();
        state.RequireForUpdate<GenerationRandomData>();
        state.RequireForUpdate<TilePrefabsData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] LevelRecreationSystem starts");

        var em = state.EntityManager;
        var prefabs = SystemAPI.GetSingleton<TilePrefabsData>();

        _random = SystemAPI.GetSingletonRW<GenerationRandomData>().ValueRW.Value;
        var levelScale = SystemAPI.GetSingleton<LevelGenerationData>().LevelScale;
        var walls = new NativeList<TilePositionComponent>(Allocator.Temp);
        var corridorFloor = new NativeList<TilePositionComponent>(Allocator.Temp);
        var hallFloor = new NativeList<TilePositionComponent>(Allocator.Temp);
        var roomFloor = new NativeList<TilePositionComponent>(Allocator.Temp);

        foreach ((TilePositionComponent position, RoomElementComponent element, RoomTypeComponent type) in SystemAPI.Query<TilePositionComponent, RoomElementComponent, RoomTypeComponent>()) {
            if (type.Value == RoomType.Space) {
                continue;
            }

            if (element.Value == RoomElement.Floor) {
                switch (type.Value) {
                    case RoomType.Hall:
                        hallFloor.Add(position);
                        continue;
                    case RoomType.Room:
                        roomFloor.Add(position);
                        continue;
                    case RoomType.Corridor:
                        corridorFloor.Add(position);
                        continue;
                }
            }

            walls.Add(position);
        }

        var rendWalls = em.Instantiate(prefabs.WallTilePrefab, walls.Length, Allocator.Temp);
        for (int i = 0; i < walls.Length; i++) {
            float rotation = math.radians(90f * _random.NextInt(0, 5));
            em.SetComponentData(rendWalls[i], new LocalTransform {
                Position = walls[i].Value,
                Rotation = quaternion.RotateY(rotation),
                Scale = levelScale
            });
        }

        var rendHalls = em.Instantiate(prefabs.HallTilePrefab, hallFloor.Length, Allocator.Temp);
        for (int i = 0; i < hallFloor.Length; i++) {
            em.SetComponentData(rendHalls[i], new LocalTransform {
                Position = hallFloor[i].Value,
                Scale = levelScale
            });
        }

        var rendCorr = em.Instantiate(prefabs.CorridorTilePrefab, corridorFloor.Length, Allocator.Temp);
        for (int i = 0; i < corridorFloor.Length; i++) {
            float rotation = math.radians(90f * _random.NextInt(0, 5));
            em.SetComponentData(rendCorr[i], new LocalTransform {
                Position = corridorFloor[i].Value,
                Rotation = quaternion.RotateY(rotation),
                Scale = levelScale
            });
        }

        var rendRooms = em.Instantiate(prefabs.RoomTilePrefab, roomFloor.Length, Allocator.Temp);
        for (int i = 0; i < roomFloor.Length; i++) {
            em.SetComponentData(rendRooms[i], new LocalTransform {
                Position = roomFloor[i].Value,
                Scale = levelScale
            });
        }

        em.AddComponent<RenderableLevelEntity>(rendRooms);
        em.AddComponent<RenderableLevelEntity>(rendCorr);
        em.AddComponent<RenderableLevelEntity>(rendHalls);
        em.AddComponent<RenderableLevelEntity>(rendWalls);

        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] LevelRecreationSystem ends");
    }
}

