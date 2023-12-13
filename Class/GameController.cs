using System.Diagnostics;
using CheckersGame.Enumeration;
using CheckersGame.Interface;

namespace CheckersGame.Class;

public class GameController
{
    private Dictionary<IPlayer, List<Piece>> _playerPieces = new();
    private IBoard<Piece?[,]> _board = null!;
    private GameStatus _status;
    private IPlayer _currentPlayer = new Player(0, "dummy");

    #region Get-Set Player
    private bool IsPlayerValid(IPlayer player)
    {
        return _playerPieces.ContainsKey(player);
    }
    public bool AddPlayer(IPlayer player)
    {
        return _playerPieces.TryAdd(player, new List<Piece>());
    }
    public bool SetPlayerPieces(IPlayer player, List<Piece> pieces)
    {
        if (!IsPlayerValid(player))
            return false;
        
        _playerPieces[player] = pieces;
        return true;
    }
    public Dictionary<IPlayer, List<Piece>> GetPlayerPieces()
    {
        return _playerPieces;
    }

    public IEnumerable<Piece> GetPlayerPieces(IPlayer player)
    {
        return _playerPieces[player];
    }
    
    public bool SetCurrentPlayer(IPlayer player)
    {
        if (!IsPlayerValid(player))
            return false;

        _currentPlayer = player;
        return true;
    }
    public IPlayer GetCurrentPlayer()
    {
        return _currentPlayer;
    }
    public IEnumerable<IPlayer> GetActivePlayer()
    {
        return _playerPieces.Keys;
    }
    public void RemoveAllPlayers()
    {
        _playerPieces.Clear();
    }
    #endregion
    
    #region Get-Set Pieces & Board
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
        
        Debug.WriteLine($"{count} {color} pieces created.", "Information");
        return pieces;
    }
    public bool SetBoard(IBoard<Piece?[,]> board)
    {
        if (_status != GameStatus.NotReady)
            return false;
        
        _board = board;
        return true;
    }
    public int GetBoardSize()
    {
        return _board.Layout.GetLength(0);
    }
    public Piece?[,] GetBoardLayout()
    {
        return _board.Layout;
    }
    public bool SetPieceToBoard()
    {
        if (_status != GameStatus.NotReady || _playerPieces.Count == 0)
            return false;
        
        foreach (IPlayer player in _playerPieces.Keys)
            SetPieceToBoard(player);

        return true;
    }
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

            _board.Layout[row, column] = piece;
            pieceCounter++;

            if (column >= GetBoardSize() - 2 && row < GetBoardSize())
            {
                pieceCounter = 0;
                nextRow++;
            }
        }

        return true;
    }
    public IEnumerable<Piece> GetPieceOnBoard()
    {
        return _playerPieces.Values.SelectMany(playerPiece => playerPiece).ToList();
    }
    public IEnumerable<Piece> GetPieceOnBoard(IPlayer player)
    {
        if (_playerPieces.TryGetValue(player, out List<Piece>? playerPieces))
        {
            return playerPieces;
        }
        return Enumerable.Empty<Piece>();
    }
    public int CountPieceOnBoard()
    {
        int count = 0;
        for (int i = 0; i < GetBoardSize(); i++)
        {
            for (int j = 0; j < GetBoardSize(); j++)
            {
                if (_board.Layout[i , j] != null)
                    continue;
                count++;
            }
        }
        return count;
    }
    public int MaxPlayerPieces()
    {
        return GetBoardSize() * (GetBoardSize() - 2) / 4 + (GetBoardSize() % 2);
    }
    #endregion
    
    #region Check Valid Movement
    public Position? GetPosition(Piece piece)
    {
        for (int i = 0; i < GetBoardSize(); i++)
        {
            for (int j = 0; j < GetBoardSize(); j++)
            {
                if (_board.Layout[i, j] == null)
                    continue;
                
                if (_board.Layout[i, j]!.Color != piece.Color)
                    continue;
                
                if (_board.Layout[i, j]!.Id != piece.Id)
                    continue;
                
                return new Position(i, j);
            }
        }
        
        return null;
    }
    public Piece? GetPiece(Position position)
    {
        return GetPiece(position.Row, position.Column);
    }
    public Piece? GetPiece(int row, int column)
    {
        return _board.Layout[row, column];
    }
    public Piece? GetPiece(IPlayer player, int id)
    {
        Piece? selectedPiece = null;
        foreach (var piece in GetPlayerPieces(player))
        {
            if (piece.Id == id)
            {
                selectedPiece = piece;
                return selectedPiece;
            }
        }

        return selectedPiece;
    }
    public IEnumerable<Position> GetPossibleMoves(IPlayer player)
    {
        return _playerPieces[player].SelectMany(GetPossibleMoves);
    }
    public IEnumerable<Position> GetPossibleMoves(Piece piece)
    {
        List<Position> possiblePieceMoves = new List<Position>();
        possiblePieceMoves.AddRange(GetPossibleStandardMoves(piece));
        possiblePieceMoves.AddRange(GetPossibleJumpMoves(piece));
        return possiblePieceMoves;
    }
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

                // Possible Standard Move
                int toRow = piecePos.Row + row;
                int toColumn = piecePos.Column + column;
                if (CanMove(toRow, toColumn))
                {
                    possibleStandardMoves.Add(new Position(toRow, toColumn));
                }
            }
        }
        return possibleStandardMoves;
    }
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

                // Possible Standard Move
                int enemyRow = piecePos.Row + row;
                int enemyColumn = piecePos.Column + column;
                int jumpRow = piecePos.Row + 2 * row;
                int jumpColumn = piecePos.Column + 2 * column;
                
                if (!CanMove(jumpRow, jumpColumn))
                    continue;
                if (_board.Layout[enemyRow, enemyColumn] == null)
                    continue;
                if (_board.Layout[enemyRow, enemyColumn]!.Color == piece.Color)
                    continue;
                
                possibleJumpMoves.Add(new Position(jumpRow, jumpColumn));
            }
        }
        return possibleJumpMoves;
    }
    private bool CanMoveBackward(Piece piece)
    {
        return (piece.Status == PieceStatus.King);
    }
    private bool CanMove(int toRow, int toColumn)
    {
        if (!WithinBoundaries(toRow, toColumn))
            return false;

        return GetPiece(toRow, toColumn) == null;
    }
    private bool WithinBoundaries(int row, int column)
    {
        return (row >= 0 && row < GetBoardSize()) && (column >= 0 && column < GetBoardSize());
    }
    #endregion
    
    #region Move & Promote Piece
    public bool MovePiece(Position source, Position target)
    {
        Piece? piece = GetPiece(source);
        if (piece == null)
            return false;

        return MovePiece(piece, target);
    }

    public bool MovePiece(Piece piece, Position target, bool firstMove = true)
    {
        if (!IsNewPositionValid(piece, target, firstMove))
            return false;
        
        Position? source = GetPosition(piece);
        if (source == null)
            return false;
        
        if (IsCaptureMove(source, target))
            CapturePieceInBetween(source, target);

        _board.Layout[source.Row, source.Column] = null;
        _board.Layout[target.Row, target.Column] = piece;
        SetGameStatus(GameStatus.OnGoing);
        return true;
    }
    public bool PromotePiece(Piece piece)
    {
        Position? piecePos = GetPosition(piece);
        if (piecePos == null)
            return false;
        
        int endRow = (_board.Layout[piecePos.Row, piecePos.Column]!.Color == PieceColor.Blue) ? GetBoardSize() - 1 : 0;
        if (piecePos.Row != endRow)
            return false;

        _board.Layout[piecePos.Row, piecePos.Column]!.Status = PieceStatus.King;
        PromotePieceFromPlayers(piece);
        return true;
    }
    private bool IsNewPositionValid(Piece piece, Position target, bool firstMove = true)
    {
        IEnumerable<Position> possibleMoves = firstMove ? GetPossibleMoves(piece) : GetPossibleJumpMoves(piece);
        
        foreach (var position in possibleMoves)
        {
            if (position.Row == target.Row && position.Column == target.Column)
                return true;
        }
        return false;
    }
    private bool IsCaptureMove(Position source, Position target)
    {
        int deltaRow = target.Row - source.Row;
        int deltaColumn = target.Column - source.Column;

        return (Math.Abs(deltaRow) == 2) && (Math.Abs(deltaColumn) == 2);
    }

    private void CapturePieceInBetween(Position source, Position target)
    {
        int deltaRow = target.Row - source.Row;
        int deltaColumn = target.Column - source.Column;

        int captureRow = source.Row + deltaRow / 2;
        int captureColumn = source.Column + deltaColumn / 2;

        Piece? capturedPiece = GetPiece(captureRow, captureColumn);
        if (!RemovePieceFromPlayers(capturedPiece!))
            return;
        
        _board.Layout[captureRow, captureColumn] = null;
    }
    private bool RemovePieceFromPlayers(Piece removedPiece)
    {
        foreach (IPlayer player in _playerPieces.Keys)
        {
            foreach (Piece piece in _playerPieces[player])
            {
                if (piece.Color != removedPiece.Color)
                    break;
                
                if (piece.Id != removedPiece.Id)
                    continue;
                
                return _playerPieces[player].Remove(piece);
            }
        }
        return false;
    }
    private void PromotePieceFromPlayers(Piece promotedPiece)
    {
        foreach (IPlayer player in _playerPieces.Keys)
        {
            foreach (Piece piece in _playerPieces[player])
            {
                if (piece.Color != promotedPiece.Color)
                    break;
                
                if (piece.Id != promotedPiece.Id)
                    continue;

                piece.Status = PieceStatus.King;
            }
        }
    }
    #endregion
    
    #region Game Status
    private bool SetGameStatus(GameStatus status)
    {
        if (status == _status)
            return false;
        
        _status = status;
        return true;
    }
    public GameStatus GetStatus()
    {
        return _status;
    }
    public bool Start()
    {
        if (_playerPieces.Count < 2)
            return false;
        if (_status != GameStatus.NotReady)
            return false;
        
        return SetGameStatus(GameStatus.Ready) && SetCurrentPlayer(_playerPieces.Keys.First());
    }
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
        
        return (_status == GameStatus.GameOver);
    }
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
    public bool NextTurn()
    {
        int currentIndex = _playerPieces.Keys.ToList().IndexOf(_currentPlayer);
        
        int nextIndex = (currentIndex + 1) % _playerPieces.Count;
        
        IPlayer nextPlayer = _playerPieces.Keys.ElementAt(nextIndex);
        
        return SetCurrentPlayer(nextPlayer);
    }
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