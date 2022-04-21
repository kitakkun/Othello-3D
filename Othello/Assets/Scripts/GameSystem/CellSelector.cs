using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace GameSystem
{
    public class CellSelector : MonoBehaviour
    {
        private Board _board;
        void Start()
        {
            _board = FindObjectOfType<Board>();
            this.UpdateAsObservable()
                .Where(_ => Input.GetMouseButtonDown(0))
                .Subscribe(_ =>
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 0.5f, false);
                    if (Physics.Raycast(ray, out hit, 100))
                    {
                        var obj = hit.collider.gameObject;
                        var cell = obj.GetComponent<BoardCell>();
                        Debug.Log($"Hit: x = {cell.X} y = {cell.Y}");
                        _board.PutDisc(cell.X + Board.CellSize * cell.Y, _board.Turn);
                    }
                })
                .AddTo(this);
        }
    }
}
