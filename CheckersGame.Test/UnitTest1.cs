using CheckersGame.Class;
using CheckersGame.Enumeration;
using CheckersGame.Interface;

namespace CheckersGame.Test;

public class GameControllerTest
{
    private GameController _gameController = null!;
    
    [SetUp]
    public void Setup()
    {
        _gameController = new GameController();
    }

    [Test]
    public void AddPlayer_ShouldReturnTrue()
    {
        //Arrange
        IPlayer player = new Player(1, "Robert");

        //Act
        bool result = _gameController.AddPlayer(player);

        //Assert
        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    public void AddPlayer_ShouldReturnFalse()
    {
        //Arrange
        IPlayer player1 = new Player(1, "Robert");
        IPlayer player2 = new Player(1, "Andi");

        //Act
        _gameController.AddPlayer(player1);
        bool result = _gameController.AddPlayer(player2);

        //Assert
        Assert.That(result, Is.EqualTo(false));
    }
    
    [Test]
    public void GetPlayers_ShouldReturnDictionaryOfPlayer()
    {
        //Arrange
        IPlayer player1 = new Player(1, "Robert");
        Dictionary<IPlayer, HashSet<Piece>> compares = new Dictionary<IPlayer, HashSet<Piece>>();
        
        //Act
        compares.Add(player1, new HashSet<Piece>());
        _gameController.AddPlayer(player1);
        
        //Assert
        Assert.That(_gameController.GetPlayers(), Is.EqualTo(compares));
    }
    
    [Test]
    public void GetPlayers_ShouldReturnNull()
    {
        //Arrange
        Dictionary<IPlayer, HashSet<Piece>> players = new Dictionary<IPlayer, HashSet<Piece>>();
        
        //Act
        
        //Assert
        Assert.That(_gameController.GetPlayers(), Is.EqualTo(players));
    }
    
    [TestCase(PieceColor.Red, 4, PieceColor.Red, 4)]
    [TestCase(PieceColor.Blue, 4, PieceColor.Blue, 4)]
    public void GeneratePieces_ShouldCorrectColorAndQuantity_TestCase(PieceColor color, int quantity,
        PieceColor expectedColor, int expectedQuantity)
    {
        //Arrange
        
        //Act
        List<Piece> pieces = _gameController.GeneratePieces(color, quantity).ToList();
        
        //Assert
        Assert.That(pieces.Count, Is.EqualTo(expectedQuantity));
        Assert.That(pieces.First().Color, Is.EqualTo(expectedColor));
        Assert.That(pieces.Last().Color, Is.EqualTo(expectedColor));
    }
}