using Unity.Entities;

public struct TilePrefabsData : IComponentData {
    public Entity CorridorTilePrefab;
    public Entity WallTilePrefab;
    public Entity SpaceTilePrefab;
    public Entity SpawnPointPrefab;
    public Entity RoomTilePrefab;
    public Entity HallTilePrefab;
}
