using System.Collections.Generic;
using TheMythalProphecy.Game.Data.Definitions;

namespace TheMythalProphecy.Game.Data.Mock
{
    /// <summary>
    /// Mock enemy data for testing battle system
    /// </summary>
    public static class MockEnemyData
    {
        public static List<EnemyDefinition> CreateEnemies()
        {
            return new List<EnemyDefinition>
            {
                new EnemyDefinition
                {
                    Id = "goblin",
                    Name = "Goblin",
                    Description = "A weak goblin scout armed with a rusty dagger",
                    Stats = new BaseStats
                    {
                        Level = 1,
                        MaxHP = 30,
                        MaxMP = 5,
                        Strength = 8,
                        Defense = 5,
                        MagicPower = 3,
                        MagicDefense = 3,
                        Speed = 12,
                        Luck = 5
                    },
                    Rewards = new EnemyRewards
                    {
                        GoldMin = 5,
                        GoldMax = 15,
                        Experience = 10,
                        ItemDrops = new List<ItemDrop>
                        {
                            new ItemDrop { ItemId = "potion", DropChance = 0.3f } // 30% chance
                        }
                    }
                },

                new EnemyDefinition
                {
                    Id = "wolf",
                    Name = "Wolf",
                    Description = "A fierce wolf with sharp fangs and quick reflexes",
                    Stats = new BaseStats
                    {
                        Level = 2,
                        MaxHP = 40,
                        MaxMP = 0,
                        Strength = 10,
                        Defense = 6,
                        MagicPower = 0,
                        MagicDefense = 4,
                        Speed = 15,
                        Luck = 8
                    },
                    Rewards = new EnemyRewards
                    {
                        GoldMin = 8,
                        GoldMax = 20,
                        Experience = 15,
                        ItemDrops = new List<ItemDrop>
                        {
                            new ItemDrop { ItemId = "potion", DropChance = 0.2f } // 20% chance
                        }
                    }
                },

                new EnemyDefinition
                {
                    Id = "bandit",
                    Name = "Bandit",
                    Description = "A dangerous bandit wielding a short sword",
                    Stats = new BaseStats
                    {
                        Level = 3,
                        MaxHP = 50,
                        MaxMP = 10,
                        Strength = 12,
                        Defense = 8,
                        MagicPower = 5,
                        MagicDefense = 6,
                        Speed = 10,
                        Luck = 10
                    },
                    Rewards = new EnemyRewards
                    {
                        GoldMin = 15,
                        GoldMax = 30,
                        Experience = 20,
                        ItemDrops = new List<ItemDrop>
                        {
                            new ItemDrop { ItemId = "potion", DropChance = 0.4f },    // 40% chance
                            new ItemDrop { ItemId = "ether", DropChance = 0.15f }      // 15% chance
                        }
                    }
                }
            };
        }
    }
}
