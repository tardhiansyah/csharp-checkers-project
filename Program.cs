using System.Text;
using System.Text.RegularExpressions;
using CheckersGame.Class;
using CheckersGame.Enumeration;
using CheckersGame.Interface;

namespace CheckersGame;

static class Program
{
    #region Main
    static void Main()
    {
        GameController checkers = new();
        checkers.PieceCaptured += HandlePieceCaptured;
        checkers.PieceMoved += HandlePieceMoved;
        checkers.PiecePromoted += HandlePiecePromoted;
        checkers.PlayerAdded += HandlePlayerAdded;
        
        SetupGame(checkers);
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
            PrintCurrentPlayerInfo(checkers.GetCurrentPlayer()!);

            if (selectedPiece == null)
            {
                selectedPiece = SelectPiece(checkers);
                Console.Clear();
                PrintPlayerInfo(firstPlayer, checkers.GetPlayerPieces(firstPlayer).Count());
                PrintBoard(checkers, selectedPiece, firstMove);
                PrintPlayerInfo(secondPlayer, checkers.GetPlayerPieces(secondPlayer).Count());
                PrintCurrentPlayerInfo(checkers.GetCurrentPlayer()!);   
            }
            PrintSelectedPiece(selectedPiece);
            
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
            if (currentPieceCount == lastPieceCount || !checkers.GetPossibleMoves(selectedPiece, false).Any())
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
        Console.Clear();
        PrintWinner(checkers);
        Console.ReadLine();
    }
    static Piece SelectPiece(GameController checkers)
    {
        IPlayer? currentPlayer = checkers.GetCurrentPlayer();
        
        Piece? selectedPiece = null;
        while (selectedPiece == null)
        {
            string pieceNumber = GetUserInput("\u001b[33m* Choose Piece To Move (e.g: 2): ");
            if (!Int32.TryParse(pieceNumber, out int pieceId))
                continue;
        
            if (pieceId > checkers.MaxPlayerPieces())
                continue;
            
            Console.ResetColor();
            Console.CursorVisible = false;
            
            selectedPiece = checkers.GetPiece(currentPlayer!, pieceId);
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
            string selectedPosition = GetUserInput("\u001b[33m* Select new position (e.g: D5 or d5): ");
            Console.ResetColor();
            Position? newPosition = ConvertInputToPosition(selectedPosition, checkers.GetBoardSize());
            if (newPosition != null)
            {
                Console.CursorVisible = false;
                return newPosition;
            }
        }
    }
    static string GetUserInput(string? promptMessage = null)
    {
        Console.CursorVisible = true;
        while (true)
        {
            Console.Write(promptMessage);
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
                continue;
            
            Console.CursorVisible = false;
            return input.ToUpper();
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
    #endregion
    
    #region Printing Board and Information
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
                        IEnumerable<Position> validMovePositions = checkers.GetPossibleMoves(selectedPiece, firstMove);
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
    static void PrintSelectedPiece(Piece selectedPiece)
    {
        Console.ForegroundColor = (selectedPiece.Color == PieceColor.Blue) ? ConsoleColor.Blue : ConsoleColor.Red;
        Console.WriteLine($"\u001b[32mPIECE SELECTED: {selectedPiece.Id}-{selectedPiece.Status}\u001b[0m");
        Console.ResetColor();
    }
    static void PrintWinner(GameController checkers)
    {
        Console.ReadLine();
        
        IPlayer? winner = checkers.GetWinner();
        if (winner == null)
        {
            Console.WriteLine("END RESULT: DRAW");
        }
        else
        {
            Console.ForegroundColor = (winner.Id == 1) ? ConsoleColor.Blue : ConsoleColor.Red; 
            Console.WriteLine($"Congratulations {winner.Name}, you have won!");
            Console.ResetColor();
        }
        
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
            sb.Append($"   {(RankName)i}   ");
        }

        return sb.ToString();
    }
    #endregion
    
    #region Event Handler
    private static void HandlePlayerAdded(IPlayer player)
    {
        Console.WriteLine($"🤴 New Player Added: {player.Name}");
        Thread.Sleep(500);
    }
    private static void HandlePieceMoved(Piece piece, Position position)
    {
        Console.ForegroundColor = (piece.Color == PieceColor.Blue) ? ConsoleColor.Blue : ConsoleColor.Red;
        Console.WriteLine($"Piece {piece.Id} moved to: {position}");
        Console.ResetColor();
        Thread.Sleep(500);
    }
    private static void HandlePieceCaptured(Piece piece)
    {
        Console.ForegroundColor = (piece.Color == PieceColor.Blue) ? ConsoleColor.Blue : ConsoleColor.Red;
        Console.WriteLine($"Piece {piece.Id} have been captured");
        Console.ResetColor();
        Thread.Sleep(500);
    }
    private static void HandlePiecePromoted(Piece piece)
    {
        Console.ForegroundColor = (piece.Color == PieceColor.Blue) ? ConsoleColor.Blue : ConsoleColor.Red;
        Console.WriteLine($"Piece {piece.Id} have been Promoted to {PieceStatus.King.ToString()}");
        Console.ResetColor();
        Thread.Sleep(500);
    }
    #endregion

    #region Setup Game
    public static void SetupGame(GameController checkers)
    {
        int displayLevel = 0;
        while (true)
        {
            switch (displayLevel)
            {
                case 0:
                    MainMenu(out displayLevel);
                    break;
                case 1:
                    AddBoard(checkers, out displayLevel);
                    break;
                case 2:
                    AddPlayer(checkers, out displayLevel);
                    break;
                case 3:
                    AddPiece(checkers, out displayLevel);
                    break;
                case 4:
                    FinalizeSetup(checkers, out displayLevel);
                    break;
                case 6:
                    ResetSetup(checkers, out displayLevel);
                    break;
                case 7:
                    Console.Clear();
                    return;
            }   
        }
    }
    private static void MainMenu(out int displayLevel)
    {
        Console.Clear();
        Console.CursorVisible = false;
        Console.OutputEncoding = Encoding.UTF8;
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Welcome to Checkers Game");
        Console.ResetColor();
        Console.WriteLine("\nCheckers Game Main Menu \u001b[36m(Use ⬆️ and ⬇️ key to navigate, and ENTER key to select)\u001b[0m:");
        
        var menuOptions = new[]
        {
            "Start",
            "Exit"
        };
        
        int selectedOption = SelectionMenu(menuOptions);
        if (selectedOption == 0)
        {
            displayLevel = 1;
        }
        else
        {
            displayLevel = 0;
            Environment.Exit(0);
        }
    }
    private static void AddBoard(GameController checkers, out int displayLevel)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Board Setup");
        Console.ResetColor();
        Console.WriteLine();
        
        Console.WriteLine("\u001b[36mSelect The Board Size:\u001b[0m");
        int[] menuOptions = new[]
        {
            8,
            10,
            12
        };

        int selectedOption = SelectionMenu(menuOptions);
        CheckersBoard board = new CheckersBoard(menuOptions[selectedOption]);
        checkers.SetBoard(board);
        
        displayLevel = 2;
    }
    private static void AddPlayer(GameController checkers, out int displayLevel)
    {
        Console.Clear();
        Console.CursorVisible = true;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Add Two Players");
        Console.ResetColor();
        Console.WriteLine();
        
        int maxPlayer = 2;
        for (int i = 0; i < maxPlayer; i++)
        {
            int playerId = i + 1;
            Console.WriteLine($"PLAYER {playerId}");
            string name = GetUserInput("Enter username: ");
            Player player = new Player(playerId, name);
            checkers.AddPlayer(player);
            Console.WriteLine();
        }
            
        displayLevel = 3;
    }
    private static void AddPiece(GameController checkers, out int displayLevel)
    {
        foreach (var player in checkers.GetActivePlayer())
        {
            int pieceQty = checkers.MaxPlayerPieces();
            PieceColor color = (player.Id == 1) ? PieceColor.Blue : PieceColor.Red;
            
            List<Piece> newPiece = (List<Piece>) checkers.GeneratePieces(color, pieceQty);
            checkers.SetPlayerPieces(player, newPiece);
        }
        checkers.SetPieceToBoard();
        
        displayLevel = 4;
    }
    private static void FinalizeSetup(GameController checkers, out int displayLevel)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Finalize Setup");
        Console.ResetColor();
        Console.WriteLine();
        
        Console.WriteLine($"Board Size: {checkers.GetBoardSize()}");
        foreach (var player in checkers.GetActivePlayer())
        {
            Console.WriteLine($"Player {player.Id}: {player.Name}");
        }
        
        Console.WriteLine("\n\u001b[36mPlay Checkers?\u001b[0m");
        string[] menuOptions = new[]
        {
            "Yes",
            "No"
        };

        int selectedOption = SelectionMenu(menuOptions);
        displayLevel = (selectedOption == 0) ? 7 : 6;
    }
    private static void ResetSetup(GameController checkers, out int displayLevel)
    {
        checkers.RemoveAllPlayers();
        displayLevel = 0;
    }
    private static int SelectionMenu<T> (T[] menuOptions)
    {
        int nMenu = menuOptions.Length - 1;
        int selectedOption = 0;
        bool menuSelected = false;
        var decorator = "➡️ \u001b[34m";
        (int left, int top) = Console.GetCursorPosition();
        
        while (!menuSelected)
        {
            Console.SetCursorPosition(left, top);
            for (int i = 0; i <= nMenu; i++)
            {
                Console.WriteLine($"{(selectedOption == i ? decorator : "   ")}{menuOptions[i]}\u001b[0m");
            }

            ConsoleKeyInfo key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    selectedOption = (selectedOption == 0) ? 0 : selectedOption - 1;
                    break;
                case ConsoleKey.DownArrow:
                    selectedOption = (selectedOption == nMenu) ? nMenu : selectedOption + 1;
                    break;
                case ConsoleKey.Enter:
                    menuSelected = true;
                    break;
            }
        }
        return selectedOption;
    }
    #endregion
}