using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateCode : MonoBehaviour
{
    public int CellWidth = 20;
    public int CellHeight = 20;

    public int MaxRoomWidth = 7;
    public int MaxRoomHeight = 7;
    public int RoomTryCount = 100;


    public GameObject Prefab;

    public Color FloorColor = Color.green;
    public Color WallColor = Color.red;
    public Color RoomWallColor = Color.yellow;
    public List<Color> RoomColors = new List<Color>();

    public Vector3 StartPos = Vector3.zero;

    private CellModel[,] cells = null;



    public void Init()
    {
        cells = new CellModel[CellWidth, CellHeight];
        for (int x = 0; x < CellWidth; x++)
        {
            for (int y = 0; y < CellHeight; y++)
            {
                CellModel model = new CellModel();
                model.Obj = GameObject.Instantiate(Prefab);
                model.Width = x;
                model.Height = y;
                if (x == 0 || x == CellWidth - 1 || y == 0 || y == CellHeight - 1)
                {
                    model.BlockType = BlockTypeEnum.Bound;
                }
                else
                {
                    model.BlockType = BlockTypeEnum.Unused;
                }
                model.Obj.transform.position = new Vector3(x, y);
                model.Obj.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                model.Obj.transform.SetParent(this.transform);
                cells[x, y] = model;
            }
        }
    }

    public void Reset()
    {
        for (int x = 0; x < CellWidth; x++)
        {
            for (int y = 0; y < CellHeight; y++)
            {
                cells[x, y].BlockType = BlockTypeEnum.Unused;
                cells[x, y].Obj.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

    public void Generate()
    {

        GenerateRoom();

        GenerateLine();
        _startX = 1;
        _startY = 1;
        _startList.Clear();
        _lastMazeDir = Dir.None;
    }

    private int _startX, _startY;

    private List<CellModel> _startList = new List<CellModel>();

    private Dir _lastMazeDir;
    private const int WIGGLE_PERCENT = 50;

    private void GenerateLine()
    {
        #region 选择开始点

        if (_startY >= CellHeight - 1) return;
        while (cells[_startX, _startY].BlockType != BlockTypeEnum.Unused)
        {
            _startX += 2;
            if (_startX >= CellWidth - 1)
            {
                _startX = 1;
                _startY += 2;

                // Stop if we've scanned the whole dungeon.
                if (_startY >= CellHeight - 1)
                {
                    return;
                }
            }
        }
        SetType(_startX, _startY, BlockTypeEnum.Floor, FloorColor);
        _startList.Add(new CellModel() { Width = _startX, Height = _startY });
        #endregion

        while (_startList.Count > 0)
        {
            CellModel cell = _startList.Last();

            // See which adjacent cells are open.
            List<Dir> openDirs = new List<Dir>();

            //左
            if (InBounds(cell.Width - 3, cell.Height))
            {
                if (cells[cell.Width - 2, cell.Height].BlockType == BlockTypeEnum.Unused)
                {
                    openDirs.Add(Dir.Left);
                }
            }
            //右
            if (InBounds(cell.Width + 3, cell.Height))
            {
                if (cells[cell.Width + 2, cell.Height].BlockType == BlockTypeEnum.Unused)
                {
                    openDirs.Add(Dir.Right);
                }
            }
            //上
            if (InBounds(cell.Width, cell.Height + 3))
            {
                if (cells[cell.Width, cell.Height + 2].BlockType == BlockTypeEnum.Unused)
                {
                    openDirs.Add(Dir.Up);
                }
            }
            //下
            if (InBounds(cell.Width, cell.Height - 3))
            {
                if (cells[cell.Width, cell.Height - 2].BlockType == BlockTypeEnum.Unused)
                {
                    openDirs.Add(Dir.Down);
                }
            }
            if (openDirs.Count <= 0)
            {
                // No adjacent uncarved cells.
                _startList.Remove(cell);
                continue;
            }
            int rand = UnityEngine.Random.Range(0, 100);

            Dir dir = Dir.None;
            if (openDirs.Contains(_lastMazeDir) && rand > WIGGLE_PERCENT)
            {
                dir = _lastMazeDir;
            }
            else
            {
                int index = rand % openDirs.Count;
                dir = openDirs[index];
            }
            _lastMazeDir = dir;
            switch (dir)
            {
                case Dir.Up:
                    _startList.Add(new CellModel() { Width = cell.Width, Height = cell.Height + 2 });
                    SetType(cell.Width, cell.Height + 1, BlockTypeEnum.Floor, FloorColor);
                    SetType(cell.Width, cell.Height + 2, BlockTypeEnum.Floor, FloorColor);
                    break;
                case Dir.Down:
                    _startList.Add(new CellModel() { Width = cell.Width, Height = cell.Height - 2 });
                    SetType(cell.Width, cell.Height - 1, BlockTypeEnum.Floor, FloorColor);
                    SetType(cell.Width, cell.Height - 2, BlockTypeEnum.Floor, FloorColor);
                    break;
                case Dir.Left:
                    _startList.Add(new CellModel() { Width = cell.Width - 2, Height = cell.Height });
                    SetType(cell.Width - 1, cell.Height, BlockTypeEnum.Floor, FloorColor);
                    SetType(cell.Width - 2, cell.Height, BlockTypeEnum.Floor, FloorColor);
                    break;
                case Dir.Right:
                    _startList.Add(new CellModel() { Width = cell.Width + 2, Height = cell.Height });
                    SetType(cell.Width + 1, cell.Height, BlockTypeEnum.Floor, FloorColor);
                    SetType(cell.Width + 2, cell.Height, BlockTypeEnum.Floor, FloorColor);
                    break;
            }

        }
        GenerateLine();
        //FloodFill(1, 1, Dir.None);

        //for (int x = 0; x < CellWidth; x++)
        //{
        //    for (int y = 0; y < CellHeight; y++)
        //    {
        //        if (InBounds(x, y))
        //        {
        //            continue;
        //        }

        //    }
        //}
    }

    private void FloodFill(int x, int y, Dir dir)
    {
        if (InBounds(x, y))
        {
            BlockTypeEnum blockType = cells[x, y].BlockType;
            if (blockType == BlockTypeEnum.Unused)
            {
                //int rand = UnityEngine.Random.Range(0, 1000);
                int left = UnityEngine.Random.Range(0, 1000);
                int right = UnityEngine.Random.Range(0, 1000);
                int up = UnityEngine.Random.Range(0, 1000);
                int down = UnityEngine.Random.Range(0, 1000);
                SetType(x, y, BlockTypeEnum.Floor, FloorColor);
                if (left > 500)
                {
                    FloodFill(x - 1, y, Dir.Left);//Left
                }
                else
                {
                    if (GetBlockType(x - 1, y) == BlockTypeEnum.Unused)
                    {
                        SetType(x - 1, y, BlockTypeEnum.Wall, WallColor);
                    }
                }
                if (right > 500)
                {
                    FloodFill(x + 1, y, Dir.Left);//Right
                }
                else
                {
                    if (GetBlockType(x + 1, y) == BlockTypeEnum.Unused)
                    {
                        SetType(x + 1, y, BlockTypeEnum.Wall, WallColor);
                    }
                }
                if (up > 500)
                {
                    FloodFill(x, y + 1, Dir.Left);//Up
                }
                else
                {
                    if (GetBlockType(x, y + 1) == BlockTypeEnum.Unused)
                    {
                        SetType(x, y + 1, BlockTypeEnum.Wall, WallColor);
                    }
                }
                if (down > 500)
                {
                    FloodFill(x, y - 1, Dir.Left);//Down
                }
                else
                {
                    if (GetBlockType(x, y - 1) == BlockTypeEnum.Unused)
                    {
                        SetType(x, y - 1, BlockTypeEnum.Wall, WallColor);
                    }
                }

                //bool isHaveFloor = false;
                //switch (dir)
                //{
                //    case Dir.Up:
                //        blockType = GetBlockType(x, y - 1);
                //        if (blockType == BlockTypeEnum.Floor)
                //        {
                //            SetType(x, y, BlockTypeEnum.Floor, FloorColor);
                //            blockType = GetBlockType(x - 1, y);
                //            if (blockType == BlockTypeEnum.Unused)
                //            {
                //                SetType(x - 1, y, BlockTypeEnum.Wall, WallColor);
                //            }
                //            blockType = GetBlockType(x + 1, y);
                //            if (blockType == BlockTypeEnum.Unused)
                //            {
                //                SetType(x + 1, y, BlockTypeEnum.Wall, WallColor);
                //            }
                //        }
                //        FloodFill(x, y + 1, Dir.Up);//Down
                //        break;
                //    case Dir.Down:
                //        blockType = GetBlockType(x, y + 1);
                //        if (blockType == BlockTypeEnum.Floor)
                //        {
                //            SetType(x, y, BlockTypeEnum.Floor, FloorColor);
                //            blockType = GetBlockType(x - 1, y);
                //            if (blockType == BlockTypeEnum.Unused)
                //            {
                //                SetType(x - 1, y, BlockTypeEnum.Wall, WallColor);
                //            }
                //            blockType = GetBlockType(x + 1, y);
                //            if (blockType == BlockTypeEnum.Unused)
                //            {
                //                SetType(x + 1, y, BlockTypeEnum.Wall, WallColor);
                //            }
                //        }
                //        FloodFill(x, y - 1, Dir.Down);//Down
                //        break;
                //    case Dir.Left:
                //        blockType = GetBlockType(x + 1, y);
                //        if (blockType == BlockTypeEnum.Floor)
                //        {
                //            SetType(x, y, BlockTypeEnum.Floor, FloorColor);
                //            blockType = GetBlockType(x, y - 1);
                //            if (blockType == BlockTypeEnum.Unused)
                //            {
                //                SetType(x, y - 1, BlockTypeEnum.Wall, WallColor);
                //            }
                //            blockType = GetBlockType(x, y + 1);
                //            if (blockType == BlockTypeEnum.Unused)
                //            {
                //                SetType(x, y + 1, BlockTypeEnum.Wall, WallColor);
                //            }
                //        }
                //        FloodFill(x - 1, y, Dir.Left);//Left
                //        break;
                //    case Dir.Right:
                //        blockType = GetBlockType(x - 1, y);
                //        if (blockType == BlockTypeEnum.Floor)
                //        {
                //            SetType(x, y, BlockTypeEnum.Floor, FloorColor);
                //            blockType = GetBlockType(x, y - 1);
                //            if (blockType == BlockTypeEnum.Unused)
                //            {
                //                SetType(x, y - 1, BlockTypeEnum.Wall, WallColor);
                //            }
                //            blockType = GetBlockType(x, y + 1);
                //            if (blockType == BlockTypeEnum.Unused)
                //            {
                //                SetType(x, y + 1, BlockTypeEnum.Wall, WallColor);
                //            }
                //        }
                //        FloodFill(x + 1, y, Dir.Right);//Right
                //        break;
                //    case Dir.None:
                //    default:
                //        SetType(x, y, BlockTypeEnum.Floor, FloorColor);
                //        if (left < 70)
                //        {
                //            FloodFill(x - 1, y, Dir.Left);//Left
                //            isHaveFloor = true;
                //        }
                //        else
                //        {
                //            //SetType(x - 1, y, BlockTypeEnum.Wall, WallColor);
                //        }
                //        if (right < 70)
                //        {
                //            FloodFill(x + 1, y, Dir.Right);//Right
                //            isHaveFloor = true;
                //        }
                //        else
                //        {
                //            //SetType(x + 1, y, BlockTypeEnum.Wall, WallColor);
                //        }
                //        if (up < 70)
                //        {
                //            FloodFill(x, y + 1, Dir.Up);//Up
                //            isHaveFloor = true;
                //        }
                //        else
                //        {
                //            //SetType(x, y + 1, BlockTypeEnum.Wall, WallColor);
                //        }
                //        if (down < 70)
                //        {
                //            FloodFill(x, y - 1, Dir.Down);//Down
                //            isHaveFloor = true;
                //        }
                //        else
                //        {
                //            //SetType(x, y - 1, BlockTypeEnum.Wall, WallColor);
                //        }
                //        if (!isHaveFloor)
                //        {
                //            FloodFill(x, y + 1, Dir.Up);//Up
                //            FloodFill(x - 1, y, Dir.Left);//Left
                //            FloodFill(x + 1, y, Dir.Right);//Right
                //            FloodFill(x, y - 1, Dir.Down);//Down
                //        }
                //        break;
                //}
            }
        }


        //if (InBounds(x, y))
        //{
        //    if (cells[x, y].BlockType == BlockTypeEnum.Unused)
        //    {
        //        int rand = UnityEngine.Random.Range(0, 100);
        //        bool nextRoomWall = false;
        //        if (GetBlockType(x, y + 1) == BlockTypeEnum.RoomWall)
        //        {
        //            nextRoomWall = true;
        //        }
        //        else if (GetBlockType(x, y - 1) == BlockTypeEnum.RoomWall)
        //        {
        //            nextRoomWall = true;
        //        }
        //        else if (GetBlockType(x + 1, y) == BlockTypeEnum.RoomWall)
        //        {
        //            nextRoomWall = true;
        //        }
        //        else if (GetBlockType(x - 1, y) == BlockTypeEnum.RoomWall)
        //        {
        //            nextRoomWall = true;
        //        }
        //        if (nextRoomWall)
        //        {
        //            if (rand < 80)
        //            {
        //                SetType(x, y, BlockTypeEnum.Floor, FloorColor);
        //            }
        //            else
        //            {
        //                SetType(x, y, BlockTypeEnum.Wall, WallColor);
        //            }
        //        }
        //        else
        //        {
        //            BlockTypeEnum blockType = BlockTypeEnum.Unused;
        //            //rand 路的概率
        //            int floorRate = 0;
        //            switch (dir)
        //            {
        //                case Dir.Up:
        //                    blockType = GetBlockType(x, y - 1);
        //                    if (blockType == BlockTypeEnum.Wall)
        //                    {
        //                        floorRate = 20;
        //                        //if (rand < 80)
        //                        //{
        //                        //    SetType(x, y, BlockTypeEnum.Wall, WallColor);
        //                        //}
        //                        //else
        //                        //{
        //                        //    SetType(x, y, BlockTypeEnum.Floor, FloorColor);
        //                        //}
        //                    }
        //                    else if (blockType == BlockTypeEnum.Floor)
        //                    {
        //                        floorRate = 80;
        //                        //if (rand < 80)
        //                        //{
        //                        //    SetType(x, y, BlockTypeEnum.Floor, FloorColor);
        //                        //}
        //                        //else
        //                        //{
        //                        //    SetType(x, y, BlockTypeEnum.Wall, WallColor);
        //                        //}
        //                    }
        //                    else
        //                    {
        //                        floorRate = 50;
        //                        //if (rand < 50)
        //                        //{
        //                        //    SetType(x, y, BlockTypeEnum.Wall, WallColor);
        //                        //}
        //                        //else
        //                        //{
        //                        //    SetType(x, y, BlockTypeEnum.Floor, FloorColor);
        //                        //}
        //                    }
        //                    break;
        //                case Dir.Down:
        //                    blockType = GetBlockType(x, y + 1);
        //                    if (blockType == BlockTypeEnum.Wall)
        //                    {
        //                        floorRate = 20;
        //                    }
        //                    else if (blockType == BlockTypeEnum.Floor)
        //                    {
        //                        floorRate = 80;
        //                    }
        //                    else
        //                    {
        //                        floorRate = 50;
        //                    }
        //                    break;
        //                case Dir.Left:
        //                    blockType = GetBlockType(x + 1, y);
        //                    if (blockType == BlockTypeEnum.Wall)
        //                    {
        //                        floorRate = 20;
        //                    }
        //                    else if (blockType == BlockTypeEnum.Floor)
        //                    {
        //                        floorRate = 80;
        //                    }
        //                    else
        //                    {
        //                        floorRate = 50;
        //                    }
        //                    break;
        //                case Dir.Right:
        //                    blockType = GetBlockType(x - 1, y);
        //                    if (blockType == BlockTypeEnum.Wall)
        //                    {
        //                        floorRate = 20;
        //                    }
        //                    else if (blockType == BlockTypeEnum.Floor)
        //                    {
        //                        floorRate = 80;
        //                    }
        //                    else
        //                    {
        //                        floorRate = 50;
        //                    }
        //                    break;
        //                case Dir.None:
        //                default:
        //                    break;
        //            }
        //            if (rand < floorRate)
        //            {
        //                SetType(x, y, BlockTypeEnum.Floor, FloorColor);
        //            }
        //            else
        //            {
        //                SetType(x, y, BlockTypeEnum.Wall, WallColor);
        //            }
        //        }
        //        FloodFill(x, y + 1, Dir.Up);//Up
        //        FloodFill(x - 1, y, Dir.Left);//Left
        //        FloodFill(x + 1, y, Dir.Right);//Right
        //        FloodFill(x, y - 1, Dir.Down);//Down

        //    }

        //}

    }

    private void SetType(int x, int y, BlockTypeEnum type, Color color)
    {
        try
        {
            cells[x, y].BlockType = type;
            cells[x, y].Obj.GetComponent<Renderer>().material.color = color;
        }
        catch (Exception ex)
        {

            throw new Exception("x:" + x + "====y:" + y);
        }

    }



    private BlockTypeEnum GetBlockType(int x, int y)
    {
        if (InBounds(x, y))
        {
            return cells[x, y].BlockType;
        }
        return BlockTypeEnum.Wall;
        //throw new Exception("x:" + x + "===>y:" + y);
    }

    //private List<Point> GetPoint(int x, int y)
    //{
    //    List<Point> points = new List<Point>()
    //    {
    //        new Point(){ X = x, Y = y + 1,Dir = Dir.Up},
    //        new Point(){ X = x - 1, Y = y,Dir = Dir.Left},
    //        new Point(){ X = x + 1, Y = y,Dir = Dir.Right},
    //        new Point(){ X = x, Y = y - 1,Dir = Dir.Down},
    //    };
    //    return points.Where(p => InBounds(p.X, p.Y)).ToList();
    //}

    public bool InBounds(int x, int y)
    {
        return x > 0 && x < CellWidth - 1 && y > 0 && y < CellHeight - 1;
    }

    private void GenerateRoom()
    {
        for (int i = 0; i < RoomTryCount; i++)
        {
            int roomHeight = UnityEngine.Random.Range(3, MaxRoomHeight);
            int roomWidth = UnityEngine.Random.Range(3, MaxRoomWidth);
            int startX = UnityEngine.Random.Range(1, CellWidth - 2);
            //int roomHeight = UnityEngine.Random.Range(2, MaxRoomHeight) * 2 + 1;
            //int roomWidth = UnityEngine.Random.Range(2, MaxRoomWidth) * 2 + 1;
            //int startX = UnityEngine.Random.Range(0, Mathf.FloorToInt((CellWidth - roomWidth) * 1f / 2) * 2 + 1);
            int endX = startX + roomWidth;
            if (endX >= CellWidth)
            {
                continue;
            }
            int startY = UnityEngine.Random.Range(1, CellHeight - 2);
            //int startY = UnityEngine.Random.Range(0, Mathf.FloorToInt((CellHeight - roomHeight) * 1f / 2) * 2 + 1);
            int endY = startY + roomHeight;
            if (endY >= CellHeight)
            {
                continue;
            }
            bool isCanCreate = true;

            int checkStartX = startX - 1;
            int checkEndX = endX + 1;

            int checkStartY = startY - 1;
            int checkEndY = endY + 1;
            for (int x = checkStartX; x < checkEndX; x++)
            {
                for (int y = checkStartY; y < checkEndY; y++)
                {
                    if (cells[x, y].BlockType != BlockTypeEnum.Unused)
                    {
                        isCanCreate = false;
                        goto End;
                    }
                }
            }
            End:
            if (isCanCreate)
            {
                //Debug.LogError(string.Format("StartX:{0}---EndX:{1}====StartY:{2}---EndY:{3}", startX, endX, startY, endY));
                Color floorColor = RoomColors[UnityEngine.Random.Range(0, RoomColors.Count
                    )];

                for (int x = startX; x < endX; x++)
                {
                    for (int y = startY; y < endY; y++)
                    {

                        if (y == startY || y == (endY - 1) || x == startX || x == (endX - 1))
                        {
                            cells[x, y].BlockType = BlockTypeEnum.RoomWall;
                            cells[x, y].Obj.GetComponent<Renderer>().material.color = RoomWallColor;
                        }
                        else
                        {
                            cells[x, y].BlockType = BlockTypeEnum.RoomFloor;
                            cells[x, y].Obj.GetComponent<Renderer>().material.color = floorColor;
                        }
                    }
                }
            }
        }
    }
}
