// Console formatting
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

GameManager game = new GameManager();
game.Run();

public static class ConsoleManager
{
    private static int _consoleHeight { get => Console.WindowHeight; }
    private static int _consoleWidth { get => Console.WindowWidth; }
    private static int _logBorderWidth { get => (int)(_consoleWidth - _consoleWidth * 0.3); }
    private static int _actionBorderHeigth { get => (int)(_consoleHeight - _consoleHeight * 0.4); }
    private static int _entityDisplayHeight = 3;
    private static Dictionary<SelectionArea, ConsoleAreaCoordinates> _areaCoords = new Dictionary<SelectionArea, ConsoleAreaCoordinates>();

    static ConsoleManager()
    {
        _areaCoords.Add(SelectionArea.PartyLeft, new ConsoleAreaCoordinates(1, _actionBorderHeigth, 1, (int)(_logBorderWidth / 2)));
        _areaCoords.Add(SelectionArea.PartyRight, new ConsoleAreaCoordinates(1, _actionBorderHeigth, (int)(_logBorderWidth / 2), _logBorderWidth));
        _areaCoords.Add(SelectionArea.ActionColumnOne, new ConsoleAreaCoordinates(_actionBorderHeigth,_consoleHeight,1, (int)(_logBorderWidth / 2)));
        _areaCoords.Add(SelectionArea.ActionColumnTwo, new ConsoleAreaCoordinates(_actionBorderHeigth,_consoleHeight, (int)(_logBorderWidth / 2), _logBorderWidth));
    }

    // Borders
    public static void BuildBorders()
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.CursorVisible = false;
        Console.Clear();
        BuildOuterBorder();
        BuildBattleLogBorder();
        BuildActionBlockBorder();
    }

    private static void BuildOuterBorder(
    char widthBorders = '|', char heightBorder = '-')
    {
        for (int i = 0; i < _consoleWidth; i++)
        {
            for (int j = 0; j < _consoleHeight; j++)
            {
                Console.SetCursorPosition(i, j);
                if (i == 0) { Console.Write(widthBorders); continue; }
                if (i == _consoleWidth - 1)
                { Console.Write(widthBorders); continue; }
                if (j == 0) { Console.Write(heightBorder); continue; }
                if (j == _consoleHeight - 1)
                { Console.Write(heightBorder); continue; }
            }
        }
    }
    private static void BuildBattleLogBorder(
    char heigthBorders = '|')
    {
        Console.SetCursorPosition(_logBorderWidth + 1, 1);
        Console.Write("Battle log:");
        for (int j = 0; j < _consoleHeight; j++)
        {
            Console.SetCursorPosition(_logBorderWidth, j);
            Console.Write(heigthBorders);
        }
    }
    private static void BuildActionBlockBorder(
    char widthBorders = '-')
    {
        Console.SetCursorPosition(2, _actionBorderHeigth + 1);
        Console.Write("Actions:");
        for (int i = 1; i < _logBorderWidth; i++)
        {
            Console.SetCursorPosition(i, _actionBorderHeigth);
            Console.Write(widthBorders);
        }
    }
    public static void Round(int currentRound, int maxRound)
    {
        Console.SetCursorPosition(1, 1);
        Console.Write($"Round: {currentRound}/{maxRound}");
    }
    public static void DisplayItems(SelectionArea area, params string[] items)
    {
        ConsoleAreaCoordinates c = _areaCoords[area];
        for (int i = 0; i < items.Length; i++)
        {
            Console.SetCursorPosition(c.WidthStart, c.HeightStart + i);
            Console.Write($"{items[i]}");
        }
    }
    public static void ClearArea(SelectionArea area)
    {
        ConsoleAreaCoordinates c = _areaCoords[area];
        for (int i = c.HeightStart; i < c.HeightEnd; i++)
            for (int j = c.WidthStart; j < c.WidthEnd; j++)
            {
                Console.SetCursorPosition(j, i);
                Console.Write(" ");
            }
    }

    // Ask for methods

    public static string AskForChoice(SelectionArea area, ConsoleColor highlightColor = ConsoleColor.Yellow, params string[] displayItems)
    {
        int currentAction = 0;
        int previousAction = 0;
        HighlightAction(currentAction, ConsoleColor.Yellow);
        while (true)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.DownArrow:
                    if (currentAction + 1 > ActionFactory.ActionTypes.Count - 1)
                    {
                        currentAction = 0;
                        break;
                    }
                    currentAction++;
                    break;
                case ConsoleKey.UpArrow:
                    if (currentAction - 1 < 0)
                    {
                        currentAction = ActionFactory.ActionTypes.Count - 1;
                        break;
                    }
                    currentAction--;
                    break;
                case ConsoleKey.Enter:
                    HighlightAction(currentAction, ConsoleColor.Black);
                    return ActionFactory.ActionTypes[currentAction];
                default:
                    continue;
            }
            HighlightAction(currentAction, ConsoleColor.Yellow);
            HighlightAction(previousAction, ConsoleColor.Black);
            previousAction = currentAction;
        }
    }

    public static void DisplayList(SelectionArea area)
    {
        switch (area)
        {
            case SelectionArea.PartyLeft:

        }
    }

    public static string AskForPlayerName()
    {
        ConsoleManager.BattleLog.AddMessage("What is Your name? ");
        string? name;
        do
        {
            name = Console.ReadLine();
        }
        while (name == null);
        return name;
    }
    public static PlayerType AskForPlayerTypeWithHighlight(Faction faction)
    {
        ConsoleManager.DisplayUserTypes();
        ConsoleManager.BattleLog.AddMessage($"Who should control the {faction.ToString()} party?");
        int currentType = 0;
        int previousType = 0;
        Array types = Enum.GetValues(typeof(PlayerType));
        ConsoleManager.HighlightUserType(currentType, ConsoleColor.Yellow);
        while (true)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.DownArrow:
                    if (currentType + 1 > types.Length - 1)
                    {
                        currentType = 0;
                        break;
                    }
                    currentType++;
                    break;
                case ConsoleKey.UpArrow:
                    if (currentType - 1 < 0)
                    {
                        currentType = types.Length - 1;
                        break;
                    }
                    currentType--;
                    break;
                case ConsoleKey.Enter:
                    ConsoleManager.HighlightUserType(currentType, ConsoleColor.Black);
                    ConsoleManager.ClearBattleScreen();
                    return (PlayerType)types.GetValue(currentType);
                default:
                    continue;
            }
            ConsoleManager.HighlightUserType(currentType, ConsoleColor.Yellow);
            ConsoleManager.HighlightUserType(previousType, ConsoleColor.Black);
            previousType = currentType;
        }
    }
    public static Type AskForActionWithHighlight()
    {
        ConsoleManager.BattleLog.AddMessage("What do You want to do?");
        int currentAction = 0;
        int previousAction = 0;
        ConsoleManager.HighlightAction(currentAction, ConsoleColor.Yellow);
        while (true)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.DownArrow:
                    if(currentAction + 1 > ActionFactory.ActionTypes.Count - 1)
                    {
                        currentAction = 0;
                        break;
                    }
                    currentAction++;
                    break;
                case ConsoleKey.UpArrow:
                    if (currentAction - 1 < 0)
                    {
                        currentAction = ActionFactory.ActionTypes.Count - 1;
                        break;
                    }
                    currentAction--;
                    break;
                case ConsoleKey.Enter:
                    ConsoleManager.HighlightAction(currentAction, ConsoleColor.Black);
                    return ActionFactory.ActionTypes[currentAction];
                default:
                    continue;
            }
            ConsoleManager.HighlightAction(currentAction, ConsoleColor.Yellow);
            ConsoleManager.HighlightAction(previousAction, ConsoleColor.Black);
            previousAction = currentAction;
        }
    }
    public static Entity AskForTargetChoiceWithHighlight(List<Entity> targets)
    {
        ConsoleManager.BattleLog.AddMessage($"Please select a target.");
        int currentEntity = 0;
        int previousEntity = 0;
        ConsoleManager.HighlightEnemy(targets, currentEntity, ConsoleColor.Yellow);
        while (true)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.DownArrow:
                    if (currentEntity + 1 > targets.Count - 1)
                    {
                        currentEntity = 0;
                        break;
                    }
                    currentEntity++;
                    break;
                case ConsoleKey.UpArrow:
                    if (currentEntity - 1 < 0)
                    {
                        currentEntity = targets.Count - 1;
                        break;
                    }
                    currentEntity--;
                    break;
                case ConsoleKey.Enter:
                    ConsoleManager.HighlightEnemy(targets, currentEntity, ConsoleColor.Black);
                    ConsoleManager.ClearBattleScreen();
                    return targets[currentEntity];
                default:
                    continue;
            }
            ConsoleManager.HighlightEnemy(targets, currentEntity, ConsoleColor.Yellow);
            ConsoleManager.HighlightEnemy(targets, previousEntity, ConsoleColor.Black);
            previousEntity = currentEntity;
        }
    }

    // Basic methods
    public static void Turn(string name) =>
        ConsoleManager.BattleLog.AddMessage($"It is {name}'s turn...");
    public static void BlankLine() =>
        Console.WriteLine();
    public static void GameStart() =>
        ConsoleManager.BattleLog.AddMessage("Welcome to the \"Final Battle\".");

    public static class BattleLog
    {
        public static List<string> _battleLogMessages = new List<string>();
        public static int _battleLogMaxCharacters = (_consoleHeight - 2) * (_consoleWidth - _logBorderWidth - 3);
        public static int _battleLogLineCharCount = (_consoleWidth - _logBorderWidth - 3);

        public static void AddMessage(string input)
        {
            _battleLogMessages.Add(input);
            WriteLog();
        }
        public static void WriteLog()
        {
            ClearLog();
            string? s = null;
            int newLineCounter = 0;
            int currentRowIndex = 0;
            int currentMesage = 0;
            for (int i = GetMinMessageIndex(); i < _battleLogMessages.Count; i++)
            {
                Console.SetCursorPosition(_logBorderWidth + 1, 3 + newLineCounter + currentMesage);
                currentRowIndex = 0;
                s = _battleLogMessages[i];
                for(int c = 0; c < s.Length; c++)
                {
                    if (currentRowIndex > _consoleWidth - _logBorderWidth - 3)
                    {
                        newLineCounter++;
                        Console.SetCursorPosition(_logBorderWidth + 1, 3 + newLineCounter + currentMesage);
                        currentRowIndex = 0;
                    }
                    Console.Write(s[c]);
                    currentRowIndex++;
                }
                newLineCounter++;
                currentMesage++;
            }
        }
        public static void ClearLog()
        {
            for(int i = 2; i < _consoleHeight - 1; i++)
                for(int j = _logBorderWidth + 1; j < _consoleWidth - 1; j++)
                {
                    Console.SetCursorPosition(j,i);
                    Console.Write(" ");
                }
        }
        public static int GetMinMessageIndex()
        {
            int runningTotal = 0;
            for(int i = _battleLogMessages.Count - 1; i >= 0; i--)
            {
                runningTotal += (int)Math.Ceiling(((double)_battleLogMessages[i].Length / (double)_battleLogLineCharCount)) * _battleLogLineCharCount ;
                runningTotal += _battleLogLineCharCount;
                if (runningTotal >= _battleLogMaxCharacters)
                    return i + 1;
            }
            return 0;
        }
    }
}

public record ConsoleAreaCoordinates(int HeightStart, int HeightEnd, int WidthStart, int WidthEnd);

public class GameManager
{
    public EntityManager Entities { get; }
    public PlayerType PlayerPartyMode { get; }
    public PlayerType EnemyPartyMode { get; }
    private GameUserManager _gameUserManager;
    private Dictionary<int, List<Entity>> _roundEnemyRoster =
        new Dictionary<int, List<Entity>>();
    private int _currentRound = 0;
    private int _maxRounds = 2;

    static GameManager()
    {
        Console.CursorVisible = false;
        ConsoleManager.BuildBorders();
    }

    public GameManager(PlayerType ally, PlayerType enemy, string playerName)
    {
        Entities = new EntityManager();
        Entities.AddEntity(new Entity(EntityType.PLAYER, playerName));
        PlayerPartyMode = ally;
        EnemyPartyMode = enemy;
        _gameUserManager = new GameUserManager(Entities);
        InitializeEnemyRoster();
    }

    public GameManager() : this(
        ConsoleManager.AskForPlayerTypeWithHighlight(Faction.Ally),
        ConsoleManager.AskForPlayerTypeWithHighlight(Faction.Enemy),
        ConsoleManager.AskForPlayerName())
    { }
    
    public void InitializeEnemyRoster()
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

    public void SetUpRoundRoster()
    {
        foreach(Entity e in _roundEnemyRoster[_currentRound])
            Entities.AddEntity(e);
    }

    private bool IsFinalRound() => _currentRound >= _maxRounds;
    private bool EnemiesCleared() => Entities.GetEnemyParty().Count == 0;
    private bool PlayersCleared() => Entities.GetPlayerParty().Count == 0;

    public bool Run()
    {
        bool moveToNextRound = false;
        SetUpRoundRoster();
        while (true) {
            moveToNextRound = false;
            foreach (Entity entity in Entities.GetPlayerParty())
            {
                ExecuteTurn(entity, PlayerPartyMode);
                if (EnemiesCleared())
                {
                    if (IsFinalRound())
                    {
                        ConsoleManager.BattleLog.AddMessage("All enemies have been defeated! You Win!");
                        return true;
                    }
                    else
                    {
                        _currentRound++;
                        moveToNextRound = true;
                        SetUpRoundRoster();
                        ConsoleManager.BattleLog.AddMessage($"All enemies have been defeated in " +
                            $"round {_currentRound}. Proceeding to round " +
                            $"{_currentRound + 1}/{_maxRounds + 1}.");
                    }
                }
            }
            if(moveToNextRound) continue;
            foreach (Entity entity in Entities.GetEnemyParty())
            {
                ExecuteTurn(entity, EnemyPartyMode);
                if (PlayersCleared())
                {
                    ConsoleManager.BattleLog.AddMessage("You have been slain! GAME OVER...");
                    return false;
                }
            }
        }
    }

    private void ExecuteTurn(Entity entity, PlayerType type)
    {
        CallTurnUI(entity);
        ConsoleManager.Turn(entity.Name);
        _gameUserManager.DoTurn(entity, type);
    }

    public void CallTurnUI()
    {
        ConsoleManager.ClearBattleScreen();
        ConsoleManager.DisplayEnemyParty(Entities.GetEnemyParty());
        ConsoleManager.DisplayAllyParty(Entities.GetPlayerParty());
        ConsoleManager.DisplayActions();
        ConsoleManager.Round(_currentRound + 1, _maxRounds + 1);
    }
    public void CallTurnUI(Entity entity)
    {
        ConsoleManager.ClearBattleScreen();
        ConsoleManager.DisplayEnemyParty(Entities.GetEnemiesForEntity(entity));
        ConsoleManager.DisplayAllyParty(Entities.GetAlliesForEntity(entity));
        ConsoleManager.DisplayActions();
        ConsoleManager.Round(_currentRound + 1, _maxRounds + 1);
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
            PlayerType.User => ConsoleManager.AskForTargetChoiceWithHighlight(entities),
            _ => throw new NotSupportedException()
        };
    }

    private IAction GetAction(PlayerType type)
    {
        return type switch
        {
            PlayerType.Robot => ActionFactory.GetAction(typeof(ActionAttack)),
            PlayerType.User => ActionFactory.GetAction(ConsoleManager.AskForActionWithHighlight()),
            _ => throw new NotSupportedException()
        };
    }
}
 
public class EntityManager
{
    private Dictionary<Guid, Entity> _entityDict;
    
    public EntityManager()
    {
        _entityDict = new Dictionary<Guid, Entity>();
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
    private void OnEntityDeath(Entity entity)
    {
        _entityDict.Remove(entity.UniqueID);
    }
}

// ACTIONS
public interface IAction
{
    public void ExecuteAction(Entity source, Entity target);
    public string Name();
}

public class ActionAttack : IAction
{
    public void ExecuteAction(Entity source, Entity target)
    {
        int damage = source.GetDamage();
        string attackName = source.GetDamageString();
        ConsoleManager.BattleLog.AddMessage($"{source.Name} used {attackName}" +
            $" on {target.Name}");
        ConsoleManager.BattleLog.AddMessage($"{attackName} dealt {damage} damage to " +
            $"{target.Name}");
        target.DamageHealth(damage);
    }
    public string Name() => "Attack";
}

public class ActionNothing : IAction
{ 
    public void ExecuteAction(Entity source, Entity target) =>
        ConsoleManager.BattleLog.AddMessage($"{source.Name} did NOTHING");

    public string Name() => "Do nothing";
}

public class ActionKill : IAction
{
    public void ExecuteAction(Entity source, Entity target)
    {
        ConsoleManager.BattleLog.AddMessage($"{target.Name} was disintegrated!");
        target.Kill();
    }

    public string Name() => "Kill";
}

public class ActionUseItem
{
    private IConsumable _item;

    public ActionUseItem(Entity target, IConsumable item)
    {
        _item = item;
    }

    public void ExecuteAction(Entity source, Entity target)
    {
        _item.Use(target);
    }
}

//public static class ActionFactory
//{
//    public static List<Type> ActionTypes = new List<Type>();

//    static ActionFactory() 
//    {
//        foreach (Type prodType in Assembly.GetExecutingAssembly().GetTypes()
//            .Where(prodType => prodType.GetInterfaces().Contains(typeof(IAction))))
//                ActionTypes.Add(prodType);
//    }

//    public static IAction GetAction(Type type)
//    {
//        object? a_Context = Activator.CreateInstance(type);
//        if (a_Context == null)
//            throw new ArgumentNullException();
//        return (IAction)a_Context;
//    }
//}

public class BattleManager
{
    public PartyManager PlayerParty { get; }
    public PartyManager EnemyParty { get; }

    public BattleManager()
    {
        PlayerParty = new PartyManager();
        EnemyParty = new PartyManager();
    }

    public void DoAction(IAction action)
    {
        
    }
}

public class PartyManager
{
    public Party Party { get; }
    public Inventory Inventory { get; }

    public PartyManager()
    {
        Party = new Party();
        Inventory = new Inventory();
    }
}

public class Party
{
    private Dictionary<Guid, Entity> _party;

    public Party() => _party = new Dictionary<Guid, Entity>();

    public List<Entity> GetParty() => _party.Values.ToList();

    public void AddEntity(Entity entity)
    {
        entity.EntityDied += OnEntityDeath;
        _party.Add(entity.UniqueID, entity);
    }
    private void OnEntityDeath(Entity entity)
    {
        _party.Remove(entity.UniqueID);
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
    public event Action<Entity> ?EntityDied;

    private Entity (EntityType type, int hp, Faction faction, int maxhp, string name, Guid guid)
    {
        Type = type;
        HP = hp;
        Faction = faction;
        MaxHP = maxhp;
        Name = name;
        UniqueID = guid;
    }

    public Entity (EntityType type)
    {
        Type = type;
        HP = _entityStats[Type].HP;
        Faction = _entityStats[Type].Faction;
        MaxHP = HP;
        Name = type.ToString();
        UniqueID = Guid.NewGuid();
    }
    public Entity (EntityType type, string name)
    {
        Type = type;
        HP = _entityStats[Type].HP;
        Faction = _entityStats[Type].Faction;
        MaxHP = HP;
        Name = name;
        UniqueID = Guid.NewGuid();
    }

    public string GetDamageString() => _entityStats[Type].DamageString;
    public int GetDamage() => _entityStats[Type].Damage.Invoke();

    public void DamageHealth(int damage)
    {
        if(this.HP - damage > 0)
        {
            this.HP -= damage;
            HPAmountMessage();
            return;
        }
        DeathMessage();
        EntityDied?.Invoke(this);
    }
    public void HealHealth(int amount)
    {
        this.HP = Math.Min(this.MaxHP, this.HP + amount);
        HPAmountMessage();
    }

    private void HPAmountMessage()
    {
        ConsoleManager.BattleLog.AddMessage($"{this.Name} is now at {this.HP} / " +
            $"{this.MaxHP}");
    }
    private void DeathMessage() =>
        ConsoleManager.BattleLog.AddMessage($"{this.Name} has been defeated!");
    public void Kill() => this.DamageHealth(10000);
}

public record EntityStats(int HP, Func<int> Damage, string DamageString, Faction Faction);

public class Inventory
{
    private List<IConsumable> _inventory { get; }
    public Inventory() 
    {
        _inventory = new List<IConsumable>();
    }

    public List<IConsumable> GetInventory() => _inventory;

    public void AddItem(IConsumable item) => _inventory.Add(item);

    // Doesn't matter which one is removed, just that a single of that type is removed
    public void RemoveItem(IConsumable item) => _inventory.Remove(item);
}

public interface IConsumable
{
    public string Name { get; }
    public void Use(Entity target);
}

public class HealthPotion : IConsumable
{
    public string Name { get => "Health potion"; }
    private int _healAmount = 10;

    public void Use(Entity target)
    {
        target.HealHealth(_healAmount);
    }
}

public class Bomb : IConsumable
{
    public string Name { get => "Bomb"; }
    private int _damageAmount = 5;
    
    public void Use(Entity target) => target.DamageHealth(_damageAmount);
}

// ENUMS
public enum PlayerType { User, Robot }
public enum Faction { Ally, Enemy }
public enum EntityType { SKELETON, UNCODED_ONE, PLAYER }
public enum SelectionArea { ActionColumnOne, ActionColumnTwo, PartyLeft, PartyRight }