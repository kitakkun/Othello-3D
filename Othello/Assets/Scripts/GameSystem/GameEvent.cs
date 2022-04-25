using UnityEngine;

namespace GameSystem
{
    public class GameEvent
    {

        public class TurnChange
        {
            public IPlayer Player { get; }

            public TurnChange(IPlayer player)
            {
                this.Player = player;
            }
        }

        public class PlaceRequest
        {
            public IPlayer Player { get; }
            public Vector2Int Position { get; }

            public PlaceRequest(IPlayer player, Vector2Int position)
            {
                this.Player = player;
                this.Position = position;
            }
        }
        public class BlackTurn{}
        public class WhiteTurn{}
    }
}