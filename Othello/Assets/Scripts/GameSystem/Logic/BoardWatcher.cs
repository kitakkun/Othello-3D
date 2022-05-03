using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace GameSystem.Logic
{
    public class BoardWatcher : MonoBehaviour
    {
        private UInt64 _oldBlack;
        private UInt64 _oldWhite;
                    
        public void Setup(BitBoard board)
        {
            _oldBlack = 0;
            _oldWhite = 0;
            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    var currentBlack = board.Black;
                    var currentWhite = board.White;
                    var blackChange = currentBlack ^ _oldBlack;
                    var whiteChange = currentWhite ^ _oldWhite;
                    var blackChangedPositions = board.Bit2xy(blackChange);
                    var whiteChangedPositions = board.Bit2xy(whiteChange);
                    _oldBlack = board.Black;
                    _oldWhite = board.White;
                })
                .AddTo(this);

        }
        
    }
}