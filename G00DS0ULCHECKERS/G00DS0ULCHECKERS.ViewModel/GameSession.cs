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

        public GameSession()
        {
            NewGame();
        }

        public void NewGame()
        {
            CurrentBoard = new Board();
            CurrentPlayerTurn = PlayerColor.Red;
        }
    }
}

