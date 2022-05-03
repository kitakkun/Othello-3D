using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
                .Subscribe(async(_) =>
                    {
                        Debug.Log("AI turn");
                        await UniTask.SwitchToThreadPool();
                        var pos = await PlaceDisc();
                        await UniTask.SwitchToMainThread();
                        _manager.Broker.Publish(
                            new GameEvent.PlaceRequest(this, pos));
                    });
        }

        private async UniTask<Vector2Int> PlaceDisc()
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

            // シミュレーション
            SimulatorBoard simulator = new SimulatorBoard(_manager.GetCloneBoard(), _manager.Color(this));
            return await DoSimulation(simulator, list, 80);
        }

        async UniTask<Vector2Int> DoSimulation(SimulatorBoard simulator, List<Vector2Int> options, int repeats = 100)
        {
            List<UniTask<int>> tasks = new List<UniTask<int>>();
            foreach (var v in options)
            {
                for (var i = 0; i < repeats; i++)
                {
                    tasks.Add(simulator.Simulate(v));
                }
            }

            var results = await UniTask.WhenAll(tasks);
            var estimatedCounts = new Dictionary<Vector2Int, int>();
            for (var i = 0; i < repeats * options.Count; i += repeats)
            {
                double sum = 0;
                for (var j = i; j < repeats; j++)
                {
                    sum += results[j];
                }

                var estimatedResult = (int)(sum / repeats);
                
                estimatedCounts.Add(options[i/repeats], estimatedResult);
            }

            int maxValue = 0;
            Vector2Int result = options[0];
            foreach ((var key, var value) in estimatedCounts)
            {
                if (value > maxValue)
                {
                    maxValue = value;
                    result = key;
                }
            }

            return result;
        }
    }

    // シミュレーション用
    public class SimulatorBoard : BitBoard
    {
        private bool Turn;
        private BitBoard baseBoard;
        private bool selfColor;
        
        public SimulatorBoard(BitBoard current, bool selfColor)
        {
            baseBoard = current;
            this.selfColor = selfColor;
        }

        // posに置いた場合の最終結果(自石の数)を返します
        public async UniTask<int> Simulate(Vector2Int pos)
        {
            this.Black = baseBoard.Black;
            this.White = baseBoard.White;
            Turn = selfColor;
            var code = Put(Turn, pos);
            await DoUntilConcludes();
            return Count(selfColor);
        }

        private async UniTask<Unit> DoUntilConcludes()
        {
            return await Task.Run(() =>
            {
                while (!Concluded)
                {
                    ChangeTurn();
                    var options = Bit2xy(AvailablePositions(Turn));
                    if (options.Count == 0)
                    {
                        continue;
                    }

                    var rnd = new System.Random();
                    var selectedPos = options[rnd.Next(0, options.Count)];
                    Put(Turn, selectedPos);
                }

                return new Unit();
            });
            
        }

        // ターン変更
        public void ChangeTurn()
        {
            Turn = !Turn;
        }
    }

}