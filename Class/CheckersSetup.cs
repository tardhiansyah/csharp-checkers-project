using System.ComponentModel.Design;
using System.Diagnostics;
using CheckersGame.Interface;
using System.Text;
using CheckersGame.Enumeration;

namespace CheckersGame.Class;

public static class CheckersSetup
{
    private static int _displayLevel = 0;
        
    public static void Show(GameController checkers)
    {
        while (true)
        {
            switch (_displayLevel)
            {
                case 0:
                    MainMenu();
                    break;
                case 1:
                    AddBoard(checkers);
                    break;
                case 2:
                    AddPlayer(checkers);
                    break;
                case 3:
                    AddPiece(checkers);
                    break;
                case 4:
                    FinalizeSetup(checkers);
                    break;
                case 6:
                    ResetSetup(checkers);
                    break;
                case 7:
                    Console.Clear();
                    return;
            }   
        }
    }
    private static void MainMenu()
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
        
        int selectedOption = SelectionMenu<string>(menuOptions);
        if (selectedOption == 0)
        {
            _displayLevel = 1;
        }
        else
        {
            Environment.Exit(0);
        }
    }
    private static void AddBoard(GameController checkers)
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

        int selectedOption = SelectionMenu<int>(menuOptions);
        CheckersBoard board = new CheckersBoard(menuOptions[selectedOption]);
        if (checkers.SetBoard(board))
            _displayLevel = 2;
    }
    private static void AddPlayer(GameController checkers)
    {
        Console.Clear();
        Console.CursorVisible = true;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Add Players (2 Players)");
        Console.ResetColor();
        Console.WriteLine();
        
        int maxPlayer = 2;
        for (int i = 0; i < maxPlayer; i++)
        {
            int playerId = i + 1;
            Console.WriteLine($"PLAYER {playerId}");
            string name = GetUserName();
            Player player = new Player(playerId, name);
            checkers.AddPlayer(player);
            Console.WriteLine();
        }
            
        _displayLevel = 3;
    }
    private static void AddPiece(GameController checkers)
    {
        foreach (var player in checkers.GetActivePlayer())
        {
            int pieceQty = checkers.MaxPlayerPieces();
            PieceColor color = (player.Id == 1) ? PieceColor.Blue : PieceColor.Red;
            
            List<Piece> newPiece = (List<Piece>) checkers.GeneratePieces(color, pieceQty);
            checkers.SetPlayerPieces(player, newPiece);
        }

        if (checkers.SetPieceToBoard())
            _displayLevel = 4;
    }
    private static void FinalizeSetup(GameController checkers)
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

        int selectedOption = SelectionMenu<string>(menuOptions);
        if (selectedOption == 0)
        {
            _displayLevel = 7;
        }
        else
        {
            _displayLevel = 6;
        }
    }
    private static void ResetSetup(GameController checkers)
    {
        checkers.RemoveAllPlayers();
        _displayLevel = 0;
    }
    
    private static string GetUserName()
    {
        Console.CursorVisible = true;
        while (true)
        {
            Console.Write("Enter Nickname: ");
            string? name = Console.ReadLine();
            if (name is { Length: > 2 })
            {
                Console.CursorVisible = false;
                Console.WriteLine($"🤴 Player Added: {name}");
                return name;
            }
        }
        
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
        Debug.WriteLine($"\n{decorator}You selected {menuOptions[selectedOption]}");
        return selectedOption;
    }
}