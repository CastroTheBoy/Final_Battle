// Console formatting
using System.Reflection;

Console.BackgroundColor = ConsoleColor.Black;
Console.ForegroundColor = ConsoleColor.Green;
Console.Title = "The Final Battle";
Console.Clear();

// Code

int playerVictoryCounter = 0;
for (int i = 0; i < 100; i++)
{
    GameManager game = new GameManager(PlayerType.Robot, PlayerType.Robot, "CASTRO");
    if (game.Run()) playerVictoryCounter++;
}

Console.WriteLine(playerVictoryCounter);

public class GameManager
{
    public EntityManager Entities { get; }
    public PlayerType PlayerPartyMode{ get; }
    public PlayerType EnemyPartyMode{ get; }
    private TurnManager _turnManager;
    private Dictionary<int, List<Entity>> _roundEnemyRoster =
        new Dictionary<int, List<Entity>>();
    private int _currentRound = 0;
    private int _maxRounds = 2;

    public GameManager(PlayerType ally, PlayerType enemy, string playerName)
    {
        Entities = new EntityManager(playerName);
        PlayerPartyMode = ally;
        EnemyPartyMode = enemy;
        _turnManager = new TurnManager(this);
        InitializeEnemyRoster();
    }

    public GameManager() : this(
        ConsoleManager.AskForPlayerType(Faction.Ally),
        ConsoleManager.AskForPlayerType(Faction.Enemy),
        ConsoleManager.AskForPlayerName())
    { }
    
    private void InitializeEnemyRoster()
    {
        for (int i = 0; i < 3; i++) 
        {
            switch(i)
            {
                case 0:
                    _roundEnemyRoster[0] = [new Enemy(EnemyType.SKELETON), 
                        new Enemy(EnemyType.SKELETON)];
                    break;
                case 1:
                    _roundEnemyRoster[1] = [new Enemy(EnemyType.SKELETON), 
                        new Enemy(EnemyType.SKELETON)];
                    break;
                case 2:
                    _roundEnemyRoster[2] = [new Enemy(EnemyType.SKELETON), 
                        new Enemy(EnemyType.SKELETON), 
                        new Enemy(EnemyType.UNCODED_ONE)];
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    private void SetUpRoundRoster()
    {
        foreach(Entity e in _roundEnemyRoster[_currentRound])
            Entities.AddEnemy(e);
    }

    private bool IsFinalRound() => _currentRound >= _maxRounds;
    private bool EnemiesCleared() => Entities.GetEnemyParty().Count == 0;
    private bool PlayersCleared() => Entities.GetPlayerParty().Count == 0;

    public bool Run()
    {
        SetUpRoundRoster();
        while (true) {
            Console.WriteLine($"Current round {_currentRound + 1}/" +
                $"{_maxRounds + 1}");
            foreach (Entity entity in Entities.GetPlayerParty())
            {
                _turnManager.ExecuteTurn(entity, PlayerPartyMode);
                if (EnemiesCleared())
                {
                    if (IsFinalRound())
                    {
                        Console.WriteLine("All enemies have been defeated! You Win!");
                        return true;
                    }
                    else
                    {
                        _currentRound++;
                        SetUpRoundRoster();
                        Console.WriteLine($"All enemies have been defeated in " +
                            $"round {_currentRound}.\nProceeding to round " +
                            $"{_currentRound + 1}/{_maxRounds + 1}.");
                    }
                }
            }
            foreach (Entity entity in Entities.GetEnemyParty())
            {
                _turnManager.ExecuteTurn(entity, EnemyPartyMode);
                if (PlayersCleared())
                {
                    Console.WriteLine("You have been slain!\nGAME OVER...");
                    return false;
                }
            }
        }
    }
}

public class TurnManager
{
    public GameManager Game { get; }
    private RobotManager _robotManager;
    private UserManager _userManager;

    public TurnManager(GameManager game)
    {
        Game = game;
        _robotManager = new RobotManager(game.Entities);
        _userManager = new UserManager(game.Entities);
    }

    public void ExecuteTurn(Entity entity, PlayerType type)
    {
        ConsoleManager.Turn(entity.Name);
        switch (type)
        {
            case PlayerType.Robot:
                _robotManager.DoTurn(entity);
                break;
            case PlayerType.User:
                _userManager.DoTurn(entity);
                break;
            default:
                throw new NotSupportedException();
        }
        ConsoleManager.BlankLine();
    }
}

public class RobotManager
{
    private EntityManager _entities { get; }

    public RobotManager(EntityManager entities)
    {
        _entities = entities;
    }

    public void DoTurn(Entity entity)
    {
        IAction newAction = GetBestTurn();
        switch (newAction.GetType().GetInterfaces())
        {
            case Type[] type when type.Contains(typeof(ITargetEnemy)):
                newAction.ExecuteAction(entity, ChooseBestEnemy(entity));
                break;
            case Type[] type when type.Contains(typeof(ITargetSelf)):
                newAction.ExecuteAction(entity, entity);
                break;
            case Type[] x when x.Contains(typeof(ITargetAlly)):
                newAction.ExecuteAction(entity, ChooseBestAlly(entity));
                break;
            default:
                throw new NotSupportedException();
        }
    }

    private Entity ChooseBestEnemy(Entity entity)
    {
        return _entities.GetEnemiesForEntity(entity)[0];
    }
    private Entity ChooseBestAlly(Entity entity)
    {
        return _entities.GetAlliesForEntity(entity)[0];
    }
    private IAction GetBestTurn()
    {
        return ActionFactory.GetAction(typeof(ActionAttack));
    }
}

public class UserManager
{
    private EntityManager _entities { get; }

    public UserManager(EntityManager entities)
    {
        _entities = entities;
    }

    public void DoTurn(Entity entity)
    {
        IAction newAction = ActionFactory.GetAction(ConsoleManager.AskForAction());
        switch (newAction.GetType().GetInterfaces())
        {
            case Type[] type when type.Contains(typeof(ITargetEnemy)):
                newAction.ExecuteAction(entity,
                    ConsoleManager.AskForTargetChoice(
                        _entities.GetEnemiesForEntity(entity)));
                break;
            case Type[] type when type.Contains(typeof(ITargetSelf)):
                newAction.ExecuteAction(entity,entity);
                break;
            case Type[] x when x.Contains(typeof(ITargetAlly)):
                newAction.ExecuteAction(entity,
                    ConsoleManager.AskForTargetChoice(
                        _entities.GetAlliesForEntity(entity)));
                break;
            default:
                throw new NotSupportedException();
        }
    }
}

public class EntityManager
{
    private List<Entity> _playerParty;
    private List<Entity> _enemyParty;
    
    public EntityManager(string playerName)
    {
        _playerParty = new List<Entity>();
        AddAlly(new Player(playerName));
        _enemyParty = new List<Entity>();
    }

    public List<Entity> GetPlayerParty()
    {
        return _playerParty;
    }
    public List<Entity> GetEnemyParty()
    {
        return _enemyParty;
    }
    public List<Entity> GetAlliesForEntity(Entity entity)
    {
        return entity.Faction switch
        {
            Faction.Ally => _playerParty,
            Faction.Enemy => _enemyParty,
            _ => throw new NotImplementedException()
        };
    }
    public List<Entity> GetEnemiesForEntity(Entity entity)
    {
        return entity.Faction switch
        {
            Faction.Ally => _enemyParty,
            Faction.Enemy => _playerParty,
            _ => throw new NotImplementedException()
        };
    }
    public void AddEnemy(Entity entity)
    {
        entity.EntityDied += OnEnemyDeath;
        _enemyParty.Add(entity);
    }
    private void AddAlly(Entity entity)
    {
        entity.EntityDied += OnAllyDeath;
        _playerParty.Add(entity);
    }
    private void OnEnemyDeath(Entity entity) => _enemyParty.RemoveAll(
        x => x.UniqueID == entity.UniqueID);
    private void OnAllyDeath(Entity entity) => _playerParty.RemoveAll(
        x => x.UniqueID == entity.UniqueID);
}

public static class ConsoleManager
{
    public static void Turn(string name)
    {
        Console.WriteLine($"It is {name}'s turn...");
    }
    public static void BlankLine()
    {
        Console.WriteLine();
    }
    public static string AskForPlayerName()
    {
        Console.Write("What is Your name? ");
        string? name;
        do
        {
            name = Console.ReadLine();
        }
        while (name == null);
        return name;
    }   
    public static void GameStart()
    {
        Console.WriteLine("Welcome to the \"Final Battle\".");
    }
    public static PlayerType AskForPlayerType(Faction faction)
    {
        Console.WriteLine($"Who should control the {faction.ToString()} party?");
        foreach (PlayerType i in Enum.GetValues(typeof(PlayerType)))
            Console.WriteLine($"{(int)i} - {i}");
        string? input;
        int enumLength = Enum.GetValues(typeof(PlayerType)).Length;
        while (true)
        {
            input = Console.ReadLine();
            if (int.TryParse(input, out int value))
            {
                if (value >= 0 && value < enumLength)
                    return (PlayerType)value;
                else
                    Console.WriteLine("Please select a valid choice!");
            }
            else
                Console.WriteLine("Please select a valid choice!");
        }
    }
    public static Type AskForAction()
    {
        Console.WriteLine("What do You want to do?");
        foreach (Type i in ActionFactory.ActionTypes)
            Console.WriteLine($"{ActionFactory.ActionTypes.IndexOf(i)} - {i}");
        string? input;
        int enumLength = ActionFactory.ActionTypes.Count;
        while (true)
        {
            input = Console.ReadLine();
            if (int.TryParse(input, out int value))
            {
                if (value >= 0 && value < enumLength)
                    return ActionFactory.ActionTypes[value];
                else
                    Console.WriteLine("Please select a valid choice!");
            }
            else
                Console.WriteLine("Please select a valid choice!");
        }
    }
    public static Entity AskForTargetChoice(List<Entity> targets)
    {
        int i = 0;
        string? input;
        Console.WriteLine("List of possible targets: ");
        foreach (Entity entity in targets)
        {
            Console.WriteLine($"{i} - {entity.Name}");
            i++;
        }
        while (true)
        {
            input = Console.ReadLine();
            if (int.TryParse(input, out int value))
            {
                if (value >= 0 && value < targets.Count)
                    return targets[value];
                else
                    Console.WriteLine("Please select a valid choice!");
            }
            else
                Console.WriteLine("Please select a valid choice!");
        }
    }
}

public interface IAction
{
    public void ExecuteAction(Entity source, Entity target);
}

public interface ITargetEnemy { }
public interface ITargetSelf { }
public interface ITargetAlly { }
public interface ITargetMuliple { }

public class ActionAttack : IAction, ITargetEnemy
{
    public void ExecuteAction(Entity source, Entity target)
    {
        int damage = source.GetDamage();
        string attackName = source.GetDamageString();
        Console.WriteLine($"{source.Name} used {attackName}" +
            $" on {target.Name}");
        Console.WriteLine($"{attackName} dealt {damage} damage to " +
            $"{target.Name}");
        target.DamageHealth(damage);
    }
}

public class ActionNothing : IAction, ITargetSelf
{ 
    public void ExecuteAction(Entity source, Entity target)
    {
        Console.WriteLine($"{source.Name} did NOTHING");
    }
}

public class ActionKill : IAction, ITargetEnemy
{
    public void ExecuteAction(Entity source, Entity target)
    {
        Console.WriteLine($"{target.Name} was disintegrated!");
        target.Kill();
    }
}

public static class ActionFactory
{
    public static List<Type> ActionTypes = new List<Type>();

    static ActionFactory() 
    {
        foreach (Type prodType in Assembly.GetExecutingAssembly().GetTypes()
            .Where(prodType => prodType.GetInterfaces().Contains(typeof(IAction))))
                ActionTypes.Add(prodType);
    }

    public static IAction GetAction(Type type)
    {
        object a_Context = Activator.CreateInstance(type);
        return (IAction)a_Context;
    }

}

public abstract class Entity
{
    public string Name { get; }
    public int HP { get; protected set; }
    public int MaxHP { get; init; }
    public abstract Faction Faction { get; }
    public Guid UniqueID { get; }
    public event Action<Entity> EntityDied;

    public Entity (string name)
    {
        Name = name;
        UniqueID = Guid.NewGuid();
    }

    public abstract string GetDamageString();

    public void DamageHealth(int damage)
    {
        if(this.HP - damage > 0)
        {
            this.HP -= damage;
            DamageMessage();
            return;
        }
        DeathMessage();
        EntityDied.Invoke(this);
    }
    private void DamageMessage()
    {
        Console.WriteLine($"{this.Name} is now at {this.HP} / " +
            $"{this.MaxHP}");
    }
    private void DeathMessage()
    {
        Console.WriteLine($"{this.Name} has been defeated!");
    }

    public abstract int GetDamage();

    public void Kill()
    {
        this.DamageHealth(10000);
    }
}

public class Enemy : Entity
{
    public override Faction Faction { get => Faction.Enemy; }
    public EnemyType Type { get; }
    public Enemy(EnemyType type) : base(type.ToString()) 
    {
        Type = type;
        HP = GetHealth(type);
        MaxHP = HP;
    }

    private static int GetHealth(EnemyType type)
    {
        return type switch
        {
            EnemyType.SKELETON => 5,
            EnemyType.UNCODED_ONE => 15,
            _ => throw new NotImplementedException()
        };
    }

    public override string GetDamageString()
    {
        return Type switch
        {
            EnemyType.SKELETON => "BONE CRUNCH",
            EnemyType.UNCODED_ONE => "UNRAVELING",
            _ => throw new NotImplementedException()
        };
    }

    public override int GetDamage()
    {
        return Type switch
        {
            EnemyType.SKELETON => new Random().Next(2),
            EnemyType.UNCODED_ONE => new Random().Next(3),
            _ => throw new NotImplementedException()
        };
    }
}

public class Player : Entity
{
    public override Faction Faction { get => Faction.Ally; }
    public Player(string name) : base(name) 
    {
        this.HP = 25;
        MaxHP = HP;
    }

    public override int GetDamage()
    {
        return 2;
    }

    public override string GetDamageString()
    {
        return "PUNCH";
    }
}

public enum ActionType { NOTHING, ATTACK }
public enum PlayerType { User, Robot }
public enum Faction { Ally, Enemy }
public enum EnemyType { SKELETON, UNCODED_ONE }