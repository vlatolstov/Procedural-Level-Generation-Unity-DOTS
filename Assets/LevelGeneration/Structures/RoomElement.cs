using System;

[Flags]
public enum RoomElement {
    Space = 0,
    Floor = 1 << 0,
    LeftWall = 1 << 1,
    RightWall = 1 << 2,
    TopWall = 1 << 3,
    BottomWall = 1 << 4,
    TopLeftWall = LeftWall | TopWall,
    TopRightWall = RightWall | TopWall,
    BottomLeftWall = LeftWall | BottomWall,
    BottomRightWall = RightWall | BottomWall,
    Column = LeftWall | RightWall | TopWall | BottomLeftWall
}