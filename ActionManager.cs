public interface IActionManager
{
    public void DoAction(Entity entity);
}

public class RobotActionManager : IActionManager
{
    private BattleManager _battle;

    public RobotActionManager(BattleManager battleManager)
    {
        _battle = battleManager;
    }

    public void DoAction(Entity entity)
    {
        Attack(entity, _battle.GetEnemyParty(entity).Entities[0]);
    }

    private void Attack(Entity attacker, Entity target)
    {
        int damage = attacker.GetDamage();
        ConsoleCharacterBuilder builder = new();
        builder.AddString(attacker.Name);
        builder.AddString($" used {attacker.AttackName} ");
        builder.AddString(target.Name);
        builder.AddString(" took ");
        builder.AddString($"{damage}", foreground: ConsoleColor.Red);
        builder.AddString(" damage.");
        UIManager.GameLog.AddEntry(builder.Build());
        target.DamageHealth(damage);
    }
}

public class PlayerActionManager : IActionManager
{
    private BattleManager _battle;

    public PlayerActionManager(BattleManager battle)
    {
        _battle = battle;
    }

    public void DoAction(Entity entity)
    {
        while (true)
        {
            int action = UIListSelector.SelectFromList(UIManager.ActionList);
            switch (action)
            {
                case 0:
                    if (Attack(entity, _battle.SelectFromEnemies(entity)))
                        return;
                    break;
                case 1:
                    if (Kill(_battle.SelectFromEnemies(entity)))
                        return;
                    break;
                case 2:
                    if (UseItem(entity, _battle.GetAllyParty(entity), _battle.GetEnemyParty(entity)))
                        return;
                    break;
                case 3:
                    if (EquipGear(entity, _battle.GetAllyParty(entity)))
                        return;
                    break;
                case 4:
                    if (UseGear(entity, _battle.GetAllyParty(entity), _battle.GetEnemyParty(entity)))
                        return;
                    break;
                default:
                    throw new NotSupportedException("Unsupported action index in ActionManager.DoAction()");
            }
        }
    }

    private bool Attack(Entity attacker, Entity target)
    {
        int damage = attacker.GetDamage();
        ConsoleCharacterBuilder builder = new();
        builder.AddString(attacker.Name);
        builder.AddString($" used {attacker.AttackName} ");
        builder.AddString(target.Name);
        builder.AddString(" took ");
        builder.AddString($"{damage}", foreground: ConsoleColor.Red);
        builder.AddString(" damage.");
        UIManager.GameLog.AddEntry(builder.Build());
        target.DamageHealth(damage);
        return true;
    }

    private bool Kill(Entity target)
    {
        ConsoleCharacterBuilder builder = new();
        builder.AddString("~ ");
        builder.AddString("Hand of God", foreground: ConsoleColor.Red);
        builder.AddString($" struck ");
        builder.AddString(target.Name);
        builder.AddString(" ~");
        UIManager.GameLog.AddEntry(builder.Build());
        target.Kill();
        return true;
    }

    private bool UseGear(Entity entity, Party allyParty, Party enemyParty)
    {
        if (!entity.HasGear())
        {
            ConsoleCharacterBuilder builder = new();
            builder.AddString(entity.Name);
            builder.AddString(" has no gear equipped.");
            UIManager.GameLog.AddEntry(builder.Build());
            return false;
        }
        if (entity.GetGear() is ITargetAllyParty)
        {
            throw new NotImplementedException();
        }
        if (entity.GetGear() is ITargetEnemyParty)
        {
            throw new NotImplementedException();
        }
        if (entity.GetGear() is ITargetSingleAlly)
        {
            throw new NotImplementedException();
        }
        if (entity.GetGear() is ITargetSingleEnemy)
        {
            ((ITargetSingleEnemy)entity.GetGear()).Target(entity, _battle.SelectFromEnemies(entity));
            return true;
        }
        return false;
    }

    private bool EquipGear(Entity entity, Party allyParty)
    {
        if (allyParty.Invetory.GetGear().Count == 0)
        {
            UIManager.GameLog.AddEntry("No gear to equip!");
            return false;
        }
        Gear gear = SelectFromGear(allyParty);
        Gear? oldGear = entity.EquipGear(gear);
        ConsoleCharacterBuilder builder = new();
        if (oldGear == null)
        {
            builder.AddString(entity.Name);
            builder.AddString(" equiped ");
            builder.AddString($"{gear.Name}.");
            UIManager.GameLog.AddEntry(builder.Build());
            allyParty.Invetory.RemoveConsumable(gear);
            return true;
        }
        else
        {
            builder.AddString(entity.Name);
            builder.AddString(" equiped ");
            builder.AddString($"{gear.Name}.");
            UIManager.GameLog.AddEntry(builder.Build());
            builder.Clear();

            UIManager.GameLog.AddEntry($"Previously equiped {oldGear.Name} was moved back to inventory.");
            allyParty.Invetory.RemoveConsumable(gear);
            allyParty.Invetory.AddConsumable(oldGear);
            return true;
        }
    }

    private bool UseItem(Entity entity, Party allyParty, Party enemyParty)
    {
        if (allyParty.Invetory.GetConsumables().Count == 0)
        {
            UIManager.GameLog.AddEntry("No Consumable in inventory!");
            return false;
        }
        Consumable item = SelectFromConsumable(allyParty);
        if (item is ITargetAllyParty)
        {
            ((ITargetAllyParty)item).Target(entity, allyParty);
            allyParty.Invetory.RemoveConsumable(item);
            return true;
        }
        if (item is ITargetEnemyParty)
        {
            ((ITargetEnemyParty)item).Target(entity, enemyParty);
            allyParty.Invetory.RemoveConsumable(item);
            return true;
        }
        if (item is ITargetSingleAlly)
        {
            ((ITargetSingleAlly)item).Target(entity, _battle.SelectFromAllies(entity));
            allyParty.Invetory.RemoveConsumable(item);
            return true;
        }
        if (item is ITargetSingleEnemy)
        {
            ((ITargetSingleEnemy)item).Target(entity, _battle.SelectFromEnemies(entity));
            allyParty.Invetory.RemoveConsumable(item);
            return true;
        }
        return false;
    }

    private Gear SelectFromGear(Party party)
    {
        UIManager.ItemList.ClearList();

        UIManager.ItemList.SetList(party.Invetory.GetGear().Select(o => o.Name).ToArray());
        int index = UIListSelector.SelectFromList(UIManager.ItemList);
        UIManager.ItemList.ClearList();
        return party.Invetory.GetGear()[index];
    }

    private Consumable SelectFromConsumable(Party party)
    {
        UIManager.ItemList.ClearList();

        var itemGroups = party.Invetory.GetConsumables().GroupBy(o => o.Name.Text).OrderBy(o => o.Key);
        var groupsWithCount = itemGroups.Select(group => new { Name = group.Key, Count = group.Count() }).OrderBy(o => o.Name);
        var groupColors = party.Invetory.GetConsumables().GroupBy(o => o.Name.Text).Select(o => o.First()).OrderBy(o => o.Name.Text);

        for (int i = 0; i < groupsWithCount.Count(); i++)
        {
            ConsoleCharacterBuilder builder = new();
            builder.AddString(
                groupsWithCount.ElementAt(i).Name,
                groupColors.ElementAt(i).Name.Foreground);
            builder.AddString($" x{groupsWithCount.ElementAt(i).Count}");
            UIManager.ItemList.AddEntry(builder.Build());
        }

        int index = UIListSelector.SelectFromList(UIManager.ItemList);
        string name = groupsWithCount.ElementAt(index).Name;
        UIManager.ItemList.ClearList();
        return party.Invetory.GetConsumables().Where(o => o.Name.Text == name).Select(o => o).First();
    }
}