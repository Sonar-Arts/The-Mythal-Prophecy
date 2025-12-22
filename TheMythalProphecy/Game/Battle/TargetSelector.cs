using System;
using System.Collections.Generic;
using System.Linq;
using TheMythalProphecy.Game.Characters.Stats;

namespace TheMythalProphecy.Game.Battle
{
    /// <summary>
    /// Helper class for target selection logic in battle
    /// </summary>
    public static class TargetSelector
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Selects a random target from alive candidates
        /// </summary>
        public static Combatant SelectRandomTarget(List<Combatant> candidates)
        {
            var aliveCandidates = candidates.Where(c => c.IsAlive).ToList();

            if (aliveCandidates.Count == 0)
                return null;

            int index = _random.Next(0, aliveCandidates.Count);
            return aliveCandidates[index];
        }

        /// <summary>
        /// Selects all alive targets from candidates
        /// </summary>
        public static List<Combatant> SelectAllTargets(List<Combatant> candidates)
        {
            return candidates.Where(c => c.IsAlive).ToList();
        }

        /// <summary>
        /// Selects the target with the lowest current HP
        /// </summary>
        public static Combatant SelectLowestHPTarget(List<Combatant> candidates)
        {
            var aliveCandidates = candidates.Where(c => c.IsAlive).ToList();

            if (aliveCandidates.Count == 0)
                return null;

            return aliveCandidates.OrderBy(c => c.Stats.GetStat(StatType.HP)).First();
        }

        /// <summary>
        /// Selects the highest threat target (simple calculation: HP + Strength)
        /// </summary>
        public static Combatant SelectHighestThreatTarget(List<Combatant> candidates)
        {
            var aliveCandidates = candidates.Where(c => c.IsAlive).ToList();

            if (aliveCandidates.Count == 0)
                return null;

            return aliveCandidates.OrderByDescending(c =>
                c.Stats.GetStat(StatType.HP) + c.Stats.GetStat(StatType.Strength)
            ).First();
        }
    }
}
