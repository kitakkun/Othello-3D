using TMPro;
using UniRx;
using UnityEngine;

namespace GameSystem
{
    public class UICounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
        private Board _board;
        void Start()
        {
            _board = FindObjectOfType<Board>();
            for (var x = 0; x < Board.CellSize; x++)
            {
                for (var y = 0; y < Board.CellSize; y++)
                {
                    _board.CellAsObservable(x, y)
                        .Subscribe(_ => UpdateUI()) .AddTo(this);
                }
            }
        }

        void UpdateUI()
        {
            var blackCnt = _board.CountCell(CellStatus.Black);
            var whiteCnt = _board.CountCell(CellStatus.White);
            _textMeshProUGUI.text = $"BLACK: {blackCnt}, WHITE: {whiteCnt}";
        }
    }
}
