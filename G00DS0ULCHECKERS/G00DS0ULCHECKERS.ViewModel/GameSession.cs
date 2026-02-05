using System.Collections.ObjectModel;
using System.ComponentModel;
using G00DS0ULCHECKERS.Model;

namespace G00DS0ULCHECKERS.ViewModel
{
    public class GameSession : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int Score { get; set; }
        public Board CurrentBoard { get; set; }
        public PlayerColor CurrentPlayerTurn { get; set; }
        public string TurnMessage => $"{CurrentPlayerTurn}'s Turn";

        public ObservableCollection<Square> Squares { get; set; }

        public GameSession()
        {
            Squares = [];
            NewGame();
        }

        public void NewGame()
        {
            CurrentBoard = new Board();
            CurrentPlayerTurn = PlayerColor.Red;
            RefreshBoard(); //Load the data into the list
        }

        public void RefreshBoard()
        {
            Squares.Clear();

            for (var r = 0; r < 8; r++)
            {
                for (var c = 0; c < 8; c++)
                {
                    var piece = CurrentBoard.Grid[r, c];


                    Squares.Add(new Square(r, c, piece));
                }
            }
        }
    }
}

