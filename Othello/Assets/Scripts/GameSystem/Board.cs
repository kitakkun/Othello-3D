using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using Color = UnityEngine.Color;

namespace GameSystem
{
    public class Board : MonoBehaviour
    {
        private ReactiveCollection<CellStatus> _cells;
        public const int CellSize = 8;
        public CellStatus Turn { get; private set; }

        public IObservable<CollectionReplaceEvent<CellStatus>> CellChangeAsObservable()
            => _cells.ObserveReplace();
            

        void Awake()
        {
            Turn = CellStatus.Black;
            _cells = new ReactiveCollection<CellStatus>();
            for (var x = 0; x < CellSize; x++)
            {
                for (var y = 0; y < CellSize; y++)
                {
                    _cells.Add(CellStatus.Empty);
                }
            }

            _cells.ObserveReplace()
                .Subscribe(replace =>
                {
                    var x = replace.Index % CellSize;
                    var y = replace.Index / CellSize;
                    var status = replace.OldValue;
                    var newStatus = replace.NewValue;
                    Debug.Log(
                        $"[REPLACE] Cell at ({x}, {y}) was replaced. {status.ToString()} => {newStatus.ToString()}");
                })
                .AddTo(this);

            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    IndicateAvailablePos();
                })
                .AddTo(this);
        }

        void Start()
        {
            PutInitialDiscs();
        }

        void IndicateAvailablePos()
        {
            var list = GetAvailableCells(Turn);
            Debug.Log(list.Count);
            foreach (var cell in FindObjectsOfType<BoardCell>())
            {
                cell.TurnOffHighlight();
                if (list.Contains(cell.X + cell.Y * CellSize))
                {
                    cell.TurnOnHighlight();
                }
            }
        }

        public void PutInitialDiscs()
        {
            ForcePutDisc(CellSize / 2, CellSize/2, CellStatus.Black);
            ForcePutDisc(CellSize / 2 - 1, CellSize/2 - 1, CellStatus.Black);
            ForcePutDisc(CellSize / 2, CellSize/2 - 1, CellStatus.White);
            ForcePutDisc(CellSize / 2 - 1, CellSize/2, CellStatus.White);
        }

        public void PutDisc(int pos, CellStatus color)
        {
            _cells[pos] = color;
            Reverse(pos, color);
        }

        private void ForcePutDisc(int x, int y, CellStatus color)
        {
            _cells[x + y * CellSize] = color;
        }

        List<int> GetAvailableCells(CellStatus color)
        {
            var availableCells = new List<int>();
            for (var x = 0; x < CellSize; x++)
            {
                for (var y = 0; y < CellSize; y++)
                {
                    var pos = x + y * CellSize;
                    // Filter empty
                    if (_cells[pos] == CellStatus.Empty)
                    {
                        if (CanPutDisc(pos, color))
                        {
                            availableCells.Add(pos);
                        }
                    }

                }
            }

            return availableCells;
        }

        void Reverse(int cellNumber, CellStatus color)
        {
            var directions = GetReversableDirection(cellNumber, color);
            foreach (var d in directions)
            {
                ReverseInDirection(cellNumber, color, d);
            }
            ChangeTurn();
        }

        void ChangeTurn()
        {
            if (Turn == CellStatus.Black)
            {
                Turn = CellStatus.White;
            }
            else
            {
                Turn = CellStatus.Black;
            }
            
        }

        void ReverseInDirection(int cellNumber, CellStatus color, Direction direction)
        {
            var position = cellNumber;

            bool flag = false;
            while (position is >= 0 and <= CellSize * CellSize - 1)
            {
                if (_cells[position] == color && flag) break;
                _cells[position] = color;
                flag = true;
                // y-axis movement
                 switch (direction)
                 {
                     case Direction.Up:
                     case Direction.UpRight:
                     case Direction.UpLeft:
                         position -= CellSize;
                         break;
                     case Direction.Down:
                     case Direction.DownLeft:
                     case Direction.DownRight:
                         position += CellSize;
                         break;
                 }
                 // x-axis movement
                 switch (direction)
                 {
                     case Direction.Left:
                     case Direction.UpLeft:
                     case Direction.DownLeft:
                         position -= 1;
                         break;
                     case Direction.Right:
                     case Direction.UpRight:
                     case Direction.DownRight:
                         position += 1;
                         break;
                 }
            }
        }

        List<Direction> GetReversableDirection(int cellNumber, CellStatus color)
        {
            var result = new List<Direction>();
            var directions = new Direction[]
            {
                Direction.Up, Direction.Down, Direction.Left, Direction.Right,
                Direction.UpLeft, Direction.UpRight, Direction.DownLeft, Direction.DownRight
            };
            foreach (Direction direction in directions)
            {
                if (CanPutDisc(cellNumber, color, direction))
                {
                    result.Add(direction);
                }
                
            }

            return result;
        }

        bool CanPutDisc(int cellNumber, CellStatus color)
        {
            return CanPutDisc(cellNumber, color, Direction.Up)
                   || CanPutDisc(cellNumber, color, Direction.Down)
                   || CanPutDisc(cellNumber, color, Direction.DownRight)
                   || CanPutDisc(cellNumber, color, Direction.DownLeft)
                   || CanPutDisc(cellNumber, color, Direction.UpRight)
                   || CanPutDisc(cellNumber, color, Direction.UpLeft)
                   || CanPutDisc(cellNumber, color, Direction.Left)
                   || CanPutDisc(cellNumber, color, Direction.Right);
        }

        bool CanPutDisc(int cellNumber, CellStatus color, Direction direction)
        {
            var sequence = GetSequenceStatus(cellNumber, direction);
            bool canPut = false;
            foreach (var c in sequence)
            {
                if (c == CellStatus.Empty) return false;
                else if (c != color)
                {
                    canPut = true;
                    continue;
                }
                else
                {
                    break;
                }
            }

            return canPut;
        }

        List<CellStatus> GetSequenceStatus(int cellNumber, Direction direction)
        {
            var sequence = new List<CellStatus>();
            var position = cellNumber;
            
            while (position is >= 0 and <= CellSize * CellSize - 1)
            {
                // add status
                sequence.Add(_cells[position]);
                // y-axis movement
                switch (direction)
                {
                    case Direction.Up:
                    case Direction.UpRight:
                    case Direction.UpLeft:
                        position -= CellSize;
                        break;
                    case Direction.Down:
                    case Direction.DownLeft:
                    case Direction.DownRight:
                        position += CellSize;
                        break;
                }
                // x-axis movement
                switch (direction)
                {
                    case Direction.Left:
                    case Direction.UpLeft:
                    case Direction.DownLeft:
                        position -= 1;
                        break;
                    case Direction.Right:
                    case Direction.UpRight:
                    case Direction.DownRight:
                        position += 1;
                        break;
                }
            }
            sequence.RemoveAt(0);

            return sequence;
        }
    }


    public enum Direction
    {
        Up = 2, Down = 3, Left = 5, Right = 7,
        UpLeft, UpRight, DownLeft, DownRight 
        // Up * Left = 10
        // Up * Right = 14
        // Down * Left = 15
        // Down * Right = 21
    }

    public enum CellStatus
    {
        Empty, Black, White
    }
}