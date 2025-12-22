using System;
using System.Collections.Generic;

namespace TheMythalProphecy.Game.Systems.Events;

/// <summary>
/// Base class for all game events
/// </summary>
public abstract class GameEvent
{
    public EventType Type { get; protected set; }
    public double Timestamp { get; protected set; }

    protected GameEvent(EventType type)
    {
        Type = type;
        Timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }
}

// Combat Events
public class CombatStartedEvent : GameEvent
{
    public List<object> Enemies { get; }

    public CombatStartedEvent(List<object> enemies) : base(EventType.CombatStarted)
    {
        Enemies = enemies;
    }
}

public class DamageDealtEvent : GameEvent
{
    public object Target { get; }
    public int Amount { get; }
    public bool IsCritical { get; }

    public DamageDealtEvent(object target, int amount, bool isCritical = false)
        : base(EventType.DamageDealt)
    {
        Target = target;
        Amount = amount;
        IsCritical = isCritical;
    }
}

public class HealingAppliedEvent : GameEvent
{
    public object Target { get; }
    public int Amount { get; }

    public HealingAppliedEvent(object target, int amount) : base(EventType.HealingApplied)
    {
        Target = target;
        Amount = amount;
    }
}

// Character Events
public class LevelUpEvent : GameEvent
{
    public object Character { get; }
    public int NewLevel { get; }

    public LevelUpEvent(object character, int newLevel) : base(EventType.LevelUp)
    {
        Character = character;
        NewLevel = newLevel;
    }
}

public class SkillLearnedEvent : GameEvent
{
    public object Character { get; }
    public string SkillId { get; }

    public SkillLearnedEvent(object character, string skillId) : base(EventType.SkillLearned)
    {
        Character = character;
        SkillId = skillId;
    }
}

// Party Events
public class PartyChangedEvent : GameEvent
{
    public PartyChangedEvent() : base(EventType.PartyChanged)
    {
    }
}

// Inventory Events
public class ItemAddedEvent : GameEvent
{
    public string ItemId { get; }
    public int Quantity { get; }

    public ItemAddedEvent(string itemId, int quantity) : base(EventType.ItemAdded)
    {
        ItemId = itemId;
        Quantity = quantity;
    }
}

public class ItemRemovedEvent : GameEvent
{
    public string ItemId { get; }
    public int Quantity { get; }

    public ItemRemovedEvent(string itemId, int quantity) : base(EventType.ItemRemoved)
    {
        ItemId = itemId;
        Quantity = quantity;
    }
}

public class ItemUsedEvent : GameEvent
{
    public string ItemId { get; }
    public object Target { get; }
    public int Quantity { get; }

    public ItemUsedEvent(string itemId, object target, int quantity = 1) : base(EventType.ItemUsed)
    {
        ItemId = itemId;
        Target = target;
        Quantity = quantity;
    }
}

// Quest Events
public class QuestStartedEvent : GameEvent
{
    public string QuestId { get; }

    public QuestStartedEvent(string questId) : base(EventType.QuestStarted)
    {
        QuestId = questId;
    }
}

public class QuestCompletedEvent : GameEvent
{
    public string QuestId { get; }

    public QuestCompletedEvent(string questId) : base(EventType.QuestCompleted)
    {
        QuestId = questId;
    }
}

public class QuestUpdatedEvent : GameEvent
{
    public string QuestId { get; }
    public string ObjectiveId { get; }

    public QuestUpdatedEvent(string questId, string objectiveId) : base(EventType.QuestUpdated)
    {
        QuestId = questId;
        ObjectiveId = objectiveId;
    }
}

// Additional Combat Events for Battle System
public class CombatEndedEvent : GameEvent
{
    public bool Victory { get; }
    public object Result { get; }

    public CombatEndedEvent(bool victory, object result) : base(EventType.CombatEnded)
    {
        Victory = victory;
        Result = result;
    }
}

public class TurnStartedEvent : GameEvent
{
    public object Combatant { get; }
    public int TurnNumber { get; }

    public TurnStartedEvent(object combatant, int turnNumber) : base(EventType.TurnStarted)
    {
        Combatant = combatant;
        TurnNumber = turnNumber;
    }
}

public class ActionExecutedEvent : GameEvent
{
    public object Action { get; }

    public ActionExecutedEvent(object action) : base(EventType.ActionExecuted)
    {
        Action = action;
    }
}
