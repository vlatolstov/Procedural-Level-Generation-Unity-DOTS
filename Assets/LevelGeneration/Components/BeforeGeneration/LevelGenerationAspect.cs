using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public readonly partial struct LevelGenerationAspect : IAspect {
    public readonly Entity Entity;
    public readonly RefRW<GenerationRandomData> Random;
    public readonly RefRO<LevelGenerationData> LevelGenerationData;

    public static NativeArray<int2> GetDirectionsArray(Allocator allocator) {
        var directions = new NativeArray<int2>(4, allocator);
        directions[0] = new(0, 1); //up
        directions[1] = new(1, 0); //right
        directions[2] = new(0, -1); //down
        directions[3] = new(-1, 0); //left

        return directions;
    }

    public static bool IsGraphNode(int2 point, int2 nodeCenter) => point.Equals(nodeCenter);
}
