using System;
using UniRx;

namespace GameSystem.Logic
{
    // BitBoardの変化を観測可能にしたもの
    public class ObservableBitBoard : BitBoard
    {
        private readonly ReactiveProperty<ulong> _black;
        private readonly ReactiveProperty<CellStatus>[,] _cells;
        private readonly ReactiveProperty<ulong> _white;

        public ObservableBitBoard()
        {
            _cells = new ReactiveProperty<CellStatus>[8, 8];
            for (var x = 0; x < 8; x++)
            for (var y = 0; y < 8; y++)
                _cells[x, y] = new ReactiveProperty<CellStatus>(CellStatus.Empty);
            _black = new ReactiveProperty<ulong>();
            _white = new ReactiveProperty<ulong>();
            _black.Subscribe(data =>
            {
                var blackpos = Bit2xy(data);
                foreach (var pos in blackpos) _cells[pos.x, pos.y].Value = CellStatus.Black;
            });
            _white.Subscribe(data =>
            {
                var whitepos = Bit2xy(data);
                foreach (var pos in whitepos) _cells[pos.x, pos.y].Value = CellStatus.White;
            });
        }

        public override ulong Black
        {
            get => _black.Value;
            protected set => _black.Value = value;
        }

        public override ulong White
        {
            get => _white.Value;
            protected set => _white.Value = value;
        }

        public IObservable<Value<CellStatus>> CellAsObservable(int x, int y)
        {
            return _cells[x, y].Zip(_cells[x, y].Skip(1), (oldStatus, newStatus) =>
                    new Value<CellStatus>(oldStatus, newStatus))
                .AsObservable();
        }

        public IObservable<ulong> BlackAsObservable()
        {
            return _black;
        }

        public IObservable<ulong> WhiteAsObservable()
        {
            return _white;
        }

        public void Reset()
        {
            for (var x = 0; x < Constants.CellSize; x++)
            for (var y = 0; y < Constants.CellSize; y++)
                _cells[x, y].Value = CellStatus.Empty;
        }
    }

    public class TwoBoards
    {
        public TwoBoards(ulong black, ulong white)
        {
            Black = black;
            White = white;
        }

        public ulong Black { get; }
        public ulong White { get; }
    }
}