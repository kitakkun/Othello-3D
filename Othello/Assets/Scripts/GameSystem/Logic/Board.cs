using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UniRx;
using UnityEngine;

namespace GameSystem.Logic
{
    public class Board
    {
        // セルのマス数（横・縦）
        public const int CellSize = 8;

        // 盤面状態
        private ReactiveProperty<CellStatus>[,] _cells;

        // セル値更新検出用
        public IObservable<Value<CellStatus>> CellAsObservable(int x, int y) => _cells[x, y].Zip(_cells[x, y].Skip(1),
            (a, b) => new Value<CellStatus>(a, b)).AsObservable();

        // セル座標のバリデーション
        private bool IsValidPos(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < CellSize && pos.y >= 0 && pos.y < CellSize;
        }

        // コンストラクタ
        public Board()
        {
            InitializeProperties();
        }

        /// <summary>
        /// 配列を初期化します
        /// </summary>
        private void InitializeProperties()
        {
            // 配列の準備
            _cells = new ReactiveProperty<CellStatus>[CellSize, CellSize];
            // すべてのセル状態を空に
            for (int x = 0; x < CellSize; x++)
            {
                for (int y = 0; y < CellSize; y++)
                {
                    _cells[x, y] = new ReactiveProperty<CellStatus>(CellStatus.Empty);
                }
            }
        }

        /// <summary>
        /// 指定座標のセル状態を取得します
        /// </summary>
        /// <param name="pos">調べる座標</param>
        /// <returns>セル状態</returns>
        /// <exception cref="Exception"></exception>
        public CellStatus GetCellStatus(Vector2Int pos)
        {
            if (IsValidPos(pos))
            {
                return _cells[pos.x, pos.y].Value;
            }
            else
            {
                throw new Exception("Invalid position.");
            }
        }

        /// <summary>
        /// 指定セル状態で指定位置のセルを上書きします．
        /// </summary>
        /// <param name="color">置きたい石の色</param>
        /// <param name="pos">盤面上の位置</param>
        /// <exception cref="Exception"></exception>
        public void ForcePlace(CellStatus color, Vector2Int pos)
        {
            // 不正な位置であればエラーを吐く
            if (pos.x is < 0 or > CellSize - 1 || pos.y is < 0 or > CellSize - 1)
            {
                throw new Exception($"Illegal position was specified. Please give a value from 0 to {CellSize - 1}.");
            }
            _cells[pos.x, pos.y].Value = color;
        }

        /// <summary>
        /// 指定セル状態で指定位置のセルの上書きを試みます
        /// </summary>
        /// <param name="color">置きたい石の色</param>
        /// <param name="pos">盤面上の位置</param>
        public PlaceOperationCode TryPlace(CellStatus color, Vector2Int pos)
        {
            var options = GetAvailablePositions(color);
            // 配置可能位置であれば
            if (options.Contains(pos))
            {
                // 石を置き
                ForcePlace(color, pos);
                // ひっくり返す
                Reverse(color, pos);
                Debug.Log("Placed");
                return PlaceOperationCode.Accepted;
            }
            else
            {
                return PlaceOperationCode.Rejected;
            }
        }

        /// <summary>
        /// 石をひっくり返します
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pos"></param>
        private void Reverse(CellStatus color, Vector2Int pos)
        {
            var directions = Enum.GetValues(typeof(Direction)).Cast<Direction>();
            foreach (var direction in directions)
            {
                if (ReversibleInDirection(color, pos, direction))
                {
                    ReverseInDirection(color, pos, direction);
                }
            }
        }

        /// <summary>
        /// 指定方向へひっくり返します
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pos"></param>
        /// <param name="direction"></param>
        private void ReverseInDirection(CellStatus color, Vector2Int pos, Direction direction)
        {
            Vector2Int cursor = pos;
            while (true)
            {
                // y movement
                switch (direction)
                {
                    case Direction.Up:
                    case Direction.UpLeft:
                    case Direction.UpRight:
                        cursor.y++;
                        break;
                    case Direction.Down:
                    case Direction.DownLeft:
                    case Direction.DownRight:
                        cursor.y--;
                        break;
                }
                // x movement
                switch (direction)
                {
                    case Direction.Right:
                    case Direction.UpRight:
                    case Direction.DownRight:
                        cursor.x++;
                        break;
                    case Direction.Left:
                    case Direction.UpLeft:
                    case Direction.DownLeft:
                        cursor.x--;
                        break;
                }
                
                // 正しい座標位置であり，異色であれば
                var cellValue = _cells[cursor.x, cursor.y].Value;
                if (IsValidPos(cursor) && cellValue != color && cellValue != CellStatus.Empty)
                {
                    ForcePlace(color, cursor);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 現在の盤面状態において，配置可能な座標を取得します
        /// </summary>
        /// <param name="color">配置したい石の色</param>
        /// <returns></returns>
        public List<Vector2Int> GetAvailablePositions(CellStatus color)
        {
            var availableCells = new List<Vector2Int>();
            for (var x = 0; x < CellSize; x++)
            {
                for (var y = 0; y < CellSize; y++)
                {
                    var pos = new Vector2Int(x, y);
                    // Filter empty
                    if (_cells[x, y].Value == CellStatus.Empty)
                    {
                        if (Reversible(color, pos))
                        {
                            availableCells.Add(pos);
                        }
                    }

                }
            }

            return availableCells;
        }

        /// <summary>
        /// セル状態をカウントします
        /// </summary>
        /// <param name="status">カウントするセルの状態</param>
        /// <returns></returns>
        public int Count(CellStatus status)
        {
            int count = 0;
            for (var x = 0; x < CellSize; x++)
            {
                for (var y = 0; y < CellSize; y++)
                {
                    if (_cells[x, y].Value == status)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// ひっくり返すことが可能かどうかを判定します
        /// </summary>
        /// <param name="color">配置したい石の色</param>
        /// <param name="pos">配置したい位置</param>
        /// <returns></returns>
        public bool Reversible(CellStatus color, Vector2Int pos)
        {
            var directions = Enum.GetValues(typeof(Direction)).Cast<Direction>();
            bool isReversible = false;
            foreach (var direction in directions)
            {
                isReversible |= ReversibleInDirection(color, pos, direction);
            }

            return isReversible;
        }

        /// <summary>
        /// 指定方向においてひっくり返すことが可能かどうかを判定します
        /// </summary>
        /// <param name="color">配置したい石の色</param>
        /// <param name="pos">配置したい位置</param>
        /// <param name="direction">確認する方向</param>
        /// <returns></returns>
        public bool ReversibleInDirection(CellStatus color, Vector2Int pos, Direction direction)
        {
            var sequences = GetSequenceStatus(pos, direction);
            bool reversibleAppeared = false;
            foreach (var status in sequences)
            {
                if (status == CellStatus.Empty)
                {
                    return false;
                }
                else if (status != color)
                {
                    reversibleAppeared = true;
                }
                else
                {
                    if (reversibleAppeared)
                    {
                        return true;
                    }
                    break;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 指定位置から指定方向へ盤面情報を見ていきます
        /// </summary>
        /// <param name="pos">開始位置</param>
        /// <param name="direction">調べる方向</param>
        /// <returns>セル状態のリスト</returns>
        private List<CellStatus> GetSequenceStatus(Vector2Int pos, Direction direction, bool includeStartPoint = false)
        {
            var sequence = new List<CellStatus>();
            var x = pos.x;
            var y = pos.y;
            if (includeStartPoint)
            {
                sequence.Add(_cells[x, y].Value);
            }
            while (true)
            {
                // y-axis movement
                switch (direction)
                {
                    case Direction.Up:
                    case Direction.UpRight:
                    case Direction.UpLeft:
                        y++;
                        break;
                    case Direction.Down:
                    case Direction.DownLeft:
                    case Direction.DownRight:
                        y--;
                        break;
                }
                // x-axis movement
                switch (direction)
                {
                    case Direction.Left:
                    case Direction.UpLeft:
                    case Direction.DownLeft:
                        x--;
                        break;
                    case Direction.Right:
                    case Direction.UpRight:
                    case Direction.DownRight:
                        x++;
                        break;
                }
                if (IsValidPos(new Vector2Int(x, y)))
                {
                    sequence.Add(_cells[x, y].Value);
                }
                else
                {
                    break;
                }
            }
            return sequence;
        }
    }
}