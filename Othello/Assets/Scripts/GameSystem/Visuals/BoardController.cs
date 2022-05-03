using GameSystem.Logic;
using UnityEngine;

namespace GameSystem.Visuals
{
    public class BoardController : MonoBehaviour
    {
        private BoardCell[] _boardCells;
        public ObservableBitBoard Board { get; private set; }

        // セットアップ
        public void Setup()
        {
            // 盤面を初期化
            Board = new ObservableBitBoard();
            _boardCells = FindObjectsOfType<BoardCell>();
            foreach (var cell in _boardCells) cell.Setup(this);
        }

        // ライトアップ
        public void IndicateAvailablePos(bool turnColor)
        {
            var list = Board.Bit2xy(Board.AvailablePositions(turnColor));
            foreach (var cell in _boardCells)
            {
                cell.TurnOffHighlight();
                if (list.Contains(new Vector2Int(cell.X, cell.Y))) cell.TurnOnHighlight();
            }
        }
    }
}