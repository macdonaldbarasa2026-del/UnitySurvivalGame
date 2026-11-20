using UnityEngine;
using SurvivalGame.Core;
using SurvivalGame.Player;

namespace SurvivalGame.Environment
{
    public class DayNightCycle : MonoBehaviour
    {
        public static DayNightCycle Instance { get; private set; }

        [Header("Cycle Timings")]
        [Tooltip("Full day duration in real-time seconds")]
        [SerializeField] private float dayDurationInSeconds = 180f; // 3 minutes per in-game day
        [Range(0f, 1f)]
        [SerializeField] private float timeOfDay = 0.25f; // Start at sunrise (0.25)

        [Header("Sun & Moon References")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Gradient sunColorGradient;
        [SerializeField] private AnimationCurve sunIntensityCurve;

        [Header("Temperature Influence")]
        [SerializeField] private float dayTemperature = 25f;
        [SerializeField] private float nightTemperature = 10f;
        [SerializeField] private float temperatureLerpSpeed = 2f;

        private int _currentDay = 1;
        private bool _dayCompletionFired = false;
        private PlayerSurvivalStats _playerStats;

        public float TimeOfDay => timeOfDay;
        public int CurrentDay => _currentDay;
        public bool IsNight => timeOfDay < 0.2f || timeOfDay > 0.8f;
        public float AmbientTemperature => Mathf.Lerp(nightTemperature, dayTemperature, Mathf.Sin(timeOfDay * Mathf.PI));

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _playerStats = FindObjectOfType<PlayerSurvivalStats>();
        }

        private void Update()
        {
            if (GameLoop.Instance?.State != RunState.Running) return;

            UpdateTime();
            UpdateLighting();
            UpdateTemperature();
        }

        private void UpdateTime()
        {
            timeOfDay += (Time.deltaTime / dayDurationInSeconds);

            // Handle dawn/next day transition
            if (timeOfDay >= 1f)
            {
                timeOfDay -= 1f;
                _currentDay++;
                _dayCompletionFired = false;
                GameLoop.Instance?.RegisterDayCompleted();
            }
        }

        private void UpdateLighting()
        {
            if (sunLight == null) return;

            // Rotate sun across the sky
            float sunRotationX = (timeOfDay * 360f) - 90f;
            sunLight.transform.rotation = Quaternion.Euler(sunRotationX, 170f, 0f);

            // Adjust color and intensity
            if (sunColorGradient != null)
            {
                sunLight.color = sunColorGradient.Evaluate(timeOfDay);
            }

            if (sunIntensityCurve != null)
            {
                sunLight.intensity = sunIntensityCurve.Evaluate(timeOfDay);
            }
            else
            {
                // Fallback smooth curve
                float intensity = Mathf.Clamp01(Vector3.Dot(sunLight.transform.forward, Vector3.down));
                sunLight.intensity = intensity * 1.2f;
            }
        }

        private void UpdateTemperature()
        {
            if (_playerStats != null)
            {
                _playerStats.AdjustTemperature(AmbientTemperature, temperatureLerpSpeed);
            }
        }
    }
}
