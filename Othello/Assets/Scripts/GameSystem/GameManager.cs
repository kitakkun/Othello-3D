using System;
using System.Collections.Generic;
using GameSystem.Logic;
using GameSystem.Visuals;
using UniRx;
using UnityEngine;

namespace GameSystem
{
    // オセロゲーム進行を行うクラス
    public class GameManager : MonoBehaviour
    {
        private Board _board;  // 盤面の状態制御
        private BoardController _boardController; // 盤面情報の管理
        private IPlayer[] _players; // プレイヤー
        private Dictionary<IPlayer, CellStatus> _discColor;     // 各プレイヤーの石の色
        private ReactiveProperty<IPlayer> _turn;  // 現在ターンのプレイヤー
        public MessageBroker Broker;    // イベント発行

        public IObservable<IPlayer> Turn => _turn;

        private Vector2Int _initialRightUp = new Vector2Int(Board.CellSize / 2, Board.CellSize / 2);
        private Vector2Int _initialRightDown = new Vector2Int(Board.CellSize / 2, Board.CellSize / 2 - 1);
        private Vector2Int _initialLeftUp = new Vector2Int(Board.CellSize / 2 - 1, Board.CellSize / 2);
        private Vector2Int _initialLeftDown = new Vector2Int(Board.CellSize / 2 - 1, Board.CellSize / 2 - 1);

        void Awake()
        {
            _boardController = FindObjectOfType<BoardController>();
            _boardController.Setup();
            _board = _boardController.Board;
            Broker = new MessageBroker();
            _turn = new ReactiveProperty<IPlayer>();
            _players = GetComponents<IPlayer>();
            foreach (var player in _players)
            {
                player.Setup(this);
            }
            _discColor = new Dictionary<IPlayer, CellStatus>
            {
                {_players[0], CellStatus.Black},
                {_players[1], CellStatus.White}
            };

            // ターンの変更時に石配置を要求
            Turn.Where(t => t != null).Subscribe(turn =>
            {
                Broker.Publish(new GameEvent.TurnChange(turn));
                _boardController.IndicateAvailablePos(GetTurnColor());
            })
            .AddTo(this);

            // 配置要求を受けたとき
            Broker.Receive<GameEvent.PlaceRequest>()
                .Where(r => r.Player == _turn.Value)
                .Subscribe(r =>
                {
                    var color = _discColor[r.Player];
                    var code = _board.TryPlace(color, r.Position);
                    // 再度配置を要求
                    if (code == PlaceOperationCode.Rejected)
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
            // 初期石配置
            PlaceInitialDiscs();
            _turn.Value = _players[0];
        }

        void PlaceInitialDiscs()
        {
            _board.ForcePlace(CellStatus.Black, _initialLeftUp);
            _board.ForcePlace(CellStatus.Black, _initialRightDown);
            _board.ForcePlace(CellStatus.White, _initialLeftDown);
            _board.ForcePlace(CellStatus.White, _initialRightUp);
            
        }
        
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
            return _board.GetAvailablePositions(GetTurnColor());
        }
        
    }
}