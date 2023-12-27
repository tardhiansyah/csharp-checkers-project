namespace CheckersGame.Class;

public class Position
{
    public int Row { get; }
    public int Column { get; }

    public Position() {}
    public Position(int row, int column)
    {
        Row = row;
        Column = column;
    }
    public override string ToString()
    {
        return $"({Row},{Column})";
    }

    public override int GetHashCode()
    {
        return Row + Column;
    }
}