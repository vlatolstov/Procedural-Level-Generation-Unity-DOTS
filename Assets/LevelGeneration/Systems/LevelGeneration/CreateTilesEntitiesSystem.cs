using Unity.Collections;
using Unity.Entities;
using Unity.Burst;

[UpdateInGroup(typeof(LevelGenerationSystemGroup))]
[BurstCompile]
partial struct CreateTilesEntitiesSystem : ISystem {

    private EntityQuery _query;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithPresent<LevelEntity, Simulate>()
            ;
        _query = state.GetEntityQuery(builder);

        state.RequireForUpdate<MatrixData>();
        state.RequireForUpdate<GenerationRandomData>();
        state.RequireForUpdate<LevelGenerationData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var entityManager = state.EntityManager;
        var entitiesCount = SystemAPI.GetSingleton<MatrixData>().Matrix.Length;

        var types = new NativeArray<ComponentType>(6, Allocator.Temp);
        types[0] = ComponentType.ReadWrite<LevelEntity>();
        types[1] = ComponentType.ReadWrite<TilePositionComponent>();
        types[2] = ComponentType.ReadWrite<RoomElementComponent>();
        types[3] = ComponentType.ReadWrite<RoomTypeComponent>();
        types[4] = ComponentType.ReadWrite<IsGraphNodeComponent>();
        types[5] = ComponentType.ReadWrite<IsSpawnPointComponent>();

        var tileArchetype = entityManager.CreateArchetype(types);

        entityManager.CreateEntity(tileArchetype, entitiesCount);

        entityManager.SetComponentEnabled(_query, ComponentType.ReadWrite<Simulate>(), false);
    }
}
