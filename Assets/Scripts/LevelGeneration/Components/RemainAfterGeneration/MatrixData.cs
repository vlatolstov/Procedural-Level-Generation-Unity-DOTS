using Unity.Collections;
using Unity.Entities;

public struct MatrixData : IComponentData {
    public NativeArray<Tile> Matrix;
}
