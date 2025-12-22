using System.Collections.Generic;

namespace TheMythalProphecy.Game.Data.Definitions
{
    /// <summary>
    /// Defines an enemy template
    /// </summary>
    public class EnemyDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public BaseStats Stats { get; set; }
        public EnemyRewards Rewards { get; set; }
        public string SpriteSheet { get; set; }
    }

    /// <summary>
    /// Rewards dropped by an enemy upon defeat
    /// </summary>
    public class EnemyRewards
    {
        public int GoldMin { get; set; }
        public int GoldMax { get; set; }
        public int Experience { get; set; }
        public List<ItemDrop> ItemDrops { get; set; } = new List<ItemDrop>();
    }

    /// <summary>
    /// Represents a possible item drop with a chance
    /// </summary>
    public class ItemDrop
    {
        public string ItemId { get; set; }
        public float DropChance { get; set; }  // 0.0 to 1.0 (0% to 100%)
    }

    /// <summary>
    /// Container for multiple enemy definitions
    /// </summary>
    public class EnemyCollection
    {
        public List<EnemyDefinition> Enemies { get; set; } = new List<EnemyDefinition>();
    }
}
