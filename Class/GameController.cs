using CheckersGame.Enumeration;
using CheckersGame.Interface;

namespace CheckersGame.Class;

public class GameController
{
    private Dictionary<IPlayer, List<Piece>> _playerPieces;
    private IBoard<Piece?[,]> _board = null!;
    private GameStatus _status;
    private IPlayer? _currentPlayer;

    public GameController()
    {
        _playerPieces = new();
        _currentPlayer = null;
    }

    #region Action / Delegate
    public event Action<Piece>? PieceCaptured;
    public event Action<Piece, Position>? PieceMoved;
    public event Action<IPlayer>? PlayerAdded;
    public event Action<IPlayer>? PlayerTurnChanged;
    public event Action<GameStatus>? StatusChanged;

    protected virtual void OnChangeStatus(GameStatus status)
    {
        StatusChanged?.Invoke(status);
    }
    protected virtual void OnChangeTurnPlayer(IPlayer player)
    {
        PlayerTurnChanged?.Invoke(player);
    }
    protected virtual void OnAddPlayer(IPlayer player)
    {
        PlayerAdded?.Invoke(player);
    }
    protected virtual void OnMovePiece(Piece piece, Position position)
    {
        PieceMoved?.Invoke(piece, position);
    }
    protected virtual void OnCapturePiece(Piece piece)
    {
        PieceCaptured?.Invoke(piece);
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
        List<Piece> pieces = new List<Piece>();
        
        int count = 0;
        for (int i = 0; i < quantity; i++)
        {
            Piece piece = new Piece(count + 1, color);
            pieces.Add(piece);
            count++;
        }
        
        return pieces;
    }
    
    /// <summary>
    /// Set a new checkers board to be used.
    /// </summary>
    /// <param name="board">New checkers board.</param>
    /// <returns>Return <c>true</c> if board was set before the game started; otherwise, <c>false</c>.</returns>
    public bool SetBoard(IBoard<Piece?[,]> board)
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

        foreach (IPlayer player in _playerPieces.Keys)
        {
            SetPieceToBoard(player);
        }

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
        int nextRow = 0, pieceCounter = 0;
        int startRow = (_playerPieces[player].First().Color == PieceColor.Blue) ? 0 : (GetBoardSize() - nRowWithPieces);
        
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
        return _playerPieces.Values.SelectMany(playerPiece => playerPiece).ToList();
    }
    
    /// <summary>
    /// Get player's active pieces.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="Piece"/> if player is found; otherwise empty.</returns>
    public IEnumerable<Piece> GetPieces(IPlayer player)
    {
        if (_playerPieces.TryGetValue(player, out List<Piece>? playerPieces))
        {
            return playerPieces;
        }
        return Enumerable.Empty<Piece>();
    }
    
    /// <summary>
    /// Select piece from player based on the piece's ID number.
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
    /// Select piece from player based on their <see cref="Position"/> on the board.
    /// </summary>
    /// <param name="position">Piece's position on the board.</param>
    /// <returns>Return the piece if the piece is on the board; otherwise null.</returns>
    public Piece? GetPiece(Position position)
    {
        return GetPiece(position.Row, position.Column);
    }
    
    /// <summary>
    /// Select piece from player based on their coordinate (Y,X) on the board.
    /// </summary>
    /// <param name="row">Row (Y Coordinate).</param>
    /// <param name="column">Column (X Coordinate).</param>
    /// <returns>Return the piece if the piece is on the board; otherwise null.</returns>
    public Piece? GetPiece(int row, int column)
    {
        return _board.Layout[row, column];
    }
    
    /// <summary>
    /// Remove piece from player and board based on the piece's ID number.
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
        
        return RemovePiece(position.Row, position.Column);
    }
    
    /// <summary>
    /// Remove piece from player and board based on their position on the board.
    /// </summary>
    /// <param name="position">Position of piece that will be removed.</param>
    /// <returns>Return <c>true</c> if piece successfully removed; otherwise, <c>false</c>.</returns>
    public bool RemovePiece(Position position)
    {
        return RemovePiece(position.Row, position.Column);
    }
    
    /// <summary>
    /// Remove piece from player and board based on their coordinate on the board.
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
        return _playerPieces[player].SelectMany(GetPossibleMoves);
    }
    
    /// <summary>
    /// Get all possible move from selected piece position (including standard move and jump move).
    /// </summary>
    /// <param name="piece">Selected piece that will be checked.</param>
    /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="Position"/> for selected piece.</returns>
    public IEnumerable<Position> GetPossibleMoves(Piece piece)
    {
        List<Position> possiblePieceMoves = new List<Position>();
        possiblePieceMoves.AddRange(GetPossibleStandardMoves(piece));
        possiblePieceMoves.AddRange(GetPossibleJumpMoves(piece));
        return possiblePieceMoves;
    }
    
    /// <summary>
    /// Get all standard / single tile move from selected piece position.
    /// </summary>
    /// <param name="piece">Selected piece that will be checked</param>
    /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="Position"/> for selected piece.</returns>
    public IEnumerable<Position> GetPossibleStandardMoves(Piece piece)
    {
        List<Position> possibleStandardMoves = new List<Position>();

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
                
                int toRow = piecePos.Row + row;
                int toColumn = piecePos.Column + column;
                if (IsValidMove(toRow, toColumn))
                    possibleStandardMoves.Add(new Position(toRow, toColumn));
            }
        }
        return possibleStandardMoves;
    }
    
    /// <summary>
    /// Get all possible jump / capture move from selected piece position.
    /// </summary>
    /// <param name="piece">Selected piece that will be checked</param>
    /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="Position"/> for selected piece.</returns>
    public IEnumerable<Position> GetPossibleJumpMoves(Piece piece)
    {
        List<Position> possibleJumpMoves = new List<Position>();

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
                
                int jumpRow = piecePos.Row + 2 * row;
                int jumpColumn = piecePos.Column + 2 * column;
                if (!IsValidMove(jumpRow, jumpColumn))
                    continue;
                
                int enemyRow = piecePos.Row + row;
                int enemyColumn = piecePos.Column + column;
                Piece? enemyPiece = GetPiece(enemyRow, enemyColumn);
                if (enemyPiece == null)
                    continue;
                if (enemyPiece.Color == piece.Color)
                    continue;
                
                possibleJumpMoves.Add(new Position(jumpRow, jumpColumn));
            }
        }
        return possibleJumpMoves;
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
    /// <returns>Return true if piece moved successfully; otherwise, return false.</returns>
    public bool MovePiece(Position source, Position target)
    {
        Piece? piece = GetPiece(source);
        if (piece == null)
            return false;

        return MovePiece(piece, target);
    }
    
    /// <summary>
    ///  /// Move selected piece to new position
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
    /// Compare piece new target position with list of valid or possible position.
    /// </summary>
    /// <param name="piece">Piece that will be moved.</param>
    /// <param name="target">New piece target position.</param>
    /// <param name="firstMove">true if this is the first piece movement, false if this is the second or more jump movements.</param>
    /// <returns>true if new position is possible to be moved on; otherwise, false.</returns>
    private bool ValidateNewPosition(Piece piece, Position target, in bool firstMove = true)
    {
        IEnumerable<Position> possibleMoves = firstMove ? GetPossibleMoves(piece) : GetPossibleJumpMoves(piece);
        
        foreach (var position in possibleMoves)
        {
            if (position.Row == target.Row && position.Column == target.Column)
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// Check if this is single tile move or double tile move (jump / capture move).
    /// </summary>
    /// <param name="source">Piece last position.</param>
    /// <param name="target">Piece new position.</param>
    /// <returns>Return true if this is a jump move; otherwise, false.</returns>
    private bool IsJumpMove(Position source, Position target)
    {
        int deltaRow = target.Row - source.Row;
        int deltaColumn = target.Column - source.Column;

        return (Math.Abs(deltaRow) == 2) && (Math.Abs(deltaColumn) == 2);
    }
    
    /// <summary>
    /// Capture piece that was successfully stepped over by enemy piece.
    /// </summary>
    /// <param name="source">Enemy previous position.</param>
    /// <param name="target">Enemy new position.</param>
    private void CapturePieceInBetween(Position source, Position target)
    {
        int deltaRow = target.Row - source.Row;
        int deltaColumn = target.Column - source.Column;

        int captureRow = source.Row + deltaRow / 2;
        int captureColumn = source.Column + deltaColumn / 2;

        Piece? capturedPiece = GetPiece(captureRow, captureColumn);
        if (capturedPiece == null)
            return;
        
        OnCapturePiece(capturedPiece);
        RemovePiece(captureRow, captureColumn);
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
        IPlayer? matchingPlayer = _playerPieces.Keys.FirstOrDefault(player => _playerPieces[player].Any(piece => piece.Equals(removedPiece)));
        if (matchingPlayer == null)
            return false;

        return _playerPieces[matchingPlayer].Remove(removedPiece);
    }
    
    /// <summary>
    /// Promote <see cref="PieceStatus.Regular"/> piece to <see cref="PieceStatus.King"/>.
    /// </summary>
    /// <param name="piece">Piece that will be promoted.</param>
    /// <returns>Return <c>true</c> if piece successfully promoted; otherwise, <c>false</c>.</returns>
    public bool PromotePiece(Piece piece)
    {
        Position? piecePos = GetPosition(piece);
        if (piecePos == null)
            return false;
        
        int endRow = (GetPiece(piecePos)!.Color == PieceColor.Blue) ? (GetBoardSize() - 1) : 0;
        if (piecePos.Row != endRow)
            return false;

        GetPiece(piecePos)!.Status = PieceStatus.King;
        return PromotePieceFromPlayer(piece);
    }
    
    /// <summary>
    /// Promote selected piece from <see cref="List{T}"/> of <see cref="Piece"/> to <see cref="PieceStatus.King"/>.
    /// </summary>
    /// <param name="promotedPiece">Piece that will be promoted.</param>
    /// <returns>Return <c>true</c> if piece successfully promoted; otherwise, <c>false</c>.</returns>
    private bool PromotePieceFromPlayer(Piece promotedPiece)
    {
        IPlayer? matchingPlayer = _playerPieces.Keys.FirstOrDefault(player => _playerPieces[player].Any(piece => piece.Equals(promotedPiece)));
        if (matchingPlayer == null)
            return false;
         
        Piece? matchingPiece = _playerPieces[matchingPlayer].FirstOrDefault(piece => piece.Equals(promotedPiece));
        if (matchingPiece == null)
            return false;
        
        matchingPiece.Status = PieceStatus.King;
        return true;
    }
    #endregion
    
    
    #region Game Status
    /// <summary>
    /// Change current <see cref="GameStatus"/>.
    /// </summary>
    /// <param name="status">New <see cref="GameStatus"/>.</param>
    /// <returns>Return <c>true</c> if successfully changed the game status; otherwise <c>false</c>.</returns>
    private bool SetGameStatus(GameStatus status)
    {
        if (status == _status)
            return false;
        
        _status = status;
        OnChangeStatus(_status);
        return true;
    }
    
    /// <summary>
    /// Check current game status.
    /// </summary>
    /// <returns>
    /// Returns <see cref="GameStatus.NotReady"/>if game not started;
    /// <see cref="GameStatus.Ready"/> if game successfully started, and waiting for first move;
    /// <see cref="GameStatus.OnGoing"/> if player already take a move; and
    /// <see cref="GameStatus.GameOver"/> if the game session is ended.
    /// </returns>
    public GameStatus GetStatus()
    {
        return _status;
    }
    
    /// <summary>
    /// Start current game session.
    /// </summary>
    /// <returns>
    /// Return <c>true</c> if game successfully started; 
    /// otherwise, <c>false</c> if not enough player or <see cref="GameStatus"/> is already started/on-going.
    /// </returns>
    public bool Start()
    {
        if (_playerPieces.Count < 2)
            return false;
        if (_status != GameStatus.NotReady)
            return false;
        
        return SetGameStatus(GameStatus.Ready) && SetCurrentPlayer(_playerPieces.Keys.First());
    }
    
    /// <summary>
    /// Check if current game session is over (one player runs out of pieces or resigns).
    /// </summary>
    /// <returns>Return <c>true</c> if game is over; otherwise, <c>false</c>.</returns>
    public bool GameOver()
    {
        foreach (IPlayer player in _playerPieces.Keys)
        {
            if (_playerPieces[player].Count == 0)
            {
                SetGameStatus(GameStatus.GameOver);
                break;
            }
        }
        
        return (GetStatus() == GameStatus.GameOver);
    }
    
    /// <summary>
    /// Get winner of current game session.
    /// </summary>
    /// <returns>Return the winning player; otherwise, null if game result is draw.</returns>
    public IPlayer? GetWinner()
    {
        foreach (IPlayer player in _playerPieces.Keys)
        {
            if (_playerPieces[player].Count == 0)
                continue;

            return player;
        }
        return null;
    }
    
    /// <summary>
    /// Change turn to the next player.
    /// </summary>
    /// <returns>Returs <c>true</c> if successfully change player turn; otherwise, <c>false</c></returns>
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
    /// Resign the player from the game.
    /// </summary>
    /// <param name="player">Resigned player.</param>
    /// <returns>Return <c>true</c> if successfully resigned from the game; otherwise <c>false</c>.</returns>
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