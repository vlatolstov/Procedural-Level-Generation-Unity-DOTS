using Unity.Entities;
using Unity.Mathematics;

public struct LevelGenerationData : IComponentData {
    public int CellSize;
    public int SideRoomsGap;
    public int CellsPerRoom;
    public float LevelScale;

    public int RoomsCount;
    public int InnerZoneHallsCount;
    public float AdditionalHallEntranceProbability;

    public int2 CorridorWidth;
    public float ConnectNotMSTNodesProbability;
    public int NodesPercentToRemove;

    public RoomType AllowedTypesMask;
}
