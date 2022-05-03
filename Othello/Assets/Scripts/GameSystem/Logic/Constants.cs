using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Logic
{
    public class Constants
    {
        public static readonly List<Vector2Int> Corners =
            new List<Vector2Int>()
            {
                new Vector2Int(0, 7),
                new Vector2Int(7, 7),
                new Vector2Int(0, 0),
                new Vector2Int(7, 0)
            };
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