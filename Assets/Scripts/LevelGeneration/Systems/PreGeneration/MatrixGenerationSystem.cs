using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateInGroup(typeof(PreGenerationSystemGroup))]
[BurstCompile]
public partial struct MatrixGenerationSystem : ISystem {
    private EntityQuery _query;
    private EntityQuery _currentLevelEntities;

    private int _roomsCount;
    private int _hallsCount;
    private int _cellSize;
    private int _sideRoomsGap;
    private int _cellsPerRoom;

    private int2 _levelSize;
    private NativeArray<Tile> _matrix;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        var mainBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAspect<LevelGenerationAspect>()
            ;

        var subBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<LevelEntity, RenderableLevelEntity>()
            ;

        _query = state.GetEntityQuery(mainBuilder);
        _currentLevelEntities = state.GetEntityQuery(subBuilder);
        state.RequireForUpdate(_query);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] MatrixGenerationSystem starts");

        var ecb = SystemAPI
            .GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        CleanUpExistingLevelData(ecb, ref state);

        var aspect = SystemAPI.GetAspect<LevelGenerationAspect>(_query.GetSingletonEntity());
        var levelProperties = aspect.LevelGenerationData.ValueRO;
        var matrixEntity = ecb.CreateEntity();
        var generationDataStorage = ecb.CreateEntity();

        _roomsCount = levelProperties.RoomsCount;
        _hallsCount = levelProperties.InnerZoneHallsCount;
        _sideRoomsGap = levelProperties.SideRoomsGap;
        _cellsPerRoom = levelProperties.CellsPerRoom;
        _cellSize = levelProperties.CellSize;

        LevelData levelData = GenerateLevelData();
        ecb.AddComponent(generationDataStorage, levelData);
        //empty conponents
        ecb.AddComponent(generationDataStorage, new EdgesData());
        ecb.AddComponent(generationDataStorage, new RoomCentersData());
        ecb.AddComponent(generationDataStorage, new HallsCentersData());

        _matrix = new NativeArray<Tile>(_levelSize.x * _levelSize.y, Allocator.Persistent);
        state.Dependency = FillMatrixWithEmptyTiles();

        ecb.AddComponent(matrixEntity, new MatrixData { Matrix = _matrix });
        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] MatrixGenerationSystem ends");
    }

    public void OnDestroy(ref SystemState state) {
        if (_matrix.IsCreated) {
            _matrix.Dispose();
        }
    }

    private void CleanUpExistingLevelData(EntityCommandBuffer ecb, ref SystemState state) {
        if (!_currentLevelEntities.IsEmpty) {
            var arr = _currentLevelEntities.ToEntityArray(Allocator.Temp);
            ecb.DestroyEntity(arr);
        }

        foreach ((MatrixData existingMatrix, Entity existingMatrixEntity) in SystemAPI.Query<MatrixData>().WithEntityAccess()) {
            if (existingMatrix.Matrix.IsCreated && existingMatrix.Matrix.Length > 0) {
                existingMatrix.Matrix.Dispose();
            }
            ecb.DestroyEntity(existingMatrixEntity);
        }

        foreach ((_, Entity generatedDataEntity) in SystemAPI.Query<LevelData>().WithEntityAccess()) {
            ecb.DestroyEntity(generatedDataEntity);
        }
    }
    private LevelData GenerateLevelData() {
        int modRoom = _roomsCount % 4 > 0 ? 1 : 0;
        int maxRoomsPerSide = _roomsCount / 4 + modRoom;
        int roomsSideSize = maxRoomsPerSide * _cellsPerRoom * _cellSize;

        int hallSideSize = _hallsCount * 2 * _cellSize + 2 * _cellSize;

        int innerZoneSize = math.max(roomsSideSize, hallSideSize);

        int halfCell = _cellSize / 2;
        int cellMod = _cellSize % 2;
        int from = _sideRoomsGap + _cellSize + halfCell + cellMod;
        int to = from + innerZoneSize - _cellSize;
        int2 innerZoneFrom = new(from, from);
        int2 innerZoneTo = new(to, to);
        Rect innerZone = new(innerZoneFrom, innerZoneTo);

        int sideSize = innerZoneSize + (_sideRoomsGap + _cellSize) * 2;
        _levelSize = new(sideSize, sideSize);
        Rect level = new(int2.zero, _levelSize);

        return new LevelData {
            Level = level,
            InnerZone = innerZone
        };
    }
    private JobHandle FillMatrixWithEmptyTiles() {
        
        var job = new FillMatrixJob {
            Matrix = _matrix,
            LevelSizeX = _levelSize.x
        };
        var handle = job.Schedule(_matrix.Length, 64);
        return handle;
    }
}
[BurstCompile]
struct FillMatrixJob : IJobParallelFor {
    public NativeArray<Tile> Matrix;
    public int LevelSizeX;

    public void Execute(int index) {
        int x = index % LevelSizeX;
        int y = index / LevelSizeX;
        int2 position = new(x, y);
        Matrix[index] = new Tile(position, RoomElement.Space, RoomType.Space);
    }
}