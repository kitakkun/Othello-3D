using GameSystem.Logic;
using GameSystem.Visuals;
using TMPro;
using UniRx;
using UnityEngine;

namespace GameSystem.UI
{
    public class UICounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
        private BoardController _boardController;
        void Start()
        {
            _boardController = FindObjectOfType<BoardController>();
            for (var x = 0; x < Board.CellSize; x++)
            {
                for (var y = 0; y < Board.CellSize; y++)
                {
                    _boardController.Board.CellAsObservable(x, y)
                        .Subscribe(_ => UpdateUI()) .AddTo(this);
                }
            }
        }

        void UpdateUI()
        {
            var blackCnt = _boardController.Board.Count(Constants.ColorBlack);
            var whiteCnt = _boardController.Board.Count(Constants.ColorWhite);
            _textMeshProUGUI.text = $"BLACK: {blackCnt}, WHITE: {whiteCnt}";
        }
    }
}
