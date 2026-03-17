using UnityEngine;

namespace MutantSurvivors
{
    [CreateAssetMenu(fileName = "LeadLinedBoots", menuName = "MutantSurvivors/Items/Lead Lined Boots")]
    public class LeadLinedBoots : ItemBase
    {
        public override void OnPickup(PlayerStats stats, PlayerInventory inv)
        {
            stats.MoveSpeed   *= 1.10f;
            stats.SprintSpeed *= 1.10f;
        }
    }
}
