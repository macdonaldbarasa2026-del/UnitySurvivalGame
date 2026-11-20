using System;
using UnityEngine;
using SurvivalGame.Core;
using SurvivalGame.Inventory;

namespace SurvivalGame.Enemies
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Config")]
        [SerializeField] private float maxHealth = 50f;
        [SerializeField] private float currentHealth;

        [Header("Loot Drop on Death")]
        [SerializeField] private ItemData lootItem;
        [SerializeField] private int minDrop = 1;
        [SerializeField] private int maxDrop = 3;
        [SerializeField] private GameObject lootPrefab;

        private bool _isDead;

        public bool IsDead => _isDead;
        public float HealthPercent => Mathf.Clamp01(currentHealth / maxHealth);

        public event Action<float, float> OnHealthChanged;
        public event Action OnDied;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (_isDead) return;

            currentHealth -= damage;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            _isDead = true;
            OnDied?.Invoke();

            // Spawn loot
            if (lootItem != null)
            {
                int count = UnityEngine.Random.Range(minDrop, maxDrop + 1);
                if (lootPrefab != null)
                {
                    Instantiate(lootPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
                }
                else if (InventorySystem.Instance != null)
                {
                    InventorySystem.Instance.AddItem(lootItem, count);
                }
            }

            Destroy(gameObject, 0.5f);
        }
    }
}
