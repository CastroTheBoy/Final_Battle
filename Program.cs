// Console formatting
using System.Reflection;

Console.BackgroundColor = ConsoleColor.Black;
Console.ForegroundColor = ConsoleColor.Green;
Console.Title = "The Final Battle";
Console.Clear();

// Code

/*int playerVictoryCounter = 0;
for (int i = 0; i < 100; i++)
{
    GameManager game = new GameManager(PlayerType.Robot, PlayerType.Robot, "CASTRO");
    if (game.Run()) playerVictoryCounter++;
}

Console.WriteLine(playerVictoryCounter);*/

//GameManager game = new GameManager();
//game.Run();

public class GameManager
{
    public EntityManager Entities { get; }
    public PlayerType PlayerPartyMode{ get; }
    public PlayerType EnemyPartyMode{ get; }
    private GameUserManager _gameUserManager;
    private Dictionary<int, List<Entity>> _roundEnemyRoster =
        new Dictionary<int, List<Entity>>();
    private int _currentRound = 0;
    private int _maxRounds = 2;

    public GameManager(PlayerType ally, PlayerType enemy, string playerName)
    {
        Entities = new EntityManager(playerName);
        PlayerPartyMode = ally;
        EnemyPartyMode = enemy;
        _gameUserManager = new GameUserManager(Entities);
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
                    _roundEnemyRoster[0] = [new Entity(EntityType.SKELETON), 
                        new Entity(EntityType.SKELETON)];
                    break;
                case 1:
                    _roundEnemyRoster[1] = [new Entity(EntityType.SKELETON), 
                        new Entity(EntityType.SKELETON)];
                    break;
                case 2:
                    _roundEnemyRoster[2] = [new Entity(EntityType.SKELETON), 
                        new Entity(EntityType.SKELETON), 
                        new Entity(EntityType.UNCODED_ONE)];
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    private void SetUpRoundRoster()
    {
        foreach(Entity e in _roundEnemyRoster[_currentRound])
            Entities.AddEntity(e);
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
                ExecuteTurn(entity, PlayerPartyMode);
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
                ExecuteTurn(entity, EnemyPartyMode);
                if (PlayersCleared())
                {
                    Console.WriteLine("You have been slain!\nGAME OVER...");
                    return false;
                }
            }
        }
    }

    private void ExecuteTurn(Entity entity, PlayerType type)
    {
        ConsoleManager.Turn(entity.Name);
        _gameUserManager.DoTurn(entity, type);
        ConsoleManager.BlankLine();
    }
}

public class GameUserManager
{
    private EntityManager _entities { get; }

    public GameUserManager(EntityManager entities) => _entities = entities;

    public void DoTurn(Entity entity, PlayerType playerType)
    {
        IAction newAction = GetAction(playerType);
        switch (newAction.GetType().GetInterfaces())
        {
            case Type[] type when type.Contains(typeof(ITargetEnemy)):
                newAction.ExecuteAction(entity, EntitySelection(
                    _entities.GetEnemiesForEntity(entity), playerType));
                break;
            case Type[] type when type.Contains(typeof(ITargetSelf)):
                newAction.ExecuteAction(entity, entity);
                break;
            case Type[] type when type.Contains(typeof(ITargetAlly)):
                newAction.ExecuteAction(entity, EntitySelection(
                    _entities.GetAlliesForEntity(entity), playerType));
                break;
            default:
                throw new NotSupportedException();
        }
    }
    private Entity EntitySelection(List<Entity> entities, PlayerType type)
    {
        return type switch
        {
            PlayerType.Robot => entities[0],
            PlayerType.User => ConsoleManager.AskForTargetChoice(entities),
            _ => throw new NotSupportedException()
        };
    }

    private IAction GetAction(PlayerType type)
    {
        return type switch
        {
            PlayerType.Robot => ActionFactory.GetAction(typeof(ActionAttack)),
            PlayerType.User => ActionFactory.GetAction(ConsoleManager.AskForAction()),
            _ => throw new NotSupportedException()
        };
    }
}

public class EntityManager
{
    private Dictionary<Guid, Entity> _entityDict;
    
    public EntityManager(string playerName)
    {
        _entityDict = new Dictionary<Guid, Entity>();
        AddEntity(new Entity(EntityType.PLAYER));
    }

    public List<Entity> GetPlayerParty() => GetFactionParty(Faction.Ally);
    public List<Entity> GetEnemyParty() => GetFactionParty(Faction.Enemy);
    
    private List<Entity> GetFactionParty(Faction faction)
    {
        List<Entity> list = new List<Entity>();
        foreach (KeyValuePair<Guid, Entity> kvp in _entityDict)
            if (kvp.Value.Faction == faction)
                list.Add(kvp.Value);
        return list;
    }
    public List<Entity> GetAlliesForEntity(Entity entity)
    {
        return entity.Faction switch
        {
            Faction.Ally => GetPlayerParty(),
            Faction.Enemy => GetEnemyParty(),
            _ => throw new NotImplementedException()
        };
    }
    public List<Entity> GetEnemiesForEntity(Entity entity)
    {
        return entity.Faction switch
        {
            Faction.Ally => GetEnemyParty(),
            Faction.Enemy => GetPlayerParty(),
            _ => throw new NotImplementedException()
        };
    }
    public void AddEntity(Entity entity)
    {
        entity.EntityDied += OnEntityDeath;
        _entityDict.Add(entity.UniqueID, entity);
    }
    private void OnEntityDeath(Entity entity) => 
        _entityDict.Remove(entity.UniqueID);
}

public static class ConsoleManager
{
    public static void Turn(string name) => 
        Console.WriteLine($"It is {name}'s turn...");
    public static void BlankLine() =>
        Console.WriteLine();
    public static void GameStart() =>
        Console.WriteLine("Welcome to the \"Final Battle\".");
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

// ACTIONS
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
    public void ExecuteAction(Entity source, Entity target) => 
        Console.WriteLine($"{source.Name} did NOTHING");
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

// ENTITY
public class Entity
{
    private static Dictionary<EntityType, EntityStats> _entityStats = 
        new Dictionary<EntityType, EntityStats>();
    static Entity()
    {
        _entityStats.Add(EntityType.PLAYER, 
            new EntityStats(25, () => 2, "PUNCH", Faction.Ally));
        _entityStats.Add(EntityType.SKELETON, 
            new EntityStats(5, () => new Random().Next(2), "BONE CRUNCH", Faction.Enemy));
        _entityStats.Add(EntityType.UNCODED_ONE, 
            new EntityStats(10, () => new Random().Next(3), "UNRAVEL", Faction.Enemy));
    }

    public string Name { get; }
    public int HP { get; private set; }
    public int MaxHP { get; }
    public Faction Faction { get; }
    public EntityType Type { get; }
    public Guid UniqueID { get; }
    public event Action<Entity> EntityDied;

    public Entity (EntityType type)
    {
        Type = type;
        HP = _entityStats[Type].HP;
        Faction = _entityStats[Type].Faction;
        MaxHP = HP;
        Name = type.ToString();
        UniqueID = Guid.NewGuid();
    }

    public string GetDamageString() => _entityStats[Type].DamageString;
    public int GetDamage() => _entityStats[Type].Damage.Invoke();

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
    private void DeathMessage() =>
        Console.WriteLine($"{this.Name} has been defeated!");
    public void Kill() => this.DamageHealth(10000);
}

public record EntityStats(int HP, Func<int> Damage, string DamageString, Faction Faction);

// ENUMS
public enum PlayerType { User, Robot }
public enum Faction { Ally, Enemy }
public enum EntityType { SKELETON, UNCODED_ONE, PLAYER }