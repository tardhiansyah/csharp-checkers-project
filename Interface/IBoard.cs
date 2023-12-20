using CheckersGame.Class;

namespace CheckersGame.Interface;

public interface IBoard<T>
{
    public int Size { get; }
    T?[,] Layout { get; set; }
}