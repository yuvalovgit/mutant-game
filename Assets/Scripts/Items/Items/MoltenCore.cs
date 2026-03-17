using UnityEngine;

namespace MutantSurvivors
{
    [CreateAssetMenu(fileName = "MoltenCore", menuName = "MutantSurvivors/Items/Molten Core")]
    public class MoltenCore : ItemBase
    {
        public override void OnPickup(PlayerStats stats, PlayerInventory inv)
        {
            stats.IgniteChance += 0.15f;
        }

        public override void OnHit(EnemyBase target, float baseDamage)
        {
            if (target == null) return;

            PlayerStats stats = PlayerStats.Instance;
            if (stats == null) return;

            if (Random.value < stats.IgniteChance)
            {
                target.TakeDamage(baseDamage * 2f);
                target.SetOnFire(3f);
            }
        }
    }
}
