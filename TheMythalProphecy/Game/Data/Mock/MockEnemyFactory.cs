using Microsoft.Xna.Framework;
using TheMythalProphecy.Game.Characters.Stats;
using TheMythalProphecy.Game.Data.Definitions;
using TheMythalProphecy.Game.Entities;
using TheMythalProphecy.Game.Entities.Components;

namespace TheMythalProphecy.Game.Data.Mock
{
    /// <summary>
    /// Factory for creating Enemy entities from EnemyDefinitions
    /// </summary>
    public static class MockEnemyFactory
    {
        /// <summary>
        /// Create an enemy entity from a definition
        /// </summary>
        public static Entity CreateEnemy(EnemyDefinition def)
        {
            var entity = new Entity(def.Id, def.Name);

            // Add transform component
            entity.AddComponent(new TransformComponent(Vector2.Zero));

            // Add stats component
            var stats = new StatsComponent();
            ApplyEnemyStats(stats, def);
            entity.AddComponent(stats);

            // Add sprite component (placeholder for now)
            // entity.AddComponent(new SpriteComponent());

            // Store the rewards data in the entity (for later access)
            // We'll store it as a simple tag/property if needed
            // For now, we can look it up from the definition when enemy dies

            return entity;
        }

        /// <summary>
        /// Apply enemy stats from definition
        /// </summary>
        private static void ApplyEnemyStats(StatsComponent stats, EnemyDefinition def)
        {
            stats.SetBaseStat(StatType.Level, def.Stats.Level);
            stats.SetBaseStat(StatType.MaxHP, def.Stats.MaxHP);
            stats.SetBaseStat(StatType.MaxMP, def.Stats.MaxMP);
            stats.SetBaseStat(StatType.HP, def.Stats.MaxHP);  // Start at full HP
            stats.SetBaseStat(StatType.MP, def.Stats.MaxMP);  // Start at full MP

            stats.SetBaseStat(StatType.Strength, def.Stats.Strength);
            stats.SetBaseStat(StatType.Defense, def.Stats.Defense);
            stats.SetBaseStat(StatType.MagicPower, def.Stats.MagicPower);
            stats.SetBaseStat(StatType.MagicDefense, def.Stats.MagicDefense);
            stats.SetBaseStat(StatType.Speed, def.Stats.Speed);
            stats.SetBaseStat(StatType.Luck, def.Stats.Luck);
        }

        /// <summary>
        /// Get the rewards for a specific enemy definition
        /// </summary>
        public static EnemyRewards GetRewards(string enemyId)
        {
            var enemies = MockEnemyData.CreateEnemies();
            var enemyDef = enemies.Find(e => e.Id == enemyId);
            return enemyDef?.Rewards;
        }
    }
}
