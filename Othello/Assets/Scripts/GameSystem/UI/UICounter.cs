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
        private ObservableBitBoard _board;
        private BoardController _boardController;

        private void Start()
        {
            _boardController = FindObjectOfType<BoardController>();
            _board = _boardController.Board;
            _board.BlackAsObservable().Subscribe(_ => UpdateUI());
            _board.WhiteAsObservable().Subscribe(_ => UpdateUI());
        }

        private void UpdateUI()
        {
            var blackCnt = _boardController.Board.Count(Constants.ColorBlack);
            var whiteCnt = _boardController.Board.Count(Constants.ColorWhite);
            _textMeshProUGUI.text = $"BLACK: {blackCnt}, WHITE: {whiteCnt}";
        }
    }
}