// DifficultyManager.cs
// Difficulty scaling is currently handled inline by GameManager.DifficultyCoefficient.
// This stub exists for future expansion (e.g., adaptive difficulty, mutators, modifiers).
using UnityEngine;

namespace MutantSurvivors
{
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>Returns the current effective difficulty multiplier.</summary>
        public float GetCoefficient()
        {
            return GameManager.Instance != null ? GameManager.Instance.DifficultyCoefficient : 1f;
        }
    }
}
