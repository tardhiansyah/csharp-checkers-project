using CheckersGame.Interface;

namespace CheckersGame.Class;

public class CheckersBoard<T> : IBoard<T>
{
    public int Size { get; }
    
    public T?[,] Layout { get; set; }

    public CheckersBoard(int size = 8)
    {
        Size = size;
        Layout = new T?[Size, Size];
    }
}