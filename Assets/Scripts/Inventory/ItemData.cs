using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame.Inventory
{
    public enum ItemType
    {
        Resource,
        Food,
        Drink,
        Tool,
        Weapon,
        Equipment
    }

    [Serializable]
    public struct CraftingCost
    {
        public ItemData item;
        public int amount;
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "Survival/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("General Info")]
        public string itemId = Guid.NewGuid().ToString();
        public string itemName = "New Item";
        [TextArea(2, 4)] public string description = "";
        public Sprite icon;
        public ItemType itemType = ItemType.Resource;
        public GameObject worldPrefab;

        [Header("Stacking")]
        public bool isStackable = true;
        public int maxStack = 64;

        [Header("Consumable Effects")]
        public float hungerGain = 0f;
        public float thirstGain = 0f;
        public float healthGain = 0f;

        [Header("Tool & Weapon")]
        public float damageValue = 10f;
        public float harvestPower = 1.0f;

        [Header("Crafting Recipe")]
        public bool isCraftable = false;
        public List<CraftingCost> recipeRequirements = new List<CraftingCost>();
    }
}
