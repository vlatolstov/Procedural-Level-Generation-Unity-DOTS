using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateInGroup(typeof(LevelGenerationSystemGroup))]
[UpdateAfter(typeof(RoomsGenerationSystem))]
[BurstCompile]
partial struct WallsGenerationSystem : ISystem {
    private EntityQuery _query;

    private NativeArray<int2> _directions;
    private NativeArray<Tile> _matrix;

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
        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] WallsGenerationSystem starts");

        var level = SystemAPI.GetSingleton<LevelData>().Level;
        _matrix = SystemAPI.GetSingleton<MatrixData>().Matrix;
        
        _directions = LevelGenerationAspect.GetDirectionsArray(Allocator.TempJob);
        var matrixCopy = new NativeArray<Tile>(_matrix, Allocator.TempJob);
        var handle = PlaceWalls(_matrix, _directions, matrixCopy, level);
        state.Dependency = matrixCopy.Dispose(_directions.Dispose(handle));

        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] WallsGenerationSystem ends");
    }

    private JobHandle PlaceWalls(NativeArray<Tile> matrix, NativeArray<int2> directions, NativeArray<Tile> matrixCopy, Rect level) {
        int levelSizeX = level.To.x;
        var readOnlyMatrix = matrixCopy.AsReadOnly();

        var job = new PlaceWallsJob {
            LevelSizeX = levelSizeX,
            Level = level,
            Matrix = matrix,
            ReadOnlyMatrix = readOnlyMatrix,
            Directions = directions
        };
        var placingJobHande = job.Schedule(matrix.Length, 64);
        return placingJobHande;
    }
}

public partial struct PlaceWallsJob : IJobParallelFor {
    public NativeArray<Tile> Matrix;
    public NativeArray<Tile>.ReadOnly ReadOnlyMatrix;
    public NativeArray<int2> Directions;
    public Rect Level;
    public int LevelSizeX;

    public void Execute(int index) {

        Tile cur = Matrix[index];

        if (cur.Element != RoomElement.Space) {
            return;
        }

        foreach (var dir in Directions) {
            int2 adjPos = cur.Position + dir;

            if (!Level.Contains(adjPos)) {
                continue;
            }

            int adjIndex = ConvertToIntegerIndex(adjPos);

            if (adjIndex < 0 || adjIndex >= ReadOnlyMatrix.Length) {
                continue;
            }

            var adjTile = ReadOnlyMatrix[adjIndex];

            if (adjTile.Element  != RoomElement.Floor) {
                continue;
            }

            RoomElement element = (dir[0], dir[1]) switch {
                (1, 0) => RoomElement.LeftWall,
                (-1, 0) => RoomElement.RightWall,
                (0, 1) => RoomElement.BottomWall,
                _ => RoomElement.TopWall
            };

            cur.Element |= element;

            Matrix[index] = new(cur.Position, cur.Element, adjTile.Type);
        }
    }
    private readonly int ConvertToIntegerIndex(int2 position) => position.y * LevelSizeX + position.x;
}
