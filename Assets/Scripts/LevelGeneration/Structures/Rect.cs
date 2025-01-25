using Unity.Mathematics;

public readonly struct Rect {
    public readonly int2 From;
    public readonly int2 To;

    public Rect(int2 from, int2 to) {
        From = from;
        To = to;
    }

    public readonly bool Contains(int2 point) {
        return point.x >= From.x && point.x <= To.x
            && point.y >= From.y && point.y <= To.y;
    }
}
