using GameSystem.Logic;
using UnityEngine;

namespace GameSystem.Visuals
{
    public class BoardController : MonoBehaviour
    {
        public Board Board { get; private set; }
        private BoardCell[] _boardCells;

        // セットアップ
        public void Setup()
        {
            // 盤面を初期化
            Board = new Board();
            _boardCells = FindObjectsOfType<BoardCell>();
            foreach (var cell in _boardCells)
            {
                cell.Setup(this);
            }
        }

        // ライトアップ
        public void IndicateAvailablePos(CellStatus turnColor)
        {
            var list = Board.GetAvailablePositions(turnColor);
            foreach (var cell in _boardCells)
            {
                cell.TurnOffHighlight();
                if (list.Contains(new Vector2Int(cell.X, cell.Y)))
                {
                    cell.TurnOnHighlight();
                }
            }
        }
    }
}