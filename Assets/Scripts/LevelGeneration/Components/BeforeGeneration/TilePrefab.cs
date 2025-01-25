using Unity.Mathematics;
using Unity.Entities;

[InternalBufferCapacity(0)]
public struct TilePrefab : IBufferElementData {
    public Entity Prefab;
}

public struct TilesIndexesInBufferComponent : IComponentData {
    public int2 BaseCorridorFloorTiles;
    public int2 VariantCorridorFloorTiles;
    public int2 BaseCorridorWallTiles;
    public int2 VariantCorridorWallTiles;
    public int2 BaseRoomFloorTiles;
    public int2 VariantRoomFloorTiles;
    public int2 BaseRoomWallTiles;
    public int2 VariantRoomWallTiles;
}