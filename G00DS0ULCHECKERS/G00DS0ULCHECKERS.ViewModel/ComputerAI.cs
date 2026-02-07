using System;
using System.Collections.Generic;
using System.Text;
using G00DS0ULCHECKERS.Model;
using D20Tek.DiceNotation;

namespace G00DS0ULCHECKERS.ViewModel
{
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }
    public class ComputerAi
    {
        private readonly IDice _diceRoller = new Dice();

        public (Square From, Square To)? GetBestMove(GameSession gameSession, DifficultyLevel level)
        {
            switch (level)
            {
                case DifficultyLevel.Easy:
                    return GetRandomMove(gameSession);

                case DifficultyLevel.Medium:
                    return GetMinimaxMove(gameSession, 1);

                case DifficultyLevel.Hard:
                    return GetMinimaxMove(gameSession, 5);

                default:
                    return GetRandomMove(gameSession);
            }
        }

        // Easy: Random move
        private (Square From, Square To)? GetRandomMove(GameSession gameSession)
        {
            var moves = GetAllValidMoves(gameSession, gameSession.CurrentBoard, PlayerColor.White);

            if (moves.Count == 0) return null;

            var jumps = moves.Where(m => Math.Abs(m.To.Row - m.From.Row) == 2).ToList();
            if (jumps.Any())
            {
                var roll = _diceRoller.Roll($"1d{jumps.Count}").Value;
                return jumps[roll - 1];
            }

            var walkRoll = _diceRoller.Roll($"1d{moves.Count}").Value;
            return moves[walkRoll - 1];
        }

        // HARD MODE: Minimax Algorithm
        private (Square From, Square To)? GetMinimaxMove(GameSession gameSession, int depth)
        {
            var bestScore = double.MinValue;
            (Square, Square)? bestMove = null;

            var possibleMoves = GetAllValidMoves(gameSession, gameSession.CurrentBoard, PlayerColor.White);

            var jumps = possibleMoves.Where(m => Math.Abs(m.To.Row - m.From.Row) == 2).ToList();
            var movesToScan = jumps.Any() ? jumps : possibleMoves;

            foreach (var move in movesToScan)
            {
                var clonedBoard = gameSession.CurrentBoard.Clone();
                SimulateMove(clonedBoard, move);


                var score = Minimax(gameSession, clonedBoard, depth - 1, false);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private double Minimax(GameSession gameSession, Board board, int depth, bool isMaximizing)
        {
            if (depth == 0)
            {
                return EvaluateBoard(board);
            }

            var turn = isMaximizing ? PlayerColor.White : PlayerColor.Red;
            var moves = GetAllValidMoves(gameSession, board, turn);

            if (moves.Count == 0)
            {
                return isMaximizing ? -1000 : 1000; // If no moves, it's a loss for the current player
            }

            var jumps = moves.Where(m => Math.Abs(m.To.Row - m.From.Row) == 2).ToList();
            if (jumps.Any()) moves = jumps;

            if (isMaximizing)
            {
                var maxEval = double.MinValue;
                foreach (var move in moves)
                {
                    var clonedBoard = board.Clone();
                    SimulateMove(clonedBoard, move);
                    var eval = Minimax(gameSession, clonedBoard, depth - 1, false);
                    maxEval = Math.Max(maxEval, eval);
                }
                return maxEval;
            }
            else
            {
                var minEval = double.MaxValue;
                foreach (var move in moves)
                {
                    var clonedBoard = board.Clone();
                    SimulateMove(clonedBoard, move);
                    var eval = Minimax(gameSession, clonedBoard, depth - 1, true);
                    minEval = Math.Min(minEval, eval);
                }
                return minEval;
            }
        }

        private double EvaluateBoard(Board board)
        {
            var score = 0.0;

            for (var r = 0; r < 8; r++)
            {
                for (var c = 0; c < 8; c++)
                {
                    var piece = board.Grid[r, c];

                    if (piece != null)
                    {
                        var pieceValue = piece.IsKing ? 10.0 : 3.0;

                        if (c >= 2 && c <= 4 && r >= 3 && r <= 4)
                        {
                            pieceValue += 0.5;
                        }

                        if (piece.Color == PlayerColor.White && !piece.IsKing)
                        {
                            pieceValue += r * 0.1;
                        }

                        if (piece.Color == PlayerColor.Red && !piece.IsKing)
                        {
                            pieceValue += (7 - r) * 0.1;
                        }

                        if (piece.Color == PlayerColor.White && r == 0) pieceValue += 1.0;
                        if (piece.Color == PlayerColor.Red && r == 7) pieceValue += 1.0;

                        if (piece.Color == PlayerColor.White) score += pieceValue;
                        else
                        {
                            score -= pieceValue;
                        }
                    }
                }
            }
            return score;
        }

        private List<(Square From, Square To)> GetAllValidMoves(GameSession gameSession, Board board, PlayerColor color)
        {
            var moves = new List<(Square, Square)>();

            for (var r = 0; r < 8; r++)
            {
                for (var c = 0; c < 8; c++)
                {
                    var piece = board.Grid[r, c];
                    if (piece != null && piece.Color == color)
                    {
                        var fromSquare = new Square(r, c, piece);

                        int[] offsets = { -2, -1, 1, 2 };
                        foreach (var rOff in offsets)
                        {
                            foreach (var cOff in offsets)
                            {
                                var tr = r + rOff;
                                var tc = c + cOff;
                                if(tr >= 0 && tr < 8 && tc >= 0 && tc < 8)
                                {
                                    var targetPiece = board.Grid[tr, tc];
                                    var toSquare = new Square(tr, tc, targetPiece);

                                    if(Math.Abs(rOff) == 2 && gameSession.IsValidJumpMoveForSim(fromSquare, toSquare, board))
                                        moves.Add((fromSquare, toSquare));
                                    else if(Math.Abs(rOff) == 1 && gameSession.IsValidSimpleMoveForSim(fromSquare, toSquare, board))
                                        moves.Add((fromSquare, toSquare));

                                }
                            }
                        }
                    }
                }
            }
            return moves;
        }

        private void SimulateMove(Board board, (Square From, Square To) move)
        {
            var piece = board.Grid[move.From.Row, move.From.Column];
            board.Grid[move.To.Row, move.To.Column] = piece;
            board.Grid[move.From.Row, move.From.Column] = null;

            if (Math.Abs(move.To.Row - move.From.Row) == 2)
            {
                var mr = (move.From.Row + move.To.Row) / 2;
                var mc = (move.From.Column + move.To.Column) / 2;
                board.Grid[mr, mc] = null; // Remove the jumped piece
            }

            if(piece?.Color == PlayerColor.White && move.To.Row == 7) piece.IsKing = true;
            if(piece?.Color == PlayerColor.Red && move.To.Row == 0) piece.IsKing = true;
        }
    }
}
