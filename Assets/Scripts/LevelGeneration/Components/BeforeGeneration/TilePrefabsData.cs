using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public struct TilePrefabsData : IComponentData {
    public NativeArray<Entity> BaseCorridorFloorTiles;
    public NativeArray<Entity> VariantCorridorFloorTiles;
    public NativeArray<Entity> BaseCorridorWallTiles;
    public NativeArray<Entity> VariantCorridorWallTiles;
    public NativeArray<Entity> BaseRoomFloorTiles;
    public NativeArray<Entity> VariantRoomFloorTiles;
    public NativeArray<Entity> BaseRoomWallTiles;
    public NativeArray<Entity> VariantRoomWallTiles;
}
