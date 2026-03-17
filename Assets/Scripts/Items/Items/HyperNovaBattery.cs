using UnityEngine;

namespace MutantSurvivors
{
    [CreateAssetMenu(fileName = "HyperNovaBattery", menuName = "MutantSurvivors/Items/Hyper Nova Battery")]
    public class HyperNovaBattery : ItemBase
    {
        public override void OnPickup(PlayerStats stats, PlayerInventory inv)
        {
            // The active ability logic lives in PlayerCombat.UseActiveAbility().
            // PlayerCombat checks PlayerInventory.HasItem("HyperNovaBattery") before firing.
            // No additional setup required here.
        }
    }
}
