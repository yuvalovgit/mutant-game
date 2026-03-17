using UnityEngine;
using UnityEngine.AI;
using TMPro;

namespace MutantSurvivors
{
    public class PlanetTransitionManager : MonoBehaviour
    {
        public static PlanetTransitionManager Instance { get; private set; }

        [Header("Portal Visual")]
        [SerializeField] private GameObject portalPrefab;           // optional; built procedurally if null
        [SerializeField] private Vector3    portalSpawnPos = new Vector3(0f, 3f, -10f);
        [SerializeField] private Color      portalGlowColor = new Color(0.3f, 1f, 0.5f);

        [Header("Portal UI")]
        [SerializeField] private GameObject portalUIPanelRoot;       // assign in Inspector
        [SerializeField] private UnityEngine.UI.Button advanceButton; // calls GameManager.AdvancePlanet

        [Header("Loot Crate Scatter")]
        [SerializeField] private GameObject lootCratePrefab;
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private int  crateCount  = 8;
        [SerializeField] private float crateMinRadius = 8f;
        [SerializeField] private float crateMaxRadius = 35f;

        private GameObject spawnedPortal;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (advanceButton != null)
                advanceButton.onClick.AddListener(() => GameManager.Instance?.AdvancePlanet());

            if (portalUIPanelRoot != null)
                portalUIPanelRoot.SetActive(false);

            ScatterLootCrates();
            RebakeNavMesh();
        }

        // ── Portal ───────────────────────────────────────────────────────────────

        public void OpenPortal()
        {
            if (spawnedPortal == null)
            {
                if (portalPrefab != null)
                {
                    spawnedPortal = Instantiate(portalPrefab, portalSpawnPos, Quaternion.identity);
                }
                else
                {
                    spawnedPortal = BuildProceduralPortal();
                }
            }

            if (portalUIPanelRoot != null)
                portalUIPanelRoot.SetActive(true);

            HUDManager.Instance?.ShowNotification("PORTAL OPEN — ENTER TO ADVANCE", portalGlowColor);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        // ── Loot Crate Scatter ───────────────────────────────────────────────────

        private void ScatterLootCrates()
        {
            if (lootCratePrefab == null) return;

            for (int i = 0; i < crateCount; i++)
            {
                float angle  = Random.Range(0f, 360f);
                float radius = Random.Range(crateMinRadius, crateMaxRadius);
                Vector3 pos  = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    0.5f,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius);

                GameObject crate = Instantiate(lootCratePrefab, pos, Quaternion.identity);

                LootCrate lc = crate.GetComponent<LootCrate>();
                // itemDatabase is assigned in the prefab Inspector; this is a fallback
                if (lc != null && itemDatabase != null)
                {
                    // The LootCrate already holds a reference set in the prefab.
                    // If it doesn't, we inject via reflection or a public setter.
                    // For now, rely on the prefab's assigned reference.
                }
            }
        }

        // ── NavMesh ──────────────────────────────────────────────────────────────

        private void RebakeNavMesh()
        {
#if UNITY_EDITOR
            // Editor-time bake is manual via Window > AI > Navigation.
            // Runtime rebake requires the NavMesh Components package.
            // NavMeshSurface.BuildNavMesh() would go here if that package is present.
#endif
        }

        // ── Procedural Portal ────────────────────────────────────────────────────

        private GameObject BuildProceduralPortal()
        {
            // Build a simple glowing torus stand-in using a cylinder + ring light
            GameObject portal = new GameObject("Portal");
            portal.transform.position = portalSpawnPos;

            // Torus visual approximation: flat cylinder scaled wide & thin
            GameObject torusVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            torusVisual.name = "PortalVisual";
            torusVisual.transform.SetParent(portal.transform, false);
            torusVisual.transform.localScale = new Vector3(3f, 0.1f, 3f);

            Renderer rend = torusVisual.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                mat.color = portalGlowColor;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", portalGlowColor * 3f);
                rend.material = mat;
            }

            // Glow light
            Light glow = portal.AddComponent<Light>();
            glow.type      = LightType.Point;
            glow.color     = portalGlowColor;
            glow.intensity = 3f;
            glow.range     = 12f;

            // Spin animation via a simple helper component
            portal.AddComponent<PortalSpin>();

            return portal;
        }
    }

    /// <summary>Spins the portal visual each frame.</summary>
    public class PortalSpin : MonoBehaviour
    {
        private void Update()
        {
            transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.World);
        }
    }
}
