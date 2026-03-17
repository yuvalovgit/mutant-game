using System;
using UnityEngine;

namespace MutantSurvivors
{
    public class LootCrate : MonoBehaviour
    {
        [Header("Database")]
        [SerializeField] private ItemDatabase itemDatabase;

        [Header("Cost")]
        [SerializeField] private float cost = 50f;

        [Header("Visual")]
        [SerializeField] private Renderer crateRenderer;
        [SerializeField] private float    rotationSpeed = 45f;   // degrees/sec
        [SerializeField] private Color    glowColor     = Color.cyan;

        private bool isOpen = false;
        private Material crateMaterial;

        private void Start()
        {
            if (crateRenderer != null)
            {
                crateMaterial = crateRenderer.material;
                crateMaterial.EnableKeyword("_EMISSION");
                crateMaterial.SetColor("_EmissionColor", glowColor * 1.5f);
            }
        }

        private void Update()
        {
            if (!isOpen)
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        // ── Open Logic ───────────────────────────────────────────────────────────

        public void TryOpen()
        {
            if (isOpen) return;

            if (!GameManager.Instance.SpendBioGold(cost))
            {
                HUDManager.Instance?.ShowNotification(
                    $"Need {cost} Bio-Gold", Color.red);
                return;
            }

            isOpen = true;

            if (itemDatabase == null)
            {
                Debug.LogWarning("LootCrate: No ItemDatabase assigned.");
                gameObject.SetActive(false);
                return;
            }

            ItemBase rolledItem = itemDatabase.RollItem();
            if (rolledItem == null)
            {
                Debug.LogWarning("LootCrate: ItemDatabase returned null.");
                gameObject.SetActive(false);
                return;
            }

            // Show popup — on confirm, give item and disable crate
            LootPopup.Instance?.Show(rolledItem, () =>
            {
                PlayerInventory.Instance?.AddItem(rolledItem);
                gameObject.SetActive(false);
            });
        }
    }
}
