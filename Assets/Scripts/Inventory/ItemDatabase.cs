using UnityEngine;

namespace SurvivalGame.Inventory
{
    /// <summary>
    /// Runtime item catalogue. Items are created with ScriptableObject.CreateInstance so the
    /// build needs no authored assets and works entirely offline.
    /// </summary>
    public static class ItemDatabase
    {
        private static ItemData _wood;
        private static ItemData _stone;
        private static ItemData _fiber;
        private static ItemData _berry;
        private static ItemData _water;
        private static ItemData _meat;
        private static ItemData _bandage;
        private static ItemData _axe;
        private static ItemData _torch;

        public static ItemData Wood => Get(ref _wood, "wood", "Wood", ItemType.Resource, "Gathered from trees.", 64);
        public static ItemData Stone => Get(ref _stone, "stone", "Stone", ItemType.Resource, "Gathered from rocks.", 64);
        public static ItemData Fiber => Get(ref _fiber, "fiber", "Fiber", ItemType.Resource, "Plant fibres.", 64);
        public static ItemData Berry => Get(ref _berry, "berry", "Wild Berries", ItemType.Food, "Restores hunger.", 20, 26f, 0f, 4f);
        public static ItemData Water => Get(ref _water, "water", "Fresh Water", ItemType.Drink, "Restores thirst.", 20, 0f, 38f);
        public static ItemData Meat => Get(ref _meat, "meat", "Raw Meat", ItemType.Food, "Restores hunger.", 20, 18f);
        public static ItemData Bandage => Get(ref _bandage, "bandage", "Bandage", ItemType.Food, "Heals wounds.", 10, 0f, 0f, 35f);
        public static ItemData Axe => Get(ref _axe, "axe", "Stone Axe", ItemType.Weapon, "Better harvesting and damage.", 1, 0f, 0f, 0f, 26f, 2.4f, true);
        public static ItemData Torch => Get(ref _torch, "torch", "Torch", ItemType.Equipment, "A crude light source.", 1, 0f, 0f, 0f, 10f, 1f, true);

        static ItemDatabase()
        {
            Axe.recipeRequirements.Add(new CraftingCost { item = Wood, amount = 3 });
            Axe.recipeRequirements.Add(new CraftingCost { item = Stone, amount = 2 });
            Axe.recipeRequirements.Add(new CraftingCost { item = Fiber, amount = 2 });

            Bandage.recipeRequirements.Add(new CraftingCost { item = Fiber, amount = 3 });

            Torch.recipeRequirements.Add(new CraftingCost { item = Wood, amount = 2 });
            Torch.recipeRequirements.Add(new CraftingCost { item = Fiber, amount = 1 });
        }

        public static ItemData[] Recipes => new[] { Axe, Bandage, Torch };

        public static ItemData[] StarterItems => new[] { Berry, Water, Fiber };

        private static ItemData Get(ref ItemData field, string id, string name, ItemType type, string description, int maxStack,
            float hunger = 0f, float thirst = 0f, float health = 0f, float damage = 10f,
            float power = 1f, bool craftable = false)
        {
            if (field == null)
            {
                field = Create(id, name, type, description, maxStack, hunger, thirst, health, damage, power, craftable);
            }
            return field;
        }

        private static ItemData Create(string id, string name, ItemType type, string description, int maxStack,
            float hunger = 0f, float thirst = 0f, float health = 0f, float damage = 10f,
            float power = 1f, bool craftable = false)
        {
            ItemData item = ScriptableObject.CreateInstance<ItemData>();
            item.itemId = id;
            item.itemName = name;
            item.description = description;
            item.itemType = type;
            item.maxStack = maxStack;
            item.hungerGain = hunger;
            item.thirstGain = thirst;
            item.healthGain = health;
            item.damageValue = damage;
            item.harvestPower = power;
            item.isCraftable = craftable;
            return item;
        }
    }
}
