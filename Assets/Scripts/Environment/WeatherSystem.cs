using UnityEngine;
using UnityEngine.Rendering;
using SurvivalGame.Core;

namespace SurvivalGame.Environment
{
    /// <summary>
    /// Procedural rain and fog cycles. All effects are generated in code, so no particle
    /// assets or downloads are involved.
    /// </summary>
    public class WeatherSystem : MonoBehaviour
    {
        [SerializeField] private float cycleSeconds = 80f;
        [SerializeField] private float rainIntensity = 520f;

        private ParticleSystem _rain;
        private Transform _player;
        private float _timer;
        private bool _raining;

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) _player = playerObject.transform;
            _rain = CreateRain();
        }

        private void Update()
        {
            if (GameLoop.Instance != null && GameLoop.Instance.State != RunState.Running) return;

            _timer += Time.deltaTime;
            if (_timer >= cycleSeconds)
            {
                _timer = 0f;
                _raining = !_raining;
                ApplyWeather();
            }

            if (_rain != null && _player != null)
            {
                _rain.transform.position = _player.position + new Vector3(0f, 14f, 0f);
            }
        }

        private void ApplyWeather()
        {
            if (_rain == null) return;

            ParticleSystem.EmissionModule emission = _rain.emission;
            emission.rateOverTime = _raining ? rainIntensity : 0f;
            RenderSettings.fogDensity = _raining ? 0.014f : 0.0065f;
            RenderSettings.fogColor = _raining
                ? new Color(0.42f, 0.48f, 0.55f)
                : new Color(0.62f, 0.71f, 0.8f);
        }

        private static ParticleSystem CreateRain()
        {
            GameObject rainObject = new GameObject("Rain");
            ParticleSystem system = rainObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.startLifetime = 1.1f;
            main.startSpeed = 22f;
            main.startSize = 0.045f;
            main.maxParticles = 1100;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.45f;
            main.playOnAwake = true;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(34f, 0.2f, 34f);

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;

            bool urp = GraphicsSettings.currentRenderPipeline != null;
            Shader shader = urp
                ? Shader.Find("Universal Render Pipeline/Particles/Unlit")
                : Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");

            ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 3f;
            renderer.material = new Material(shader) { color = new Color(0.72f, 0.82f, 1f, 0.55f) };

            return system;
        }
    }
}
