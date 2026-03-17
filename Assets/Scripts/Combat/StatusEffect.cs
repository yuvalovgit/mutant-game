using System.Collections;
using UnityEngine;

namespace MutantSurvivors
{
    public enum StatusEffectType
    {
        Burning,
        Slowed
    }

    /// <summary>
    /// Attach to an enemy GameObject. Ticks damage over time (for Burning),
    /// or applies a speed reduction (for Slowed). Destroys itself when done.
    /// </summary>
    public class StatusEffect : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private StatusEffectType effectType = StatusEffectType.Burning;
        [SerializeField] private float            duration    = 3f;
        [SerializeField] private float            tickDamage  = 15f;   // per second
        [SerializeField] private float            slowFactor  = 0.5f;  // multiplier for Slowed

        private EnemyBase targetEnemy;
        private float     originalSpeed;

        private void Start()
        {
            targetEnemy = GetComponent<EnemyBase>();
            if (targetEnemy == null)
            {
                Destroy(this);
                return;
            }

            if (effectType == StatusEffectType.Slowed)
            {
                originalSpeed            = targetEnemy.MoveSpeed;
                targetEnemy.MoveSpeed   *= slowFactor;
            }

            StartCoroutine(TickCoroutine());
        }

        private IEnumerator TickCoroutine()
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float dt = Time.deltaTime;
                elapsed += dt;

                if (effectType == StatusEffectType.Burning && targetEnemy != null && !targetEnemy.IsDead)
                    targetEnemy.TakeDamage(tickDamage * dt);

                yield return null;
            }

            if (effectType == StatusEffectType.Slowed && targetEnemy != null)
                targetEnemy.MoveSpeed = originalSpeed;

            Destroy(this);
        }
    }
}
