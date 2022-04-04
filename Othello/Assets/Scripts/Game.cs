using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    Board board;
    // Start is called before the first frame update
    void Start()
    {
        board = new Board();
        board.PlaceBlackDisc(2, 3);
        board.PrintOnConsole();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public struct Point {
    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public int x;
    public int y;
}

// セルの状態
public enum CellStatus 
{
    Empty, Black, White
}

// 方向
public enum Direction 
{
    UpperLeft, Left, LowerLeft,
    UpperRight, Right, LowerRight,
    Up, Down
}

// 盤面クラス
public class Board 
{
    private CellStatus[,] cells;
    private const int size = 8;

    // 初期化 ===============================================================
    public Board()
    {
        cells = new CellStatus[size, size];
        Init();
    }

    // 盤面を準備します
    void Init()
    {
        Reset();
        PutInitialDiscs();
    }

    // 盤面をすべてEmptyで初期化します．
    void Reset()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                cells[x, y] = CellStatus.Empty;
            }
        }
    }

    // 初期の石を置く
    void PutInitialDiscs()
    {
        UpdateCellStatus(size / 2, size / 2, CellStatus.White);
        UpdateCellStatus(size / 2 - 1, size / 2 - 1, CellStatus.White);
        UpdateCellStatus(size / 2, size / 2 - 1, CellStatus.Black);
        UpdateCellStatus(size / 2 - 1, size / 2, CellStatus.Black);
    }

    // セル状態を更新します
    void UpdateCellStatus(int x, int y, CellStatus status)
    {
        cells[x, y] = status;
    }

    // 盤面の状態をコンソールに出力します．
    public void PrintOnConsole()
    {
        string outputString = "";
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                switch (cells[x, y])
                {
                    case CellStatus.Black:  outputString += "1"; break;
                    case CellStatus.White:  outputString += "2"; break;
                    default:                outputString += "0"; break;
                }
            }
            outputString += "\n";
        }
        Debug.Log(outputString);
    }

    // 黒石を置きます
    public void PlaceBlackDisc(int x, int y)
    {
        if (CanReverseInSomeDirection(x, y, CellStatus.Black))
        {
            UpdateCellStatus(x, y, CellStatus.Black);
        }
    }

    // 白石を置きます
    public void PlaceWhiteDisc(int x, int y)
    {
        if (CanReverseInSomeDirection(x, y, CellStatus.White))
        {
            UpdateCellStatus(x, y, CellStatus.White);
        }
    }

    // 石を置ける座標を取得します
    public List<Point> GetAvailablePoints(int x, int y, CellStatus color)
    {
        List<Point> points = new List<Point>();
        for (int x_t = 0; x_t < size; x_t++)
        {
            for (int y_t = 0; y_t < size; y_t++)
            {
                if (CanReverseInSomeDirection(x_t, y_t, color))
                {
                    points.Add(new Point(x_t, y_t));
                }
            }
        }
        return points;
    }



    // 8方向いずれかでリバースできるか
    public bool CanReverseInSomeDirection(int x, int y, CellStatus color)
    {
        Direction[] directions = { Direction.Up, Direction.UpperLeft, Direction.UpperRight, 
                                   Direction.Down, Direction.LowerLeft, Direction.LowerRight,
                                   Direction.Left, Direction.Right };
        foreach (Direction direction in directions)
        {
            if (CanReverseInDirection(x, y, color, direction))
            {
                return true;
            }   
        }
        return false;
    }

    // 指定方向でリバースできるか
    bool CanReverseInDirection(int x, int y, CellStatus color, Direction direction)
    {
        CellStatus[] statuses = GetCellStatusesInDirection(x, y, direction);
        CellStatus opponentColor = GetOpponentCellStatus(color);

        if (statuses.Length == 0) return false;
        
        // 同色を探す
        int indexOfColor = new List<CellStatus>(statuses).IndexOf(color);

        // 同色の手前まで異色が続けばリバース可能．続かなければリバース不可能
        for (int i = 0; i < indexOfColor; i++)
        {
            if (statuses[i] != opponentColor)
            {
                return false;
            }
        }
        return true;
    }

    // セル状態を指定方向に向けて調べていきます
    CellStatus[] GetCellStatusesInDirection(int x, int y, Direction direction)
    {
        List<CellStatus> statuses = new List<CellStatus>();
        while (x < cells.GetLength(0) - 1 && y < cells.GetLength(1) - 1 && x > 0 && y > 0) {
            // movement in x-axis
            switch (direction)
            {
                case Direction.UpperLeft:
                case Direction.LowerLeft:
                case Direction.Left:
                    x--; break;
                case Direction.UpperRight:
                case Direction.LowerRight:
                case Direction.Right:
                    x++; break;
            }
            // movement in y-axis
            switch (direction)
            {
                case Direction.UpperLeft:
                case Direction.UpperRight:
                case Direction.Up:
                    y++; break;
                case Direction.LowerLeft:
                case Direction.LowerRight:
                case Direction.Down:
                    y--; break;
            }
            statuses.Add(GetCellStatus(x, y));
        }
        return statuses.ToArray();
    }

    // x, y座標のセル状態を取得します
    CellStatus GetCellStatus(int x, int y)
    {
        return cells[x, y];
    }

    // 敵の色を示すCellStatusを取得します
    CellStatus GetOpponentCellStatus(CellStatus color)
    {
        switch (color)
        {
            case CellStatus.Black: return CellStatus.White;
            case CellStatus.White: return CellStatus.Black;
            default:               return CellStatus.Empty;
        }
    }

}