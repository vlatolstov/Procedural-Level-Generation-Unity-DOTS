using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct HallsCentersData : IComponentData {
    public NativeList<int2> HallCenters;
}
