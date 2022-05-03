using GameSystem.Visuals;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace GameSystem.Player
{
    // 手動操作するプレイヤー
    public class Player : MonoBehaviour, IPlayer
    {
        private GameManager _manager;

        public void Setup(GameManager manager)
        {
            // マウスボタンが押下されたら
            this.UpdateAsObservable()
                // .Where(_ => _isSelecting)
                .Where(_ => Input.GetMouseButtonDown(0))
                .Select(_ => Input.mousePosition)
                .Select(PosToCellPos)
                .Subscribe(cellPos =>
                {
                    if (cellPos != null)
                        manager.Broker.Publish(
                            new GameEvent.PlaceRequest(this, cellPos.Value));
                })
                .AddTo(this);
            // タッチされたら
            this.UpdateAsObservable()
                .Where(_ => Input.touchSupported)
                .Where(_ => Input.touches.Length > 0)
                .Select(_ => (Vector3) Input.GetTouch(0).position)
                .Select(PosToCellPos)
                .Subscribe(cellPos =>
                {
                    if (cellPos != null)
                        manager.Broker.Publish(
                            new GameEvent.PlaceRequest(this, cellPos.Value));
                })
                .AddTo(this);
        }

        private Vector2Int? PosToCellPos(Vector3 position)
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(position);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 0.5f, false);
            if (Physics.Raycast(ray, out hit, 100))
            {
                var obj = hit.collider.gameObject;
                var cell = obj.GetComponent<BoardCell>();
                if (cell != null) return new Vector2Int(cell.X, cell.Y);
            }

            return null;
        }
    }
}