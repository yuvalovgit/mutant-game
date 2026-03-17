using System.Collections;
using UnityEngine;

namespace MutantSurvivors
{
    public class BossAI : EnemyBase
    {
        [Header("Boss Ranged Attack")]
        [SerializeField] private GameObject bossProjPrefab;
        [SerializeField] private Transform bossFirePoint;
        [SerializeField] private float rangedInterval  = 3f;
        [SerializeField] private float rangedMinDist   = 8f;

        private float rangedTimer;

        protected override void Start()
        {
            base.Start();

            // Scale HP to planet / difficulty
            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                MaxHP     = 2000f * Mathf.Pow(1.5f, gm.CurrentPlanet - 1) * gm.DifficultyCoefficient;
                CurrentHP = MaxHP;
            }

            HUDManager.Instance?.ShowBossBar(this);
        }

        protected override void Update()
        {
            base.Update();

            if (playerTransform == null) return;

            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist > rangedMinDist)
            {
                rangedTimer -= Time.deltaTime;
                if (rangedTimer <= 0f)
                {
                    rangedTimer = rangedInterval;
                    FireRangedAttack();
                }
            }
        }

        private void FireRangedAttack()
        {
            if (bossProjPrefab == null) return;

            Transform origin = bossFirePoint != null ? bossFirePoint : transform;
            Vector3 dir = (playerTransform.position + Vector3.up - origin.position).normalized;

            GameObject proj = Instantiate(bossProjPrefab, origin.position, Quaternion.LookRotation(dir));
            Projectile p = proj.GetComponent<Projectile>();
            p?.Initialize(dir, Damage * 0.75f, 0f);
        }

        protected override void Die()
        {
            GameManager.Instance?.NotifyBossDefeated();
            base.Die();
        }
    }
}
