using CheckersGame.Interface;

namespace CheckersGame.Class;

public class Player : IPlayer
{
    public string NickName { get; }
    public int Id { get; }

    public Player(int id, string nickName)
    {
        Id = id;
        NickName = nickName;
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

    public override string ToString()
    {
        return $"{NickName} ({Id})";
    }
}