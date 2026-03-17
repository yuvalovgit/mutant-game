using System.Collections;
using TMPro;
using UnityEngine;

namespace MutantSurvivors
{
    public class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance { get; private set; }

        [Header("Prefab")]
        [SerializeField] private GameObject floatingTextPrefab;  // needs a TextMeshProUGUI on a Canvas

        [Header("Settings")]
        [SerializeField] private float floatDistance  = 60f;   // pixels upward
        [SerializeField] private float fadeDuration   = 1f;
        [SerializeField] private float minFontSize    = 14f;
        [SerializeField] private float maxFontSize    = 34f;
        [SerializeField] private float damageForMax   = 150f;   // damage value mapped to max font

        private Camera mainCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        public void Show(Vector3 worldPos, float damage, Color color)
        {
            if (floatingTextPrefab == null) return;
            if (mainCamera == null) mainCamera = Camera.main;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            if (screenPos.z < 0f) return;  // behind camera

            GameObject obj = Instantiate(floatingTextPrefab, transform);
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt != null) rt.position = screenPos;

            TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text  = Mathf.RoundToInt(damage).ToString();
                tmp.color = color;
                float t = Mathf.Clamp01(damage / damageForMax);
                tmp.fontSize = Mathf.Lerp(minFontSize, maxFontSize, t);
            }

            StartCoroutine(FloatAndFade(obj, rt, tmp));
        }

        private IEnumerator FloatAndFade(GameObject obj, RectTransform rt, TextMeshProUGUI tmp)
        {
            float elapsed = 0f;
            Vector3 startPos = rt != null ? rt.position : Vector3.zero;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeDuration;

                if (rt  != null) rt.position = startPos + Vector3.up * (floatDistance * progress);
                if (tmp != null) tmp.alpha    = 1f - progress;

                yield return null;
            }

            Destroy(obj);
        }
    }
}
