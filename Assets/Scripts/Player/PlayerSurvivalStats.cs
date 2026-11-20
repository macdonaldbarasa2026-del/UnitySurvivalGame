using System;
using UnityEngine;
using SurvivalGame.Core;

namespace SurvivalGame.Player
{
    /// <summary>
    /// Tracks vital survival statistics: Health, Stamina, Hunger, Thirst, and Temperature.
    /// Manages depletion rates, passive regeneration, starvation/dehydration damage, and death.
    /// </summary>
    public class PlayerSurvivalStats : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Stamina Settings")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float currentStamina;
        [SerializeField] private float staminaRegenRate = 15f;
        [SerializeField] private float staminaRegenDelay = 1.2f;

        [Header("Hunger & Thirst Settings")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float currentHunger;
        [SerializeField] private float hungerDepletionRate = 0.5f; // per second

        [SerializeField] private float maxThirst = 100f;
        [SerializeField] private float currentThirst;
        [SerializeField] private float thirstDepletionRate = 0.8f; // per second

        [Header("Starvation & Dehydration Penalties")]
        [SerializeField] private float starvationDamagePerSecond = 2f;
        [SerializeField] private float dehydrationDamagePerSecond = 3f;

        [Header("Temperature Settings")]
        [SerializeField] private float normalTemperature = 37f;
        [SerializeField] private float currentTemperature = 37f;
        [SerializeField] private float freezingThreshold = 30f;
        [SerializeField] private float freezingDamagePerSecond = 1.5f;

        private float _staminaRegenTimer;
        private bool _isDead;

        public float HealthPercent => Mathf.Clamp01(currentHealth / maxHealth);
        public float StaminaPercent => Mathf.Clamp01(currentStamina / maxStamina);
        public float HungerPercent => Mathf.Clamp01(currentHunger / maxHunger);
        public float ThirstPercent => Mathf.Clamp01(currentThirst / maxThirst);
        public float CurrentTemperature => currentTemperature;
        public bool IsDead => _isDead;

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnStaminaChanged;
        public event Action<float, float> OnHungerChanged;
        public event Action<float, float> OnThirstChanged;
        public event Action OnPlayerDied;

        private void Awake()
        {
            currentHealth = maxHealth;
            currentStamina = maxStamina;
            currentHunger = maxHunger;
            currentThirst = maxThirst;
            currentTemperature = normalTemperature;
        }

        private void Update()
        {
            if (_isDead || GameLoop.Instance?.State != RunState.Running) return;

            UpdateHungerAndThirst();
            UpdateStaminaRegeneration();
            ApplyEnvironmentalPenalties();
        }

        private void UpdateHungerAndThirst()
        {
            currentHunger = Mathf.Max(0f, currentHunger - hungerDepletionRate * Time.deltaTime);
            currentThirst = Mathf.Max(0f, currentThirst - thirstDepletionRate * Time.deltaTime);

            OnHungerChanged?.Invoke(currentHunger, maxHunger);
            OnThirstChanged?.Invoke(currentThirst, maxThirst);
        }

        private void UpdateStaminaRegeneration()
        {
            if (_staminaRegenTimer > 0f)
            {
                _staminaRegenTimer -= Time.deltaTime;
                return;
            }

            if (currentStamina < maxStamina)
            {
                currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
                OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            }
        }

        private void ApplyEnvironmentalPenalties()
        {
            float damageToApply = 0f;

            if (currentHunger <= 0f)
                damageToApply += starvationDamagePerSecond * Time.deltaTime;

            if (currentThirst <= 0f)
                damageToApply += dehydrationDamagePerSecond * Time.deltaTime;

            if (currentTemperature <= freezingThreshold)
                damageToApply += freezingDamagePerSecond * Time.deltaTime;

            if (damageToApply > 0f)
            {
                TakeDamage(damageToApply);
            }
        }

        public bool UseStamina(float amount)
        {
            if (currentStamina < amount) return false;

            currentStamina = Mathf.Max(0f, currentStamina - amount);
            _staminaRegenTimer = staminaRegenDelay;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            return true;
        }

        public void TakeDamage(float amount)
        {
            if (_isDead) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (_isDead) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void Eat(float hungerRestore, float healthRestore = 0f)
        {
            currentHunger = Mathf.Min(maxHunger, currentHunger + hungerRestore);
            OnHungerChanged?.Invoke(currentHunger, maxHunger);

            if (healthRestore > 0f) Heal(healthRestore);
        }

        public void Drink(float thirstRestore)
        {
            currentThirst = Mathf.Min(maxThirst, currentThirst + thirstRestore);
            OnThirstChanged?.Invoke(currentThirst, maxThirst);
        }

        public void AdjustTemperature(float targetTemp, float rate)
        {
            currentTemperature = Mathf.MoveTowards(currentTemperature, targetTemp, rate * Time.deltaTime);
        }

        private void Die()
        {
            _isDead = true;
            OnPlayerDied?.Invoke();
            GameLoop.Instance?.Lose();
        }
    }
}
