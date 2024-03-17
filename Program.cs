// Console formatting
Console.Title = "The Final Battle";
Console.Clear();
Console.CursorVisible = false;

// Code
Console.SetWindowSize(120, 30);

Thread uiDrawer = new Thread(UIManager.Draw);
uiDrawer.Start();

GameManager game = new GameManager(PlayerType.User, PlayerType.Robot, "Castro");
game.Run();

public class GameManager
{
    public PlayerType PlayerPartyMode { get; }
    public PlayerType EnemyPartyMode { get; }
    private int _currentRound = 1;
    private int _maxRounds = 3;

    public GameManager(PlayerType ally, PlayerType enemy, string playerName)
    {
        PlayerPartyMode = ally;
        EnemyPartyMode = enemy;

        // Setup parties
        PartyManager.Instance.AddParty("Player", "Adventurers", PlayerType.User);
        PartyManager.Instance.GetParty("Player").AddEntity(EntityFactory.CreatePlayer());
        Entity vin = EntityFactory.CreateVin();
        vin.EquipGear(Weapon.CreateVinsBow());
        PartyManager.Instance.GetParty("Player").AddEntity(vin);
        PartyManager.Instance.GetParty("Player").Invetory.AddConsumable(
            new HealthPotion(), new HealthPotion(), new HealthPotion(), 
            new FirePotion(), new FirePotion(), new FirePotion(), 
            new LightningPotion(), new LightningPotion(), new LightningPotion());
        PartyManager.Instance.GetParty("Player").Invetory.AddConsumable(
            Weapon.CreateExcalibur(), Weapon.CreateWoodenSword());

        PartyManager.Instance.AddParty("Round1", "Band of Skeletons", PlayerType.Robot);
        PartyManager.Instance.GetParty("Round1").AddEntity(EntityFactory.CreateSkeleton(), EntityFactory.CreateSkeleton());

        PartyManager.Instance.AddParty("Round2", "Calcium Avengers", PlayerType.Robot);
        PartyManager.Instance.GetParty("Round2").AddEntity(EntityFactory.CreateSkeleton(), EntityFactory.CreateSkeleton());

        PartyManager.Instance.AddParty("Round3", "Forces of Evil", PlayerType.Robot);
        PartyManager.Instance.GetParty("Round3").AddEntity(EntityFactory.CreateSkeleton(), EntityFactory.CreateSkeleton(), EntityFactory.CreateUncodedOne());
    }

    //public GameManager() : this(
    //    ConsoleManager.AskForPlayerTypeWithHighlight(Faction.Ally),
    //    ConsoleManager.AskForPlayerTypeWithHighlight(Faction.Enemy),
    //    ConsoleManager.AskForPlayerName())
    //{ }

    private bool DoRound(int round)
    {
        BattleManager battle = SetupRoundBattleManager(round);
        return battle.DoBattle().Name == "Adventurers";
    }

    private BattleManager SetupRoundBattleManager(int round)
    {
        switch (round)
        {
            case 1:
                return new BattleManager(
                    PartyManager.Instance.GetParty("Player"),
                    PartyManager.Instance.GetParty("Round1"));
            case 2:
                return new BattleManager(
                    PartyManager.Instance.GetParty("Player"),
                    PartyManager.Instance.GetParty("Round2"));
            case 3:
                return new BattleManager(
                    PartyManager.Instance.GetParty("Player"),
                    PartyManager.Instance.GetParty("Round3"));
            default:
                throw new NotSupportedException();
        }
    }

    public void Run()
    {
        for (; _currentRound <= _maxRounds; _currentRound++)
        {
            if(DoRound(_currentRound))
                UIManager.GameLog.AddEntry($"All enemies have been defeated in round {_currentRound}. Proceeding to round {_currentRound + 1}/{_maxRounds}.");
            else
            {
                UIManager.GameLog.AddEntry("You have been slain! GAME OVER...");
                return;
            }
        }
        UIManager.GameLog.AddEntry("All enemies have been defeated! You Win!");
    }
}

// ENUMS
public enum PlayerType { User, Robot }