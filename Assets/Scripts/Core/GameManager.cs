using System;
using UnityEngine;

namespace MutantSurvivors
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Planet State")]
        [SerializeField] private int currentPlanet = 1;
        [SerializeField] private int currentWave = 1;

        [Header("Resources")]
        [SerializeField] private float bioGold = 0f;
        [SerializeField] private int totalKills = 0;

        [Header("Difficulty")]
        [SerializeField] private float difficultyCoefficient = 1f;

        [Header("Timer")]
        [SerializeField] private float planetTimer = 300f;

        [Header("Boss / Portal State")]
        [SerializeField] private bool bossSpawned = false;
        [SerializeField] private bool bossDefeated = false;
        [SerializeField] private bool portalOpen = false;

        // Public read-only accessors
        public int CurrentPlanet => currentPlanet;
        public int CurrentWave => currentWave;
        public float BioGold => bioGold;
        public int TotalKills => totalKills;
        public float DifficultyCoefficient => difficultyCoefficient;
        public float PlanetTimer => planetTimer;
        public bool BossSpawned => bossSpawned;
        public bool IsBossDefeated => bossDefeated;
        public bool PortalOpen => portalOpen;

        // Events
        public event Action OnBossSpawn;
        public event Action OnPlanetClear;
        public event Action OnPlayerDeath;
        public event Action<float> OnBioGoldChanged;
        public event Action<int> OnWaveChanged;
        public event Action<int> OnPlanetChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartPlanet();
        }

        private void Update()
        {
            if (!bossSpawned && planetTimer > 0f)
            {
                planetTimer -= Time.deltaTime;
                HUDManager.Instance?.UpdateTimer(planetTimer);

                if (planetTimer <= 0f)
                {
                    planetTimer = 0f;
                    TriggerBossSpawn();
                }
            }
        }

        // ── Public API ──────────────────────────────────────────────────────────

        public void StartPlanet()
        {
            planetTimer = 300f;
            bossSpawned = false;
            bossDefeated = false;
            portalOpen = false;
            difficultyCoefficient = Mathf.Pow(1.5f, currentPlanet - 1);

            HUDManager.Instance?.UpdatePlanetInfo();
            HUDManager.Instance?.UpdateBioGold(bioGold);
        }

        public void AddBioGold(float amount)
        {
            bioGold += amount;
            OnBioGoldChanged?.Invoke(bioGold);
            HUDManager.Instance?.UpdateBioGold(bioGold);
        }

        /// <returns>True if deducted successfully, false if insufficient funds.</returns>
        public bool SpendBioGold(float amount)
        {
            if (bioGold < amount) return false;
            bioGold -= amount;
            OnBioGoldChanged?.Invoke(bioGold);
            HUDManager.Instance?.UpdateBioGold(bioGold);
            return true;
        }

        public void RegisterKill()
        {
            totalKills++;
        }

        public void NotifyBossDefeated()
        {
            bossDefeated = true;
            OnPlanetClear?.Invoke();
            PlanetTransitionManager.Instance?.OpenPortal();
        }

        public void AdvancePlanet()
        {
            currentPlanet++;
            currentWave = 1;
            OnPlanetChanged?.Invoke(currentPlanet);
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void NotifyPlayerDeath()
        {
            OnPlayerDeath?.Invoke();
            HUDManager.Instance?.ShowDeathScreen();
        }

        public void SetCurrentWave(int wave)
        {
            currentWave = wave;
            OnWaveChanged?.Invoke(currentWave);
            HUDManager.Instance?.UpdateWave(currentWave);
            HUDManager.Instance?.AnnounceWave(currentWave);
        }

        // ── Private ─────────────────────────────────────────────────────────────

        private void TriggerBossSpawn()
        {
            bossSpawned = true;
            OnBossSpawn?.Invoke();
        }
    }
}
