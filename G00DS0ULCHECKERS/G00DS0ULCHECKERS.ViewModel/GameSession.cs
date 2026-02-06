using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using G00DS0ULCHECKERS.Model;

namespace G00DS0ULCHECKERS.ViewModel
{
    public class GameSession : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int Score { get; set; }
        public Board CurrentBoard { get; set; } = new Board();
        public PlayerColor CurrentPlayerTurn { get; set; }
        public string TurnMessage { get; private set; }
        public ObservableCollection<Square> Squares { get; set; }
        public Square? SelectedSquare { get; set; }
        public ICommand ClickCommand { get; set; }

        public GameSession()
        {
            Squares = new ObservableCollection<Square>();
            ClickCommand = new RelayCommand(ExecuteClick);
            NewGame();
        }

        public void NewGame()
        {
            CurrentBoard = new Board();
            CurrentPlayerTurn = PlayerColor.Red;
            TurnMessage = "Red's Turn";
            RefreshBoard(); //Load the data into the list
        }

        private void RefreshBoard()
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

        private void ExecuteClick(object parameter)
        {
            if (parameter is Square clickedSquare)
            {
                System.Diagnostics.Debug.WriteLine($"Clicked Square: {clickedSquare.Row}, {clickedSquare.Column}");

                if (clickedSquare.CurrentPiece != null && clickedSquare.CurrentPiece.Color == CurrentPlayerTurn)
                {
                    if (SelectedSquare != null)
                    {
                        SelectedSquare.IsSelected = false;
                    }

                    SelectedSquare = clickedSquare;
                    SelectedSquare.IsSelected = true;
                    System.Diagnostics.Debug.WriteLine("Piece Selected");

                    System.Diagnostics.Debug.WriteLine($"Selected: {clickedSquare.Row}, {clickedSquare.Column}");
                }
                else if (SelectedSquare != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Attempting move from {SelectedSquare.Row},{SelectedSquare.Column} to {clickedSquare.Row},{clickedSquare.Column}....");
                    if (IsValidSimpleMove(SelectedSquare, clickedSquare))
                    {
                        System.Diagnostics.Debug.WriteLine("Move Valid! Executing...");
                        MovePiece(SelectedSquare, clickedSquare);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Move Rejected by Logic");
                    }
                }
            }
        }

        private bool IsValidSimpleMove(Square from, Square to)
        {
            // Rule 1: The target square must be empty
            if (to.CurrentPiece != null)
            {
                System.Diagnostics.Debug.WriteLine("Fail: Target is not Empty");
                return false;
            }

            // Rule 2: Must be a diagonal move and Column distance must be exactly 1
            var colDiff = Math.Abs(to.Column - from.Column);
            if (colDiff != 1)
            {
                System.Diagnostics.Debug.WriteLine($"Fail: Column diff is {colDiff} (Must be 1).");
                return false;
            }

            // Rule 3: Must move Forward, Red moves up (Row -1), white moves Down (Row +1)
             var rowDiff = to.Row - from.Row;

             if (from.CurrentPiece?.Color == PlayerColor.Red)
             {
                 if (rowDiff == -1) return true;
                 System.Diagnostics.Debug.WriteLine($"Fail: Red row diff is {rowDiff} (Must be -1");
             }
             else //white
             {
                 if (rowDiff == 1) return true;
                 System.Diagnostics.Debug.WriteLine($"Fail: White row diff is {rowDiff} (Must be 1");
             }

             return false;
        }

        private void MovePiece(Square from, Square to)
        {
            var p = CurrentBoard.Grid[from.Row, from.Column];

            CurrentBoard.Grid[to.Row, to.Column] = p;
            CurrentBoard.Grid[from.Row, from.Column] = null;

            if (CurrentPlayerTurn == PlayerColor.Red)
                CurrentPlayerTurn = PlayerColor.White;
            else
                CurrentPlayerTurn = PlayerColor.Red;

            TurnMessage = $"{CurrentPlayerTurn}'s Turn";

            if (SelectedSquare != null) SelectedSquare.IsSelected = false;
            SelectedSquare = null;

            RefreshBoard();
        }
    }
}

