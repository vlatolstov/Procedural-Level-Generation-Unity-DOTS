using Unity.Mathematics;

public readonly struct Edge {
    public readonly int2 PointA;
    public readonly int2 PointB;

    public Edge(int2 pointA, int2 pointB) {
        PointA = pointA;
        PointB = pointB;
    }
}
