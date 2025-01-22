using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateInGroup(typeof(LevelGenerationSystemGroup))]
[UpdateAfter(typeof(SpawnPointsPlacingSystem))]
[BurstCompile]
partial struct MatrixToEntitiesDataConverterSystem : ISystem {
    private EntityQuery _tileEntitiesQuery;
    private ComponentTypeHandle<TilePositionComponent> _tilePositionTypeHandle;
    private ComponentTypeHandle<RoomTypeComponent> _roomTypeTypeHandle;
    private ComponentTypeHandle<RoomElementComponent> _roomElementTypeHandle;
    private ComponentTypeHandle<IsGraphNodeComponent> _isGraphNodeTypeHandle;
    private ComponentTypeHandle<IsSpawnPointComponent> _isSpawnPointTypeHandle;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithPresent<TilePositionComponent, RoomTypeComponent, RoomElementComponent, IsGraphNodeComponent, IsSpawnPointComponent>()
            ;
        _tileEntitiesQuery = state.GetEntityQuery(builder);

        _tilePositionTypeHandle = state.GetComponentTypeHandle<TilePositionComponent>();
        _roomTypeTypeHandle = state.GetComponentTypeHandle<RoomTypeComponent>();
        _roomElementTypeHandle = state.GetComponentTypeHandle<RoomElementComponent>();
        _isGraphNodeTypeHandle = state.GetComponentTypeHandle<IsGraphNodeComponent>();
        _isSpawnPointTypeHandle = state.GetComponentTypeHandle<IsSpawnPointComponent>();

        state.RequireForUpdate(_tileEntitiesQuery);
        state.RequireForUpdate<MatrixData>();
        state.RequireForUpdate<LevelGenerationData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] MatrixToEntitiesDataConverterSystem starts");
        var matrix = SystemAPI.GetSingleton<MatrixData>().Matrix.AsReadOnly();
        var levelScale = SystemAPI.GetSingleton<LevelGenerationData>().LevelScale;
        var chunks = _tileEntitiesQuery.ToArchetypeChunkArray(Allocator.TempJob);

        _tilePositionTypeHandle.Update(ref state);
        _roomTypeTypeHandle.Update(ref state);
        _roomElementTypeHandle.Update(ref state);
        _isGraphNodeTypeHandle.Update(ref state);
        _isSpawnPointTypeHandle.Update(ref state);

        var handle = new SetupTileEntitiesComponentsJob() {
            Matrix = matrix,
            Chunks = chunks,
            TilePositionTypeHandle = _tilePositionTypeHandle,
            RoomTypeTypeHandle = _roomTypeTypeHandle,
            RoomElementTypeHandle = _roomElementTypeHandle,
            IsGraphNodeTypeHandle = _isGraphNodeTypeHandle,
            IsSpawnPointTypeHandle = _isSpawnPointTypeHandle,
            LevelScale = levelScale
        }.Schedule(chunks.Length, 64, state.Dependency);

        state.Dependency = chunks.Dispose(handle);
        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] MatrixToEntitiesDataConverterSystem ends");
    }
}

[BurstCompile]
public partial struct SetupTileEntitiesComponentsJob : IJobParallelFor {
    public NativeArray<Tile>.ReadOnly Matrix;
    public NativeArray<ArchetypeChunk> Chunks;
    public ComponentTypeHandle<TilePositionComponent> TilePositionTypeHandle;
    public ComponentTypeHandle<RoomTypeComponent> RoomTypeTypeHandle;
    public ComponentTypeHandle<RoomElementComponent> RoomElementTypeHandle;
    public ComponentTypeHandle<IsGraphNodeComponent> IsGraphNodeTypeHandle;
    public ComponentTypeHandle<IsSpawnPointComponent> IsSpawnPointTypeHandle;

    public float LevelScale;

    [BurstCompile]
    public void Execute(int chunkIndex) {
        var chunk = Chunks[chunkIndex];
        var tilePositionComponents = chunk.GetNativeArray(ref TilePositionTypeHandle);
        var roomTypeComponents = chunk.GetNativeArray(ref RoomTypeTypeHandle);
        var roomElementComponents = chunk.GetNativeArray(ref RoomElementTypeHandle);

        for (int i = 0; i < chunk.Count; i++) {
            int index = chunkIndex * chunk.Capacity + i;
            var tile = Matrix[index];

            var position = tilePositionComponents[i];
            position.Value = new float3(LevelScale * tile.Position.x, 0, LevelScale * tile.Position.y);
            tilePositionComponents[i] = position;

            var element = roomElementComponents[i];
            element.Value = tile.Element;
            roomElementComponents[i] = element;

            var type = roomTypeComponents[i];
            type.Value = tile.Type;
            roomTypeComponents[i] = type;

            if (!tile.IsGraphNode) {
                chunk.SetComponentEnabled(ref IsGraphNodeTypeHandle, i, false);
            }

            if (!tile.IsSpawnPoint) {
                chunk.SetComponentEnabled(ref IsSpawnPointTypeHandle, i, false);
            }
        }
    }
}
