using UnityEngine;

namespace MutantSurvivors
{
    [CreateAssetMenu(fileName = "TitaniumCarapace", menuName = "MutantSurvivors/Items/Titanium Carapace")]
    public class TitaniumCarapace : ItemBase
    {
        public override void OnPickup(PlayerStats stats, PlayerInventory inv)
        {
            stats.MaxShield     += 50f;
            stats.CurrentShield += 50f;
            HUDManager.Instance?.UpdateShield(stats.CurrentShield, stats.MaxShield);
        }
    }
}
