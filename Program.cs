using System.Text;
using System.Text.RegularExpressions;
using CheckersGame.Class;
using CheckersGame.Enumeration;
using CheckersGame.Interface;

namespace CheckersGame;

class Program
{
    static void Main()
    {
        GameController checkers = new();
        
        CheckersSetup.Show(checkers);

        bool started = checkers.Start();
        while (started && !checkers.GameOver())
        {
            Console.Clear();
            PrintPlayerInfo(checkers.GetActivePlayer().First());
            PrintBoard(checkers);
            PrintPlayerInfo(checkers.GetActivePlayer().Last());
            
            AskPlayerToMovePiece(checkers);
            checkers.NextTurn();
        }
    }

    static void AskPlayerToMovePiece(GameController checkers)
    {
        IPlayer currentPlayer = checkers.GetCurrentPlayer();
        Console.WriteLine($"Player {currentPlayer.Id} Turn!");
        
        Piece? selectedPiece = null;
        while (selectedPiece == null)
        {
            Console.Write("Choose Piece To Move \u001b[36m(e.g: 2)\u001b[0m: ");
            int selectedPieceId = GetPieceIdInput();
        
            selectedPiece = checkers.GetPiece(currentPlayer, selectedPieceId);
            if (checkers.GetPossibleMoves(selectedPiece).Count() == 0)
            {
                Console.WriteLine("\u001b[31mPiece can't be moved, please select other piece!\u001b[0m");
                selectedPiece = null;   
            }
        }
        Console.WriteLine($"Piece Selected: {selectedPiece.Id}-{selectedPiece.Status}");
        

        int lastNumberPieceOnBoard = checkers.CountPieceOnBoard();
        while (true)
        {
            Console.Write("Select new position \u001b[36m(e.g: D5 or d5)\u001b[0m: ");
            string selectedPosition = GetPositionInput();
            Position? newPosition = ConvertInputToPosition(selectedPosition, checkers.GetBoardSize());
            if (newPosition == null)
                continue;

            if (!checkers.MovePiece(selectedPiece, newPosition))
            {
                Console.WriteLine("\u001b[31mInvalid move!\u001b[0m");
                continue;   
            }
            
            // Is it jump / kill move?
            int currentNumberPieceOnBoard = checkers.CountPieceOnBoard();
            if (currentNumberPieceOnBoard == lastNumberPieceOnBoard)
                break;
            
            // If it is jump move, check if can move again
            if (!checkers.GetPossibleJumpMoves(selectedPiece).Any())
                break;
        }
    }

    static void PrintPlayerInfo(IPlayer player)
    {
        PieceColor color = (player.Id == 1) ? PieceColor.Blue : PieceColor.Red;
        
        Console.WriteLine();
        Console.WriteLine($"======================= PLAYER {player.Id} =======================");
        Console.WriteLine($"NICKNAME: {player.Name.ToUpper()}");
        Console.WriteLine($"PIECE COLOR: {color.ToString().ToUpper()}");
        Console.WriteLine("========================================================");
        Console.WriteLine();
    }
    static void PrintBoard(GameController checkers)
    {
        Piece?[,] board = checkers.GetBoardLayout();
        
        for (int i = 0; i < board.GetLength(0); i++)
        {
            Console.WriteLine(GenerateColumnSeparator(checkers.GetBoardSize()));
            Console.Write($"{board.GetLength(0) - i}");
            
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
                
                Console.Write($"| {symbol} |");
                Console.ResetColor();
            }
            Console.WriteLine();
        }
        Console.WriteLine(GenerateColumnSeparator(checkers.GetBoardSize()));
        Console.WriteLine(GenerateRankName(checkers.GetBoardSize()));
    }
    static string GenerateColumnSeparator(int size)
    {
        StringBuilder sb = new();
        
        for (int i = 0; i < size; i++)
        {
            sb.Append("-------");
        }

        sb.Append("-");
        
        return sb.ToString();
    }
    static string GenerateRankName(int size)
    {
        StringBuilder sb = new();
        
        for (int i = 0; i < size; i++)
        {
            sb.Append($"   {(RankName) i}   ");
        }

        return sb.ToString();
    }
    static int GetPieceIdInput()
    {
        Console.CursorVisible = true;
        while (true)
        {
            string? pieceID = Console.ReadLine();
            if (pieceID == null)
                continue;
            if (Int32.TryParse(pieceID, out int id))
            {
                Console.CursorVisible = false;
                return id;
            }
        }
    }
    static string GetPositionInput()
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
        Console.WriteLine($"New Position: {row} {column}");

        return new Position(row, column);
    }
}