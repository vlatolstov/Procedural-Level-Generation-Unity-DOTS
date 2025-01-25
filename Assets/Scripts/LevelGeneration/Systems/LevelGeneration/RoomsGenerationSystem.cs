using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(LevelGenerationSystemGroup))]
[UpdateAfter(typeof(CorridorGenerationSystem))]
[BurstCompile]
partial struct RoomsGenerationSystem : ISystem {

    private EntityQuery _query;

    private int _cellSize;
    private int2 _roomSize;
    private NativeList<int2> _roomCenters;
    private NativeList<int2> _hallCenters;
    private int2 _levelSize;
    private NativeArray<Tile> _matrix;
    private Random _random;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAspect<LevelGenerationAspect>();

        _query = state.GetEntityQuery(builder);
        state.RequireForUpdate(_query);
        state.RequireForUpdate<TestRoomTileCoordinatesData>();
        state.RequireForUpdate<MatrixData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {

        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] RoomsGenerationSystem starts");

        var config = _query.GetSingletonEntity();
        var aspect = SystemAPI.GetAspect<LevelGenerationAspect>(config);
        var levelProperties = aspect.LevelGenerationData.ValueRO;
        var levelDataEntity = SystemAPI.GetSingletonEntity<LevelData>();
        var levelData = SystemAPI.GetComponent<LevelData>(levelDataEntity);
        var roomCentersData = SystemAPI.GetComponent<RoomCentersData>(levelDataEntity);
        var hallCentersData = SystemAPI.GetComponent<HallsCentersData>(levelDataEntity);
        var roomPrefabData = SystemAPI.GetSingleton<TestRoomTileCoordinatesData>();
        ref var blob = ref roomPrefabData.BlobReference;

        _matrix = SystemAPI.GetSingleton<MatrixData>().Matrix;
        _roomCenters = roomCentersData.RoomCenters;
        _hallCenters = hallCentersData.HallCenters;
        _roomSize = roomPrefabData.Size;
        _levelSize = levelData.Level.To;
        _cellSize = levelProperties.CellSize;
        _random = aspect.Random.ValueRW.Value;

        PlaceRooms(ref blob);

        PlaceHalls();

        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] RoomsGenerationSystem ends");
    }

    private void PlaceHalls() {
        foreach (int2 center in _hallCenters) {
            int hallSizeX = _random.NextInt(2 * _cellSize - 1, 3 * _cellSize - 1);
            int hallSizeY = _random.NextInt(2 * _cellSize - 1, 3 * _cellSize - 1);
            int2 offset = new(-1 * (hallSizeX / 2), -1 * (hallSizeY / 2));
            int2 hallPosition = center + offset;

            for (int i = 0; i < hallSizeX; i++) {
                for (int j = 0; j < hallSizeY; j++) {
                    int2 tilePosition = new(hallPosition.x + i, hallPosition.y + j);
                    int index = ConvertToIntegerIndex(tilePosition);
                    bool isGraphNode = LevelGenerationAspect.IsGraphNode(tilePosition, center);
                    _matrix[index] = new(tilePosition, RoomElement.Floor, RoomType.Hall, isGraphNode);
                }
            }
        }
    }
    private void PlaceRooms(ref BlobAssetReference<TestRoomTileBlob> blob) {
        foreach (int2 center in _roomCenters) {
            int2 offset = new(-1 * (_roomSize.x / 2), -1 * (_roomSize.y / 2));
            int2 roomPosition = center + offset;

            for (int i = 0; i < blob.Value.TileBlob.Length; i++) {
                var tile = blob.Value.TileBlob[i];
                int2 position = tile.Position + roomPosition;
                int index = ConvertToIntegerIndex(position);
                bool isGraphNode = LevelGenerationAspect.IsGraphNode(position, center);
                _matrix[index] = new(position, tile.Element, tile.Type, isGraphNode);
            }
        }
    }
    private readonly int ConvertToIntegerIndex(int2 position) => position.y * _levelSize.x + position.x;
}
