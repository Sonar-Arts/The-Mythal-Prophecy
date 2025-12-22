using System;
using System.Linq;
using TheMythalProphecy.Game.Characters.Stats;
using TheMythalProphecy.Game.Data.Definitions;

namespace TheMythalProphecy.Game.Battle
{
    /// <summary>
    /// Centralized combat calculations for damage, healing, and battle outcomes
    /// </summary>
    public static class DamageCalculator
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Calculates physical damage from attacker to target
        /// </summary>
        public static DamageResult CalculatePhysicalDamage(Combatant attacker, Combatant target)
        {
            var result = new DamageResult();

            // Check for evasion first
            int targetSpeed = target.Stats.GetStat(StatType.Speed);
            int targetLuck = target.Stats.GetStat(StatType.Luck);
            float evasionChance = StatCalculator.CalculateEvasionChance(targetSpeed, targetLuck);

            if (_random.NextDouble() < evasionChance)
            {
                result.IsEvaded = true;
                result.Damage = 0;
                return result;
            }

            // Calculate base damage
            int strength = attacker.Stats.GetStat(StatType.Strength);
            int defense = target.Stats.GetStat(StatType.Defense);
            int baseDamage = strength - (defense / 2);

            // Apply damage variance (±10%)
            float varianceMultiplier = 0.9f + ((float)_random.NextDouble() * 0.2f); // 0.9 to 1.1
            int variance = (int)(baseDamage * varianceMultiplier);

            // Check for critical hit
            int attackerLuck = attacker.Stats.GetStat(StatType.Luck);
            float critChance = StatCalculator.CalculateCriticalChance(attackerLuck);

            if (_random.NextDouble() < critChance)
            {
                result.IsCritical = true;
                variance = (int)(variance * 1.5f); // 1.5x damage on critical
            }

            // Check if target is defending (reduce damage by 50%)
            if (target.State == CombatantState.Defending)
            {
                result.IsBlocked = true;
                variance = (int)(variance * 0.5f);
            }

            // Ensure minimum 1 damage
            result.Damage = Math.Max(1, variance);

            return result;
        }

        /// <summary>
        /// Calculates magical damage from attacker to target
        /// </summary>
        public static DamageResult CalculateMagicalDamage(Combatant attacker, Combatant target, int basePower)
        {
            var result = new DamageResult();

            // Magic doesn't miss (no evasion check)

            // Calculate base damage
            int magicPower = attacker.Stats.GetStat(StatType.MagicPower);
            int magicDefense = target.Stats.GetStat(StatType.MagicDefense);
            int baseDamage = (basePower + magicPower) - (magicDefense / 2);

            // Apply damage variance (±5% for magic - more consistent than physical)
            float varianceMultiplier = 0.95f + ((float)_random.NextDouble() * 0.1f); // 0.95 to 1.05
            int variance = (int)(baseDamage * varianceMultiplier);

            // Magic can still crit, but lower chance
            int attackerLuck = attacker.Stats.GetStat(StatType.Luck);
            float critChance = StatCalculator.CalculateCriticalChance(attackerLuck) * 0.5f; // Half crit chance

            if (_random.NextDouble() < critChance)
            {
                result.IsCritical = true;
                variance = (int)(variance * 1.3f); // 1.3x damage on critical (less than physical)
            }

            // Defending doesn't help as much against magic
            if (target.State == CombatantState.Defending)
            {
                result.IsBlocked = true;
                variance = (int)(variance * 0.75f); // Only 25% reduction
            }

            // Ensure minimum 1 damage
            result.Damage = Math.Max(1, variance);

            return result;
        }

        /// <summary>
        /// Calculates healing amount
        /// </summary>
        public static int CalculateHealing(int baseHealAmount, int magicPower)
        {
            // Healing scales with magic power
            int healing = baseHealAmount + (magicPower / 4);

            // Small variance (±5%)
            float varianceMultiplier = 0.95f + ((float)_random.NextDouble() * 0.1f); // 0.95 to 1.05
            healing = (int)(healing * varianceMultiplier);

            return Math.Max(1, healing);
        }

        /// <summary>
        /// Calculates chance of successfully fleeing from battle
        /// </summary>
        public static bool CalculateFleeSuccess(BattleContext context)
        {
            // Calculate average speeds
            float averagePartySpeed = (float)context.PlayerCombatants
                .Where(c => c.IsAlive)
                .Average(c => c.Stats.GetStat(StatType.Speed));

            float averageEnemySpeed = (float)context.EnemyCombatants
                .Where(c => c.IsAlive)
                .Average(c => c.Stats.GetStat(StatType.Speed));

            // Base 50% chance
            float baseChance = 0.5f;

            // Modify by speed difference (1% per speed point)
            float speedDiff = (averagePartySpeed - averageEnemySpeed) * 0.01f;

            // Calculate final chance (clamped between 20% and 90%)
            float fleeChance = Math.Max(0.2f, Math.Min(0.9f, baseChance + speedDiff));

            return _random.NextDouble() < fleeChance;
        }

        /// <summary>
        /// Rolls for item drops from an enemy
        /// </summary>
        public static string RollItemDrop(EnemyRewards rewards)
        {
            if (rewards?.ItemDrops == null || rewards.ItemDrops.Count == 0)
                return null;

            foreach (var drop in rewards.ItemDrops)
            {
                if (_random.NextDouble() < drop.DropChance)
                {
                    return drop.ItemId;
                }
            }

            return null;
        }

        /// <summary>
        /// Calculates random gold reward within enemy's range
        /// </summary>
        public static int RollGoldReward(EnemyRewards rewards)
        {
            if (rewards == null)
                return 0;

            return _random.Next(rewards.GoldMin, rewards.GoldMax + 1);
        }
    }

    /// <summary>
    /// Result of a damage calculation
    /// </summary>
    public class DamageResult
    {
        public int Damage { get; set; }
        public bool IsCritical { get; set; }
        public bool IsEvaded { get; set; }
        public bool IsBlocked { get; set; }
    }
}
