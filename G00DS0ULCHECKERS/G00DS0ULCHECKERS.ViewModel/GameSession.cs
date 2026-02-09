using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Windows.Input;
using G00DS0ULCHECKERS.Model;

namespace G00DS0ULCHECKERS.ViewModel
{
    public class GameSession : INotifyPropertyChanged
    {
        private readonly SoundManager _audio = new SoundManager();
        private ComputerAi _ai = new ComputerAi();
        public event PropertyChangedEventHandler? PropertyChanged;

        #region Properties

        public Board CurrentBoard { get; set; } = new Board();
        public PlayerColor CurrentPlayerTurn { get; set; }
        public bool IsVsComputer { get; set; } = true;
        public string TurnMessage { get; private set; }
        public ObservableCollection<Square> Squares { get; set; }
        public IEnumerable<DifficultyLevel> DifficultyValues => Enum.GetValues(typeof(DifficultyLevel)).Cast<DifficultyLevel>();
        public DifficultyLevel CurrentDifficulty { get; set; } = DifficultyLevel.Hard;
        public Square? SelectedSquare { get; set; }
        public int CapturedRed { get; set; }
        public int CapturedWhite { get; set; }
        public ICommand ClickCommand { get; set; }
        public ICommand RestartCommand { get; set; }

        #endregion

        public GameSession()
        {
            Squares = new ObservableCollection<Square>();
            ClickCommand = new RelayCommand(ExecuteClick);

            RestartCommand = new RelayCommand(o => NewGame());
            NewGame();
        }

        public void NewGame()
        {
            CurrentBoard = new Board();
            CurrentPlayerTurn = PlayerColor.Red;
            TurnMessage = "Red's Turn";
            CapturedRed = 0;
            CapturedWhite = 0;
            RefreshBoard(); //Load the data into the list
        }

        public bool IsValidJumpMoveForSim(Square from, Square to, Board board)
        {
            if (to.CurrentPiece != null) return false;

            var rowDiff = to.Row - from.Row;
            var colDiff = to.Column - from.Column;
            if (Math.Abs(rowDiff) != 2 || Math.Abs(colDiff) != 2) return false;

            var midRow = (from.Row + to.Row) / 2;
            var midCol = (from.Column + to.Column) / 2;
            var victim = board.Grid[midRow, midCol];

            if (victim == null) return false;
            if(victim.Color == from.CurrentPiece?.Color) return false;

            if (from.CurrentPiece.IsKing) return true;
            if(from.CurrentPiece.Color == PlayerColor.Red) return rowDiff == -2;
            return rowDiff == 2;
        }

        public bool IsValidSimpleMoveForSim(Square from, Square to, Board board)
        {
            if (board.Grid[to.Row, to.Column] != null) return false;

            if (Math.Abs(to.Column - from.Column) != 1) return false;

            var rowDiff = to.Row - from.Row;
            if (from.CurrentPiece.IsKing) return Math.Abs(rowDiff) == 1;
            if (from.CurrentPiece.Color == PlayerColor.Red) return rowDiff == -1;
            return rowDiff == 1;
        }

        #region Private Methods
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
                    if (IsValidJumpMove(SelectedSquare, clickedSquare))
                    {
                        MovePiece(SelectedSquare, clickedSquare);
                    }
                    else if(IsValidSimpleMove(SelectedSquare, clickedSquare))
                    {
                        if (AnyCaptureAvailable())
                        {
                            TurnMessage = "You must capture if you can!";
                            return;
                        }
                        MovePiece(SelectedSquare, clickedSquare);
                    }
                }
            }
        }

        private bool AnyCaptureAvailable()
        {
            foreach (var s in Squares)
            {
                if (s.CurrentPiece != null && s.CurrentPiece.Color == CurrentPlayerTurn)
                {
                    int[] offset = [ -2, 2 ];
                    foreach (var rOff in offset)
                    {
                        foreach (var cOff in offset)
                        {
                            var targetRow = s.Row + rOff;
                            var targetCol = s.Column + cOff;

                            if (targetRow >= 0 && targetRow < 8 && targetCol >= 0 && targetCol < 8)
                            {
                                var targetPiece = CurrentBoard.Grid[targetRow, targetCol];
                                var targetSquare = new Square(targetRow, targetCol, targetPiece);

                                if (IsValidJumpMove(s, targetSquare)) return true;
                            }
                        }
                    }
                }
            }

            return false;
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

        private async void MovePiece(Square from, Square to)
        {
            var isCapture = false;

            if (Math.Abs(to.Row - from.Row) == 2)
            {
                var midRow = (from.Row + to.Row) / 2;
                var midCol = (from.Column + to.Column) / 2;

                // Kill The Enemy!!
                CurrentBoard.Grid[midRow, midCol] = null;
                isCapture = true;
                _audio.PlayCapture();

                if (from.CurrentPiece?.Color == PlayerColor.Red)
                    CapturedWhite++;
                else
                    CapturedRed++;
            }

            else
            {
                _audio.PlayMove();
            }

            var p = CurrentBoard.Grid[from.Row, from.Column];
            if (p != null)
            {
                CurrentBoard.Grid[to.Row, to.Column] = p;
                CurrentBoard.Grid[from.Row, from.Column] = null;

                if (p.Color == PlayerColor.Red && to.Row == 0 || p.Color == PlayerColor.White && to.Row == 7) p.IsKing = true;
            }
            
            RefreshBoard();

            // Always Check for victory after a move, because you might win by either a simple move or a jump move
            if (CheckForWin())
            {
                SelectedSquare = null;
                return;
            }

            if (isCapture)
            {
                var landedSquare = Squares.FirstOrDefault(s => s.Row == to.Row && s.Column == to.Column);

                if (landedSquare != null && CanCaptureAgain(landedSquare))
                {
                    SelectedSquare = landedSquare;
                    SelectedSquare.IsSelected = true;
                    TurnMessage = $"{CurrentPlayerTurn} can capture again!"; // Don't switch turns, allow the player to capture again

                    if (IsVsComputer && CurrentPlayerTurn == PlayerColor.White)
                    {
                        await Task.Delay(500);
                        PlayComputerTurn(landedSquare);
                    }

                    return;
                }
            }

            CurrentPlayerTurn = CurrentPlayerTurn == PlayerColor.Red ? PlayerColor.White : PlayerColor.Red;

            TurnMessage = $"{CurrentPlayerTurn}'s Turn";

            if (SelectedSquare != null) SelectedSquare.IsSelected = false;
            SelectedSquare = null;

            if (IsVsComputer && CurrentPlayerTurn == PlayerColor.White)
            {
                TurnMessage = "Computer is thinking...";
                await Task.Delay(500);
                PlayComputerTurn();
            }
        }

        private void PlayComputerTurn(Square? forcedSquare = null)
        {
            var move = _ai.GetBestMove(this, CurrentDifficulty, forcedSquare);

            if (move != null)
            {
                var realStart = Squares.FirstOrDefault(s => s.Row == move.Value.From.Row && s.Column == move.Value.From.Column);
                var realEnd = Squares.FirstOrDefault(s => s.Row == move.Value.To.Row && s.Column == move.Value.To.Column);

                if (realStart != null && realEnd != null)
                {
                    SelectedSquare = realStart;
                    realStart.IsSelected = true;

                    MovePiece(realStart, realEnd);
                }
            }
            else
            {
                TurnMessage = "Red Wins! (Computer has no moves)";
            }
        }

        public bool CheckForWin()
        {
            var redCount = 0;
            var whiteCount = 0;

            foreach (var square in Squares)
            {
                if (square.CurrentPiece != null)
                {
                    if(square.CurrentPiece.Color == PlayerColor.Red) redCount++;
                    else whiteCount++;
                }
            }

            if(redCount == 0)
            {
                TurnMessage = "White Wins!!";
                _audio.PlayWin();
                return true;
            }
            else if (whiteCount == 0)
            {
                TurnMessage = "Red Wins!!";
                _audio.PlayWin();
                return true;
            }

            if (!HasAnyMoves(CurrentPlayerTurn))
            {
                var winner = (CurrentPlayerTurn == PlayerColor.Red) ? "WHITE" : "RED";
                TurnMessage = $"{winner} WINS! (No Moves)";
                _audio.PlayWin();
                return true;
            }

            return false;
        }

        private bool HasAnyMoves(PlayerColor player)
        {
            foreach (var s in Squares)
            {
                if (s.CurrentPiece != null && s.CurrentPiece.Color == player)
                {
                    if (CanPieceMove(s)) return true;
                }
            }

            return false;
        }

        private bool CanPieceMove(Square s)
        {
            int[] offsets = { -1, 1, -2, 2 };

            foreach (var rOff in offsets)
            {
                foreach (var cOff in offsets)
                {
                    if (Math.Abs(rOff) == 1 && Math.Abs(cOff) == 1)
                    {
                        if (IsMoveValidMock(s, rOff, cOff, isJump: false)) return true;
                    }

                    if (Math.Abs(rOff) == 2 && Math.Abs(cOff) == 2)
                    {
                        if (IsMoveValidMock(s, rOff, cOff, isJump: true)) return true;
                        {
                            
                        }
                    }
                }
            }

            return false;
        }

        private bool IsMoveValidMock(Square from, int rOff, int cOff, bool isJump)
        {
            var targetRow = from.Row + rOff;
            var targetCol = from.Column + cOff;

            if (targetRow < 0 || targetRow >= 8 || targetCol < 0 || targetCol >= 8) return false;

            var targetPiece = CurrentBoard.Grid[targetRow, targetCol];
            var targetSquare = new Square(targetRow, targetCol, targetPiece);


            if (isJump) return IsValidJumpMove(from, targetSquare);
            return IsValidSimpleMove(from, targetSquare);
        }

        private bool CanCaptureAgain(Square currentPos)
        {
            int[] rowOffsets = [-2, 2];
            int[] colOffsets = [-2, 2];

            foreach (var rOff in rowOffsets)
            {
                foreach (var cOffset in colOffsets)
                {
                    var targetRow = currentPos.Row + rOff;
                    var targetCol = currentPos.Column + cOffset;

                    if (targetRow >= 0 && targetRow < 8 && targetCol >= 0 && targetCol < 8)
                    {
                        var targetPiece = CurrentBoard.Grid[targetRow, targetCol];
                        var targetSquare = new Square(targetRow, targetCol, targetPiece);
                        if (targetSquare != null && IsValidJumpMove(currentPos, targetSquare))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        #endregion
    }
}

