using UnityEngine;

namespace MutantSurvivors
{
    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float speed = 35f;
        [SerializeField] private float lifetime = 2f;

        [Header("Visual")]
        [SerializeField] private Light pointLight;

        // Initialized at spawn
        private Vector3 direction;
        private float   damage;
        private float   igniteChance;

        private bool hasHit;

        public void Initialize(Vector3 dir, float dmg, float ignite)
        {
            direction   = dir.normalized;
            damage      = dmg;
            igniteChance = ignite;
            transform.rotation = Quaternion.LookRotation(direction);

            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (!hasHit)
                transform.position += direction * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasHit) return;

            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy == null) return;

            hasHit = true;

            float finalDamage = damage;

            // Ignite proc (base bonus — MoltenCore OnHit also handles its own proc)
            bool ignited = Random.value < igniteChance;
            if (ignited)
            {
                enemy.SetOnFire(3f);
            }

            enemy.TakeDamage(finalDamage);
            PlayerInventory.Instance?.TriggerOnHit(enemy, finalDamage);

            Destroy(gameObject);
        }
    }
}
