using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MutantSurvivors
{
    public class LootPopup : MonoBehaviour
    {
        public static LootPopup Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private GameObject     panelRoot;
        [SerializeField] private Image          dimOverlay;

        [Header("Item Display")]
        [SerializeField] private TextMeshProUGUI rarityLabel;
        [SerializeField] private Image           itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Collect Button")]
        [SerializeField] private Button collectButton;
        [SerializeField] private TextMeshProUGUI collectButtonLabel;

        // Rarity colors
        private static readonly Color ColorSilver   = new Color(0.667f, 0.667f, 0.667f);
        private static readonly Color ColorGold     = new Color(1f, 0.667f, 0f);
        private static readonly Color ColorDiamond  = new Color(0.267f, 1f,    1f);
        private static readonly Color ColorDarkness = new Color(0.667f, 0f,    1f);

        private Action pendingCallback;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (panelRoot  != null) panelRoot.SetActive(false);
            if (collectButton != null)
                collectButton.onClick.AddListener(OnCollect);
            if (collectButtonLabel != null)
                collectButtonLabel.text = "COLLECT [E]";
        }

        public void Show(ItemBase item, Action onCollect)
        {
            if (item == null) { onCollect?.Invoke(); return; }

            pendingCallback = onCollect;

            Color rarityColor = RarityColor(item.Rarity);

            if (rarityLabel    != null) { rarityLabel.text    = item.Rarity.ToString().ToUpper(); rarityLabel.color = rarityColor; }
            if (itemIcon       != null && item.Icon != null)  itemIcon.sprite = item.Icon;
            if (itemNameText   != null) { itemNameText.text   = item.DisplayName; itemNameText.color = rarityColor; }
            if (descriptionText!= null)   descriptionText.text = item.Description;

            if (panelRoot != null) panelRoot.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            Time.timeScale   = 0f;
        }

        private void Update()
        {
            // Also allow E key to collect
            if (panelRoot != null && panelRoot.activeSelf)
            {
                if (UnityEngine.InputSystem.Keyboard.current != null &&
                    UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                {
                    OnCollect();
                }
            }
        }

        private void OnCollect()
        {
            if (panelRoot != null) panelRoot.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            Time.timeScale   = 1f;

            Action cb = pendingCallback;
            pendingCallback = null;
            cb?.Invoke();
        }

        private static Color RarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Silver   => ColorSilver,
                ItemRarity.Gold     => ColorGold,
                ItemRarity.Diamond  => ColorDiamond,
                ItemRarity.Darkness => ColorDarkness,
                _                   => Color.white
            };
        }
    }
}
