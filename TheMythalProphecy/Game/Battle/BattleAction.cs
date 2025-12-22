using System.Collections.Generic;
using TheMythalProphecy.Game.Entities;

namespace TheMythalProphecy.Game.Battle
{
    /// <summary>
    /// Represents a queued battle action to be executed
    /// </summary>
    public class BattleAction
    {
        public BattleActionType ActionType { get; set; }
        public Combatant Actor { get; set; }
        public List<Combatant> Targets { get; set; }
        public string ItemId { get; set; }
        public string SkillId { get; set; }
        public bool IsExecuted { get; set; }

        public BattleAction(BattleActionType actionType, Combatant actor, List<Combatant> targets)
        {
            ActionType = actionType;
            Actor = actor;
            Targets = targets ?? new List<Combatant>();
            IsExecuted = false;
        }

        public BattleAction(BattleActionType actionType, Combatant actor, Combatant singleTarget)
            : this(actionType, actor, new List<Combatant> { singleTarget })
        {
        }
    }

    /// <summary>
    /// Types of actions that can be performed in battle
    /// </summary>
    public enum BattleActionType
    {
        Attack,
        Defend,
        UseItem,
        Skill,
        Flee
    }
}
