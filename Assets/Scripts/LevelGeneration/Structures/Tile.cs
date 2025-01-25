using Unity.Mathematics;

public struct Tile {
    public readonly int2 Position;
    public RoomElement Element;
    public RoomType Type;
    public readonly bool IsGraphNode;
    public bool IsSpawnPoint;

    public Tile(int2 position, RoomElement element, RoomType type, bool isGraphNode = false, bool isSpawnPoint = false) {
        Position = position;
        Element = element;
        Type = type;
        IsGraphNode = isGraphNode;
        IsSpawnPoint = isSpawnPoint;
    }
}

