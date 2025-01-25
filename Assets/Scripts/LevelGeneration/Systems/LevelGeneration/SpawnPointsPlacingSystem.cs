using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(LevelGenerationSystemGroup))]
[UpdateAfter(typeof(WallsGenerationSystem))]
[BurstCompile]
partial struct SpawnPointsPlacingSystem : ISystem {
    private EntityQuery _query;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAspect<LevelGenerationAspect>();

        _query = state.GetEntityQuery(builder);
        state.RequireForUpdate(_query);
        state.RequireForUpdate<MatrixData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] SpawnPointsPlacingSystem starts");
        
        var configEntity = _query.GetSingletonEntity();
        var generationAspect = SystemAPI.GetAspect<LevelGenerationAspect>(configEntity);
        var allowedTypesMask = generationAspect.LevelGenerationData.ValueRO.AllowedTypesMask;
        var matrix = SystemAPI.GetSingletonRW<MatrixData>().ValueRW.Matrix;

        var job = new SpawnPointsPlacingJob() {
            Matrix = matrix,
            AllowedTypesMask = allowedTypesMask
        };

        state.Dependency = job.Schedule(matrix.Length, 64, state.Dependency);

        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] SpawnPointsPlacingSystem ends");
    }
}

public struct SpawnPointsPlacingJob : IJobParallelFor {

    public NativeArray<Tile> Matrix;
    public RoomType AllowedTypesMask;
    public void Execute(int index) {
        var tile = Matrix[index];
        if ((tile.Type & AllowedTypesMask) > 0 &&
            tile.IsGraphNode) {
            tile.IsSpawnPoint = true;
            Matrix[index] = tile;
        }
    }
}


