using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Logic
{
    // ビットボード
    public class BitBoard
    {
        private ulong _black;

        private ulong _white;

        public BitBoard()
        {
        }

        public BitBoard(BitBoard board)
        {
            _white = board.White;
            _black = board.Black;
        }

        // 黒の石の位置(1: 黒石, 0: 空)
        public virtual ulong Black
        {
            get => _black;
            protected set => _black = value;
        }

        // 白の石の位置(1: 白石, 0: 空)
        public virtual ulong White
        {
            get => _white;
            protected set => _white = value;
        }

        // 石が配置されている箇所が1, それ以外が0
        public ulong PlacedArea => Black | White;

        // 石が配置されていない箇所が1, それ以外が0
        public ulong EmptyArea => ~PlacedArea;

        // ゲーム終了フラグ
        public bool Concluded => EmptyArea == 0 || (AvailablePositions(Constants.ColorWhite) == 0 &&
                                                    AvailablePositions(Constants.ColorBlack) == 0);

        // ビット配列を(x, y)座標の配列へ変換 1の場所がそれぞれ座標として返る
        public List<Vector2Int> Bit2xy(ulong map)
        {
            var positions = new List<Vector2Int>();
            for (var y = 0; y < 8; y++)
            for (var x = 0; x < 8; x++)
                if ((map & CoordinateToBit(x, y)) != 0)
                    positions.Add(new Vector2Int(x, y));

            return positions;
        }

        public ulong Color2Board(bool color)
        {
            return color == Constants.ColorBlack ? Black : White;
        }

        private void UpdateBoard(bool color, ulong newBoard)
        {
            var _ = color == Constants.ColorBlack ? Black = newBoard : White = newBoard;
        }

        // ゲームの準備をします
        public void Ready()
        {
            // 初期配置
            Black = 0x0000000810000000;
            White = 0x0000001008000000;
        }

        /// <summary>
        ///     (x, y)座標をビット列へ変換します(0基準)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ulong CoordinateToBit(int x, int y)
        {
            var mask = 0x8000000000000000; // 左端のみ立っているビット列
            mask >>= x; // 横ずらし(x)
            mask >>= y * 8; // 縦ずらし(y)
            return mask;
        }

        public PlaceOperationCode Put(bool selfColor, Vector2Int pos, bool reverse = true)
        {
            return Put(selfColor, pos.x, pos.y, reverse);
        }

        // (x, y)座標に配置します
        public PlaceOperationCode Put(bool selfColor, int x, int y, bool reverse = true)
        {
            if (AvailablePositions(selfColor) == 0) return PlaceOperationCode.Skipped;

            if (!Positionable(selfColor, x, y)) return PlaceOperationCode.Rejected;

            // 石の配置
            var selfBoard = Color2Board(selfColor);
            var place = CoordinateToBit(x, y);
            // selfBoard |= place;
            // UpdateBoard(selfColor, selfBoard);

            // ひっくり返す処理
            if (reverse)
            {
                var reverseMap = ReverseMap(selfColor, place);
                Reverse(selfColor, reverseMap, place);
            }

            return PlaceOperationCode.Accepted;
        }

        // 配置可能位置を算出
        public bool Positionable(bool selfColor, int x, int y)
        {
            var place = CoordinateToBit(x, y);
            var availables = AvailablePositions(selfColor);
            return (place & availables) == place;
        }

        // カウント
        public int Count(bool color)
        {
            var tmp = Color2Board(color);
            var counter = 0;
            for (var i = 0; i < Constants.CellSize * Constants.CellSize; i++)
            {
                if ((tmp & 1) == 1) counter++;
                tmp >>= 1;
            }

            return counter;
        }

        // 反転マス検出
        public ulong ReverseMap(bool selfColor, ulong place)
        {
            ulong reverseMap = 0;
            // ８方向について
            for (var d = 0; d < 8; d++)
            {
                ulong revTMP = 0;
                var mask = Transfer(place, d);
                while (mask != 0 && (mask & Color2Board(!selfColor)) != 0)
                {
                    revTMP |= mask;
                    mask = Transfer(mask, d);
                }

                if ((mask & Color2Board(selfColor)) != 0) reverseMap |= revTMP;
            }

            return reverseMap;
        }

        // 反転処理
        public void Reverse(bool selfColor, ulong reverseMap, ulong place)
        {
            var selfBoard = Color2Board(selfColor);
            var opponentBoard = Color2Board(!selfColor);
            selfBoard ^= reverseMap | place;
            opponentBoard ^= reverseMap;
            UpdateBoard(selfColor, selfBoard);
            UpdateBoard(!selfColor, opponentBoard);
        }

        private ulong Transfer(ulong place, int direction)
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
        public ulong AvailablePositions(bool selfColor)
        {
            var player = Color2Board(selfColor);
            var opponent = Color2Board(!selfColor);
            var hChecker = opponent & 0x7e7e7e7e7e7e7e7e; // 横
            var vChecker = opponent & 0x00FFFFFFFFFFFF00; // 縦
            var hvChecker = opponent & 0x007e7e7e7e7e7e00; // 斜め
            var empty = EmptyArea;

            ulong availables;
            ulong tmp;

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
}