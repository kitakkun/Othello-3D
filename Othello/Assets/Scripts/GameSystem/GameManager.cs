using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace GameSystem
{
    // オセロゲーム進行を行うクラス
    public class GameManager : MonoBehaviour
    {
        private Board _board; // 盤面情報の管理
        private IPlayer[] _players; // プレイヤー
        private Dictionary<IPlayer, CellStatus> _discColor;     // 各プレイヤーの石の色
        private ReactiveProperty<IPlayer> _turn;  // 現在ターンのプレイヤー
        public MessageBroker Broker;    // イベント発行

        public IObservable<IPlayer> Turn => _turn;

        void Awake()
        {
            Broker = new MessageBroker();
            _turn = new ReactiveProperty<IPlayer>();
            _players = GetComponents<IPlayer>();
            _discColor = new Dictionary<IPlayer, CellStatus>
            {
                {_players[0], CellStatus.Black},
                {_players[1], CellStatus.White}
            };
            _board = FindObjectOfType<Board>();

            // ターンの変更時に石配置を要求
            Turn.Subscribe(turn =>
            {
                Broker.Publish(new GameEvent.TurnChange(turn));
            })
            .AddTo(this);

            Broker.Receive<GameEvent.PlaceRequest>()
                .Where(r => r.Player == _turn.Value)
                .Subscribe(r =>
                {
                    var status = PlaceDisc(r.Player, r.Position);
                    // 再度配置を要求
                    if (status == PlaceOperationStatus.Rejected)
                    {
                        Broker.Publish(new GameEvent.TurnChange(_turn.Value));
                    }
                    else
                    {
                        ChangeTurn();
                    }
                })
                .AddTo(this);


        }

        void Start()
        {
            _turn.Value = _players[0];
        }

        private PlaceOperationStatus PlaceDisc(IPlayer player, Vector2Int position)
        {
            var color = _discColor[player];
            return _board.PutDisc(position.x, position.y, color);
        }

        // セルに対する配置処理
        // public void PlaceDisc(IPlayer player, int x, int y)
        // {
        //     var color = _discColor[player];
        //     _board.PutDisc(x, y, color);
        //     ChangeTurn();
        // }
    

        // ターン変更処理
        void ChangeTurn()
        {
            if (_turn.Value == _players[0]) _turn.Value = _players[1];
            else if (_turn.Value == _players[1]) _turn.Value = _players[0];
        }
        
        // 石の配置要求
        void RequestPut(IPlayer player)
        {
            Broker.Publish(new GameEvent.TurnChange(player));
        }

        public CellStatus GetTurnColor()
        {
            return _discColor[_turn.Value];
        }

        public List<Vector2Int> GetAvailableCells()
        {
            return _board.GetAvailableCells(GetTurnColor());
        }
        
    }
}