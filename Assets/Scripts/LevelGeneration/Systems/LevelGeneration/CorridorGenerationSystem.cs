using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


[UpdateInGroup(typeof(LevelGenerationSystemGroup))]
[UpdateAfter(typeof(GraphGenerationSystem))]
[BurstCompile]
public partial struct CorridorGenerationSystem : ISystem {

    private EntityQuery _query;

    private NativeArray<Tile> _matrix;
    private Random _random;
    private int2 _levelSize;
    private int2 _corridorWidth;
    private float _connectNotMSTNodesProbability;
    private NativeArray<Edge> _edges;

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

        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] CorridorGenerationSystem starts");

        var config = _query.GetSingletonEntity();

        var aspect = SystemAPI.GetAspect<LevelGenerationAspect>(config);
        var levelProperties = aspect.LevelGenerationData.ValueRO;
        var levelDataEntity = SystemAPI.GetSingletonEntity<LevelData>();
        var levelData = SystemAPI.GetComponent<LevelData>(levelDataEntity);
        var edgesData = SystemAPI.GetComponent<EdgesData>(levelDataEntity);

        _corridorWidth = levelProperties.CorridorWidth;
        _connectNotMSTNodesProbability = math.clamp(levelProperties.ConnectNotMSTNodesProbability / 100, 0f, 1f);
        _levelSize = levelData.Level.To;
        _edges = edgesData.Edges;
        _matrix = SystemAPI.GetSingletonRW<MatrixData>().ValueRW.Matrix;
        _random = aspect.Random.ValueRW.Value;

        GenerateCorridors();

        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] CorridorGenerationSystem ends");
    }

    private readonly int ConvertToIntegerIndex(int2 position) => position.y * _levelSize.x + position.x;
    private void GenerateCorridors() {
        var nodes = GetNodesFromEdges();
        RunKruskalAlgorithm(nodes);
    }
    private NativeHashMap<int2, int> GetNodesFromEdges() {
        var nodes = new NativeHashMap<int2, int>(_edges.Length, Allocator.Temp);

        int number = 0;
        foreach (var edge in _edges) {
            int2 nodeA = edge.PointA;
            int2 nodeB = edge.PointB;

            if (!nodes.ContainsKey(nodeA)) {
                nodes.Add(nodeA, number++);
            }

            if (!nodes.ContainsKey(nodeB)) {
                nodes.Add(nodeB, number++);
            }
        }
        
        return nodes;
    }
    private void RunKruskalAlgorithm(NativeHashMap<int2, int> nodes) {
        var dsu = new DSU(nodes.Count, Allocator.Temp);

        foreach (Edge edge in _edges) {
            int nodeA = nodes[edge.PointA];
            int nodeB = nodes[edge.PointB];

            if (dsu.Union(nodeA, nodeB)) {
                BuildCorridor(edge);
            }
            else {
                if (_random.NextFloat() < _connectNotMSTNodesProbability) {
                    BuildCorridor(edge);
                }
            }
        }
    }
    private void BuildCorridor(Edge edge) {
        int2 a = edge.PointA;
        int2 b = edge.PointB;

        int2 direction = new(1, 1);
        if (a.x == b.x) {
            direction.x = 0;
        }
        else {
            direction.y = 0;
        }

        if (a.y > b.y) {
            direction.y *= -1;
        }

        if (a.x > b.x) {
            direction.x *= -1;
        }

        int2 perpDirection = new(-direction.y, direction.x);

        int width = _random.NextInt(_corridorWidth.x, _corridorWidth.y);
        int length = (direction.x == 0 ? math.abs(a.y - b.y) : math.abs(a.x - b.x)) + width;

        int2 startPoint = edge.PointA + -direction * (width / 2);

        for (int i = 0; i < length; i++) {
            int2 centerPos = startPoint + direction * i;
            for (int j = -width / 2; j <= width / 2; j++) {
                int2 corridorPos = centerPos + perpDirection * j;
                int index = ConvertToIntegerIndex(corridorPos);
                bool isGraphNode = LevelGenerationAspect.IsGraphNode(corridorPos, a) ||
                    LevelGenerationAspect.IsGraphNode(corridorPos, b);
                _matrix[index] = new Tile(corridorPos, RoomElement.Floor, RoomType.Corridor, isGraphNode);
            }
        }
    }
}
