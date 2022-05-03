using UnityEngine;

namespace GameSystem
{
    public class GameEvent
    {
        public class TurnChange
        {
            public TurnChange(IPlayer player)
            {
                Player = player;
            }

            public IPlayer Player { get; }
        }

        public class PlaceRequest
        {
            public PlaceRequest(IPlayer player, Vector2Int position)
            {
                Player = player;
                Position = position;
            }

            public IPlayer Player { get; }
            public Vector2Int Position { get; }
        }

        public class BlackTurn
        {
        }

        public class WhiteTurn
        {
        }
    }
}