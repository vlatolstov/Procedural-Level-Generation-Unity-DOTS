using System;

[Flags]
public enum RoomType {
    Space = 0,
    Corridor = 1 << 0,
    Room = 1 << 1,
    Hall = 1 << 2
}