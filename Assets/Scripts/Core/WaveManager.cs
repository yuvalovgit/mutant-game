// WaveManager.cs
// Wave progression is currently driven by EnemySpawner (which owns the coroutine).
// This class serves as a cross-system event hub if wave tracking needs to be
// decoupled from the spawner in the future.
using UnityEngine;

namespace MutantSurvivors
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public int CurrentWave => GameManager.Instance != null ? GameManager.Instance.CurrentWave : 1;
    }
}
