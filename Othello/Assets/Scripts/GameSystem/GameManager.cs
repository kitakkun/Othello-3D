using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using GameSystem.Logic;
using GameSystem.Visuals;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace GameSystem
{
    // オセロゲーム進行を行うクラス
    public class GameManager : MonoBehaviour
    {
        private ObservableBitBoard _board;  // 盤面の状態制御
        private BoardController _boardController; // 盤面情報の管理
        private IPlayer[] _players; // プレイヤー
        private Dictionary<IPlayer, bool> _discColor;     // 各プレイヤーの石の色
        private ReactiveProperty<IPlayer> _turn;  // 現在ターンのプレイヤー
        public MessageBroker Broker;    // イベント発行

        public IObservable<IPlayer> Turn => _turn;

        // 自石の色取得用
        public bool Color(IPlayer player) => _discColor[player]; 

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
            _discColor = new Dictionary<IPlayer, bool>
            {
                {_players[0], Constants.ColorBlack},
                {_players[1], Constants.ColorWhite}
            };
            
            // ゲーム終了時にエンターキーで再度プレイ
            this.UpdateAsObservable()
                .Where(_ => _board.Concluded)
                .Where(_ => Input.GetKeyDown(KeyCode.Return))
                .Subscribe(_ =>
                {
                    _board.Reset();
                    _board.Ready();
                })
                .AddTo(this);

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
                    var code = _board.Put(color, r.Position.x, r.Position.y);
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
            _board.Ready();
        }
        
        // ターン変更処理
        void ChangeTurn()
        {
            if (_turn.Value == _players[0]) _turn.Value = _players[1];
            else if (_turn.Value == _players[1]) _turn.Value = _players[0];
        }

        public bool GetTurnColor()
        {
            return _discColor[_turn.Value];
        }

        public List<Vector2Int> GetAvailableCells()
        {
            return _board.Bit2xy(_board.AvailablePositions(GetTurnColor()));
        }

        public BitBoard GetCloneBoard()
        {
            return new BitBoard(_board);
        }
    }
}