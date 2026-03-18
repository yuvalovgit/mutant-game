using UnityEngine;

namespace MutantSurvivors
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator animator;

        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;

        [Header("Ability")]
        [SerializeField] private float abilityCooldown = 12f;

        [Header("Interact")]
        [SerializeField] private float interactRadius = 3f;

        private float fireCooldownTimer;
        private float abilityCooldownTimer;

        private PlayerStats playerStats;
        private PlayerInventory playerInventory;

        private void Start()
        {
            playerStats     = PlayerStats.Instance;
            playerInventory = PlayerInventory.Instance;

            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);

            if (animator != null)
                animator.SetBool("IsShooting", false);
            else
                Debug.LogError("[PlayerCombat] Animator not found!");

            if (firePoint == null)
                Debug.LogError("[PlayerCombat] FirePoint not assigned!");

            if (projectilePrefab == null)
                Debug.LogError("[PlayerCombat] ProjectilePrefab not assigned!");
        }

        private void Update()
        {
            if (fireCooldownTimer > 0f) fireCooldownTimer -= Time.deltaTime;
            if (abilityCooldownTimer > 0f) abilityCooldownTimer -= Time.deltaTime;
        }

        // ── Fire ───────────────────────────────────────────────────────

        public void TryFire()
        {
            if (fireCooldownTimer > 0f) return;
            if (projectilePrefab == null || firePoint == null) return;

            fireCooldownTimer = playerStats != null ? playerStats.FireRate : 0.3f;

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Projectile p = proj.GetComponent<Projectile>();
            p?.Initialize(firePoint.forward, 
                playerStats != null ? playerStats.ProjectileDamage : 10f, 
                playerStats != null ? playerStats.IgniteChance : 0f);
        }

        public void SetShooting(bool isShooting)
        {
            if (animator != null)
                animator.SetBool("IsShooting", isShooting);
        }

        // ── Active Ability ─────────────────────────────────────────────

        public void UseActiveAbility()
        {
            if (playerInventory == null || !playerInventory.HasItem("HyperNovaBattery")) return;
            if (abilityCooldownTimer > 0f) return;

            abilityCooldownTimer = abilityCooldown;

            Collider[] hits = Physics.OverlapSphere(transform.position, 15f);
            foreach (Collider col in hits)
            {
                EnemyBase enemy = col.GetComponent<EnemyBase>();
                if (enemy != null)
                    enemy.TakeDamage(playerStats != null ? playerStats.ProjectileDamage * 5f : 50f);
            }

            HUDManager.Instance?.TriggerAbilityEffect();
            FloatingTextManager.Instance?.Show(
                transform.position + Vector3.up * 2f,
                playerStats != null ? playerStats.ProjectileDamage * 5f : 50f,
                Color.yellow);
        }

        // ── Interact ───────────────────────────────────────────────────

        public void TryInteract()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius);
            foreach (Collider col in hits)
            {
                LootCrate crate = col.GetComponent<LootCrate>();
                if (crate != null)
                {
                    crate.TryOpen();
                    return;
                }
            }
        }

        // ── Public helpers ─────────────────────────────────────────────

        public float AbilityCooldownRemaining => abilityCooldownTimer;
        public float AbilityCooldownTotal     => abilityCooldown;
    }
}