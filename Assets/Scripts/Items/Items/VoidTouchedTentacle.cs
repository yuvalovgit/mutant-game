using UnityEngine;

namespace MutantSurvivors
{
    [CreateAssetMenu(fileName = "VoidTouchedTentacle", menuName = "MutantSurvivors/Items/Void Touched Tentacle")]
    public class VoidTouchedTentacle : ItemBase
    {
        public override void OnPickup(PlayerStats stats, PlayerInventory inv)
        {
            stats.DeleteEliteChance += 0.05f;
            stats.MaxHP              = Mathf.FloorToInt(stats.MaxHP * 0.9f);
            stats.CurrentHP          = Mathf.Min(stats.CurrentHP, stats.MaxHP);
        }
    }
}
