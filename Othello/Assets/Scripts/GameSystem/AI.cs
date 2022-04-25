using System;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameSystem
{
    public class AI : MonoBehaviour, IPlayer
    {
        private GameManager _manager;
        void Start()
        {
            _manager = FindObjectOfType<GameManager>();
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
            return list[Random.Range(0, list.Count)];
        }
        
    }
}