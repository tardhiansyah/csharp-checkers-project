using CheckersGame.Enumeration;

namespace CheckersGame.Class;

public class Piece
{
    public int Id { get; }
    public PieceColor Color { get; }
    public PieceStatus Status { get; set; }

    public Piece(int id, PieceColor color, PieceStatus status = PieceStatus.Regular)
    {
        Status = status;
        Color = color;
        Id = id;
    }
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Piece other = (Piece)obj;
        return (Id == other.Id) && (Color == other.Color);
    }
    public override int GetHashCode()
    {
        return Id;
    }

    public override string ToString()
    {
        return $"{Id}-{Status}";
    }
}