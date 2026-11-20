using UnityEngine;
using SurvivalGame.Core;
using SurvivalGame.Player;

namespace SurvivalGame.Environment
{
    public class Campfire : MonoBehaviour, IInteractable
    {
        [Header("Warmth Settings")]
        [SerializeField] private float heatRadius = 5.0f;
        [SerializeField] private float heatTemperature = 35f;
        [SerializeField] private float heatTransferRate = 5f;

        [Header("Fuel & Fire Lifetime")]
        [SerializeField] private bool isLit = true;
        [SerializeField] private float fuelTimeRemaining = 300f; // 5 minutes
        [SerializeField] private GameObject fireVfx;
        [SerializeField] private Light fireLight;

        private void Update()
        {
            if (!isLit) return;

            fuelTimeRemaining -= Time.deltaTime;
            if (fuelTimeRemaining <= 0f)
            {
                Extinguish();
                return;
            }

            // Warm nearby player
            Collider[] hits = Physics.OverlapSphere(transform.position, heatRadius);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<PlayerSurvivalStats>(out var stats))
                {
                    stats.AdjustTemperature(heatTemperature, heatTransferRate);
                }
            }
        }

        public string GetInteractionPrompt()
        {
            if (!isLit) return "Press E to ignite campfire";
            return $"Campfire lit ({(int)fuelTimeRemaining}s left). Press E to add fuel";
        }

        public void Interact(PlayerController player)
        {
            if (!isLit)
            {
                isLit = true;
                fuelTimeRemaining = 180f;
                if (fireVfx != null) fireVfx.SetActive(true);
                if (fireLight != null) fireLight.enabled = true;
            }
            else
            {
                fuelTimeRemaining += 120f;
            }
        }

        private void Extinguish()
        {
            isLit = false;
            if (fireVfx != null) fireVfx.SetActive(false);
            if (fireLight != null) fireLight.enabled = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, heatRadius);
        }
    }
}
