using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TheMythalProphecy.Game.Entities;
using TheMythalProphecy.Game.Systems.Rendering;

namespace TheMythalProphecy.Game.Battle
{
    /// <summary>
    /// Shared battle state container with all combat data
    /// </summary>
    public class BattleContext
    {
        public List<Combatant> AllCombatants { get; private set; }
        public List<Combatant> PlayerCombatants { get; private set; }
        public List<Combatant> EnemyCombatants { get; private set; }
        public Combatant CurrentTurnCombatant { get; set; }
        public int TurnNumber { get; set; }
        public BattleBackgroundManager.BattlegroundTheme BackgroundTheme { get; set; }
        public BattleResultData Result { get; set; }

        public BattleContext(List<Entity> partyMembers, List<Entity> enemies, BattleBackgroundManager.BattlegroundTheme theme)
        {
            BackgroundTheme = theme;
            TurnNumber = 1;
            Result = new BattleResultData();

            // Create combatant wrappers with battle positions
            PlayerCombatants = new List<Combatant>();
            EnemyCombatants = new List<Combatant>();

            // Position party members on the left side
            for (int i = 0; i < partyMembers.Count; i++)
            {
                var position = new Vector2(200, 100 + (i * 100));
                var combatant = new Combatant(partyMembers[i], isPlayer: true, position);
                PlayerCombatants.Add(combatant);
            }

            // Position enemies on the right side
            for (int i = 0; i < enemies.Count; i++)
            {
                var position = new Vector2(600, 150 + (i * 100));
                var combatant = new Combatant(enemies[i], isPlayer: false, position);
                EnemyCombatants.Add(combatant);
            }

            // Combine all combatants
            AllCombatants = new List<Combatant>();
            AllCombatants.AddRange(PlayerCombatants);
            AllCombatants.AddRange(EnemyCombatants);
        }

        /// <summary>
        /// Gets all alive combatants
        /// </summary>
        public List<Combatant> GetAliveCombatants()
        {
            return AllCombatants.Where(c => c.IsAlive).ToList();
        }

        /// <summary>
        /// Gets alive allies for a given combatant
        /// </summary>
        public List<Combatant> GetAliveAllies(Combatant combatant)
        {
            var allies = combatant.IsPlayer ? PlayerCombatants : EnemyCombatants;
            return allies.Where(c => c.IsAlive).ToList();
        }

        /// <summary>
        /// Gets alive enemies for a given combatant
        /// </summary>
        public List<Combatant> GetAliveEnemies(Combatant combatant)
        {
            var enemies = combatant.IsPlayer ? EnemyCombatants : PlayerCombatants;
            return enemies.Where(c => c.IsAlive).ToList();
        }

        /// <summary>
        /// Checks if all player combatants are dead (defeat condition)
        /// </summary>
        public bool AreAllPlayersDead()
        {
            return PlayerCombatants.All(c => c.IsDead);
        }

        /// <summary>
        /// Checks if all enemy combatants are dead (victory condition)
        /// </summary>
        public bool AreAllEnemiesDead()
        {
            return EnemyCombatants.All(c => c.IsDead);
        }

        /// <summary>
        /// Removes dead combatants from the active list (useful for cleanup)
        /// </summary>
        public void RemoveDeadCombatants()
        {
            AllCombatants.RemoveAll(c => c.IsDead);
            PlayerCombatants.RemoveAll(c => c.IsDead);
            EnemyCombatants.RemoveAll(c => c.IsDead);
        }
    }

    /// <summary>
    /// Contains the results of a battle (rewards, victory/defeat)
    /// </summary>
    public class BattleResultData
    {
        public bool Victory { get; set; }
        public int GoldEarned { get; set; }
        public int ExperienceEarned { get; set; }
        public List<string> ItemsEarned { get; set; } = new List<string>();
        public Dictionary<Entity, int> ExperiencePerCharacter { get; set; } = new Dictionary<Entity, int>();
    }
}
