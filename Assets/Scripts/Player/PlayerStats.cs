using System.Collections;
using UnityEngine;

namespace MutantSurvivors
{
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        [Header("Health")]
        [SerializeField] private float maxHP = 200f;
        [SerializeField] private float currentHP;

        [Header("Shield")]
        [SerializeField] private float maxShield = 0f;
        [SerializeField] private float currentShield = 0f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float sprintSpeed = 14f;

        [Header("Combat")]
        [SerializeField] private float fireRate = 0.18f;
        [SerializeField] private float projectileDamage = 25f;

        [Header("Status Procs")]
        [SerializeField] private float igniteChance = 0f;
        [SerializeField] private float deleteEliteChance = 0f;

        // Public properties — items and other systems write through these
        public float MaxHP
        {
            get => maxHP;
            set { maxHP = value; HUDManager.Instance?.UpdateHealth(currentHP, maxHP); }
        }
        public float CurrentHP
        {
            get => currentHP;
            set { currentHP = value; HUDManager.Instance?.UpdateHealth(currentHP, maxHP); }
        }
        public float MaxShield
        {
            get => maxShield;
            set { maxShield = value; HUDManager.Instance?.UpdateShield(currentShield, maxShield); }
        }
        public float CurrentShield
        {
            get => currentShield;
            set { currentShield = value; HUDManager.Instance?.UpdateShield(currentShield, maxShield); }
        }
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
        public float SprintSpeed { get => sprintSpeed; set => sprintSpeed = value; }
        public float FireRate { get => fireRate; set => fireRate = value; }
        public float ProjectileDamage { get => projectileDamage; set => projectileDamage = value; }
        public float IgniteChance { get => igniteChance; set => igniteChance = value; }
        public float DeleteEliteChance { get => deleteEliteChance; set => deleteEliteChance = value; }

        // Shield recharge
        private const float ShieldRechargeDelay = 10f;
        private const float ShieldRechargeAmount = 50f;
        private float shieldRechargeTimer = 0f;
        private bool rechargeTimerRunning = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            currentHP = maxHP;
            HUDManager.Instance?.UpdateHealth(currentHP, maxHP);
            HUDManager.Instance?.UpdateShield(currentShield, maxShield);
        }

        private void Update()
        {
            TickShieldRecharge();
        }

        // ── Shield recharge ──────────────────────────────────────────────────────

        private void TickShieldRecharge()
        {
            if (!rechargeTimerRunning || maxShield <= 0f) return;
            if (currentShield >= maxShield) return;

            shieldRechargeTimer -= Time.deltaTime;
            if (shieldRechargeTimer <= 0f)
            {
                currentShield = Mathf.Min(currentShield + ShieldRechargeAmount, maxShield);
                HUDManager.Instance?.UpdateShield(currentShield, maxShield);
                rechargeTimerRunning = false;
            }
        }

        private void ResetShieldRechargeTimer()
        {
            shieldRechargeTimer = ShieldRechargeDelay;
            rechargeTimerRunning = true;
        }

        // ── Damage ───────────────────────────────────────────────────────────────

        public void TakeDamage(float damage)
        {
            ResetShieldRechargeTimer();

            // Absorb into shield first
            if (currentShield > 0f)
            {
                float absorbed = Mathf.Min(currentShield, damage);
                currentShield -= absorbed;
                damage -= absorbed;
                HUDManager.Instance?.UpdateShield(currentShield, maxShield);
            }

            if (damage <= 0f) return;

            currentHP -= damage;
            HUDManager.Instance?.UpdateHealth(currentHP, maxHP);

            if (currentHP <= 0f)
            {
                currentHP = 0f;
                GameManager.Instance?.NotifyPlayerDeath();
            }
        }

        // ── Healing (for future use) ─────────────────────────────────────────────

        public void Heal(float amount)
        {
            currentHP = Mathf.Min(currentHP + amount, maxHP);
            HUDManager.Instance?.UpdateHealth(currentHP, maxHP);
        }
    }
}
