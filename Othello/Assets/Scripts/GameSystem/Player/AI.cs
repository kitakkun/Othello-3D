using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameSystem.Logic;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Unit = UniRx.Unit;

namespace GameSystem.Player
{
    public class AI : MonoBehaviour, IPlayer
    {
        private GameManager _manager;
        // 角を優先して取るかどうか
        public bool cornerPriority = true;
        // 敵が角を取れるようになる手を避けるか
        public bool avoidGivingCorner = true;
        // シミュレーションを行う回数
        [SerializeField] int simulationRepeats = 150;

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
            // 角を優先的に取る
            if (cornerPriority)
            {
                foreach (var v in Constants.Corners)
                {
                    if (list.Contains(v))
                    {
                        return v;
                    }
                }    
            }
            // 角を取られてしまうような手は省く
            SimulatorBoard simulator = new SimulatorBoard(_manager.GetCloneBoard(), _manager.Color(this));
            if (avoidGivingCorner)
            {
                var removeVectors = new List<Vector2Int>();
                for (var i = 0; i < list.Count; i++)
                {
                    if (simulator.CornerWillTakenUp(list[i]))
                    {
                        removeVectors.Add(list[i]);
                    }
                }

                if (list.Count != removeVectors.Count)
                {
                    foreach (var v in removeVectors)
                    {
                        list.Remove(v);
                    }
                }
            }

            // シミュレーション
            return await DoSimulation(simulator, list, simulationRepeats);
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
            if (options.Count == 0) return Vector2Int.zero;
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
        
        // 指定座標に置いた場合，角が取られてしまうかどうか
        public bool CornerWillTakenUp(Vector2Int pos)
        {
            Init();
            Put(Turn, pos);
            ChangeTurn();
            var opponentOptions = Bit2xy(AvailablePositions(Turn));
            foreach (var option in opponentOptions)
            {
                if (Constants.Corners.Contains(option))
                {
                    return true;
                }
            }

            return false;
        }

        private void Init()
        {
            this.Black = baseBoard.Black;
            this.White = baseBoard.White;
            Turn = selfColor;
        }
        

        // posに置いた場合の最終結果(自石の数)を返します
        public async UniTask<int> Simulate(Vector2Int pos)
        {
            Init();
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