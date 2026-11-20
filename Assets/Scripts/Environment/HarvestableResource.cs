using System.Collections;
using UnityEngine;
using SurvivalGame.Inventory;

namespace SurvivalGame.Environment
{
    public class HarvestableResource : MonoBehaviour
    {
        [Header("Resource Config")]
        [SerializeField] private string resourceName = "Oak Tree";
        [SerializeField] private float maxHealth = 60f;
        [SerializeField] private float currentHealth;

        [Header("Drops")]
        [SerializeField] private ItemData dropItem;
        [SerializeField] private int minDropAmount = 2;
        [SerializeField] private int maxDropAmount = 5;
        [SerializeField] private GameObject dropPickupPrefab;

        [Header("Visual Feedback")]
        [SerializeField] private float wobbleAngle = 4f;
        [SerializeField] private float wobbleDuration = 0.15f;

        private bool _isDestroyed;
        private Quaternion _originalRotation;
        private Coroutine _wobbleCoroutine;

        private void Awake()
        {
            currentHealth = maxHealth;
            _originalRotation = transform.localRotation;
        }

        public void Hit(float damage, Vector3 hitPoint)
        {
            if (_isDestroyed) return;

            currentHealth -= damage;

            if (_wobbleCoroutine != null) StopCoroutine(_wobbleCoroutine);
            _wobbleCoroutine = StartCoroutine(WobbleRoutine(hitPoint));

            if (currentHealth <= 0f)
            {
                Harvest();
            }
        }

        private IEnumerator WobbleRoutine(Vector3 hitPoint)
        {
            Vector3 wobbleAxis = Vector3.Cross(Vector3.up, (hitPoint - transform.position).normalized);
            Quaternion targetRot = _originalRotation * Quaternion.AngleAxis(wobbleAngle, wobbleAxis);

            float elapsed = 0f;
            while (elapsed < wobbleDuration)
            {
                elapsed += Time.deltaTime;
                transform.localRotation = Quaternion.Slerp(targetRot, _originalRotation, elapsed / wobbleDuration);
                yield return null;
            }
            transform.localRotation = _originalRotation;
        }

        private void Harvest()
        {
            _isDestroyed = true;
            int dropCount = Random.Range(minDropAmount, maxDropAmount + 1);

            if (dropItem != null)
            {
                // Either directly add to inventory or spawn world pickups
                if (InventorySystem.Instance != null && InventorySystem.Instance.AddItem(dropItem, dropCount))
                {
                    // Directly collected
                }
                else if (dropPickupPrefab != null)
                {
                    for (int i = 0; i < dropCount; i++)
                    {
                        Vector3 spawnPos = transform.position + Vector3.up * 0.5f + Random.insideUnitSphere * 0.5f;
                        Instantiate(dropPickupPrefab, spawnPos, Random.rotation);
                    }
                }
            }

            Destroy(gameObject, 0.1f);
        }
    }
}
