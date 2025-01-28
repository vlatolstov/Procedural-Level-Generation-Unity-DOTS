using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

[UpdateInGroup(typeof(PostGenerationSystemGroup))]
[UpdateBefore(typeof(EndGenerationSystem))]

partial class PlayerSpawnSystem : SystemBase {
    private EntityQuery _spawnPoints;
    private EntityQuery _players;

    protected override void OnCreate() {
        var builder1 = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<IsSpawnPointComponent, LocalTransform>();
        var builder2 = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<PlayerTag, NeedToSpawnTag, LocalTransform>();
        _spawnPoints = GetEntityQuery(builder1);
        _players = GetEntityQuery(builder2);
        
        RequireForUpdate(_players);
        RequireForUpdate(_spawnPoints);
        RequireForUpdate<GenerationRandomData>();
    }

    protected override void OnUpdate() {
        UnityEngine.Debug.Log($"[{World.Name}] PlayerSpawnSystem starts");

        var spawnPoints = _spawnPoints.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        var players = _players.ToEntityArray(Allocator.Temp);

        for (int i = 0; i < players.Length; i++) {
            var curTransform = SystemAPI.GetComponent<LocalTransform>(players[i]);
            var nextPos = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].Position;
            curTransform.Position = new(nextPos.x, 0.3f, nextPos.z);
            EntityManager.SetComponentEnabled<NeedToSpawnTag>(players[i], false);
            EntityManager.SetComponentData(players[i], curTransform);
            UnityEngine.Debug.Log($"[{World.Name}] Player {i + 1} spawned");
        }

        UnityEngine.Debug.Log($"[{World.Name}] PlayerSpawnSystem ends");
    }
}

public struct NeedToSpawnTag : IComponentData, IEnableableComponent {
}
