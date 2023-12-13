using CheckersGame.Enumeration;

namespace CheckersGame.Class;

public class Piece
{
    public int Id { get; }
    public PieceColor Color { get; }
    public PieceStatus Status { get; set; }

    public Piece(int id, PieceColor color)
    {
        Status = PieceStatus.Regular;
        Color = color;
        Id = id;
    }
}