using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using G00DS0ULCHECKERS.Model;
using D20Tek.DiceNotation;

namespace G00DS0ULCHECKERS.ViewModel
{
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard,
        GodMode
    }
    public class ComputerAi
    {
        private readonly IDice _diceRoller = new Dice();

        private int _positionEvaluated = 0;

        private readonly Dictionary<string, (double Score, int Depth)> _transpositionTable = new();

        public (Square From, Square To)? GetBestMove(GameSession gameSession, DifficultyLevel level, Square? forceStart = null)
        {
            if (forceStart != null)
            {
                return GetMinimaxMove(gameSession, 3, forceStart);
            }
            switch (level)
            {
                case DifficultyLevel.Easy:
                    return GetRandomMove(gameSession);

                case DifficultyLevel.Medium:
                    return GetMinimaxMove(gameSession, 2, null);

                case DifficultyLevel.Hard:
                    return GetMinimaxMove(gameSession, 4, null);
                case DifficultyLevel.GodMode:
                    return GetMinimaxMove(gameSession, 6, null);

                default:
                    return GetRandomMove(gameSession);
            }
        }

        // Easy: Random move
        private (Square From, Square To)? GetRandomMove(GameSession gameSession)
        {
            var moves = GetAllValidMoves(gameSession, gameSession.CurrentBoard, PlayerColor.White, null);

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
        private (Square From, Square To)? GetMinimaxMove(GameSession gameSession, int maxDepth, Square? forcedStart)
        {
            var bestScore = double.MinValue;
            (Square, Square)? bestMove = null;
            
            _positionEvaluated = 0;
            _transpositionTable.Clear();
            var timer = new Stopwatch();
            timer.Start();
            
            var timeLimit = maxDepth switch
            {
                2 => 1000,    // 1 second for Medium
                4 => 5000,    // 5 seconds for Hard
                6 => 15000,   // 15 seconds for God Mode
                _ => 3000
            };

            // Iterative deepening
            for (var depth = 1; depth <= maxDepth; depth++)
            {
                if (timer.ElapsedMilliseconds > timeLimit) break;
                
                var possibleMoves = GetAllValidMoves(gameSession, gameSession.CurrentBoard, PlayerColor.White, forcedStart);
                possibleMoves = OrderMoves(possibleMoves, gameSession.CurrentBoard);

                foreach (var move in possibleMoves)
                {
                    if (timer.ElapsedMilliseconds > timeLimit) break;
                    
                    var clonedBoard = gameSession.CurrentBoard.Clone();
                    SimulateMove(clonedBoard, move);

                    var score = Minimax(gameSession, clonedBoard, depth - 1, false, double.MinValue, double.MaxValue);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                }
                
                Debug.WriteLine($"Completed depth {depth}, Best Score: {bestScore:F2}");
            }

            timer.Stop();
            Debug.WriteLine($"----------------------------------");
            Debug.WriteLine($"AI Report:");
            Debug.WriteLine($"Future Seen: {_positionEvaluated:N0} positions");
            Debug.WriteLine($"Time Taken: {timer.ElapsedMilliseconds} ms");
            Debug.WriteLine($"----------------------------------");

            return bestMove;
        }

        private string GetBoardHash(Board board)
        {
            var sb = new StringBuilder();
            for (var r = 0; r < 8; r++)
                for (var c = 0; c < 8; c++)
                {
                    var p = board.Grid[r, c];
                    if (p == null) sb.Append('_');
                    else sb.Append(p.Color == PlayerColor.White ? (p.IsKing ? 'W' : 'w') : (p.IsKing ? 'R' : 'r'));
                }
            return sb.ToString();
        }

        private double Minimax(GameSession gameSession, Board board, int depth, bool isMaximizing, double alpha, double beta)
        {
            _positionEvaluated++;

            // Check transposition table
            var hash = GetBoardHash(board);
            if (_transpositionTable.TryGetValue(hash, out var cached) && cached.Depth >= depth)
                return cached.Score;

            if (depth == 0)
            {
                return EvaluateBoard(gameSession, board, PlayerColor.White);
            }

            var turn = isMaximizing ? PlayerColor.White : PlayerColor.Red;
            var moves = GetAllValidMoves(gameSession, board, turn, null);
            moves = OrderMoves(moves, board);  // ✅ ADD THIS

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

                    var eval = Minimax(gameSession, clonedBoard, depth - 1, false, alpha, beta);
                    maxEval = Math.Max(maxEval, eval);

                    alpha = Math.Max(alpha, eval);  
                    if (beta <= alpha) break;  
                }
                // Store result before returning
                _transpositionTable[hash] = (maxEval, depth);
                return maxEval;
            }
            else
            {
                var minEval = double.MaxValue;
                foreach (var move in moves)
                {
                    var clonedBoard = board.Clone();
                    SimulateMove(clonedBoard, move);
                    var eval = Minimax(gameSession, clonedBoard, depth - 1, true, alpha, beta);

                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);

                    if (beta <= alpha) break;
                }
                // Store result before returning
                _transpositionTable[hash] = (minEval, depth);
                return minEval;
            }
        }

        private double EvaluateBoard(GameSession gameSession, Board board, PlayerColor aiColor)
        {
            var score = 0.0;

            int[,] weights =
            {
                { 0, 4, 0, 4, 0, 4, 0, 4 },
                { 3, 0, 3, 0, 3, 0, 3, 0 },
                { 0, 2, 0, 2, 0, 2, 0, 2 },
                { 2, 0, 2, 0, 2, 0, 2, 0 },
                { 0, 2, 0, 2, 0, 2, 0, 2 },
                { 2, 0, 2, 0, 2, 0, 2, 0 },
                { 0, 3, 0, 3, 0, 3, 0, 3 },
                { 4, 0, 4, 0, 4, 0, 4, 0 }
            };

            int whiteKings = 0, redKings = 0;
            int whitePieces = 0, redPieces = 0;
            
            for (var r = 0; r < 8; r++)
            {
                for (var c = 0; c < 8; c++)
                {
                    var piece = board.Grid[r, c];

                    if (piece != null)
                    {
                        var pieceValue = piece.IsKing ? 10.0 : 3.0;
                        var positionBonus = weights[r, c] * 0.1;
                        
                        // ✅ Advancement bonus (encourage pushing pieces forward)
                        var advancementBonus = 0.0;
                        if (!piece.IsKing)
                        {
                            advancementBonus = piece.Color == PlayerColor.White ? r * 0.2 : (7 - r) * 0.2;
                        }
                        
                        // ✅ Center control bonus
                        var centerBonus = 0.0;
                        if (c >= 2 && c <= 5) centerBonus += 0.3;
                        if (r >= 2 && r <= 5) centerBonus += 0.3;
                        
                        // ✅ Back row defense (keep at least one piece back)
                        var defenseBonus = 0.0;
                        if (piece.Color == PlayerColor.White && r == 0) defenseBonus = 0.5;
                        if (piece.Color == PlayerColor.Red && r == 7) defenseBonus = 0.5;
                        
                        pieceValue += positionBonus + advancementBonus + centerBonus + defenseBonus;

                        if (piece.Color == PlayerColor.White)
                        {
                            score += pieceValue;
                            if (piece.IsKing) whiteKings++;
                            whitePieces++;
                        }
                        else
                        {
                            score -= pieceValue;
                            if (piece.IsKing) redKings++;
                            redPieces++;
                        }
                    }
                }
            }

            // ✅ Mobility (more important in endgame)
            var whiteMoves = GetAllValidMoves(gameSession, board, PlayerColor.White, null).Count;
            var redMoves = GetAllValidMoves(gameSession, board, PlayerColor.Red, null).Count;
            score += (whiteMoves - redMoves) * 0.15;
            
            // ✅ King advantage in endgame
            var totalPieces = whitePieces + redPieces;
            if (totalPieces <= 8)
            {
                score += (whiteKings - redKings) * 3.0;
            }
            
            // ✅ Material advantage scaling (winning? be aggressive!)
            if (whitePieces > redPieces + 2)
                score += (whitePieces - redPieces) * 0.5;

            return score;
        }

        private List<(Square From, Square To)> GetAllValidMoves(GameSession gameSession, Board board, PlayerColor color, Square? forceStart)
        {
            var moves = new List<(Square, Square)>();

            for (var r = 0; r < 8; r++)
            {
                for (var c = 0; c < 8; c++)
                {
                    if (forceStart != null && (r != forceStart.Row || c != forceStart.Column)) continue;

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

        private List<(Square From, Square To)> OrderMoves(List<(Square From, Square To)> moves, Board board)
        {
            return moves.OrderByDescending(m =>
            {
                var score = 0.0;
                
                // Jumps first (captures)
                if (Math.Abs(m.To.Row - m.From.Row) == 2) score += 1000;
                
                // King promotion moves
                var piece = board.Grid[m.From.Row, m.From.Column];
                if (piece?.Color == PlayerColor.White && m.To.Row == 7) score += 500;
                if (piece?.Color == PlayerColor.Red && m.To.Row == 0) score += 500;
                
                // Center moves
                if (m.To.Column >= 2 && m.To.Column <= 5) score += 50;
                
                // Forward progression
                if (piece?.Color == PlayerColor.White) score += m.To.Row * 10;
                else score += (7 - m.To.Row) * 10;
                
                return score;
            }).ToList();
        }
    }
}
