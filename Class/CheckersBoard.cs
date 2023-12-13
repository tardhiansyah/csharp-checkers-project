using CheckersGame.Interface;

namespace CheckersGame.Class;

public class CheckersBoard : IBoard<Piece?[,]>
{
    public int Size { get; }
    
    public Piece?[,] Layout { get; set; }

    public CheckersBoard(int size = 8)
    {
        Size = size;
        Layout = new Piece?[Size, Size];
    }
}