using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

[UpdateInGroup(typeof(PostGenerationSystemGroup))]
[UpdateBefore(typeof(EndGenerationSystem))]

partial struct PlayerSpawnSystem : ISystem {
    private EntityQuery _spawnPoints;
    private EntityQuery _players;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        var builder1 = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<IsSpawnPointComponent, LocalTransform>();
        var builder2 = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<PlayerTag, NeedToSpawnTag, LocalTransform>();
        _spawnPoints = state.GetEntityQuery(builder1);
        _players = state.GetEntityQuery(builder2);
        
        state.RequireForUpdate(_players);
        state.RequireForUpdate(_spawnPoints);
        state.RequireForUpdate<GenerationRandomData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] PlayerSpawnSystem starts");

        var em = state.EntityManager;
        var random = SystemAPI.GetSingletonRW<GenerationRandomData>().ValueRW.Value;
        var spawnPoints = _spawnPoints.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        var players = _players.ToEntityArray(Allocator.Temp);

        for (int i = 0; i < players.Length; i++) {
            var curTransform = SystemAPI.GetComponent<LocalTransform>(players[i]);
            var nextPos = spawnPoints[random.NextInt(0, spawnPoints.Length)].Position;
            curTransform.Position = new(nextPos.x, 0.3f, nextPos.z);
            em.SetComponentEnabled<NeedToSpawnTag>(players[i], false);
            em.SetComponentData(players[i], curTransform);
            UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] Player {i} spawned");
        }

        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] PlayerSpawnSystem ends");
    }
}

public struct NeedToSpawnTag : IComponentData, IEnableableComponent {

}
