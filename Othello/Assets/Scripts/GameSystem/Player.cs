using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace GameSystem
{
    // 手動操作するプレイヤー
    public class Player : MonoBehaviour, IPlayer
    {
        private GameManager _manager;
        private CellSelector _selector;

        void Start()
        {
            _manager = FindObjectOfType<GameManager>();
       
            // マウスボタンが押下されたら
            this.UpdateAsObservable()
                // .Where(_ => _isSelecting)
                .Where(_ => Input.GetMouseButtonDown(0))
                .Select(_ => Input.mousePosition)
                .Select(pos => PosToCellPos(pos))
                .Subscribe(cellPos =>
                {
                    if (cellPos != null)
                    {
                        _manager.Broker.Publish(
                            new GameEvent.PlaceRequest(this, cellPos.Value));
                    }
                })
                .AddTo(this);
        }

        Vector2Int? PosToCellPos(Vector3 position)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(position);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 0.5f, false);
            if (Physics.Raycast(ray, out hit, 100))
            {
                var obj = hit.collider.gameObject;
                var cell = obj.GetComponent<BoardCell>();
                if (cell != null)
                {
                    return new Vector2Int(cell.X, cell.Y);
                }
            }

            return null;
        }

    }
}
