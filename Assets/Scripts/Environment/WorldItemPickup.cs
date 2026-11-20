using UnityEngine;
using SurvivalGame.Core;
using SurvivalGame.Inventory;
using SurvivalGame.Player;

namespace SurvivalGame.Environment
{
    public class WorldItemPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private int quantity = 1;
        [SerializeField] private float bobSpeed = 2.0f;
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private float rotateSpeed = 45f;

        private Vector3 _startPosition;

        private void Start()
        {
            _startPosition = transform.position;
        }

        private void Update()
        {
            // Floating & spinning visual juice
            float newY = _startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }

        public string GetInteractionPrompt()
        {
            if (itemData == null) return "Pick up item";
            return $"Press E to pick up {itemData.itemName} x{quantity}";
        }

        public void Interact(PlayerController player)
        {
            if (InventorySystem.Instance != null && itemData != null)
            {
                bool added = InventorySystem.Instance.AddItem(itemData, quantity);
                if (added)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
