using UnityEngine;

namespace MutantSurvivors
{
    /// <summary>
    /// Static helper for area-of-effect explosions.
    /// Called by PlayerCombat and items that trigger on-kill explosions.
    /// </summary>
    public static class AoeAbility
    {
        private static readonly int EnemyLayer = ~0; // all layers

        /// <summary>
        /// Hit all EnemyBase components within <paramref name="radius"/> of
        /// <paramref name="center"/> for <paramref name="damage"/> each.
        /// Optionally spawns a simple particle effect.
        /// </summary>
        public static void Explode(Vector3 center, float radius, float damage, Color vfxColor)
        {
            Collider[] hits = Physics.OverlapSphere(center, radius, EnemyLayer);
            foreach (Collider col in hits)
            {
                EnemyBase enemy = col.GetComponent<EnemyBase>();
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.TakeDamage(damage);
                    FloatingTextManager.Instance?.Show(
                        enemy.transform.position + Vector3.up * 1.5f,
                        damage,
                        vfxColor);
                }
            }

            SpawnVFX(center, radius, vfxColor);
        }

        private static void SpawnVFX(Vector3 center, float radius, Color color)
        {
            // Create a simple expanding sphere gizmo using a temporary GameObject.
            // Replace with a proper particle prefab in production.
            GameObject vfx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            vfx.name = "AoeVFX";

            // Remove collider so it doesn't interact
            Object.Destroy(vfx.GetComponent<Collider>());

            vfx.transform.position   = center;
            vfx.transform.localScale = Vector3.one * radius * 2f;

            Renderer rend = vfx.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                mat.color   = new Color(color.r, color.g, color.b, 0.3f);
                rend.material = mat;
            }

            Object.Destroy(vfx, 0.25f);
        }
    }
}
