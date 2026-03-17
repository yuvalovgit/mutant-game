using System;
using System.Collections;
using UnityEngine;

namespace MutantSurvivors
{
    [Serializable]
    public struct EnemyTier
    {
        public GameObject Prefab;
        public float      BaseHP;
        public float      BaseDamage;
        public float      BaseSpeed;
        public int        BioGoldReward;
        public bool       IsElite;
        public int        UnlockAtWave;
    }

    public class EnemySpawner : MonoBehaviour
    {
        [Header("Enemy Tiers")]
        [SerializeField] private EnemyTier[] enemyTiers;

        [Header("Boss")]
        [SerializeField] private GameObject bossPrefab;

        [Header("Spawn Points (radius ~30 from center)")]
        [SerializeField] private Transform[] spawnPoints;

        // Wave state
        private int   currentWave   = 1;
        private int   enemiesThisWave = 8;
        private float spawnInterval = 2.5f;
        private Coroutine waveCoroutine;

        private IEnumerator Start()
        {
            while (GameManager.Instance == null)
                yield return null;

            GameManager.Instance.OnBossSpawn += HandleBossSpawn;
            yield return new WaitForSeconds(3f);
            StartWave();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnBossSpawn -= HandleBossSpawn;
        }

        // ── Wave Logic ───────────────────────────────────────────────────────────

        private void StartWave()
        {
            if (waveCoroutine != null)
                StopCoroutine(waveCoroutine);
            waveCoroutine = StartCoroutine(WaveCoroutine());
        }

        private IEnumerator WaveCoroutine()
        {
            GameManager.Instance?.SetCurrentWave(currentWave);

            for (int i = 0; i < enemiesThisWave; i++)
            {
                if (GameManager.Instance == null || GameManager.Instance.BossSpawned) yield break;

                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            // Wave complete — advance
            currentWave++;
            enemiesThisWave = Mathf.FloorToInt(enemiesThisWave * 1.3f + 3f);
            spawnInterval   = Mathf.Max(0.8f, spawnInterval * 0.92f);

            StartWave();
        }

        private void SpawnEnemy()
        {
            if (spawnPoints == null || spawnPoints.Length == 0) return;

            // Pick eligible tiers
            EnemyTier tier = PickTier();
            if (tier.Prefab == null) return;

            Transform sp = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            GameObject obj = Instantiate(tier.Prefab, sp.position, sp.rotation);

            EnemyBase enemy = obj.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                int   planet     = GameManager.Instance != null ? GameManager.Instance.CurrentPlanet        : 1;
                float difficulty = GameManager.Instance != null ? GameManager.Instance.DifficultyCoefficient : 1f;
                enemy.ApplyScaledStats(
                    tier.BaseHP, tier.BaseDamage, tier.BaseSpeed,
                    tier.BioGoldReward, tier.IsElite,
                    planet, difficulty, currentWave);
            }
        }

        private EnemyTier PickTier()
        {
            // Filter to unlocked tiers
            System.Collections.Generic.List<EnemyTier> available =
                new System.Collections.Generic.List<EnemyTier>();

            foreach (EnemyTier t in enemyTiers)
            {
                if (t.Prefab != null && currentWave >= t.UnlockAtWave)
                    available.Add(t);
            }

            if (available.Count == 0 && enemyTiers.Length > 0)
                return enemyTiers[0];  // fallback

            if (available.Count == 0)
                return default;

            return available[UnityEngine.Random.Range(0, available.Count)];
        }

        // ── Boss Spawn ───────────────────────────────────────────────────────────

        private void HandleBossSpawn()
        {
            if (waveCoroutine != null)
            {
                StopCoroutine(waveCoroutine);
                waveCoroutine = null;
            }

            if (bossPrefab == null) return;

            Transform sp = spawnPoints != null && spawnPoints.Length > 0
                ? spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)]
                : transform;

            Instantiate(bossPrefab, sp.position, sp.rotation);
        }
    }
}
