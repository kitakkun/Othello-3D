using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                                    new GameEvent.PlaceRequest(this, pos.Result));
                            });
                    });
        }

        private async Task<Vector2Int> PlaceDisc()
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
            // var simResults = new Dictionary<Vector2Int, float>();
            // var simlationRepeats = 20;
            // List<Task> tasks = new List<Task>();
            // foreach (var pos in list)
            // {
            //     var t = Task.Run(
            //         () => simResults.Add(pos, SimulateGame(_manager.BoardCells, list[0], simlationRepeats)));
            //     tasks.Add(t);
            // }
            //
            // foreach (var task in tasks)
            // {
            //     await task.ConfigureAwait(false);
            // }
            //
            var rnd = new System.Random();
            Vector2Int selectPos = list[rnd.Next(0, list.Count)];
            // float maxTMP = 0;
            // foreach (var (pos, count) in simResults)
            // {
            //     if (count >= maxTMP)
            //     {
            //         selectPos = pos;
            //         maxTMP = count;
            //     }
            // }

            return selectPos;
        }

        // placePointに石を設置した場合の最終的な自石の数をシミュレートします．平均値を返します
        private float SimulateGame(CellStatus[,] current, Vector2Int placePoint, int repeats = 1)
        {
            SimulatorBoard simBoard = new SimulatorBoard(current);
            float sum = 0;
            for (int i = 0; i < repeats; i++)
            {
                // sum += simBoard.Simulate(_manager.Color(this), placePoint);
            }

            sum /= repeats;
            return sum;
        }
    }

    // シミュレーション用
    public class SimulatorBoard
    {
        public Board Board;
        public CellStatus Turn;
        private CellStatus[,] baseBoard;

        public SimulatorBoard(CellStatus[,] current)
        {
            baseBoard = current;
        }

        // posに置いた場合の最終結果(自石の数)を返します
        public int Simulate(CellStatus color, Vector2Int pos)
        {
            Board = new Board(baseBoard);
            Turn = color;
            var code = Board.TryPlace(Turn, pos);
            while (!Board.FilledAll)
            {
                ChangeTurn();
                var options = Board.GetAvailablePositions(Turn);
                if (options.Count == 0)
                {
                    continue;
                }

                var rnd = new System.Random();
                var selectedPos = options[rnd.Next(0, options.Count)];
                Board.TryPlace(Turn, selectedPos);
            }

            return Board.Count(color);
        }

        // ターン変更
        public void ChangeTurn()
        {
            if (Turn == CellStatus.Black) Turn = CellStatus.White;
            else if (Turn == CellStatus.White) Turn = CellStatus.Black;
        }
    }

}