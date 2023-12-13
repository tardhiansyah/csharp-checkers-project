using System.Text;
using System.Text.RegularExpressions;
using CheckersGame.Class;
using CheckersGame.Enumeration;
using CheckersGame.Interface;

namespace CheckersGame;

static class Program
{
    static void Main()
    {
        GameController checkers = new();
        
        CheckersSetup.Show(checkers);

        bool started = checkers.Start();

        Piece? selectedPiece = null;
        int lastPieceCount = checkers.CountPieceOnBoard();
        bool firstMove = true;
        while (started && !checkers.GameOver())
        {
            Console.Clear();
            IPlayer firstPlayer = checkers.GetActivePlayer().First();
            IPlayer secondPlayer = checkers.GetActivePlayer().Last();
            
            PrintPlayerInfo(firstPlayer, checkers.GetPlayerPieces(firstPlayer).Count());
            PrintBoard(checkers, selectedPiece, firstMove);
            PrintPlayerInfo(secondPlayer, checkers.GetPlayerPieces(secondPlayer).Count());
            PrintCurrentPlayerInfo(checkers.GetCurrentPlayer());

            if (selectedPiece == null)
            {
                selectedPiece = SelectPiece(checkers);
                Console.Clear();
                PrintPlayerInfo(firstPlayer, checkers.GetPlayerPieces(firstPlayer).Count());
                PrintBoard(checkers, selectedPiece, firstMove);
                PrintPlayerInfo(secondPlayer, checkers.GetPlayerPieces(secondPlayer).Count());
                PrintCurrentPlayerInfo(checkers.GetCurrentPlayer());   
            }
            
            Console.ForegroundColor = (checkers.GetCurrentPlayer().Id == 1) ? ConsoleColor.Blue : ConsoleColor.Red;
            Console.WriteLine($"\u001b[32mPIECE SELECTED: {selectedPiece.Id}-{selectedPiece.Status}\u001b[0m");
            Console.ResetColor();
            
            while (true)
            {
                Position newPosition = SelectPosition(checkers);
                
                if (!checkers.MovePiece(selectedPiece, newPosition, firstMove))
                    Console.WriteLine("\u001b[31mInvalid move!\u001b[0m");
                else
                    break;
            }
            
            // Check if jump / kill move
            int currentPieceCount = checkers.CountPieceOnBoard();
            if (currentPieceCount == lastPieceCount || !checkers.GetPossibleJumpMoves(selectedPiece).Any())
            {
                checkers.PromotePiece(selectedPiece);
                lastPieceCount = currentPieceCount;
                selectedPiece = null;
                firstMove = true;
                checkers.NextTurn();   
            }
            else
            {
                firstMove = false;
            }
        }
        PrintWinner(checkers);
    }

    static Piece SelectPiece(GameController checkers)
    {
        IPlayer currentPlayer = checkers.GetCurrentPlayer();
        
        Piece? selectedPiece = null;
        while (selectedPiece == null)
        {
            Console.Write("\u001b[33m* Choose Piece To Move (e.g: 2): ");
            string pieceNumber = GetUserInput();
            if (!Int32.TryParse(pieceNumber, out int pieceId))
                continue;

            if (pieceId > checkers.MaxPlayerPieces())
                continue;
            
            Console.ResetColor();
            Console.CursorVisible = false;
            
            selectedPiece = checkers.GetPiece(currentPlayer, pieceId);
            if (selectedPiece == null)
                continue;
            
            if (!checkers.GetPossibleMoves(selectedPiece).Any())
            {
                Console.WriteLine("\u001b[31mPiece can't be moved, please select other piece!\u001b[0m");
                selectedPiece = null;
            }
        }
        return selectedPiece;
    }
    static Position SelectPosition(GameController checkers)
    {
        while (true)
        {
            Console.Write("\u001b[33m* Select new position (e.g: D5 or d5): ");
            string selectedPosition = GetUserInput();
            Console.ResetColor();
            Position? newPosition = ConvertInputToPosition(selectedPosition, checkers.GetBoardSize());
            if (newPosition != null)
            {
                Console.CursorVisible = false;
                return newPosition;
            }
        }
    }
    static void PrintCurrentPlayerInfo(IPlayer player)
    {
        Console.ForegroundColor = (player.Id == 1) ? ConsoleColor.Blue : ConsoleColor.Red;
        Console.WriteLine($"PLAYER {player.Id} TURN!");
        Console.ResetColor();
    }
    static void PrintPlayerInfo(IPlayer player, int remainingPieces)
    {
        PieceColor color = (player.Id == 1) ? PieceColor.Blue : PieceColor.Red;
        
        Console.ForegroundColor = (player.Id == 1) ? ConsoleColor.Blue : ConsoleColor.Red;
        Console.WriteLine($"======================== PLAYER {player.Id} ========================");
        Console.WriteLine($"NICKNAME: {player.Name.ToUpper()}");
        Console.WriteLine($"PIECE COLOR: {color.ToString().ToUpper()}");
        Console.WriteLine($"PIECE REMAINING: {remainingPieces} Pieces");
        Console.WriteLine("==========================================================");
        Console.ResetColor();
    }
    static void PrintBoard(GameController checkers, Piece? selectedPiece, bool firstMove)
    {
        Piece?[,] board = checkers.GetBoardLayout();
        
        for (int i = 0; i < board.GetLength(0); i++)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(GenerateColumnSeparator(checkers.GetBoardSize()));
            Console.Write($"{board.GetLength(0) - i:D2}");
            Console.ResetColor();
            
            for (int j = 0; j < board.GetLength(1); j++)
            {
                string symbol = "   ";
                if (board[i, j] != null)
                {
                    Piece piece = board[i, j]!;
                    string type = (piece.Status == PieceStatus.King) ? "K" : "R";
                    symbol = $"{type}{piece.Id:D2}";
                    Console.ForegroundColor = (piece.Color == PieceColor.Blue) ? ConsoleColor.Blue : ConsoleColor.Red;
                }
                else
                {
                    if (selectedPiece != null)
                    {
                        IEnumerable<Position> validMovePositions = firstMove ? checkers.GetPossibleMoves(selectedPiece) : checkers.GetPossibleJumpMoves(selectedPiece);
                        foreach (var position in validMovePositions)
                        {
                            if (position.Row != i || position.Column != j)
                                continue;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            symbol = $" X ";
                        }
                    }
                }
                
                Console.Write($"| {symbol} |");
                Console.ResetColor();
            }
            Console.WriteLine();
        }
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(GenerateColumnSeparator(checkers.GetBoardSize()));
        Console.WriteLine(GenerateRankName(checkers.GetBoardSize()));
        Console.ResetColor();
    }
    static void PrintWinner(GameController checkers)
    {
        IPlayer? winner = checkers.GetWinner();
        Console.WriteLine(winner == null ? "END RESULT: DRAW" : $"GAME WINNER: {winner}");
    }
    static string GenerateColumnSeparator(int size)
    {
        StringBuilder sb = new();
        
        for (int i = 0; i < size; i++)
        {
            sb.Append("-------");
        }

        sb.Append("--");
        
        return sb.ToString();
    }
    static string GenerateRankName(int size)
    {
        StringBuilder sb = new();
        sb.Append("  ");
        for (int i = 0; i < size; i++)
        {
            sb.Append($"   {(RankName) i}   ");
        }

        return sb.ToString();
    }
    static string GetUserInput()
    {
        Console.CursorVisible = true;
        while (true)
        {
            string? newPosition = Console.ReadLine();
            if (newPosition == null)
                continue;
            
            return newPosition.ToUpper();
        }
    }
    static Position? ConvertInputToPosition(string inputPosition, in int boardSize)
    {
        string pattern = @"([A-Z]+)(\d+)";
        Match match = Regex.Match(inputPosition, pattern);

        if (!match.Success)
            return null;

        string letterPart = match.Groups[1].Value;
        if (!Enum.TryParse(letterPart, out RankName rank))
            return null;

        int column = (int)rank;
        int row = boardSize - int.Parse(match.Groups[2].Value);

        return new Position(row, column);
    }
}