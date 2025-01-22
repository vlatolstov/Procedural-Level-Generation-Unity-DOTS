using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

[UpdateInGroup(typeof(PostGenerationSystemGroup))]
[BurstCompile]
partial struct EndGenerationSystem : ISystem {
    private EntityQuery _query;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAspect<LevelGenerationAspect>()
            ;

        _query = state.GetEntityQuery(builder);
        state.RequireForUpdate(_query);
        state.RequireForUpdate<LevelData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var entityManager = state.EntityManager;
        entityManager.DestroyEntity(_query);
        var dataEntity = SystemAPI.GetSingletonEntity<LevelData>();
        entityManager.DestroyEntity(dataEntity);
        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] Generation done!");
    }
}
