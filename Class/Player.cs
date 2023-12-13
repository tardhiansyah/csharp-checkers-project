using CheckersGame.Interface;

namespace CheckersGame.Class;

public class Player : IPlayer
{
    public string Name { get; }
    public int Id { get; }

    public Player(int id, string name)
    {
        Id = id;
        Name = name;
    }
}