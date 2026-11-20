using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivalGame.Core;
using SurvivalGame.Player;
using SurvivalGame.Environment;

namespace SurvivalGame.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Survival Stat Sliders")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider staminaSlider;
        [SerializeField] private Slider hungerSlider;
        [SerializeField] private Slider thirstSlider;

        [Header("Text Displays")]
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI temperatureText;
        [SerializeField] private TextMeshProUGUI interactionPromptText;
        [SerializeField] private TextMeshProUGUI goalStatusText;

        [Header("Crosshair")]
        [SerializeField] private Image crosshairImage;

        private PlayerSurvivalStats _stats;
        private PlayerInteraction _interaction;

        private void Start()
        {
            _stats = FindObjectOfType<PlayerSurvivalStats>();
            _interaction = FindObjectOfType<PlayerInteraction>();

            if (_stats != null)
            {
                _stats.OnHealthChanged += UpdateHealth;
                _stats.OnStaminaChanged += UpdateStamina;
                _stats.OnHungerChanged += UpdateHunger;
                _stats.OnThirstChanged += UpdateThirst;
            }

            if (_interaction != null)
            {
                _interaction.OnPromptUpdated += UpdateInteractionPrompt;
            }

            GameLoop.OnDayProgressChanged += UpdateDayProgress;

            if (interactionPromptText != null)
            {
                interactionPromptText.text = string.Empty;
            }
        }

        private void OnDestroy()
        {
            if (_stats != null)
            {
                _stats.OnHealthChanged -= UpdateHealth;
                _stats.OnStaminaChanged -= UpdateStamina;
                _stats.OnHungerChanged -= UpdateHunger;
                _stats.OnThirstChanged -= UpdateThirst;
            }

            if (_interaction != null)
            {
                _interaction.OnPromptUpdated -= UpdateInteractionPrompt;
            }

            GameLoop.OnDayProgressChanged -= UpdateDayProgress;
        }

        private void Update()
        {
            if (_stats != null && temperatureText != null)
            {
                temperatureText.text = $"{_stats.CurrentTemperature:F1}°C";
            }
        }

        private void UpdateHealth(float current, float max)
        {
            if (healthSlider != null) healthSlider.value = current / max;
        }

        private void UpdateStamina(float current, float max)
        {
            if (staminaSlider != null) staminaSlider.value = current / max;
        }

        private void UpdateHunger(float current, float max)
        {
            if (hungerSlider != null) hungerSlider.value = current / max;
        }

        private void UpdateThirst(float current, float max)
        {
            if (thirstSlider != null) thirstSlider.value = current / max;
        }

        private void UpdateInteractionPrompt(string prompt)
        {
            if (interactionPromptText != null)
            {
                interactionPromptText.text = prompt;
            }
        }

        private void UpdateDayProgress(int currentDay, int targetDays)
        {
            if (dayText != null)
            {
                dayText.text = $"Day {currentDay + 1}";
            }

            if (goalStatusText != null)
            {
                if (targetDays > 0)
                {
                    goalStatusText.text = $"Survive {targetDays} Days (Current: {currentDay}/{targetDays})";
                }
                else
                {
                    goalStatusText.text = "Survive as long as possible";
                }
            }
        }
    }
}
