using UnityEngine;
using UnityEngine.UI;

namespace MutantSurvivors
{
    /// <summary>
    /// Optional standalone boss health bar component.
    /// Attach to the boss bar Slider if you want it to auto-update independently.
    /// HUDManager also handles this via ShowBossBar(); this class is provided as
    /// an alternative for a dedicated boss-bar prefab that carries its own slider.
    /// </summary>
    public class BossHealthBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider bossSlider;

        private BossAI boss;

        public void SetBoss(BossAI bossRef)
        {
            boss = bossRef;
            if (bossSlider == null) return;
            bossSlider.minValue = 0f;
            bossSlider.maxValue = boss.MaxHP;
            bossSlider.value    = boss.CurrentHP;
        }

        private void Update()
        {
            if (boss == null || bossSlider == null) return;
            bossSlider.value = boss.CurrentHP;

            if (boss.IsDead)
                gameObject.SetActive(false);
        }
    }
}
