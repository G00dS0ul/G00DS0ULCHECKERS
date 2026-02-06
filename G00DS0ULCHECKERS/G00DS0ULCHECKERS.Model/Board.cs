namespace G00DS0ULCHECKERS.Model
{
    public class Board
    {
        public Piece?[,] Grid { get; set; }

        public Board()
        {
            Grid = new Piece[8, 8];
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            for (var row = 0; row < 8; row++)
            {
                for (var col = 0; col < 8; col++)
                {
                    //Logic guarding pieces go on the dark squares alone
                    // A square is dark if (row + col) is an odd number
                    if ((row + col) % 2 != 0)
                    {
                        // Top 3 Rows (0 - 2): White Pieces
                        if (row < 3)
                        {
                            Grid[row, col] = new Piece(PlayerColor.White);
                        }

                        // Bottom 3 Rows (5 - 7): Red pieces
                        else if (row > 4)
                        {
                            Grid[row, col] = new Piece(PlayerColor.Red);
                        }

                        //Middle Rows(3 - 4): Empty
                        else
                        {
                            Grid[row, col] = null;
                        }
                    }
                }
            }
        }

        public Board Clone()
        {
            var newBoard = new Board();

            for (var r = 0; r < 8; r++)
            {
                for (var c = 0; c < 8; c++)
                {
                    var original = Grid[r, c];
                    if (original != null)
                    {
                        newBoard.Grid[r, c] = new Piece(original.Color) {IsKing = original.IsKing};
                    }
                    else
                    {
                        newBoard.Grid[r, c] = null;
                    }
                }
            }

            return newBoard;
        }
    }
}