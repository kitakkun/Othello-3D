using System;
using UnityEngine;

namespace GameSystem
{
    // プレイヤーインタフェース
    public interface IPlayer
    {
        public void Setup(GameManager manager);
        // public IObservable<Vector2Int> PlacePosition { get; }
    }
}