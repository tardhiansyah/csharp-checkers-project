using CheckersGame.Enumeration;
using CheckersGame.Interface;

namespace CheckersGame.Class;

public class GameController
{
    private Dictionary<IPlayer, List<Piece>> _playerPieces;
    private IBoard<Piece> _board = null!;
    private GameStatus _status;
    private IPlayer? _currentPlayer;
    
    /// <summary>
    /// Create new checkers game instance (manual setup). Manually assign board, players, and pieces later.
    /// </summary>
    public GameController()
    {
        _playerPieces = new();
        _currentPlayer = null;
    }

    /// <summary>
    /// Create new checkers game instance (simple setup).
    /// </summary>
    /// <param name="board">Checkers board.</param>
    /// <param name="players">The players.</param>
    public GameController(IBoard<Piece> board, params IPlayer[] players)
    {
        _board = board;
        _playerPieces = new Dictionary<IPlayer, List<Piece>>();
        
        foreach (var player in players)
        {
            _playerPieces.Add(player, new List<Piece>());
        }

        SetPlayerPieces(_playerPieces.Keys.First(), (List<Piece>)GeneratePieces(PieceColor.Blue, MaxPlayerPieces()));
        SetPlayerPieces(_playerPieces.Keys.Last(), (List<Piece>)GeneratePieces(PieceColor.Red, MaxPlayerPieces()));
    }
    
    #region Action / Delegate
    public event Action<Piece>? PieceCaptured;
    public event Action<Piece, Position>? PieceMoved;
    public event Action<Piece>? PiecePromoted;
    public event Action<IPlayer>? PlayerAdded;
    public event Action<IPlayer>? PlayerTurnChanged;
    public event Action<GameStatus>? StatusChanged;
    
    /// <summary>
    /// Action to be invoked when game status changed.
    /// </summary>
    /// <param name="status">New game status.</param>
    protected virtual void OnChangeStatus(GameStatus status)
    {
        StatusChanged?.Invoke(status);
    }
    
    /// <summary>
    /// Action to be invoked when turn changed.
    /// </summary>
    /// <param name="player">player turn</param>
    protected virtual void OnChangeTurnPlayer(IPlayer player)
    {
        PlayerTurnChanged?.Invoke(player);
    }
    
    /// <summary>
    /// Action to be invoked when there are new player added.
    /// </summary>
    /// <param name="player">New player added.</param>
    protected virtual void OnAddPlayer(IPlayer player)
    {
        PlayerAdded?.Invoke(player);
    }
    
    /// <summary>
    /// Action to be invoked when there are piece moved.
    /// </summary>
    /// <param name="piece">Piece moved.</param>
    /// <param name="position">New piece position.</param>
    protected virtual void OnMovePiece(Piece piece, Position position)
    {
        PieceMoved?.Invoke(piece, position);
    }
    
    /// <summary>
    /// Action to be invoked when there are piece captured.
    /// </summary>
    /// <param name="piece">Piece captured.</param>
    protected virtual void OnCapturePiece(Piece piece)
    {
        PieceCaptured?.Invoke(piece);
    }
    
    /// <summary>
    /// Action to be invoked when there are piece promoted to <see cref="PieceStatus.King"/>.
    /// </summary>
    /// <param name="piece">Piece promoted</param>
    protected virtual void OnPromotePiece(Piece piece)
    {
        PiecePromoted?.Invoke(piece);
    }
    #endregion
    
    #region Get-Set Player
    /// <summary>
    /// Adding new player to the game.
    /// </summary>
    /// <param name="player">New player that implemented <see cref="IPlayer"/></param>
    /// <returns>
    ///     <c>true</c> - if the player is successfully added; otherwise, <c>false</c>.
    /// </returns>
    public bool AddPlayer(IPlayer player)
    {
        if (_playerPieces.TryAdd(player, new List<Piece>()))
        {
            OnAddPlayer(player);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Set current turn player.
    /// </summary>
    /// <param name="player">Player for current turn</param>
    /// <returns>
    ///     <c>Return true</c> if the current turn player was successfully changed; otherwise, <c>false</c>.
    /// </returns>
    public bool SetCurrentPlayer(IPlayer player)
    {
        if (!IsPlayerValid(player))
            return false;
        
        _currentPlayer = player;
        OnChangeTurnPlayer(_currentPlayer);
        return true;
    }
    
    /// <summary>
    /// Get this turn player.
    /// </summary>
    /// <returns>Return this turn player which implements <see cref="IPlayer"/>; otherwise, null if no player have been set for this turn.</returns>
    public IPlayer? GetCurrentPlayer()
    {
        return _currentPlayer;
    }
    
    /// <summary>
    /// Get all active players in the game.
    /// </summary>
    /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="IPlayer"/> which has elements of all active players.</returns>
    public IEnumerable<IPlayer> GetActivePlayer()
    {
        return _playerPieces.Keys;
    }
    
    /// <summary>
    /// Remove all players and their pieces from the game.
    /// </summary>
    public void RemoveAllPlayers()
    {
        _playerPieces.Clear();
    }
    
    /// <summary>
    /// Assign new pieces to players in the game.
    /// </summary>
    /// <param name="player">Players who have been added to the game.</param>
    /// <param name="pieces">A <see cref="List{T}"/> of type <see cref="Piece"/> to be assigned to the player.</param>
    /// <returns>Return <c>true</c> if successfully assigned; otherwise, <c>false</c> if the player not found in the game.</returns>
    public bool SetPlayerPieces(IPlayer player, List<Piece> pieces)
    {
        if (!IsPlayerValid(player))
            return false;
        
        _playerPieces[player] = pieces;
        return true;
    }
    
    /// <summary>
    /// Get all active pieces from all active players.
    /// </summary>
    /// <returns>Returns <see cref="Dictionary{T,T}"/> with key <see cref="IPlayer"/> and value <see cref="IEnumerable{T}"/> of type <see cref="Piece"/>.</returns>
    public Dictionary<IPlayer, List<Piece>> GetPlayerPieces()
    {
        return _playerPieces;
    }
    
    /// <summary>
    /// Get all active pieces from one player.
    /// </summary>
    /// <param name="player">player to be checked.</param>
    /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="Piece"/> if the player found; otherwise, returns empty <see cref="IEnumerable{T}"/>.</returns>
    public IEnumerable<Piece> GetPlayerPieces(IPlayer player)
    {
        if (!IsPlayerValid(player))
            return Enumerable.Empty<Piece>();

        return _playerPieces[player];
    }
    
    /// <summary>
    /// Check if the player is an active player.
    /// </summary>
    /// <param name="player">Player to be checked.</param>
    /// <returns>Return <c>true</c> if player found in the game; otherwise, <c>false</c>.</returns>
    private bool IsPlayerValid(IPlayer player)
    {
        return _playerPieces.ContainsKey(player);
    }
    #endregion
    
    #region Get-Set Pieces & Board
    /// <summary>
    /// Generate new checkers pieces.
    /// </summary>
    /// <param name="color">Color of the piece.</param>
    /// <param name="quantity">Number of piece to be generated.</param>
    /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="Piece"/>.</returns>
    public IEnumerable<Piece> GeneratePieces(PieceColor color, int quantity = 12)
    {
        return Enumerable.Range(1, quantity).Select(count => new Piece(count, color));
    }
    
    /// <summary>
    /// Set a new checkers board to be used.
    /// </summary>
    /// <param name="board">New checkers board.</param>
    /// <returns>Return <c>true</c> if board was set before the game started; otherwise, <c>false</c>.</returns>
    public bool SetBoard(IBoard<Piece> board)
    {
        if (_status != GameStatus.NotReady)
            return false;
        
        _board = board;
        
        return true;
    }
    
    /// <summary>
    /// Get number of tiles in one axis (symmetrical board).
    /// </summary>
    /// <returns>number of tiles in a board axis.</returns>
    public int GetBoardSize()
    {
        return _board.Layout.GetLength(0);
    }
    
    /// <summary>
    /// Get a 2 dimensional array representing piece position on the board.
    /// </summary>
    /// <returns>Returns an <see cref="Array"/> of <see cref="Piece"/> representing it's position on the board.</returns>
    public Piece?[,] GetBoardLayout()
    {
        return _board.Layout;
    }
    
    /// <summary>
    /// Assign position on the board for all pieces owned by both player.
    /// </summary>
    /// <returns>
    /// Return <c>true</c> if all pieces successfully assigned to board; 
    /// otherwise, <c>false</c> if player don't have any pieces or current game status is not <see cref="GameStatus.NotReady"/>.
    /// </returns>
    public bool SetPieceToBoard()
    {
        if (_status != GameStatus.NotReady || _playerPieces.Count == 0)
            return false;
        
        GetActivePlayer().ToList().ForEach(player => SetPieceToBoard(player));
        
        return true;
    }
    
    /// <summary>
    /// Assign position on the board (2D Array) for all pieces owned by player.
    /// </summary>
    /// <param name="player">The Player.</param>
    /// <returns>
    /// Return <c>true</c> if all pieces successfully assigned to board; 
    /// otherwise, <c>false</c> if player don't have any pieces or current game status is not <see cref="GameStatus.NotReady"/>.
    /// </returns>
    public bool SetPieceToBoard(IPlayer player)
    {
        if (_status != GameStatus.NotReady || _playerPieces.Count == 0)
            return false;
        
        int nRowWithPieces = (GetBoardSize() - 2) / 2;
        int startRow = (_playerPieces[player].First().Color == PieceColor.Blue) ? 0 : (GetBoardSize() - nRowWithPieces);
        
        int nextRow = 0, pieceCounter = 0;
        foreach (Piece piece in _playerPieces[player])
        {
            int row = startRow + nextRow;
            int column = (row % 2 == 0) ? pieceCounter * 2 : pieceCounter * 2 + 1;
            
            if (SetPieceToBoard(piece, row, column))
                pieceCounter++;
        
            if (column >= GetBoardSize() - 2 && row < GetBoardSize())
            {
                pieceCounter = 0;
                nextRow++;
            }
        }

        return true;
    }
    
    /// <summary>
    /// Assign piece a position on the board (2D Array).
    /// </summary>
    /// <param name="piece">Piece selected.</param>
    /// <param name="position">New position for the piece.</param>
    /// <returns>Return <c>true</c> if piece successfully assigned to new location; otherwise, <c>false</c> if location is not empty (null).</returns>
    private bool SetPieceToBoard(Piece piece, Position position)
    {
        return SetPieceToBoard(piece, position.Row, position.Column);
    }
    
    /// <summary>
    /// Assign piece a position (coordinate X, Y) on the board (2D Array).
    /// </summary>
    /// <param name="piece">Piece selected.</param>
    /// <param name="row">New row position (Y Coordinate).</param>
    /// <param name="column">New column position (X Coordinate).</param>
    /// <returns>Return <c>true</c> if piece successfully assigned to new location; otherwise, <c>false</c> if location is not empty (null).</returns>
    private bool SetPieceToBoard(Piece piece, int row, int column)
    {
        if (GetPiece(row, column) != null)
            return false;
        
        _board.Layout[row, column] = piece;
        return true;
    }
    
    /// <summary>
    /// Get all player's active pieces.
    /// </summary>
    /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="Piece"/>.</returns>
    public IEnumerable<Piece> GetPieces()
    {
        return _playerPieces.Values.SelectMany(playerPiece => playerPiece);
    }
    
    /// <summary>
    /// Gets the pieces associated with the specified player.
    /// </summary>
    /// <param name="player">The player whose pieces are to be retrieved.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="Piece"/> representing the pieces associated with the player.
    /// If the player is not found, returns an empty collection.
    /// </returns>
    public IEnumerable<Piece> GetPieces(IPlayer player)
    {
        if (_playerPieces.TryGetValue(player, out List<Piece>? pieces))
            return pieces;
        
        return Enumerable.Empty<Piece>();
    }
    
    /// <summary>
    /// Get piece from player <see cref="List{T}"/> of <see cref="Piece"/> based on the piece's ID number.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="id">ID of the piece.</param>
    /// <returns>
    /// Return piece if the piece still on Player's <see cref="List{T}"/> of type <see cref="Piece"/>; otherwise null.</returns>
    public Piece? GetPiece(IPlayer player, int id)
    {
        Piece? selectedPiece = GetPlayerPieces(player).FirstOrDefault(piece => piece.Id == id);
        return selectedPiece;
    }
    
    /// <summary>
    /// Get piece from board based on their <see cref="Position"/> on the board.
    /// </summary>
    /// <param name="position">Piece's position on the board.</param>
    /// <returns>Return the piece if the piece is on the board; otherwise null.</returns>
    public Piece? GetPiece(Position position)
    {
        return GetPiece(position.Row, position.Column);
    }
    
    /// <summary>
    /// Select piece from board based on their coordinate (Y,X) on the board.
    /// </summary>
    /// <param name="row">Row (Y Coordinate).</param>
    /// <param name="column">Column (X Coordinate).</param>
    /// <returns>Return the piece if the piece is on the board; otherwise null.</returns>
    public Piece? GetPiece(int row, int column)
    {
        return _board.Layout[row, column];
    }
    
    /// <summary>
    /// Remove piece from player <see cref="List{T}"/> of <see cref="Piece"/> and board based on the piece's ID number.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="id">ID of the piece.</param>
    /// <returns>Return <c>true</c> if piece successfully removed; otherwise, <c>false</c>.</returns>
    public bool RemovePiece(IPlayer player, int id)
    {
        Piece? piece = GetPiece(player, id);
        if (piece == null)
            return false;

        Position? position = GetPosition(piece);
        if (position == null)
            return false;
        
        return RemovePieceFromBoard(position) && RemovePieceFromPlayer(piece);
    }
    
    /// <summary>
    /// Remove piece from player <see cref="List{T}"/> of <see cref="Piece"/> and board based on their position on the board.
    /// </summary>
    /// <param name="position">Position of piece that will be removed.</param>
    /// <returns>Return <c>true</c> if piece successfully removed; otherwise, <c>false</c>.</returns>
    public bool RemovePiece(Position position)
    {
        return RemovePiece(position.Row, position.Column);
    }
    
    /// <summary>
    /// Remove piece from player <see cref="List{T}"/> of <see cref="Piece"/> and board based on their coordinate on the board.
    /// </summary>
    /// <param name="row">Row (Y Coordinate) of piece that will be removed</param>
    /// <param name="column">Column (X Coordinate) of piece that will be removed</param>
    /// <returns>Return <c>true</c> if piece successfully removed; otherwise, <c>false</c>.</returns>
    public bool RemovePiece(int row, int column)
    {
        Piece? piece = GetPiece(row, column);
        if (piece == null)
            return false;
        
        return RemovePieceFromBoard(row, column) && RemovePieceFromPlayer(piece);
    }
    
    /// <summary>
    /// Remove piece from player based on their <see cref="Position"/> on the board.
    /// </summary>
    /// <param name="position">Piece's position on the board.</param>
    /// <returns>Return <c>true</c> if successfully remove piece from board; otherwise, <c>false</c>.</returns>
    private bool RemovePieceFromBoard(Position position)
    {
        return RemovePieceFromBoard(position.Row, position.Column);
    }
    
    /// <summary>
    /// Remove piece from player based on their coordinate (Y,X) on the board.
    /// </summary>
    /// <param name="row">Row (Y Coordinate).</param>
    /// <param name="column">Column (X Coordinate).</param>
    /// <returns>Return <c>true</c> if successfully remove piece from board; otherwise, <c>false</c>.</returns>
    private bool RemovePieceFromBoard(int row, int column)
    {
        if (GetPiece(row, column) == null)
            return false;
        
        _board.Layout[row, column] = null;
        return true;
    }
    
    /// <summary>
    /// Remove piece from players <see cref="List{T}"/> of <see cref="Piece"/> .
    /// </summary>
    /// <param name="removedPiece">Piece that will be removed.</param>
    /// <returns>Return <c>true</c> if piece successfully removed; otherwise, <c>false</c>.</returns>
    private bool RemovePieceFromPlayer(Piece removedPiece)
    {
        IPlayer? matchingPlayer = GetActivePlayer().FirstOrDefault(player => _playerPieces[player].Any(piece => piece.Equals(removedPiece)));
        if (matchingPlayer == null)
            return false;

        return _playerPieces[matchingPlayer].Remove(removedPiece);
    }
    
    /// <summary>
    /// Count all active pieces (player and enemy combined) on the board.
    /// </summary>
    /// <returns>Return number of active piece.</returns>
    public int CountPieceOnBoard()
    {
        int count = 0;
        for (int i = 0; i < GetBoardSize(); i++)
        {
            for (int j = 0; j < GetBoardSize(); j++)
            {
                if (GetPiece(i, j) != null)
                    count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Calculate the maximum number of pieces on the board for each player.
    /// </summary>
    /// <returns>Maximum number of pieces for each player.</returns>
    public int MaxPlayerPieces()
    {
        return GetBoardSize() * (GetBoardSize() - 2) / 4 + (GetBoardSize() % 2);
    }
    #endregion
    
    #region Check Valid Movement
    /// <summary>
    /// Find piece position on the board.
    /// </summary>
    /// <param name="piece">Piece that will be searched.</param>
    /// <returns>Return <see cref="Position"/> of the piece; Otherwise, null if piece not found on the board.</returns>
    public Position? GetPosition(Piece piece)
    {
        for (int i = 0; i < GetBoardSize(); i++)
        {
            for (int j = 0; j < GetBoardSize(); j++)
            {
                Piece? pieceOnBoard = GetPiece(i, j);
                if (pieceOnBoard == null)
                    continue;
                
                if (pieceOnBoard.Equals(piece))
                    return new Position(i, j);
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Get all possible move from player pieces (including standard move and jump move).
    /// </summary>
    /// <param name="player">Player that will be checked.</param>
    ///<returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="Position"/> for all piece of the player.</returns>
    public IEnumerable<Position> GetPossibleMoves(IPlayer player)
    {
        return _playerPieces[player].SelectMany(piece => GetPossibleMoves(piece));
    }

    /// <summary>
    /// Get all possible move of selected piece .
    /// </summary>
    /// <param name="piece">Selected piece that will be checked</param>
    /// <param name="firstMove">when <c>true</c> all possible move will be checked; otherwise, when <c>false</c> only jump move will be checked</param>
    /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="Position"/> for selected piece.</returns>
    public IEnumerable<Position> GetPossibleMoves(Piece piece, bool firstMove = true)
    {
        List<Position> possibleMoves = new List<Position>();

        Position? piecePos = GetPosition(piece);
        if (piecePos == null)
            return Enumerable.Empty<Position>();

        for (int row = -1; row <= 1; row++)
        {
            for (int column = -1; column <= 1; column++)
            {
                if (row == 0 || column == 0)
                    continue;

                if (!CanMoveBackward(piece))
                {
                    int skippedRow = (piece.Color == PieceColor.Blue) ? -1 : 1;
                    if (row == skippedRow)
                        continue;
                }

                int targetRow = piecePos.Row + row;
                int targetColumn = piecePos.Column + column;
                if (!WithinBoundaries(targetRow, targetColumn))
                    continue;
                
                // Check possible single tile and jump move
                Piece? enemyPiece = GetPiece(targetRow, targetColumn);
                if (enemyPiece == null)
                {
                    if (!firstMove)
                        continue;

                    possibleMoves.Add(new Position(targetRow, targetColumn));
                }
                else if (enemyPiece.Color != piece.Color)
                {
                    int jumpRow = targetRow + row;
                    int jumpColumn = targetColumn + column;
                    if (!IsValidMove(jumpRow, jumpColumn))
                        continue;
                    
                    possibleMoves.Add(new Position(jumpRow, jumpColumn));
                }
            }
        }
        return possibleMoves;
    }
    
    /// <summary>
    /// Check if the piece can move backward.
    /// </summary>
    /// <param name="piece">Selected piece.</param>
    /// <returns>Return <c>true</c> if piece is <see cref="PieceStatus.King"/>; otherwise, <c>false</c> if piece is <see cref="PieceStatus.Regular"/></returns>
    private bool CanMoveBackward(Piece piece)
    {
        return (piece.Status == PieceStatus.King);
    }
    
    /// <summary>
    /// Check if piece can be moved here.
    /// </summary>
    /// <param name="toRow">Y Coordinate that will be checked</param>
    /// <param name="toColumn">X Coordinate that will be checked</param>
    /// <returns></returns>
    private bool IsValidMove(int toRow, int toColumn)
    {
        if (!WithinBoundaries(toRow, toColumn))
            return false;

        return GetPiece(toRow, toColumn) == null;
    }
    
    /// <summary>
    /// Check if new piece coordinate is still on board boundaries.
    /// </summary>
    /// <param name="row">Row (Y Coordinate).</param>
    /// <param name="column">Column (X Coordinate).</param>
    /// <returns></returns>
    private bool WithinBoundaries(int row, int column)
    {
        return (row >= 0 && row < GetBoardSize()) && (column >= 0 && column < GetBoardSize());
    }
    #endregion
    
    #region Move & Promote Piece
    /// <summary>
    /// Move piece to new position.
    /// </summary>
    /// <param name="source">Selected piece position.</param>
    /// <param name="target">Selected piece new position.</param>
    /// <param name="firstMove">true if this is the first piece movement, false if this is the second or more jump movements.</param>
    /// <returns>Return true if piece moved successfully; otherwise, return false.</returns>
    public bool MovePiece(Position source, Position target, in bool firstMove = true)
    {
        Piece? piece = GetPiece(source);
        if (piece == null)
            return false;

        return MovePiece(piece, target, firstMove);
    }
    
    /// <summary>
    /// Move selected piece to new position
    /// </summary>
    /// <param name="piece">Piece that will be moved.</param>
    /// <param name="target">New piece target position.</param>
    /// <param name="firstMove">true if this is the first piece movement, false if this is the second or more jump movements.</param>
    /// <returns>Return true if piece moved successfully; otherwise, return false.</returns>
    public bool MovePiece(Piece piece, Position target, in bool firstMove = true)
    {
        if (!ValidateNewPosition(piece, target, firstMove))
            return false;
        
        Position? source = GetPosition(piece);
        if (source == null)
            return false;

        if (!SetPieceToBoard(piece, target))
            return false;

        if (!RemovePieceFromBoard(source))
            return false;
        
        if (IsJumpMove(source, target))
            CapturePieceInBetween(source, target);
        
        OnMovePiece(piece, target);
        SetGameStatus(GameStatus.OnGoing);
        
        return true;
    }

    /// <summary>
    /// Validates whether the specified <paramref name="target"/> position is a valid move for the given <paramref name="piece"/>.
    /// </summary>
    /// <param name="piece">The piece for which the move is being validated.</param>
    /// <param name="target">The target position to be validated.</param>
    /// <param name="firstMove">A boolean value indicating whether it is the first move of the piece (default is <c>true</c>)</param>
    /// <returns>
    ///   <c>true</c> if the move to the <paramref name="target"/> position is valid; otherwise, <c>false</c>.
    /// </returns>
    private bool ValidateNewPosition(Piece piece, Position target, in bool firstMove = true)
    {
        return GetPossibleMoves(piece, firstMove).Any(position => 
            position.Row == target.Row && position.Column == target.Column);
    }
    
    /// <summary>
    /// Determines whether a move from the specified <paramref name="source"/> to <paramref name="target"/>
    /// is a jump move, involving a piece moving two rows and two columns.
    /// </summary>
    /// <param name="source">The source position from which the capture is initiated.</param>
    /// <param name="target">The target position where the capturing piece moves.</param>
    /// <returns>
    ///   <c>true</c> if the move is a jump move; otherwise, <c>false</c>.
    /// </returns>
    private bool IsJumpMove(Position source, Position target)
    {
        int deltaRow = target.Row - source.Row;
        int deltaColumn = target.Column - source.Column;

        return (Math.Abs(deltaRow) == 2) && (Math.Abs(deltaColumn) == 2);
    }
    
    /// <summary>
    /// Captures a piece located in between the last position and new position.
    /// </summary>
    /// <param name="source">The source position from which the capture is initiated.</param>
    /// <param name="target">The target position where the capturing piece moves.</param>
    /// <returns>
    ///   <c>true</c> if a piece is successfully captured and removed; otherwise, <c>false</c>.
    /// </returns>
    private bool CapturePieceInBetween(Position source, Position target)
    {
        int captureRow = (source.Row + target.Row) / 2;
        int captureColumn = (source.Column + target.Column) / 2;

        Piece? capturedPiece = GetPiece(captureRow, captureColumn);
        if (capturedPiece == null)
            return false;
        
        OnCapturePiece(capturedPiece);
        return RemovePiece(captureRow, captureColumn);
    }
    
    /// <summary>
    /// Promotes a specified <paramref name="piece"/> to the status of a <see cref="PieceStatus.King"/> if it is eligible for promotion.
    /// </summary>
    /// <param name="piece">The piece to be promoted.</param>
    /// <returns>
    ///   <c>true</c> if the piece is successfully promoted; otherwise, <c>false</c>.
    /// </returns>
    public bool PromotePiece(Piece piece)
    {
        if (!CanPromotePiece(piece))
            return false;

        piece.Status = PieceStatus.King;
        return PromotePieceFromPlayer(piece);
    }
    /// <summary>
    /// Determines whether a specified <paramref name="piece"/> is eligible for promotion.
    /// </summary>
    /// <param name="piece">The piece to check for promotion eligibility.</param>
    /// <returns>
    ///   <c>true</c> if the piece is eligible for promotion; otherwise, <c>false</c>.
    /// </returns>
    private bool CanPromotePiece(Piece piece)
    {
        Position? piecePos = GetPosition(piece);
        if (piecePos == null)
            return false;
        
        int endRow = (GetPiece(piecePos)!.Color == PieceColor.Blue) ? (GetBoardSize() - 1) : 0;
        if (piecePos.Row != endRow)
            return false;

        return true;
    }
    /// <summary>
    /// Promotes a specified <paramref name="piece"/> to the status of a king.
    /// </summary>
    /// <param name="piece">The piece to be promoted.</param>
    /// <returns>
    ///   <c>true</c> if the piece is successfully promoted; otherwise, <c>false</c>.
    /// </returns>
    private bool PromotePieceFromPlayer(Piece piece)
    {
        IEnumerable<Piece> pieces = GetPieces();
        Piece? promotedPiece = pieces.FirstOrDefault(playerPiece => playerPiece.Equals(piece));
        if (promotedPiece == null)
            return false;
        
        promotedPiece.Status = PieceStatus.King;
        OnPromotePiece(promotedPiece);
        return true;
    }
    #endregion
    
    #region Game Status
    /// <summary>
    /// Sets the current status of the game to the specified <paramref name="status"/>.
    /// </summary>
    /// <param name="status">The new <see cref="GameStatus"/> to set.</param>
    /// <returns>
    ///   <c>true</c> if the game status is successfully set; otherwise, <c>false</c>.
    /// </returns>
    private bool SetGameStatus(GameStatus status)
    {
        if (status == _status)
            return false;
        
        _status = status;
        OnChangeStatus(status);
        
        return true;
    }
    
    /// <summary>
    /// Retrieves the current status of the game.
    /// </summary>
    /// <returns>
    ///   The current <see cref="GameStatus"/> of the game.
    /// </returns>
    /// <remarks>
    /// The method returns the current status of the game, indicating whether it is NotReady, Ready, OnGoing, or GameOver.
    /// </remarks>
    public GameStatus GetStatus()
    {
        return _status;
    }
    
    /// <summary>
    /// Starts the game, setting it to the ready state and initializing the first player's turn.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if the game is successfully started; otherwise, <c>false</c>.
    /// </returns>
    public bool Start()
    {
        if (_playerPieces.Count < 2 || _status != GameStatus.NotReady)
            return false;
        
        return SetGameStatus(GameStatus.Ready) && SetCurrentPlayer(_playerPieces.Keys.First());
    }
    
    /// <summary>
    /// Checks if the game is over by determining if any player has eliminated all their enemy pieces.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if the game is over; otherwise, <c>false</c>.
    /// </returns>
    public bool GameOver()
    {
        if (_playerPieces.Keys.FirstOrDefault(player => _playerPieces[player].Count == 0) != null)
            return SetGameStatus(GameStatus.GameOver);

        return false;
    }
    
    /// <summary>
    /// Retrieves the player who has won the game by eliminating all enemy pieces.
    /// </summary>
    /// <returns>
    ///   The winning player if the game is over and a winner is found; otherwise, returns <c>null</c>.
    /// </returns>
    public IPlayer? GetWinner()
    {
        if (GameOver())
            return _playerPieces.Keys.FirstOrDefault(player => _playerPieces[player].Count != 0);
        
        return null;
    }
    
    /// <summary>
    /// Advances the game to the next turn, changing the current player.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if the next turn is successfully set; otherwise, <c>false</c>.
    /// </returns>
    public bool NextTurn()
    {
        if (_currentPlayer == null)
            return false;
        
        int currentIndex = _playerPieces.Keys.ToList().IndexOf(_currentPlayer);
        
        int nextIndex = (currentIndex + 1) % _playerPieces.Count;
        
        IPlayer nextPlayer = _playerPieces.Keys.ElementAt(nextIndex);
        
        return SetCurrentPlayer(nextPlayer);
    }
    
    /// <summary>
    /// Allows a player to resign from the game.
    /// </summary>
    /// <param name="player">The player who wants to resign.</param>
    /// <returns>
    ///   <c>true</c> if the player successfully resigns; otherwise, <c>false</c>.
    /// </returns>
    public bool Resign(IPlayer player)
    {
        if (GetStatus() != GameStatus.OnGoing || GetStatus() != GameStatus.Ready)
            return false;

        if (!IsPlayerValid(player))
            return false;
        
        _playerPieces[player].Clear();
        return true;
    }
    #endregion
}