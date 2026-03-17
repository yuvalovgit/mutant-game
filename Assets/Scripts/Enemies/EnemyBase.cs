using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace MutantSurvivors
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBase : MonoBehaviour
    {
        // ── Stats (set by spawner at runtime) ────────────────────────────────────
        [Header("Stats (set at runtime by spawner)")]
        public float MaxHP        = 100f;
        public float CurrentHP;
        public float Damage       = 10f;
        public float MoveSpeed    = 4f;
        public float AttackRange  = 1.8f;
        public float AttackCooldown = 1.2f;
        public int   BioGoldReward = 5;
        public bool  IsElite      = false;

        // ── Internal ─────────────────────────────────────────────────────────────
        protected NavMeshAgent agent;
        protected Transform playerTransform;
        private float attackTimer;
        private bool isDead;

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        protected virtual void Start()
        {
            CurrentHP = MaxHP;
            attackTimer = 0f;

            // Cache player transform once
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;

            if (agent != null)
                agent.speed = MoveSpeed;
        }

        protected virtual void Update()
        {
            if (isDead || playerTransform == null) return;

            MoveTowardPlayer();
            TryAttack();
        }

        // ── Movement ─────────────────────────────────────────────────────────────

        private void MoveTowardPlayer()
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(playerTransform.position);
            }
            else
            {
                // Fallback direct movement when no NavMesh
                Vector3 dir = (playerTransform.position - transform.position).normalized;
                transform.position += dir * MoveSpeed * Time.deltaTime;
            }

            // Always face player
            Vector3 lookDir = playerTransform.position - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }

        // ── Attack ───────────────────────────────────────────────────────────────

        private void TryAttack()
        {
            attackTimer -= Time.deltaTime;
            float dist = Vector3.Distance(transform.position, playerTransform.position);

            if (dist <= AttackRange && attackTimer <= 0f)
            {
                attackTimer = AttackCooldown;
                PerformAttack();
            }
        }

        protected virtual void PerformAttack()
        {
            PlayerStats.Instance?.TakeDamage(Damage);
            FloatingTextManager.Instance?.Show(
                playerTransform.position + Vector3.up,
                Damage,
                Color.red);
        }

        // ── Damage / Death ───────────────────────────────────────────────────────

        public virtual void TakeDamage(float damage)
        {
            if (isDead) return;
            CurrentHP -= damage;

            FloatingTextManager.Instance?.Show(
                transform.position + Vector3.up * 1.5f,
                damage,
                Color.white);

            if (CurrentHP <= 0f)
                Die();
        }

        public void SetOnFire(float duration)
        {
            if (isDead) return;
            StartCoroutine(BurnCoroutine(duration));
        }

        private IEnumerator BurnCoroutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration && !isDead)
            {
                TakeDamage(15f * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        protected virtual void Die()
        {
            if (isDead) return;
            isDead = true;

            // Stop NavMesh agent
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
                agent.ResetPath();

            GameManager.Instance?.AddBioGold(BioGoldReward);
            GameManager.Instance?.RegisterKill();
            PlayerInventory.Instance?.TriggerOnKill(this);

            // VoidTouchedTentacle proc: delete nearest elite
            TryDeleteNearbyElite();

            Destroy(gameObject);
        }

        private void TryDeleteNearbyElite()
        {
            PlayerStats stats = PlayerStats.Instance;
            if (stats == null || stats.DeleteEliteChance <= 0f) return;
            if (Random.value >= stats.DeleteEliteChance) return;

            // Find nearest elite within 20 units
            Collider[] cols = Physics.OverlapSphere(transform.position, 20f);
            EnemyBase nearest = null;
            float nearestDist = float.MaxValue;

            foreach (Collider col in cols)
            {
                EnemyBase e = col.GetComponent<EnemyBase>();
                if (e == null || e == this || !e.IsElite) continue;
                float d = Vector3.Distance(transform.position, e.transform.position);
                if (d < nearestDist)
                {
                    nearestDist = d;
                    nearest = e;
                }
            }

            if (nearest != null)
            {
                nearest.CurrentHP = 0f;
                nearest.Die();
            }
        }

        // ── Public helpers ───────────────────────────────────────────────────────

        public bool IsDead => isDead;

        public void ApplyScaledStats(float baseHP, float baseDamage, float baseSpeed,
                                     int goldReward, bool elite,
                                     int planet, float difficulty, int wave)
        {
            float hpMultiplier = Mathf.Pow(1.5f, planet - 1) * difficulty * (1f + (wave - 1) * 0.15f);
            MaxHP          = baseHP * hpMultiplier;
            CurrentHP      = MaxHP;
            Damage         = baseDamage * Mathf.Pow(1.5f, planet - 1) * difficulty;
            MoveSpeed      = baseSpeed;
            BioGoldReward  = goldReward;
            IsElite        = elite;

            if (agent != null)
                agent.speed = MoveSpeed;
        }
    }
}
