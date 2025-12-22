using Microsoft.Xna.Framework;
using TheMythalProphecy.Game.Entities;
using TheMythalProphecy.Game.Entities.Components;
using TheMythalProphecy.Game.Characters.Stats;

namespace TheMythalProphecy.Game.Battle
{
    /// <summary>
    /// Wrapper for Entity with battle-specific data
    /// </summary>
    public class Combatant
    {
        public Entity Entity { get; }
        public bool IsPlayer { get; }
        public float TurnPriority { get; set; }
        public BattleAction QueuedAction { get; set; }
        public Vector2 BattlePosition { get; set; }
        public CombatantState State { get; set; }

        // Quick accessors
        public StatsComponent Stats => Entity.GetComponent<StatsComponent>();
        public AnimationComponent Animation => Entity.GetComponent<AnimationComponent>();
        public bool IsAlive => Stats?.IsAlive ?? false;
        public bool IsDead => !IsAlive;
        public string Name => Entity.Name;

        public Combatant(Entity entity, bool isPlayer, Vector2 battlePosition)
        {
            Entity = entity;
            IsPlayer = isPlayer;
            BattlePosition = battlePosition;
            State = CombatantState.Ready;
            TurnPriority = 0f;
        }

        public void SetAction(BattleAction action)
        {
            QueuedAction = action;
        }

        public void ClearAction()
        {
            QueuedAction = null;
        }

        public bool HasQueuedAction()
        {
            return QueuedAction != null && !QueuedAction.IsExecuted;
        }

        public override string ToString()
        {
            return $"{Name} ({(IsPlayer ? "Player" : "Enemy")}) - HP: {Stats?.GetStat(StatType.HP)}/{Stats?.GetStat(StatType.MaxHP)}";
        }
    }

    /// <summary>
    /// Represents the current state of a combatant during battle
    /// </summary>
    public enum CombatantState
    {
        Ready,
        Defending,
        Attacking,
        Casting,
        Hurt,
        Dead,
        Fleeing
    }
}
