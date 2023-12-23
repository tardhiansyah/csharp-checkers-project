using System.Text;
using System.Text.RegularExpressions;
using CheckersGame.Class;
using CheckersGame.Enumeration;
using CheckersGame.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CheckersGame;

public static class Program
{
    #region Main
    static void Main()
    {
        GameController checkers = SetupGameController();

        SubscribeToEvents(checkers);
        
        SetupDisplay(checkers);

        PlayGame(checkers);
        
        ShowTheWinner(checkers);
        
        Console.ReadLine();
    }

    static GameController SetupGameController()
    {
        // Dependency Injection for creating new GameController
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        return serviceProvider.GetRequiredService<GameController>();
    }
    static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddNLog("nlog.config");
        });
        services.AddTransient<GameController>();
    }
    static void SubscribeToEvents(GameController checkers)
    {
        checkers.PieceCaptured += HandlePieceCaptured;
        checkers.PieceMoved += HandlePieceMoved;
        checkers.PiecePromoted += HandlePiecePromoted;
        checkers.PlayerAdded += HandlePlayerAdded;
        checkers.CurrentPlayerChanged += HandleCurrentPlayerChanged;
        checkers.StatusChanged += HandleGameStatusChanged;
    }
    static void PlayGame(GameController checkers)
    {
        Piece? selectedPiece = null;
        List<Position> validMovePositions = new List<Position>();
        int currentNPiece = checkers.CountPieceOnBoard();
        int lastNPiece = currentNPiece;
        bool firstMove = true;
    
        bool started = checkers.Start();
        while (started && !checkers.GameOver())
        {
            IPlayer currentPlayer = checkers.GetCurrentPlayer()!;
            int remainingPlayerPieces = checkers.GetPlayerPieces(currentPlayer).Count();
            
            if (selectedPiece == null)
            {
                firstMove = true;
                validMovePositions.Clear();
                
                Console.Clear();
                ShowBoard(checkers, validMovePositions);
                ShowPlayerInformation(currentPlayer, remainingPlayerPieces);
                
                selectedPiece = SelectPiece(checkers);
                validMovePositions = checkers.GetPossibleMoves(selectedPiece, firstMove).ToList();
            }
            
            while (validMovePositions.Count != 0)
            {
                Console.Clear();
                ShowBoard(checkers, validMovePositions);
                ShowPlayerInformation(currentPlayer, remainingPlayerPieces);
                ShowSelectedPiece(selectedPiece);
                
                Position newPosition = SelectPosition(checkers);
                if (!checkers.MovePiece(selectedPiece, newPosition, firstMove))
                {
                    Console.WriteLine("\u001b[31mInvalid move!\u001b[0m");
                    continue;
                }
                firstMove = false;
                
                currentNPiece = checkers.CountPieceOnBoard();
                if (currentNPiece != lastNPiece)
                {
                    validMovePositions = checkers.GetPossibleMoves(selectedPiece, firstMove).ToList();
                    continue;
                }
                break;
            }
            
            checkers.PromotePiece(selectedPiece);
            lastNPiece = currentNPiece;
            selectedPiece = null;
            checkers.NextTurn();
        }
    }
    #endregion
    
    #region User Input
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
    static void ShowPlayerInformation(IPlayer player, int remainingPieces)
    {
        PieceColor color = (player.Id == 1) ? PieceColor.Blue : PieceColor.Red;
        
        Console.ForegroundColor = (player.Id == 1) ? ConsoleColor.Blue : ConsoleColor.Red;
        Console.WriteLine($"======================== PLAYER {player.Id} ========================");
        Console.WriteLine($"NICKNAME: {player.Name.ToUpper()}");
        Console.WriteLine($"PIECE COLOR: {color.ToString().ToUpper()}");
        Console.WriteLine($"PIECE REMAINING: {remainingPieces:D2} Pieces");
        Console.WriteLine("==========================================================");
        Console.ResetColor();
    }
    static void ShowBoard(GameController checkers, List<Position> validMovePositions)
    {
        Piece?[,] board = checkers.GetBoardLayout();

        for (int i = 0; i < board.GetLength(0); i++)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(GenerateRowSeparator(checkers.GetBoardSize()));
            Console.Write($"{board.GetLength(0) - i :D2}");
            Console.ResetColor();
            
            for (int j = 0; j < board.GetLength(1); j++)
            {
                Piece? piece = checkers.GetPiece(i, j);
                string symbol = (piece == null) ? "   " : GetOccupiedCellSymbol(piece);
                
                if (validMovePositions.Any(position => position.Row == i && position.Column == j))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    symbol = $" X ";
                }
                
                Console.Write($"| {symbol} |");
                Console.ResetColor();
            }
            Console.WriteLine();
        }
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(GenerateRowSeparator(checkers.GetBoardSize()));
        Console.WriteLine(GenerateRankName(checkers.GetBoardSize()));
        Console.ResetColor();
    }
    static string GetOccupiedCellSymbol(Piece piece)
    {
        string type = (piece.Status == PieceStatus.King) ? "K" : "R";
        Console.ForegroundColor = (piece.Color == PieceColor.Blue) ? ConsoleColor.Blue : ConsoleColor.Red;
        return $"{type}{piece.Id:D2}";
    }
    static void ShowSelectedPiece(Piece selectedPiece)
    {
        Console.ForegroundColor = (selectedPiece.Color == PieceColor.Blue) ? ConsoleColor.Blue : ConsoleColor.Red;
        Console.WriteLine("PIECE SELECTED: {0}-{1}", selectedPiece.Status, selectedPiece.Id);
        Console.ResetColor();
    }
    static void ShowTheWinner(GameController checkers)
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
    static string GenerateRowSeparator(int size)
    {
        string dashSegment = new string('-', 7); // "-------"
        string separator = string.Join("", Enumerable.Repeat(dashSegment, size));
        return "--" + separator;
    }
    static string GenerateRankName(int size)
    {
        return "  " + string.Join("   ", Enumerable.Range(0, size).Select(i => $"   {(RankName)i}")) + "   ";
    }
    #endregion
    
    #region Event Handler
    private static void HandlePlayerAdded(IPlayer player)
    {
        Console.WriteLine("🤴 New Player Added: {0}", player);
    }
    private static void HandlePieceMoved(Piece piece, Position position)
    {
        Console.ForegroundColor = (piece.Color == PieceColor.Blue) ? ConsoleColor.Blue : ConsoleColor.Red;
        Console.WriteLine("Piece {0} moved to: {1}", piece, position);
        Console.ResetColor();
    }
    private static void HandlePieceCaptured(Piece piece)
    {
        Console.ForegroundColor = (piece.Color == PieceColor.Blue) ? ConsoleColor.Blue : ConsoleColor.Red;
        Console.WriteLine("Piece {0} has been captured", piece);
        Console.ResetColor();
    }
    private static void HandlePiecePromoted(Piece piece)
    {
        Console.ForegroundColor = (piece.Color == PieceColor.Blue) ? ConsoleColor.Blue : ConsoleColor.Red;
        Console.WriteLine("Piece has been promoted: {0}", piece);
        Console.ResetColor();
    }
    private static void HandleCurrentPlayerChanged(IPlayer player)
    {
        Console.WriteLine("Next player is: {0}", player);
    }
    private static void HandleGameStatusChanged(GameStatus status)
    {
        switch (status)
        {
            case GameStatus.Ready:
                Console.WriteLine("Game is ready. Waiting for the first move..");
                break;
            case GameStatus.GameOver:
                Console.WriteLine("GAME OVER!!");
                break;
        }
    }
    #endregion

    #region Setup Game
    public static void SetupDisplay(GameController checkers)
    {
        MenuLevel level = MenuLevel.MainMenu;
        while (true)
        {
            switch (level)
            {
                case MenuLevel.MainMenu:
                    MainMenu(checkers, out level);
                    break;
                case MenuLevel.AddBoard:
                    AddBoard(checkers, out level);
                    break;
                case MenuLevel.AddPlayer:
                    AddPlayer(checkers, out level);
                    break;
                case MenuLevel.AddPiece:
                    AddPiece(checkers, out level);
                    break;
                case MenuLevel.Finalize:
                    FinalizeSetup(checkers, out level);
                    break;
                case MenuLevel.Exit:
                    Console.Clear();
                    return;
            }   
        }
    }
    private static void MainMenu(GameController checkers, out MenuLevel level)
    {
        Console.Clear();
        Console.CursorVisible = false;
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("\u001B[32mWelcome to Checkers Game\u001b[0m\n");
        Console.WriteLine("Main Menu \u001b[36m(Use ⬆️ and ⬇️ key to navigate, and ENTER key to select)\u001b[0m:");
        
        var menuOptions = new[]
        {
            "Start",
            "Exit"
        };
        
        int selectedOption = SelectionMenu(menuOptions);
        if (selectedOption == 0)
        {
            level = MenuLevel.AddBoard;
            ResetSetup(checkers);
        }
        else
        {
            level = MenuLevel.MainMenu;
            Environment.Exit(0);
        }
    }
    private static void AddBoard(GameController checkers, out MenuLevel level)
    {
        Console.Clear();
        Console.WriteLine("\u001B[32mBoard Setup\u001B[0m\n");
        Console.WriteLine("\u001b[36mSelect The Board Size:\u001b[0m");
        
        int[] menuOptions = new[]
        {
            8,
            10,
            12
        };

        int selectedOption = SelectionMenu(menuOptions);
        checkers.SetBoard(new CheckersBoard<Piece>(menuOptions[selectedOption]));

        level = MenuLevel.AddPlayer;
    }
    private static void AddPlayer(GameController checkers, out MenuLevel level)
    {
        Console.Clear();
        Console.CursorVisible = true;
        Console.WriteLine("\u001B[32mAdd Two Players\u001B[0m\n");
        
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
            
        level = MenuLevel.AddPiece;
    }
    private static void AddPiece(GameController checkers, out MenuLevel level)
    {
        Parallel.ForEach(checkers.GetActivePlayer(), player =>
        {
            PieceColor color = (player.Id == 1) ? PieceColor.Blue : PieceColor.Red;
            int pieceQty = checkers.MaxPlayerPieces();
            checkers.SetPlayerPieces(player, checkers.GeneratePieces(color, pieceQty).ToHashSet());
        });
        
        checkers.SetPieceToBoard();
        
        level = MenuLevel.Finalize;
    }
    private static void FinalizeSetup(GameController checkers, out MenuLevel level)
    {
        Console.Clear();
        Console.WriteLine("\u001B[32mFinalize Setup\u001B[0m\n");
        Console.WriteLine("Board Size: {0}", checkers.GetBoardSize());

        foreach (var player in checkers.GetActivePlayer())
        {
            Console.WriteLine("Player: {0}", player);
        }
        
        Console.WriteLine("\n\u001B[36mPlay Checkers?\u001B[0m");
        
        var menuOptions = new[]
        {
            "Yes",
            "No"
        };

        int selectedOption = SelectionMenu(menuOptions);
        level = (selectedOption == 0) ? MenuLevel.Exit : MenuLevel.MainMenu;
    }
    private static void ResetSetup(GameController checkers)
    {
        checkers.RemoveAllPlayers();
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