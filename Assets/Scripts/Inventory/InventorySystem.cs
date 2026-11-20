using System;
using System.Collections.Generic;
using UnityEngine;
using SurvivalGame.Player;

namespace SurvivalGame.Inventory
{
    [Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int count;

        public InventorySlot(ItemData item, int count)
        {
            this.item = item;
            this.count = count;
        }

        public bool IsEmpty => item == null || count <= 0;

        public void Clear()
        {
            item = null;
            count = 0;
        }
    }

    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance { get; private set; }

        [Header("Inventory Capacity")]
        [SerializeField] private int slotCapacity = 24;
        [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

        public int SlotCount => slotCapacity;
        public IReadOnlyList<InventorySlot> Slots => slots;

        public event Action OnInventoryChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeSlots();
        }

        private void InitializeSlots()
        {
            slots.Clear();
            for (int i = 0; i < slotCapacity; i++)
            {
                slots.Add(new InventorySlot(null, 0));
            }
        }

        public bool AddItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;

            int remaining = amount;

            // 1. Fill existing matching stacks if stackable
            if (item.isStackable)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].item == item && slots[i].count < item.maxStack)
                    {
                        int spaceInSlot = item.maxStack - slots[i].count;
                        int toAdd = Mathf.Min(spaceInSlot, remaining);
                        slots[i].count += toAdd;
                        remaining -= toAdd;

                        if (remaining <= 0)
                        {
                            OnInventoryChanged?.Invoke();
                            return true;
                        }
                    }
                }
            }

            // 2. Put remainder into first empty slots
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty)
                {
                    int toAdd = item.isStackable ? Mathf.Min(item.maxStack, remaining) : 1;
                    slots[i].item = item;
                    slots[i].count = toAdd;
                    remaining -= toAdd;

                    if (remaining <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }

            OnInventoryChanged?.Invoke();
            return remaining == 0;
        }

        public bool HasItem(ItemData item, int requiredAmount)
        {
            int total = 0;
            foreach (var slot in slots)
            {
                if (slot.item == item)
                {
                    total += slot.count;
                    if (total >= requiredAmount) return true;
                }
            }
            return false;
        }

        public bool RemoveItem(ItemData item, int amountToRemove)
        {
            if (!HasItem(item, amountToRemove)) return false;

            int remaining = amountToRemove;
            for (int i = slots.Count - 1; i >= 0; i--)
            {
                if (slots[i].item == item)
                {
                    if (slots[i].count <= remaining)
                    {
                        remaining -= slots[i].count;
                        slots[i].Clear();
                    }
                    else
                    {
                        slots[i].count -= remaining;
                        remaining = 0;
                    }

                    if (remaining <= 0) break;
                }
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        public void UseItem(int slotIndex, PlayerSurvivalStats playerStats)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return;
            var slot = slots[slotIndex];
            if (slot.IsEmpty) return;

            var item = slot.item;

            switch (item.itemType)
            {
                case ItemType.Food:
                    playerStats.Eat(item.hungerGain, item.healthGain);
                    slot.count--;
                    break;
                case ItemType.Drink:
                    playerStats.Drink(item.thirstGain);
                    slot.count--;
                    break;
            }

            if (slot.count <= 0) slot.Clear();
            OnInventoryChanged?.Invoke();
        }
    }
}
