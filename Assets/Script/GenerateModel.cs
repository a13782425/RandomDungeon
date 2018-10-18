using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Dir
{
    None,
    Up,
    Down,
    Left,
    Right
}

public enum BlockTypeEnum
{
    Unused,
    Floor,
    Wall,
    RoomWall,
    RoomFloor,
    Door,
    Start,
    End,
    Bound,
}
public class Point
{
    public int X;
    public int Y;
    public Dir Dir;
}


public class CellModel
{
    public int X;
    public int Y;
    public GameObject Obj;
    public BlockTypeEnum BlockType;
}
