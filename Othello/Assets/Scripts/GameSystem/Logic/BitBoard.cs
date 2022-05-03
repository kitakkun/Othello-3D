using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GameSystem.Logic
{
    // ビットボード
    public class BitBoard
    {
        // 黒の石の位置(1: 黒石, 0: 空)
        public virtual UInt64 Black { get => _black; protected set => _black = value; } 
        private UInt64 _black;
        // 白の石の位置(1: 白石, 0: 空)
        public virtual UInt64 White { get => _white; protected set => _white = value; }    
        private UInt64 _white;

        // 石が配置されている箇所が1, それ以外が0
        public UInt64 PlacedArea => Black | White;
        // 石が配置されていない箇所が1, それ以外が0
        public UInt64 EmptyArea => ~PlacedArea;
        // ゲーム終了フラグ
        public bool Concluded => EmptyArea == 0 || AvailablePositions(Constants.ColorWhite) == 0 &&
            AvailablePositions(Constants.ColorBlack) == 0;
        
        // ビット配列を(x, y)座標の配列へ変換 1の場所がそれぞれ座標として返る
        public List<Vector2Int> Bit2xy(UInt64 map)
        {
            var positions = new List<Vector2Int>();
            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    if ((map & CoordinateToBit(x, y)) != 0)
                    {
                        positions.Add(new Vector2Int(x, y));
                    }
                }
            }

            return positions;
        }
        
        public UInt64 Color2Board(bool color)
        {
            return color == Constants.ColorBlack ? Black : White;
        }

        private void UpdateBoard(bool color, UInt64 newBoard)
        {
            var _ = color == Constants.ColorBlack ? Black = newBoard : White = newBoard;
        }
        
        public BitBoard(){}

        public BitBoard(BitBoard board)
        {
            this._white = board.White;
            this._black = board.Black;
        }

        public void DebugPrint(UInt64 map)
        {
            var str = "";
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if ((map & CoordinateToBit(x, y)) != 0)
                    {
                        str += " 1️ ";
                    }
                    else
                    {
                        str += " 0 ";
                    }
                }

                str += "\n";
            }

            Debug.Log(str);  
        }
        public void DebugPrint()
        {
            var str = "";
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if ((Black & CoordinateToBit(x, y)) != 0)
                    {
                        str += " 1️ ";
                    }
                    else if ((White & CoordinateToBit(x, y)) != 0)
                    {
                        str += " 2 ️";
                    }
                    else
                    {
                        str += " 0 ";
                    }
                }

                str += "\n";
            }

            Debug.Log(str);
        }

        // ゲームの準備をします
        public void Ready()
        {
            // 初期配置
            Black = 0x0000000810000000;
            White = 0x0000001008000000;
        }

        /// <summary>
        /// (x, y)座標をビット列へ変換します(0基準)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public UInt64 CoordinateToBit(int x, int y)
        {
            UInt64 mask = 0x8000000000000000;  // 左端のみ立っているビット列
            mask >>= x;               // 横ずらし(x)
            mask >>= y * 8;         // 縦ずらし(y)
            return mask;
        }

        public PlaceOperationCode Put(bool selfColor, Vector2Int pos, bool reverse = true)
        {
            return Put(selfColor, pos.x, pos.y, reverse);
        }

        // (x, y)座標に配置します
        public PlaceOperationCode Put(bool selfColor, int x, int y, bool reverse=true)
        {
            if (AvailablePositions(selfColor) == 0)
            {
                return PlaceOperationCode.Skipped;
            }
            if (!Positionable(selfColor, x, y))
            {
                return PlaceOperationCode.Rejected;
            }
            // 石の配置
            var selfBoard = Color2Board(selfColor);
            var place = CoordinateToBit(x, y);
            // selfBoard |= place;
            // UpdateBoard(selfColor, selfBoard);
            
            // ひっくり返す処理
            if (reverse)
            {
                var reverseMap = ReverseMap(selfColor, place);
                DebugPrint(reverseMap);
                Reverse(selfColor, reverseMap, place);
            }

            return PlaceOperationCode.Accepted;
        }

        // 配置可能位置を算出
        public bool Positionable(bool selfColor, int x, int y)
        {
            UInt64 place = CoordinateToBit(x, y);
            UInt64 availables = AvailablePositions(selfColor);
            return (place & availables) == place;
        }
        
        // カウント
        public int Count(bool color)
        {
            var board = Color2Board(color);
            board = board - ((board >> 1) & 0x5555555555555555UL);
            board = (board & 0x3333333333333333UL) + ((board >> 2) & 0x3333333333333333UL);
            return (int)(unchecked(((board + (board >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
        }

        // 反転マス検出
        public UInt64 ReverseMap(bool selfColor, UInt64 place)
        {
            UInt64 reverseMap = 0;
            // ８方向について
            for (var d = 0; d < 8; d++)
            {
                UInt64 revTMP = 0;
                UInt64 mask = Transfer(place, d);
                while (mask != 0 && (mask & Color2Board(!selfColor)) != 0)
                {
                    revTMP |= mask;
                    mask = Transfer(mask, d);
                }
                if ((mask & Color2Board(selfColor)) != 0)
                {
                    reverseMap |= revTMP;
                }
            }

            return reverseMap;
        }

        // 反転処理
        public void Reverse(bool selfColor, UInt64 reverseMap, UInt64 place)
        {
            var selfBoard = Color2Board(selfColor);
            var opponentBoard = Color2Board(!selfColor);
            selfBoard ^= reverseMap | place;
            opponentBoard ^= reverseMap;
            UpdateBoard(selfColor, selfBoard);
            UpdateBoard(!selfColor, opponentBoard);
        }
        
        private UInt64 Transfer(UInt64 place, int direction)
        {
            switch (direction)
            {
                case Constants.Up:
                    return (place << Constants.VerticalShift) & 0xffffffffffffff00;
                case Constants.UpRight: 
                    return (place << Constants.SlashShift) & 0x7f7f7f7f7f7f7f00;
                case Constants.Right:
                    return (place >> Constants.HorizontalShift) & 0x7f7f7f7f7f7f7f7f;
                case Constants.DownRight: 
                    return (place >> Constants.BackslashShift) & 0x007f7f7f7f7f7f7f;
                case Constants.Down:
                    return (place >> Constants.VerticalShift) & 0x00ffffffffffffff;
                case Constants.DownLeft:
                    return (place >> Constants.SlashShift) & 0x00fefefefefefefe;
                case Constants.Left:
                    return (place << Constants.HorizontalShift) & 0xfefefefefefefefe;
                case Constants.UpLeft: 
                    return (place << Constants.BackslashShift) & 0xfefefefefefefe00;
                default:
                    return 0;
            }
            
        }

        // 配置可能位置を取得
        public UInt64 AvailablePositions(bool selfColor)
        {
            var player = Color2Board(selfColor);
            var opponent = Color2Board(!selfColor);
            UInt64 hChecker = opponent & 0x7e7e7e7e7e7e7e7e;    // 横
            UInt64 vChecker = opponent & 0x00FFFFFFFFFFFF00;    // 縦
            UInt64 hvChecker = opponent & 0x007e7e7e7e7e7e00;   // 斜め
            UInt64 empty = EmptyArea;

            UInt64 availables;
            UInt64 tmp;
            
            // 左チェック
            tmp = hChecker & (player << Constants.HorizontalShift);
            tmp |= hChecker & (tmp << Constants.HorizontalShift);
            tmp |= hChecker & (tmp << Constants.HorizontalShift);
            tmp |= hChecker & (tmp << Constants.HorizontalShift);
            tmp |= hChecker & (tmp << Constants.HorizontalShift);
            tmp |= hChecker & (tmp << Constants.HorizontalShift);
            availables = empty & (tmp << Constants.HorizontalShift);

            // 右チェック
            tmp = hChecker & (player >> Constants.HorizontalShift);
            tmp |= hChecker & (tmp >> Constants.HorizontalShift);
            tmp |= hChecker & (tmp >> Constants.HorizontalShift);
            tmp |= hChecker & (tmp >> Constants.HorizontalShift);
            tmp |= hChecker & (tmp >> Constants.HorizontalShift);
            tmp |= hChecker & (tmp >> Constants.HorizontalShift);
            availables |= empty & (tmp >> Constants.HorizontalShift);
            
            // 上チェック
            tmp = vChecker & (player << Constants.VerticalShift);
            tmp |= vChecker & (tmp << Constants.VerticalShift);
            tmp |= vChecker & (tmp << Constants.VerticalShift);
            tmp |= vChecker & (tmp << Constants.VerticalShift);
            tmp |= vChecker & (tmp << Constants.VerticalShift);
            tmp |= vChecker & (tmp << Constants.VerticalShift);
            availables |= empty & (tmp << Constants.VerticalShift);
                         
            // 下チェック
            tmp = vChecker & (player >> Constants.VerticalShift);
            tmp |= vChecker & (tmp >> Constants.VerticalShift);
            tmp |= vChecker & (tmp >> Constants.VerticalShift);
            tmp |= vChecker & (tmp >> Constants.VerticalShift);
            tmp |= vChecker & (tmp >> Constants.VerticalShift);
            tmp |= vChecker & (tmp >> Constants.VerticalShift);
            availables |= empty & (tmp >> Constants.VerticalShift);
            
            // 左上チェック
            tmp = hvChecker & (player << Constants.BackslashShift);
            tmp |= hvChecker & (tmp << Constants.BackslashShift);
            tmp |= hvChecker & (tmp << Constants.BackslashShift);
            tmp |= hvChecker & (tmp << Constants.BackslashShift);
            tmp |= hvChecker & (tmp << Constants.BackslashShift);
            tmp |= hvChecker & (tmp << Constants.BackslashShift);
            availables |= empty & (tmp << Constants.BackslashShift);
            
            // 右下チェック
            tmp = hvChecker & (player >> Constants.BackslashShift);
            tmp |= hvChecker & (tmp >> Constants.BackslashShift);
            tmp |= hvChecker & (tmp >> Constants.BackslashShift);
            tmp |= hvChecker & (tmp >> Constants.BackslashShift);
            tmp |= hvChecker & (tmp >> Constants.BackslashShift);
            tmp |= hvChecker & (tmp >> Constants.BackslashShift);
            availables |= empty & (tmp >> Constants.BackslashShift);
                        
            // 右上チェック
            tmp = hvChecker & (player << Constants.SlashShift);
            tmp |= hvChecker & (tmp << Constants.SlashShift);
            tmp |= hvChecker & (tmp << Constants.SlashShift);
            tmp |= hvChecker & (tmp << Constants.SlashShift);
            tmp |= hvChecker & (tmp << Constants.SlashShift);
            tmp |= hvChecker & (tmp << Constants.SlashShift);
            availables |= empty & (tmp << Constants.SlashShift);
            
            // 左下チェック
            tmp = hvChecker & (player >> Constants.SlashShift);
            tmp |= hvChecker & (tmp >> Constants.SlashShift);
            tmp |= hvChecker & (tmp >> Constants.SlashShift);
            tmp |= hvChecker & (tmp >> Constants.SlashShift);
            tmp |= hvChecker & (tmp >> Constants.SlashShift);
            tmp |= hvChecker & (tmp >> Constants.SlashShift);
            availables |= empty & (tmp >> Constants.SlashShift);

            return availables;
        }

    }
    
    public class Constants
    {
        public const int HorizontalShift = 1;    // 横方向にずらす際のビットシフト回数
        public const int VerticalShift = 8;     // 縦方向にずらす際のビットシフト回数
        public const int SlashShift = 7;        // 斜め方向（/）にずらす際のビットシフト回数
        public const int BackslashShift = 9;    // 斜め方向(\)にずらす際のビットシフト回数
        public const int Up = 0, UpRight = 1, Right = 2, DownRight = 3, Down = 4,
            DownLeft = 5, Left = 6, UpLeft = 7; // 方向定義

        public const bool ColorBlack = true;
        public const bool ColorWhite = false;
    }
}