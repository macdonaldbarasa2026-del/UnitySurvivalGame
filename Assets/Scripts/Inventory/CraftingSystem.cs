using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame.Inventory
{
    public class CraftingSystem : MonoBehaviour
    {
        public static CraftingSystem Instance { get; private set; }

        [Header("Available Crafting Recipes")]
        [SerializeField] private List<ItemData> recipes = new List<ItemData>();

        public IReadOnlyList<ItemData> Recipes => recipes;
        public event Action<ItemData> OnItemCrafted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool CanCraft(ItemData recipe)
        {
            if (recipe == null || !recipe.isCraftable) return false;
            var inv = InventorySystem.Instance;
            if (inv == null) return false;

            foreach (var req in recipe.recipeRequirements)
            {
                if (!inv.HasItem(req.item, req.amount))
                {
                    return false;
                }
            }
            return true;
        }

        public bool Craft(ItemData recipe)
        {
            if (!CanCraft(recipe)) return false;

            var inv = InventorySystem.Instance;

            // Deduct required ingredients
            foreach (var req in recipe.recipeRequirements)
            {
                inv.RemoveItem(req.item, req.amount);
            }

            // Award crafted item
            bool added = inv.AddItem(recipe, 1);
            if (added)
            {
                OnItemCrafted?.Invoke(recipe);
            }
            return added;
        }
    }
}
