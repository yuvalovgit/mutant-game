using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MutantSurvivors
{
    public class HUDManager : MonoBehaviour
    {
        public static HUDManager Instance { get; private set; }

        // ── Top Bar ──────────────────────────────────────────────────────────────
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI planetText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI bioGoldText;
        [SerializeField] private TextMeshProUGUI killCountText;

        // ── Boss Bar ─────────────────────────────────────────────────────────────
        [Header("Boss Bar")]
        [SerializeField] private GameObject     bossBarRoot;
        [SerializeField] private TextMeshProUGUI bossNameLabel;
        [SerializeField] private Slider         bossHealthSlider;

        // ── Bottom ───────────────────────────────────────────────────────────────
        [Header("Player Health / Shield")]
        [SerializeField] private Slider         healthSlider;
        [SerializeField] private Image          healthFill;
        [SerializeField] private Slider         shieldSlider;
        [SerializeField] private GameObject     shieldBarRoot;

        [Header("Inventory Strip")]
        [SerializeField] private Transform      inventoryStrip;
        [SerializeField] private GameObject     itemSlotPrefab;

        [Header("Ability Slot")]
        [SerializeField] private Image          abilityCooldownOverlay;
        [SerializeField] private TextMeshProUGUI abilityKeyLabel;

        [Header("Wave Announce")]
        [SerializeField] private TextMeshProUGUI waveAnnounceText;
        [SerializeField] private CanvasGroup     waveAnnounceGroup;

        [Header("Notification Banner")]
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private CanvasGroup     notificationGroup;

        [Header("Death Screen")]
        [SerializeField] private GameObject deathScreenRoot;

        // ── Runtime ──────────────────────────────────────────────────────────────
        private BossAI trackedBoss;
        private PlayerCombat playerCombat;

        private static readonly Color ColorHealthHigh   = new Color(0.2f, 0.9f, 0.2f);
        private static readonly Color ColorHealthMid    = new Color(1f, 0.55f, 0f);
        private static readonly Color ColorHealthLow    = new Color(0.9f, 0.1f, 0.1f);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            playerCombat = FindObjectOfType<PlayerCombat>();

            if (bossBarRoot    != null) bossBarRoot.SetActive(false);
            if (deathScreenRoot != null) deathScreenRoot.SetActive(false);
            if (shieldBarRoot   != null) shieldBarRoot.SetActive(false);
            if (waveAnnounceGroup != null) waveAnnounceGroup.alpha = 0f;
            if (notificationGroup != null) notificationGroup.alpha = 0f;
            if (abilityKeyLabel != null)   abilityKeyLabel.text = "Q";
        }

        private void Update()
        {
            UpdateBossBar();
            UpdateAbilityCooldown();
        }

        // ── Timer ────────────────────────────────────────────────────────────────

        public void UpdateTimer(float seconds)
        {
            if (timerText == null) return;
            int m = Mathf.FloorToInt(seconds / 60f);
            int s = Mathf.FloorToInt(seconds % 60f);
            timerText.text = $"{m:D2}:{s:D2}";
            timerText.color = seconds < 30f ? Color.red : Color.white;

            if (seconds < 30f)
            {
                float pulse = Mathf.Abs(Mathf.Sin(Time.time * 4f));
                timerText.transform.localScale = Vector3.one * (1f + pulse * 0.08f);
            }
            else
            {
                timerText.transform.localScale = Vector3.one;
            }
        }

        // ── Resources ────────────────────────────────────────────────────────────

        public void UpdateBioGold(float amount)
        {
            if (bioGoldText != null)
                bioGoldText.text = $"{amount:F0} BG";
        }

        public void UpdateWave(int wave)
        {
            if (waveText != null)
                waveText.text = $"Wave {wave}";
        }

        // ── Health / Shield ──────────────────────────────────────────────────────

        public void UpdateHealth(float current, float max)
        {
            if (healthSlider != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = max;
                healthSlider.value    = current;
            }

            if (healthFill != null)
            {
                float pct = max > 0f ? current / max : 1f;
                healthFill.color = pct > 0.6f ? ColorHealthHigh
                                 : pct > 0.3f ? ColorHealthMid
                                              : ColorHealthLow;
            }
        }

        public void UpdateShield(float current, float max)
        {
            if (shieldBarRoot != null) shieldBarRoot.SetActive(max > 0f);
            if (shieldSlider  != null)
            {
                shieldSlider.minValue = 0f;
                shieldSlider.maxValue = Mathf.Max(max, 0.01f);
                shieldSlider.value    = current;
            }
        }

        // ── Planet Info ──────────────────────────────────────────────────────────

        public void UpdatePlanetInfo()
        {
            if (planetText != null && GameManager.Instance != null)
                planetText.text = $"Planet {GameManager.Instance.CurrentPlanet}";
            if (killCountText != null && GameManager.Instance != null)
                killCountText.text = $"Kills: {GameManager.Instance.TotalKills}";
        }

        // ── Wave Announce ────────────────────────────────────────────────────────

        public void AnnounceWave(int wave)
        {
            if (waveAnnounceText  == null || waveAnnounceGroup == null) return;
            waveAnnounceText.text = $"WAVE {wave}";
            StopCoroutine(nameof(FadeOutAnnounce));
            StartCoroutine(FadeOutAnnounce());
        }

        private IEnumerator FadeOutAnnounce()
        {
            waveAnnounceGroup.alpha = 1f;
            yield return new WaitForSeconds(1.5f);
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                waveAnnounceGroup.alpha = 1f - (t / 0.5f);
                yield return null;
            }
            waveAnnounceGroup.alpha = 0f;
        }

        // ── Boss Bar ─────────────────────────────────────────────────────────────

        public void ShowBossBar(BossAI boss)
        {
            trackedBoss = boss;
            if (bossBarRoot    != null) bossBarRoot.SetActive(true);
            if (bossNameLabel  != null) bossNameLabel.text = "\u26a0 PLANETARY OVERSEER \u26a0";
            if (bossHealthSlider != null)
            {
                bossHealthSlider.minValue = 0f;
                bossHealthSlider.maxValue = boss.MaxHP;
                bossHealthSlider.value    = boss.CurrentHP;
            }
        }

        private void UpdateBossBar()
        {
            if (trackedBoss == null || bossHealthSlider == null) return;
            bossHealthSlider.value = trackedBoss.CurrentHP;
            if (trackedBoss.IsDead)
            {
                bossBarRoot?.SetActive(false);
                trackedBoss = null;
            }
        }

        // ── Notification Banner ──────────────────────────────────────────────────

        public void ShowNotification(string msg, Color color)
        {
            if (notificationText == null || notificationGroup == null) return;
            notificationText.text  = msg;
            notificationText.color = color;
            StopCoroutine(nameof(DismissNotification));
            StartCoroutine(DismissNotification());
        }

        private IEnumerator DismissNotification()
        {
            notificationGroup.alpha = 1f;
            yield return new WaitForSeconds(2f);
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                notificationGroup.alpha = 1f - (t / 0.5f);
                yield return null;
            }
            notificationGroup.alpha = 0f;
        }

        // ── Inventory Strip ──────────────────────────────────────────────────────

        public void RefreshInventoryStrip(IReadOnlyList<ItemBase> items)
        {
            if (inventoryStrip == null || itemSlotPrefab == null) return;

            // Clear existing slots
            foreach (Transform child in inventoryStrip)
                Destroy(child.gameObject);

            // Count stacks
            Dictionary<string, (ItemBase item, int count)> stacks =
                new Dictionary<string, (ItemBase, int)>();

            foreach (ItemBase item in items)
            {
                if (item == null) continue;
                if (stacks.ContainsKey(item.ItemID))
                    stacks[item.ItemID] = (item, stacks[item.ItemID].count + 1);
                else
                    stacks[item.ItemID] = (item, 1);
            }

            foreach (var kv in stacks)
            {
                GameObject slot = Instantiate(itemSlotPrefab, inventoryStrip);
                Image iconImg = slot.GetComponentInChildren<Image>();
                TextMeshProUGUI countText = slot.GetComponentInChildren<TextMeshProUGUI>();

                if (iconImg   != null && kv.Value.item.Icon != null)
                    iconImg.sprite = kv.Value.item.Icon;
                if (countText != null)
                    countText.text = kv.Value.count > 1 ? kv.Value.count.ToString() : "";
            }
        }

        // ── Ability Slot ─────────────────────────────────────────────────────────

        private void UpdateAbilityCooldown()
        {
            if (abilityCooldownOverlay == null || playerCombat == null) return;
            float total = playerCombat.AbilityCooldownTotal;
            float remaining = playerCombat.AbilityCooldownRemaining;
            abilityCooldownOverlay.fillAmount = total > 0f ? remaining / total : 0f;
        }

        public void TriggerAbilityEffect()
        {
            if (abilityCooldownOverlay != null)
                abilityCooldownOverlay.color = Color.yellow;
            StartCoroutine(ResetAbilityColor());
        }

        private IEnumerator ResetAbilityColor()
        {
            yield return new WaitForSeconds(0.2f);
            if (abilityCooldownOverlay != null)
                abilityCooldownOverlay.color = new Color(0f, 0f, 0f, 0.6f);
        }

        // ── Death Screen ─────────────────────────────────────────────────────────

        public void ShowDeathScreen()
        {
            if (deathScreenRoot != null) deathScreenRoot.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            Time.timeScale   = 0f;
        }
    }
}
