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

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Player other = (Player)obj;
        return (Id == other.Id);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}