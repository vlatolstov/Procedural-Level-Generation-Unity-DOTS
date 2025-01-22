using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct RoomCentersData : IComponentData {
    public NativeList<int2> RoomCenters;
}
