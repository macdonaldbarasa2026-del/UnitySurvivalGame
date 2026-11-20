using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivalGame.Core;
using SurvivalGame.Inventory;
using SurvivalGame.Player;

namespace SurvivalGame.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject craftingPanel;

        [Header("Prefabs & Containers")]
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Transform craftingRecipeContainer;
        [SerializeField] private GameObject recipeButtonPrefab;

        private PlayerSurvivalStats _playerStats;
        private bool _isOpen = false;

        private void Start()
        {
            _playerStats = FindObjectOfType<PlayerSurvivalStats>();

            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.OnInventoryChanged += RefreshInventory;
            }

            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (craftingPanel != null) craftingPanel.SetActive(false);

            BuildCraftingMenu();
        }

        private void OnDestroy()
        {
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.OnInventoryChanged -= RefreshInventory;
            }
        }

        private void Update()
        {
            if (GameInput.InventoryPressed)
            {
                ToggleInventory();
            }
        }

        public void ToggleInventory()
        {
            _isOpen = !_isOpen;

            if (inventoryPanel != null) inventoryPanel.SetActive(_isOpen);
            if (craftingPanel != null) craftingPanel.SetActive(_isOpen);

            if (_isOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                RefreshInventory();
            }
            else
            {
                if (GameLoop.Instance?.State == RunState.Running)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        public void RefreshInventory()
        {
            if (slotContainer == null || InventorySystem.Instance == null) return;

            // Clear old slot UI views
            foreach (Transform child in slotContainer)
            {
                Destroy(child.gameObject);
            }

            var slots = InventorySystem.Instance.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                int index = i;
                var slot = slots[i];
                var slotObj = Instantiate(slotPrefab, slotContainer);

                var iconImg = slotObj.transform.Find("Icon")?.GetComponent<Image>();
                var countText = slotObj.transform.Find("Count")?.GetComponent<TextMeshProUGUI>();
                var button = slotObj.GetComponent<Button>();

                if (!slot.IsEmpty)
                {
                    if (iconImg != null)
                    {
                        iconImg.sprite = slot.item.icon;
                        iconImg.enabled = slot.item.icon != null;
                    }
                    if (countText != null)
                    {
                        countText.text = slot.count > 1 ? slot.count.ToString() : "";
                    }

                    if (button != null)
                    {
                        button.onClick.AddListener(() =>
                        {
                            InventorySystem.Instance.UseItem(index, _playerStats);
                        });
                    }
                }
                else
                {
                    if (iconImg != null) iconImg.enabled = false;
                    if (countText != null) countText.text = "";
                }
            }
        }

        private void BuildCraftingMenu()
        {
            if (craftingRecipeContainer == null || recipeButtonPrefab == null || CraftingSystem.Instance == null) return;

            foreach (var recipe in CraftingSystem.Instance.Recipes)
            {
                var btnObj = Instantiate(recipeButtonPrefab, craftingRecipeContainer);
                var label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = recipe.itemName;
                }

                var btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() =>
                    {
                        CraftingSystem.Instance.Craft(recipe);
                    });
                }
            }
        }
    }
}
