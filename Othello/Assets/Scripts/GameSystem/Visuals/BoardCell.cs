using GameSystem.Logic;
using UniRx;
using UnityEngine;

namespace GameSystem.Visuals
{
    public class BoardCell : MonoBehaviour
    {
        [SerializeField] private GameObject _discPrefab;
        [SerializeField] private float _discYOffset = .5f;
        [SerializeField] private Light _light;
        private Animator _animator;
        private BoardController _boardController;
        private GameObject _disc;

        public int X { get; private set; }

        public int Y { get; private set; }

        // セル状態をリセットします
        private void Reset()
        {
            _animator.SetTrigger("reset");
        }

        public void Setup(BoardController controller)
        {
            FindSelfPosition();
            _disc = Instantiate(_discPrefab, transform.position + Vector3.up * _discYOffset, Quaternion.identity,
                transform);
            _animator = _disc.GetComponent<Animator>();
            controller.Board.CellAsObservable(X, Y)
                .Subscribe(v =>
                {
                    var oldStatus = v.oldValue;
                    var newStatus = v.newValue;
                    if (oldStatus == CellStatus.Empty && newStatus == CellStatus.Black)
                        PlaceBlack();
                    else if (oldStatus == CellStatus.Empty && newStatus == CellStatus.White)
                        PlaceWhite();
                    else if (oldStatus == CellStatus.Black && newStatus == CellStatus.White)
                        FlipDiscToWhite();
                    else if (oldStatus == CellStatus.White && newStatus == CellStatus.Black)
                        FlipDiscToBlack();
                    else if (newStatus == CellStatus.Empty) Reset();
                })
                .AddTo(this);
        }

        // めんどくさいので計算でx, y座標を出します
        private void FindSelfPosition()
        {
            var position = transform.localPosition;
            X = (int) (position.x + 3.5);
            Y = (int) (position.z + 3.5);
        }

        public void TurnOnHighlight()
        {
            _light.enabled = true;
        }

        public void TurnOffHighlight()
        {
            _light.enabled = false;
        }

        private void FlipDiscToWhite()
        {
            _animator.SetTrigger("flipToWhite");
        }

        private void FlipDiscToBlack()
        {
            _animator.SetTrigger("flipToBlack");
        }

        private void PlaceWhite()
        {
            _animator.SetTrigger("putWhite");
        }

        private void PlaceBlack()
        {
            _animator.SetTrigger("putBlack");
        }
    }
}