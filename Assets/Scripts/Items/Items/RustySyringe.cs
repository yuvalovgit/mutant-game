using UnityEngine;

namespace MutantSurvivors
{
    [CreateAssetMenu(fileName = "RustySyringe", menuName = "MutantSurvivors/Items/Rusty Syringe")]
    public class RustySyringe : ItemBase
    {
        public override void OnPickup(PlayerStats stats, PlayerInventory inv)
        {
            stats.FireRate *= 0.85f;
        }
    }
}
