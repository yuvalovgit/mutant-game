using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MutantSurvivors
{
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance { get; private set; }

        [Header("Items")]
        [SerializeField] private List<ItemBase> items = new List<ItemBase>();

        public IReadOnlyList<ItemBase> Items => items;

        private PlayerStats playerStats;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            playerStats = PlayerStats.Instance;
        }

        // ── Public API ───────────────────────────────────────────────────────────

        public void AddItem(ItemBase item)
        {
            if (item == null) return;
            items.Add(item);
            item.OnPickup(playerStats, this);
            HUDManager.Instance?.RefreshInventoryStrip(items);
        }

        public bool HasItem(string id)
        {
            return items.Any(i => i != null && i.ItemID == id);
        }

        public int GetCount(string id)
        {
            return items.Count(i => i != null && i.ItemID == id);
        }

        /// <summary>Called by Projectile after hitting an enemy.</summary>
        public void TriggerOnHit(EnemyBase enemy, float damage)
        {
            foreach (ItemBase item in items)
                item?.OnHit(enemy, damage);
        }

        /// <summary>Called by EnemyBase.Die().</summary>
        public void TriggerOnKill(EnemyBase killed)
        {
            foreach (ItemBase item in items)
                item?.OnKill(killed);
        }
    }
}
