using G00DS0ULCHECKERS.Model;

Console.WriteLine("=== G00dS0ul's Checkers Engine Test ===\n");

var board = new Board();

//Looping through the grid and print symbols
for (var row = 0; row < 8; row++)
{
    Console.Write($"Row {row}: ");
    for (var col = 0; col < 8; col++)
    {
        Piece p = board.Grid[row, col];

        if (p == null)
        {
            Console.Write(". "); // Empty
        }
        else if (p.Color == PlayerColor.Red)
        {
            Console.Write("R "); //Red
        }    
        else if (p.Color == PlayerColor.White)
        {
            Console.Write("W "); //White
        }
    }

    Console.WriteLine();
}

Console.ReadLine();
