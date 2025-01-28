using Unity.Entities;

public struct InputData : IComponentData {
    public float Horizontal;
    public float Vertical;

    public float MouseX;
    public float MouseY;

    public bool Spacebar;
    public bool LShift;
}
