using UnityEngine;

namespace MutantSurvivors
{
    public enum ItemRarity
    {
        Silver,
        Gold,
        Diamond,
        Darkness
    }

    public abstract class ItemBase : ScriptableObject
    {
        [Header("Identity")]
        public string ItemID;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        public ItemRarity Rarity;

        /// <summary>Called once when the item is picked up. Apply permanent stat changes here.</summary>
        public abstract void OnPickup(PlayerStats stats, PlayerInventory inv);

        /// <summary>Called every time the player kills an enemy.</summary>
        public virtual void OnKill(EnemyBase killed) { }

        /// <summary>Called every time the player's projectile hits an enemy.</summary>
        public virtual void OnHit(EnemyBase target, float baseDamage) { }
    }
}
