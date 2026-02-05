namespace G00DS0ULCHECKERS.Model
{
    public enum PlayerColor
    {
        None,
        Red,
        White
    }

    public class Piece
    {
        public PlayerColor Color { get; set; }
        public bool IsKing { get; set; }

        public Piece(PlayerColor color)
        {
            Color = color;
            IsKing = false; //Everyone starts as a regular player
        }
    }
}
