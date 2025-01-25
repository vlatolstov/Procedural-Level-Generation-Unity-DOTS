using Unity.Entities;

public struct LevelData : IComponentData {
    public Rect Level;
    public Rect InnerZone;
}
