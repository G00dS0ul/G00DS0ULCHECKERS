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
                if (clickedSquare.CurrentPiece != null && clickedSquare.CurrentPiece.Color == CurrentPlayerTurn)
                {
                    if (SelectedSquare != null)
                    {
                        SelectedSquare.IsSelected = false;
                    }

                    SelectedSquare = clickedSquare;
                    SelectedSquare.IsSelected = true;
                }
                else if (SelectedSquare != null)
                {
                    if (IsValidSimpleMove(SelectedSquare, clickedSquare))
                    {
                        MovePiece(SelectedSquare, clickedSquare);
                    }
                    else if(IsValidJumpMove(SelectedSquare, clickedSquare))
                    {
                        MovePiece(SelectedSquare, clickedSquare);
                    }
                }
            }
        }

        private bool IsValidSimpleMove(Square from, Square to)
        {
            // Rule 1: The target square must be empty
            if (to.CurrentPiece != null)
            {
                return false;
            }

            // Rule 2: Must be a diagonal move and Column distance must be exactly 1
            var colDiff = Math.Abs(to.Column - from.Column);
            if (colDiff != 1)
            {
                return false;
            }

            // Rule 3: Must move Forward, Red moves up (Row -1), white moves Down (Row +1)
             var rowDiff = to.Row - from.Row;

             // If it is a king, It can go any direction
             if (from.CurrentPiece?.IsKing ?? false)
             {
                 return Math.Abs(rowDiff) == 1;
             }

             if (from.CurrentPiece?.Color == PlayerColor.Red)
             {
                 if (rowDiff == -1) return true;
             }
             else //white
             {
                 if (rowDiff == 1) return true;
             }

             return false;
        }

        private bool IsValidJumpMove(Square from, Square to)
        {
            //Target must be Empty
            if (to.CurrentPiece != null) return false;

            // Geometry: Must move exactly 2 square diagonally
            var rowDiff = to.Row - from.Row;
            var colDiff = to.Column - from.Column;

            if (Math.Abs(rowDiff) != 2 || Math.Abs(colDiff) != 2) return false;

            // Find the victim (the piece in between)
            var midRow = (from.Row + to.Row) / 2;
            var midCol = (from.Column + to.Column) / 2;
            var victimPiece = CurrentBoard.Grid[midRow, midCol];

            // there must be a victim, and it must be the second color
            if (victimPiece == null) return false;
            if (victimPiece.Color == from.CurrentPiece?.Color) return false;

            // Direction (Red moves up while White moves down) Todo: Add King
            if (from.CurrentPiece?.IsKing ?? false)
            {
                return Math.Abs(rowDiff) == 2;
            }

            if (from.CurrentPiece?.Color == PlayerColor.Red)
            {
                if (rowDiff != -2) return false; // Red must move up by -2
            }
            else //White
            {
                if (rowDiff != 2) return false; //White must move up by +2
            }

            return true;
        }

        private void MovePiece(Square from, Square to)
        {
            if (Math.Abs(to.Row - from.Row) == 2)
            {
                var midRow = (from.Row + to.Row) / 2;
                var midCol = (from.Column + to.Column) / 2;

                // Kill The Enemy!!
                CurrentBoard.Grid[midRow, midCol] = null;
            }

            var p = CurrentBoard.Grid[from.Row, from.Column];
            if (p != null)
            {
                CurrentBoard.Grid[to.Row, to.Column] = p;
                CurrentBoard.Grid[from.Row, from.Column] = null;
            }
            

            // The Coronation Ceremony
            if (p?.Color == PlayerColor.Red && to.Row == 0)
            {
                p.IsKing = true;
                System.Diagnostics.Debug.WriteLine("A Red King is Crowned!!");
            }

            else if (p.Color == PlayerColor.White && to.Row == 7)
            {
                p.IsKing = true;
                System.Diagnostics.Debug.WriteLine("A White king is Crown!!!");
            }

            CurrentPlayerTurn = CurrentPlayerTurn == PlayerColor.Red ? PlayerColor.White : PlayerColor.Red;

            TurnMessage = $"{CurrentPlayerTurn}'s Turn";

            if (SelectedSquare != null) SelectedSquare.IsSelected = false;
            SelectedSquare = null;

            RefreshBoard();
        }
    }
}

