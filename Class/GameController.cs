using CheckersGame.Enumeration;
using CheckersGame.Interface;
using Microsoft.Extensions.Logging;

namespace CheckersGame.Class;

public class GameController
{
    private readonly ILogger<GameController>? _logger;
    private readonly Dictionary<IPlayer, HashSet<Piece>> _players;
    private IBoard<Piece> _board = null!;
    private GameStatus _status;
    private IPlayer? _currentPlayer;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GameController"/> class.
    /// </summary>
    /// <param name="logger">Optional logger for recording log messages related to the game controller.</param>
    public GameController(ILogger<GameController>? logger = null)
    {
        _logger = logger;
        _players = new Dictionary<IPlayer, HashSet<Piece>>();
        _currentPlayer = null;
        _logger?.LogInformation("Create new game controller instances");
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GameController"/> class with specified parameters.
    /// </summary>
    /// <param name="board">The game board used in the controller.</param>
    /// <param name="player1">The first player participating in the game.</param>
    /// <param name="player2">The second player participating in the game.</param>
    /// <param name="logger">Optional logger for recording log messages related to the game controller.</param>
    public GameController(IBoard<Piece> board, IPlayer player1, IPlayer player2, ILogger<GameController>? logger = null)
    {
        _logger = logger;
        _board = board;
        _players = new Dictionary<IPlayer, HashSet<Piece>>();
        _logger?.LogInformation("Create new game controller instances");
        
        AddPlayer(player1);
        AddPlayer(player2);
        
        SetPlayerPieces(player1, (HashSet<Piece>)GeneratePieces(PieceColor.Blue, MaxPlayerPieces()));
        SetPlayerPieces(player2, (HashSet<Piece>)GeneratePieces(PieceColor.Red, MaxPlayerPieces()));
        
        SetCurrentPlayer(player1);
    }
    
    #region Action / Delegate
    public event Action<Piece>? PieceCaptured;
    public event Action<Piece, Position>? PieceMoved;
    public event Action<Piece>? PiecePromoted;
    public event Action<IPlayer>? PlayerAdded;
    public event Action<IPlayer>? CurrentPlayerChanged;
    public event Action<GameStatus>? StatusChanged;
    
    /// <summary>
    /// Raises the event indicating a change in the game status.
    /// </summary>
    /// <param name="status">The new game status.</param>
   protected virtual void OnChangeStatus(GameStatus status)
    {
        _logger?.LogInformation("Game status changed to: {Status}", status);
        StatusChanged?.Invoke(status);
    }

    /// <summary>
    /// Raises the event indicating a change in the game current turn player.
    /// </summary>
    /// <param name="player">The player whose turn it is now.</param>
    protected virtual void OnChangeCurrentPlayer(IPlayer player)
    {
        _logger?.LogInformation("Current player changed to: {Player}", player);
        CurrentPlayerChanged?.Invoke(player);
    }
    
    /// <summary>
    /// Raises the event indicating the addition of a new player to the game.
    /// </summary>
    /// <param name="player">The player added to the game.</param>
    protected virtual void OnAddPlayer(IPlayer player)
    {
        _logger?.LogInformation("New player added: {Player}", player);
        PlayerAdded?.Invoke(player);
    }
    
    /// <summary>
    /// Raises the event indicating the movement of a piece to a new position on the board.
    /// </summary>
    /// <param name="piece">The piece that has been moved.</param>
    /// <param name="position">The new position of the piece on the board.</param>
    protected virtual void OnMovePiece(Piece piece, Position position)
    {
        _logger?.LogInformation("Piece {Piece} moved to {Position}", piece, position);
        PieceMoved?.Invoke(piece, position);
    }
    
    /// <summary>
    /// Raises the event indicating the capture of a piece on the board.
    /// </summary>
    /// <param name="piece">The piece that has been captured.</param>
    protected virtual void OnCapturePiece(Piece piece)
    {
        _logger?.LogInformation("Piece captured: {Piece}", piece);
        PieceCaptured?.Invoke(piece);
    }
    
    /// <summary>
    /// Raises the event indicating the promotion of a piece on the board.
    /// </summary>
    /// <param name="piece">The piece that has been promoted.</param>
    protected virtual void OnPromotePiece(Piece piece)
    {
        _logger?.LogInformation("Piece promoted: {Piece}", piece);
        PiecePromoted?.Invoke(piece);
    } 
    #endregion
    
    #region Get-Set Player
    /// <summary>
    /// Adds a new player to the game.
    /// </summary>
    /// <param name="player">The player to be added.</param>
    /// <returns>True if the player is successfully added; otherwise, false.</returns>
    public bool AddPlayer(IPlayer player)
    {
        if (!_players.TryAdd(player, new HashSet<Piece>()))
        {
            _logger?.LogWarning("Attempt to add new player {Player} failed", player);
            return false;
        }
        
        OnAddPlayer(player);
        return true;
    }
    
    /// <summary>
    /// Sets the player for this game turn.
    /// </summary>
    /// <param name="player">The player to be set as the current player.</param>
    /// <returns>True if the current player is successfully set; otherwise, false.</returns>
    public bool SetCurrentPlayer(IPlayer player)
    {
        if (!IsPlayerValid(player))
        {
            _logger?.LogWarning("Attempt to change current player to {Player} failed", player);
            return false;
        }
        
        _currentPlayer = player;
        OnChangeCurrentPlayer(player);
        return true;
    }
    
    /// <summary>
    /// Gets the current player of the game.
    /// </summary>
    /// <returns>The current player, or null if no current player is set.</returns>
    public IPlayer? GetCurrentPlayer()
    {
        if (_currentPlayer == null)
        {
            _logger?.LogWarning("Attempt to get current player failed: {CurrentPlayer}", _currentPlayer);
        }
        
        return _currentPlayer;
    }
    
    /// <summary>
    /// Gets the active players in the game.
    /// </summary>
    /// <returns>An enumerable of active players.</returns>
    public IEnumerable<IPlayer> GetActivePlayer()
    {
        return _players.Keys;
    }
    
    /// <summary>
    /// Removes all players from the game.
    /// </summary>
    public void RemoveAllPlayers()
    {
        _logger?.LogInformation("Removing all players from the game");
        _players.Clear();
    }
    
    /// <summary>
    /// Sets the pieces for a specific player.
    /// </summary>
    /// <param name="player">The player whose pieces are to be set.</param>
    /// <param name="pieces">The set of pieces to be associated with the player.</param>
    /// <returns>True if the pieces are successfully set for the player; otherwise, false.</returns>
    public bool SetPlayerPieces(IPlayer player, HashSet<Piece> pieces)
    {
        if (!IsPlayerValid(player))
        {
            _logger?.LogWarning("Attempt to add pieces to player {Player} failed", player);
            return false;
        }
        
        _players[player] = pieces;
        _logger?.LogInformation("New pieces added to player {Player}", player);
        return true;
    }
    
    /// <summary>
    /// Gets the dictionary of players and their associated pieces.
    /// </summary>
    /// <returns>A dictionary containing players and their associated pieces.</returns>
    public Dictionary<IPlayer, HashSet<Piece>> GetPlayers()
    {
        return _players;
    }
    
    /// <summary>
    /// Gets the pieces associated with a specific player.
    /// </summary>
    /// <param name="player">The player whose pieces are to be retrieved.</param>
    /// <returns>An enumerable of pieces associated with the player, or an empty enumerable if the player is not found.</returns>
    public IEnumerable<Piece> GetPlayerPieces(IPlayer player)
    {
        if (!IsPlayerValid(player))
        {
            _logger?.LogWarning("Attempt to get pieces from player {Player} failed", player);
            return Enumerable.Empty<Piece>();
        }
        
        return _players[player];
    }

    /// <summary>
    /// Retrieves the player who owns the specified piece.
    /// </summary>
    /// <param name="piece">The piece for which the owner player is to be retrieved.</param>
    /// <returns>
    /// The player who owns the specified piece, or null if the piece is not found among any players.
    /// </returns>
    public IPlayer? GetPlayerByPieces(Piece piece)
    {
        IPlayer? player = _players.Keys.FirstOrDefault(player => _players[player].Contains(piece));
        if (player == null)
        {
            _logger?.LogWarning("Attempt to get player that owns piece with ID {Id} and Color {Color} failed", piece.Id, piece.Color);
            return null;
        }
        
        return player;
    }
    
    /// <summary>
    /// Checks if a player is valid (exists in the game).
    /// </summary>
    /// <param name="player">The player to be checked.</param>
    /// <returns>True if the player is valid; otherwise, false.</returns>
    private bool IsPlayerValid(IPlayer player)
    {
        if (!_players.ContainsKey(player))
        {
            _logger?.LogWarning("Player {Player} is not found", player);
            return false;
        }
        
        return true;
    }
    #endregion
    
    #region Get-Set Pieces & Board
    /// <summary>
    /// Generates a specified quantity of pieces with the given color.
    /// </summary>
    /// <param name="color">The color of the pieces to generate.</param>
    /// <param name="quantity">The number of pieces to generate (default is 12).</param>
    /// <returns>An enumerable of generated pieces.</returns>
    public IEnumerable<Piece> GeneratePieces(PieceColor color, int quantity = 12)
    {
        _logger?.LogInformation("Generating {Quantity} {Color} Pieces", quantity, color);
        
        return Enumerable.Range(1, quantity).Select(count => new Piece(count, color));
    }
    
    /// <summary>
    /// Sets the game board to the specified board.
    /// </summary>
    /// <param name="board">The new game board.</param>
    /// <returns>True if the board is successfully set; otherwise, false.</returns>
    public bool SetBoard(IBoard<Piece> board)
    {
        if (GetStatus() != GameStatus.NotReady)
        {
            _logger?.LogWarning("Attempt to set new board {Board} with size {Board.Size} failed", board, board.Size);
            return false;
        }
        
        _board = board;
        _logger?.LogInformation("Attempt to set new board {Board} with size {Board.Size} is success", board, board.Size);
        return true;
    }
    
    /// <summary>
    /// Gets the size of the game board.
    /// </summary>
    /// <returns>The size of the game board.</returns>
    public int GetBoardSize()
    {
        return _board.Layout.GetLength(0);
    }
    
    /// <summary>
    /// Gets the layout of the game board.
    /// </summary>
    /// <returns>The layout of the game board as a 2D array of pieces.</returns>
    public Piece?[,] GetBoardLayout()
    {
        return _board.Layout;
    }
    
    /// <summary>
    /// Sets pieces for all active players on the game board.
    /// </summary>
    /// <returns>True if the pieces are successfully set; otherwise, false.</returns>
    public bool SetPieceToBoard()
    {
        if (GetStatus() != GameStatus.NotReady || !GetActivePlayer().Any())
        {
            _logger?.LogWarning("Attempt to set pieces on the board failed");
            return false;
        }

        Parallel.ForEach(GetActivePlayer(), player => SetPieceToBoard(player));
        _logger?.LogInformation("Attempt to set pieces on the board is success");
        return true;
    }
    
    /// <summary>
    /// Sets pieces for a specific player on the game board.
    /// </summary>
    /// <param name="player">The player whose pieces are to be set on the board.</param>
    /// <returns>True if the pieces are successfully set; otherwise, false.</returns>
    public bool SetPieceToBoard(IPlayer player)
    {
        if (GetStatus() != GameStatus.NotReady || !GetActivePlayer().Any())
        {
            _logger?.LogWarning("Attempt to set player {Player} pieces on the board failed", player);
            return false;
        }
        
        int boardSize = GetBoardSize();
        int nRowWithPieces = (boardSize - 2) / 2;
        int startRow = (_players[player].First().Color == PieceColor.Blue) ? 0 : (GetBoardSize() - nRowWithPieces);
        int pieceCounter = 0, nextRow = 0;
        
        foreach (Piece piece in _players[player])
        {
            int row = startRow + nextRow;
            int column = (row % 2 == 0) ? pieceCounter * 2 : pieceCounter * 2 + 1;
            
            if (SetPieceToBoard(piece, row, column))
                pieceCounter++;
        
            if (column >= GetBoardSize() - 2 && row < GetBoardSize())
            {
                pieceCounter = 0; nextRow++;
            }
        }
        
        _logger?.LogInformation("Attempt to set player {Player}'s pieces on the board is success", player);
        return true;
    }
    
    /// <summary>
    /// Sets a piece to the specified position on the game board.
    /// </summary>
    /// <param name="piece">The piece to be set.</param>
    /// <param name="position">The position on the board where the piece is to be set.</param>
    /// <returns>True if the piece is successfully set; otherwise, false.</returns>
    private bool SetPieceToBoard(Piece piece, Position position)
    {
        return SetPieceToBoard(piece, position.Row, position.Column);
    }
    
    /// <summary>
    /// Sets a piece to the specified position on the game board.
    /// </summary>
    /// <param name="piece">The piece to be set.</param>
    /// <param name="row">The row index on the board where the piece is to be set.</param>
    /// <param name="column">The column index on the board where the piece is to be set.</param>
    /// <returns>True if the piece is successfully set; otherwise, false.</returns>
    private bool SetPieceToBoard(Piece piece, int row, int column)
    {
        if (GetPiece(row, column) != null)
        {
            _logger?.LogWarning("Position (row,column) ({Row},{Column}) is occupied by {Piece}", row, column, piece);
            return false;
        }
        
        _board.Layout[row, column] = piece;
        _logger?.LogInformation("Attempt to set {Piece} to (row,column) ({Row},{Column}) is success", piece, row, column);
        return true;
    }
    
    /// <summary>
    /// Gets all pieces on the game board.
    /// </summary>
    /// <returns>An enumerable of all pieces on the board.</returns>
    public IEnumerable<Piece> GetPieces()
    {
        return _players.Values.SelectMany(playerPiece => playerPiece);
    }
    
    /// <summary>
    /// Gets all pieces owned by a specific player.
    /// </summary>
    /// <param name="player">The player whose pieces are to be retrieved.</param>
    /// <returns>An enumerable of pieces owned by the player, or an empty enumerable if the player is not found.</returns>
    public IEnumerable<Piece> GetPieces(IPlayer player)
    {
        if (!_players.TryGetValue(player, out HashSet<Piece>? pieces))
        {
            _logger?.LogWarning("Attempt to get player {Player}'s pieces failed", player);
            return Enumerable.Empty<Piece>();
        }
        
        _logger?.LogInformation("Attempt to get player {Player}'s pieces is success", player);
        return pieces;
    }

    /// <summary>
    /// Gets a piece owned by a specific player with the specified ID.
    /// </summary>
    /// <param name="player">The player whose piece is to be retrieved.</param>
    /// <param name="id">The ID of the piece to be retrieved.</param>
    /// <returns>The piece with the specified ID owned by the player, or null if not found.</returns>
    public Piece? GetPiece(IPlayer player, int id)
    {
        Piece? selectedPiece = GetPlayerPieces(player).FirstOrDefault(piece => piece.Id == id);
        if (selectedPiece == null)
        {
            _logger?.LogWarning("Attempt to get player {Player}'s piece with ID {Id} failed", player, id);
            return null;
        }
        
        _logger?.LogInformation("Attempt to get player {Player}'s piece with ID {Id} is success", player, id);
        return selectedPiece;
    }
    
    /// <summary>
    /// Gets the piece at the specified position on the game board.
    /// </summary>
    /// <param name="position">The position on the board where the piece is to be retrieved.</param>
    /// <returns>The piece at the specified position, or null if no piece is found.</returns>
    public Piece? GetPiece(Position position)
    {
        return GetPiece(position.Row, position.Column);
    }
    
    /// <summary>
    /// Gets the piece at the specified position on the game board.
    /// </summary>
    /// <param name="row">The row index on the board where the piece is to be retrieved.</param>
    /// <param name="column">The column index on the board where the piece is to be retrieved.</param>
    /// <returns>The piece at the specified position, or null if no piece is found.</returns>
    public Piece? GetPiece(int row, int column)
    {
        return _board.Layout[row, column];
    }

    /// <summary>
    /// Removes a piece from player and game board.
    /// </summary>
    /// <param name="piece">The piece that will be removed</param>
    /// <returns>True if the piece is successfully removed; otherwise, false.</returns>
    public bool RemovePiece(Piece piece)
    {
        Position? position = GetPosition(piece);
        if (position == null)
        {
            return false;
        }
        
        IPlayer? player = GetPlayerByPieces(piece);
        if (player == null)
        {
            return false;
        }
        
        return RemovePieceFromBoard(position.Row, position.Column) && RemovePieceFromPlayer(player, piece);
    }
    
    /// <summary>
    /// Removes a piece at the specified position on the game board.
    /// </summary>
    /// <param name="position">The position on the board where the piece is to be removed.</param>
    /// <returns>True if the piece is successfully removed; otherwise, false.</returns>
    private bool RemovePieceFromBoard(Position position)
    {
        return RemovePieceFromBoard(position.Row, position.Column);
    }
    
    /// <summary>
    /// Removes a piece at the specified position on the game board.
    /// </summary>
    /// <param name="row">The row index on the board where the piece is to be removed.</param>
    /// <param name="column">The column index on the board where the piece is to be removed.</param>
    /// <returns>True if the piece is successfully removed; otherwise, false.</returns>
    private bool RemovePieceFromBoard(int row, int column)
    {
        if (GetPiece(row, column) == null)
        {
            _logger?.LogWarning("Attempt to remove player piece from (row,column) ({Row},{Column}) failed", row, column);
            return false;
        }
        
        _board.Layout[row, column] = null;
        _logger?.LogInformation("Attempt to remove player piece from (row,column) ({Row},{Column}) is success", row, column);
        return true;
    }
    
    /// <summary>
    /// Removes a piece from a specific player's collection.
    /// </summary>
    /// <param name="player">The player whose piece is to be removed.</param>
    /// <param name="removedPiece">The piece to be removed.</param>
    /// <returns>True if the piece is successfully removed from the player's collection; otherwise, false.</returns>
    private bool RemovePieceFromPlayer(IPlayer player, Piece removedPiece)
    {
        if (!_players[player].Remove(removedPiece))
        {
            _logger?.LogWarning("Attempt to remove player {Player}'s piece {Piece} failed", player, removedPiece);
            return false;
        }

        _logger?.LogInformation("Attempt to remove player {Player}'s piece {Piece} is success", player, removedPiece);
        return true;
    }
    
    /// <summary>
    /// Counts the number of pieces currently on the game board.
    /// </summary>
    /// <returns>The number of pieces on the board.</returns>
    public int CountPieceOnBoard()
    {
        int count = 0;
        
        for (int i = 0; i < GetBoardSize(); i++)
        {
            for (int j = 0; j < GetBoardSize(); j++)
            {
                if (GetPiece(i, j) != null)
                {
                    count++;
                }
            }
        }
        
        _logger?.LogInformation("Attempt to get number of pieces on board is success: {Count}", count);
        return count;
    }
    
    /// <summary>
    /// Calculates the maximum number of pieces each player can have based on the current board size.
    /// </summary>
    /// <returns>The maximum number of pieces each player can have.</returns>
    public int MaxPlayerPieces()
    {
        int maxPieces = GetBoardSize() * (GetBoardSize() - 2) / 4 + (GetBoardSize() % 2);
        
        _logger?.LogInformation("Attempt to get max number of pieces on player is success: {NPieces}", maxPieces);
        return maxPieces;
    }
    #endregion
    
    #region Check Valid Movement
    /// <summary>
    /// Gets the position of a specific piece on the game board.
    /// </summary>
    /// <param name="piece">The piece for which the position is to be retrieved.</param>
    /// <returns>
    /// The position of the piece on the board, or null if the piece is not found.
    /// </returns>
    public Position? GetPosition(Piece piece)
    {
        for (int i = 0; i < GetBoardSize(); i++)
        {
            for (int j = 0; j < GetBoardSize(); j++)
            {
                if (!piece.Equals(GetPiece(i, j)))
                {
                    continue;
                }
                  
                _logger?.LogInformation("Piece {Piece} found at (Row,Column) ({Row},{Column})", piece, i, j);
                return new Position(i, j);
            }
        }
        
        _logger?.LogWarning("Piece {Piece} not found on the board", piece);
        return null;
    }
    
    /// <summary>
    /// Gets all possible moves for every piece owned by a specific player.
    /// </summary>
    /// <param name="player">The player for whom to retrieve possible moves.</param>
    /// <returns>
    /// An enumerable of positions representing all possible moves for the player's pieces.
    /// </returns>
    public IEnumerable<Position> GetPossibleMoves(IPlayer player)
    {
        _logger?.LogInformation("Get all possible moves for every pieces player {Player} own", player);
        return _players[player].SelectMany(piece => GetPossibleMoves(piece));
    }

    /// <summary>
    /// Gets possible moves for a specific piece on the game board.
    /// </summary>
    /// <param name="piece">The piece for which to retrieve possible moves.</param>
    /// <param name="firstMove">Flag indicating whether it's the first move of the piece (default is true).</param>
    /// <returns>
    /// An enumerable of positions representing possible moves for the piece.
    /// </returns>  
    public IEnumerable<Position> GetPossibleMoves(Piece? piece, bool firstMove = true)
    {
        if (piece == null)
        {
            return Enumerable.Empty<Position>();
        }
        
        HashSet<Position> possibleMoves = new HashSet<Position>();
        Position? piecePos = GetPosition(piece);
        if (piecePos == null)
        {
            _logger?.LogWarning("Attempt to obtained piece {Piece}'s possible moves failed", piece);
            return Enumerable.Empty<Position>();
        }

        bool isAnyJumpMoves = false;
        for (int row = -1; row <= 1; row++)
        {
            for (int column = -1; column <= 1; column++)
            {
                if (row == 0 || column == 0)
                {
                    continue;
                }
                
                if (!CanMoveBackward(piece))
                {
                    int skippedRow = (piece.Color == PieceColor.Blue) ? -1 : 1;
                    if (row == skippedRow)
                    {
                        continue;
                    }
                }

                int targetRow = piecePos.Row + row;
                int targetColumn = piecePos.Column + column;
                if (!WithinBoundaries(targetRow, targetColumn))
                {
                    continue;
                }
                
                // Check possible single tile and jump move
                Piece? enemyPiece = GetPiece(targetRow, targetColumn);
                if (enemyPiece == null)
                {
                    if (!firstMove || isAnyJumpMoves)
                    {
                        continue;
                    }
                    
                    _logger?.LogInformation("Adding piece {Piece}'s possible move: (Row,Column) ({Row},{Column})", piece, targetRow, targetColumn);
                    possibleMoves.Add(new Position(targetRow, targetColumn));
                }
                else if (enemyPiece.Color != piece.Color)
                {
                    int jumpRow = targetRow + row;
                    int jumpColumn = targetColumn + column;
                    if (!IsValidMove(jumpRow, jumpColumn))
                    {
                        continue;
                    }

                    if (!isAnyJumpMoves)
                    {
                        possibleMoves.Clear();
                        isAnyJumpMoves = true;
                    }
                    
                    
                    _logger?.LogInformation("Adding piece {Piece}'s possible move: (Row,Column) ({Row},{Column})", piece, jumpRow, jumpColumn);
                    possibleMoves.Add(new Position(jumpRow, jumpColumn));
                }
            }
        }
        
        _logger?.LogInformation("Successfully obtained piece {Piece}'s possible move", piece);
        return possibleMoves;
    }
    
    /// <summary>
    /// Determines whether a piece can move backward.
    /// </summary>
    /// <param name="piece">The piece to check.</param>
    /// <returns>
    /// True if the piece can move backward; otherwise, false.
    /// </returns>
    private bool CanMoveBackward(Piece piece)
    {
        return (piece.Status == PieceStatus.King);
    }
    
    /// <summary>
    /// Determines whether a move to the specified position is valid.
    /// </summary>
    /// <param name="toRow">The target row index.</param>
    /// <param name="toColumn">The target column index.</param>
    /// <returns>
    /// True if the move is valid (within board boundaries and the target position is empty);
    /// otherwise, false.
    /// </returns>
    private bool IsValidMove(int toRow, int toColumn)
    {
        if (!WithinBoundaries(toRow, toColumn))
        {
            _logger?.LogWarning("New position (row,column) ({Row},{Column}) is out of board boundaries", toRow, toColumn);
            return false;
        }

        if (GetPiece(toRow, toColumn) != null)
        {
            return false;
        }
        
        _logger?.LogInformation("Piece can be moved to (row,column) ({Row},{Column})", toRow, toColumn);
        return true;
    }
    
    /// <summary>
    /// Determines whether the specified position is within the board boundaries.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <returns>
    /// True if the position is within the board boundaries; otherwise, false.
    /// </returns>
    private bool WithinBoundaries(int row, int column)
    {
        if ((row >= 0 && row < GetBoardSize()) && (column >= 0 && column < GetBoardSize()))
        {
            _logger?.LogInformation("New position (row,column) ({Row},{Column}) is in board boundaries", row, column);
            return true;
        }
        
        _logger?.LogWarning("New position (row,column) ({Row},{Column}) is out of board boundaries", row, column);
        return false;
    }
    #endregion
    
    #region Move & Promote Piece
    /// <summary>
    /// Moves a piece from the specified source position to the target position.
    /// </summary>
    /// <param name="source">The source position of the piece.</param>
    /// <param name="target">The target position to move the piece to.</param>
    /// <param name="firstMove">Flag indicating whether it's the first move of the piece (default is true).</param>
    /// <returns>
    /// True if the piece is successfully moved; otherwise, false.
    /// </returns>
    public bool MovePiece(Position source, Position target, in bool firstMove = true)
    {
        Piece? piece = GetPiece(source);
        if (piece == null)
        {
            _logger?.LogWarning("Attempt to move piece from {Position} to {Position} failed", source, target);
            return false;
        }
        
        return MovePiece(piece, target, firstMove);
    }
    
    /// <summary>
    /// Moves a specific piece to the target position.
    /// </summary>
    /// <param name="piece">The piece to be moved.</param>
    /// <param name="target">The target position to move the piece to.</param>
    /// <param name="firstMove">Flag indicating whether it's the first move of the piece (default is true).</param>
    /// <returns>
    /// True if the piece is successfully moved; otherwise, false.
    /// </returns>
    public bool MovePiece(Piece piece, Position target, in bool firstMove = true)
    {
        if (!ValidateNewPosition(piece, target, firstMove))
        {
            _logger?.LogWarning("Attempt to move piece {Piece} to {Position} failed", piece, target);
            return false;
        }
        
        Position? source = GetPosition(piece);
        if (source == null)
        {
            _logger?.LogWarning("Attempt to move piece {Piece} to {Position} failed", piece, target);
            return false;
        }

        if (!SetPieceToBoard(piece, target))
        {
            return false;
        }

        if (!RemovePieceFromBoard(source))
        {
            return false;
        }
        
        if (IsJumpMove(source, target))
        {
            CapturePieceInBetween(source, target);
        }
        
        OnMovePiece(piece, target);
        SetGameStatus(GameStatus.OnGoing);
        return true;
    }

    /// <summary>
    /// Validates whether the new position for the piece is valid.
    /// </summary>
    /// <param name="piece">The piece to be moved.</param>
    /// <param name="target">The target position to move the piece to.</param>
    /// <param name="firstMove">Flag indicating whether it's the first move of the piece (default is true).</param>
    /// <returns>
    /// True if the new position is valid; otherwise, false.
    /// </returns>
    private bool ValidateNewPosition(Piece piece, Position target, in bool firstMove = true)
    {
        if (!GetPossibleMoves(piece, firstMove).Any(pos => pos.Row == target.Row && pos.Column == target.Column))
        {
            _logger?.LogWarning("Piece {Piece} can't be moved to {Position}", piece, target);
            return false;
        }
        
        _logger?.LogInformation("Piece {Piece} can be moved to {Position}", piece, target);
        return true;
    }
    
    /// <summary>
    /// Checks if the move is a jump move (capturing an opponent's piece).
    /// </summary>
    /// <param name="source">The source position of the move.</param>
    /// <param name="target">The target position of the move.</param>
    /// <returns>
    /// True if the move is a jump move; otherwise, false.
    /// </returns>
    private bool IsJumpMove(Position source, Position target)
    {
        int deltaRow = target.Row - source.Row;
        int deltaColumn = target.Column - source.Column;

        return (Math.Abs(deltaRow) == 2) && (Math.Abs(deltaColumn) == 2);
    }
    
    /// <summary>
    /// Captures the piece that is in between the source and target positions.
    /// </summary>
    /// <param name="source">The source position of the move.</param>
    /// <param name="target">The target position of the move.</param>
    /// <returns>
    /// True if the piece in between is successfully captured; otherwise, false.
    /// </returns>
    private bool CapturePieceInBetween(Position source, Position target)
    {
        int captureRow = (source.Row + target.Row) / 2;
        int captureColumn = (source.Column + target.Column) / 2;

        Piece? capturedPiece = GetPiece(captureRow, captureColumn);
        if (capturedPiece == null)
        {
            _logger?.LogWarning("Attempt to capture piece between Position {Source} and {Target} failed", source, target);
            return false;
        }

        if (!RemovePiece(capturedPiece))
        {
            return false;
        }
        
        OnCapturePiece(capturedPiece);
        return true;
    }
    
    /// <summary>
    /// Promotes a piece to the king status if it reaches the opposite end of the board.
    /// </summary>
    /// <param name="piece">The piece to be promoted.</param>
    /// <returns>
    /// True if the piece is successfully promoted; otherwise, false.
    /// </returns>
    public bool PromotePiece(Piece piece)
    {
        Position? position = GetPosition(piece);
        if (position == null)
        {
            _logger?.LogWarning("Attempt to promote Piece {Piece} failed", piece);
            return false;
        }
        
        IPlayer? player = GetPlayerByPieces(piece);
        if (player == null)
        {
            _logger?.LogWarning("Attempt to promote Piece {Piece} failed", piece);
            return false;
        }
        
        if (!CanPromotePiece(piece, position))
        {
            _logger?.LogWarning("Attempt to promote Piece {Piece} failed", piece);
            return false;
        }
        
        if (!PromotePieceFromPlayer(player, piece) || !PromotePieceFromBoard(position))
        {
            _logger?.LogWarning("Attempt to promote Piece {Piece} failed", piece);
            return false;
        }
        
        _logger?.LogInformation("Attempt to promote Piece {Piece} is success", piece);
        return true;
    }

    /// <summary>
    /// Checks if a piece can be promoted based on its position.
    /// </summary>
    /// <param name="piece">The piece to be checked for promotion.</param>
    /// <param name="position">The position of the piece on the board.</param>
    /// <returns>
    /// True if the piece can be promoted; otherwise, false.
    /// </returns>
    private bool CanPromotePiece(Piece piece, Position position)
    {
        int endRow = (piece.Color == PieceColor.Blue) ? (GetBoardSize() - 1) : 0;
        if (position.Row != endRow)
        {
            _logger?.LogWarning("Piece {Piece} can't be promoted", piece);
            return false;
        }
        
        _logger?.LogInformation("Piece {Piece} can be promoted", piece);
        return true;
    }

    /// <summary>
    /// Promotes a piece in the player's collection to the king status.
    /// </summary>
    /// <param name="player">The player who owns the piece.</param>
    /// <param name="piece">The piece to be promoted.</param>
    /// <returns>
    /// True if the piece is successfully promoted; otherwise, false.
    /// </returns>
    private bool PromotePieceFromPlayer(IPlayer player, Piece piece)
    {
        Piece? promotedPiece = GetPiece(player, piece.Id);
        if (promotedPiece == null)
        {
            _logger?.LogWarning("Attempt to promote player {Player}'s piece {Piece} failed", player, piece);
            return false;   
        }
        
        promotedPiece.Status = PieceStatus.King;
        OnPromotePiece(promotedPiece);
        return true;
    }
    
    /// <summary>
    /// Promotes a piece on the board to the king status.
    /// </summary>
    /// <param name="position">The position of the piece on the board.</param>
    /// <returns>
    /// True if the piece is successfully promoted; otherwise, false.
    /// </returns>
    private bool PromotePieceFromBoard(Position position)
    {
        return PromotePieceFromBoard(position.Row, position.Column);
    }
    
    /// <summary>
    /// Promotes a piece on the board to the king status.
    /// </summary>
    /// <param name="row">The row index of the piece on the board.</param>
    /// <param name="column">The column index of the piece on the board.</param>
    /// <returns>
    /// True if the piece is successfully promoted; otherwise, false.
    /// </returns>
    private bool PromotePieceFromBoard(int row, int column)
    {
        Piece? piece = GetPiece(row, column);
        if (piece == null)
        {
            _logger?.LogWarning("Attempt to promote player piece from (row,column) ({Row},{Column}) failed", row, column);
            return false;
        }
        
        piece.Status = PieceStatus.King;
        return true;
    }
    #endregion
    
    #region Game Status
    /// <summary>
    /// Sets the game status to the specified status.
    /// </summary>
    /// <param name="status">The new game status to set.</param>
    /// <returns>
    /// True if the game status is successfully set; otherwise, false.
    /// </returns>
    private bool SetGameStatus(GameStatus status)
    {
        if (status == _status)
        {
            _logger?.LogWarning("Attempt to change game status to {Status} failed", status);
            return false;
        }
        
        _status = status;
        OnChangeStatus(status);
        return true;
    }
    
    /// <summary>
    /// Gets the current game status.
    /// </summary>
    /// <returns>The current game status.</returns>
    public GameStatus GetStatus()
    {
        return _status;
    }
    
    /// <summary>
    /// Starts a new game session.
    /// </summary>
    /// <returns>
    /// True if the game session is successfully started; otherwise, false.
    /// </returns>
    public bool Start()
    {
        if (GetActivePlayer().Count() < 2 || _status != GameStatus.NotReady)
        {
            _logger?.LogWarning("Attempt to start new game session failed");
            return false;
        }
            
        _logger?.LogInformation("Attempt to start new game session is success");
        return SetGameStatus(GameStatus.Ready) && SetCurrentPlayer(_players.Keys.First());
    }
    
    /// <summary>
    /// Checks if the game is over by determining if any player has no pieces left.
    /// </summary>
    /// <returns>
    /// True if the game is over; otherwise, false.
    /// </returns>
    public bool GameOver()
    {
        if (GetActivePlayer().FirstOrDefault(player => !GetPieces(player).Any()) != null)
        {
            _logger?.LogInformation("One of the player have no pieces left. Game is over");
            SetGameStatus(GameStatus.GameOver);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Gets the winner of the game.
    /// </summary>
    /// <returns>
    /// The player who is the winner, or null if the game is not over or there is no winner.
    /// </returns>
    public IPlayer? GetWinner()
    {
        if (GameOver())
        {
            IPlayer? winner = GetActivePlayer().FirstOrDefault(player => GetPieces(player).Count() != 0);
            if (winner != null)
            {
                _logger?.LogInformation("The game winner is player {Player}", winner);
                return winner;
            }
        }
        
        _logger?.LogWarning("Attempt to get  game winner is failed");
        return null;
    }
    
    /// <summary>
    /// Advances the game to the next turn.
    /// </summary>
    /// <returns>
    /// True if the turn is successfully advanced; otherwise, false.
    /// </returns>
    public bool NextTurn()
    {
        if (_currentPlayer == null)
        {
            _logger?.LogWarning("Attempt to advances game turn failed");
            return false;
        }
        
        int currentIndex = GetActivePlayer().ToList().IndexOf(_currentPlayer);
        
        int nextIndex = (currentIndex + 1) % GetPlayers().Count;
        
        IPlayer nextPlayer = GetActivePlayer().ElementAt(nextIndex);
        
        _logger?.LogInformation("Attempt to advances game turn is success");
        return SetCurrentPlayer(nextPlayer);
    }
    
    /// <summary>
    /// Allows a player to resign from the game by clearing their pieces.
    /// </summary>
    /// <param name="player">The player who is resigning.</param>
    /// <returns>
    /// True if the player resigns successfully; otherwise, false.
    /// </returns>
    public bool Resign(IPlayer player)
    {
        if (GetStatus() != GameStatus.OnGoing || GetStatus() != GameStatus.Ready || !IsPlayerValid(player))
        {
            _logger?.LogWarning("Attempt to resign by player {Player} failed", player);
            return false;
        }
        
        _players[player].Clear();
        _logger?.LogInformation("Attempt to resign by player {Player} is success", player);
        return true;
    }
    #endregion
}