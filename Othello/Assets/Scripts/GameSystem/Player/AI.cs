using System;
using System.Collections.Generic;
using GameSystem.Logic;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameSystem.Player
{
    public class AI : MonoBehaviour, IPlayer
    {
        private GameManager _manager;
        // 角を優先して取るかどうか
        public bool cornerPriority = true;

        private List<Vector2Int> _corners =
            new List<Vector2Int>()
            {
                new Vector2Int(0, Board.CellSize - 1),
                new Vector2Int(Board.CellSize - 1, Board.CellSize - 1),
                new Vector2Int(0, 0),
                new Vector2Int(Board.CellSize - 1, 0)
            };

        public void Setup(GameManager manager)
        {
            _manager = manager;
            _manager.Broker.Receive<GameEvent.TurnChange>()
                .Where(e => ReferenceEquals(this, e.Player))
                .Subscribe(_ =>
                {
                    Debug.Log("AI turn");
                    Observable.Timer(TimeSpan.FromSeconds(1f))
                        .Subscribe(_ =>
                        {
                            var pos = PlaceDisc();
                            _manager.Broker.Publish(
                                new GameEvent.PlaceRequest(this, pos));
                        });
                });
        }

        public Vector2Int PlaceDisc()
        {
            var list = _manager.GetAvailableCells();
            if (cornerPriority)
            {
                foreach (var v in _corners)
                {
                    if (list.Contains(v))
                    {
                        return v;
                    }
                }    
            }
            
            return list[Random.Range(0, list.Count)];
        }

        // placePointに石を設置した場合の最終的な自石の数をシミュレートします．
        // private int simulateGame(BoardController currentBoardController, Vector2Int placePoint)
        // {
        //     
        // }
    }

}