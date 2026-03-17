using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MutantSurvivors
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "MutantSurvivors/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [Header("All Items")]
        public List<ItemBase> AllItems = new List<ItemBase>();

        // Rarity weights
        private const float WeightSilver   = 60f;
        private const float WeightGold     = 35f;
        private const float WeightDiamond  = 4.8f;
        private const float WeightDarkness = 0.2f;
        private const float WeightTotal    = WeightSilver + WeightGold + WeightDiamond + WeightDarkness;

        /// <summary>Weighted random roll — returns a random ItemBase, or null if none exist.</summary>
        public ItemBase RollItem()
        {
            ItemRarity rarity = RollRarity();

            List<ItemBase> pool = AllItems.Where(i => i != null && i.Rarity == rarity).ToList();
            if (pool.Count == 0)
            {
                // Fallback: any item
                pool = AllItems.Where(i => i != null).ToList();
                if (pool.Count == 0) return null;
            }

            return pool[Random.Range(0, pool.Count)];
        }

        private ItemRarity RollRarity()
        {
            float roll = Random.Range(0f, WeightTotal);
            if (roll < WeightSilver)                            return ItemRarity.Silver;
            if (roll < WeightSilver + WeightGold)              return ItemRarity.Gold;
            if (roll < WeightSilver + WeightGold + WeightDiamond) return ItemRarity.Diamond;
            return ItemRarity.Darkness;
        }
    }
}
