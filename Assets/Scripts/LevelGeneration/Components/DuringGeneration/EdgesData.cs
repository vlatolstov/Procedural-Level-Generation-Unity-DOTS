using Unity.Collections;
using Unity.Entities;

public struct EdgesData : IComponentData {
    public NativeArray<Edge> Edges;
}
