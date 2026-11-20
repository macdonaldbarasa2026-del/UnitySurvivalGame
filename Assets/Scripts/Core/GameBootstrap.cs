using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using SurvivalGame.Audio;
using SurvivalGame.Cameras;
using SurvivalGame.Core;
using SurvivalGame.Enemies;
using SurvivalGame.Environment;
using SurvivalGame.Inventory;
using SurvivalGame.Player;
using SurvivalGame.UI;

namespace SurvivalGame.Core
{
    /// <summary>
    /// Single entry point for the game. The committed scene only contains this component,
    /// a camera and a light; everything else is assembled here at runtime. That keeps the
    /// scene tiny, deterministic, and independent of Editor-only authoring steps.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (FindFirstObjectByType<GameLoop>() != null)
            {
                return;
            }

            if (GetComponent<GameInputDriver>() == null) gameObject.AddComponent<GameInputDriver>();

            GameObject systems = new GameObject("=== SYSTEMS ===");
            systems.transform.SetParent(transform, false);

            GameLoop gameLoop = systems.AddComponent<GameLoop>();
            ReflectionUtil.Set(gameLoop, "targetDaysToSurvive", 5);
            systems.AddComponent<InventorySystem>();
            systems.AddComponent<CraftingSystem>();
            SoundManager soundManager = systems.AddComponent<SoundManager>();

            GameObject player = CreatePlayer();
            CreateCamera(player);

            ProceduralAudio.Install(soundManager);

            WorldGenerator.Generate(transform, player);

            WeatherSystem weather = systems.AddComponent<WeatherSystem>();
            ReflectionUtil.Set(weather, "cycleSeconds", 70f);

            InventorySystem inventory = InventorySystem.Instance;
            if (inventory != null)
            {
                inventory.AddItem(ItemDatabase.Berry, 3);
                inventory.AddItem(ItemDatabase.Water, 2);
                inventory.AddItem(ItemDatabase.Fiber, 2);
            }

            CraftingSystem crafting = CraftingSystem.Instance;
            if (crafting != null)
            {
                ReflectionUtil.Set(crafting, "recipes", new List<ItemData>(ItemDatabase.Recipes));
            }

            GameUI.Create(transform);
            MobileControls.Create(transform);

            Debug.Log("[UnitySurvivalGame] Runtime world assembled successfully.");
        }

        private static GameObject CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, WorldGenerator.Height(0f, 0f) + 0.15f, 0f);

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.4f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 48f;

            player.AddComponent<PlayerSurvivalStats>();
            PlayerController movement = player.AddComponent<PlayerController>();
            ReflectionUtil.Set(movement, "groundLayers", ~0);
            player.AddComponent<PlayerCombat>();
            player.AddComponent<PlayerInteraction>();

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Visuals";
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.8f, 0.9f, 0.8f);
            Collider bodyCollider = body.GetComponent<Collider>();
            if (bodyCollider != null) Destroy(bodyCollider);

            bool urp = GraphicsSettings.currentRenderPipeline != null;
            Shader bodyShader = urp ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
            if (bodyShader == null) bodyShader = Shader.Find("Unlit/Color");
            Material bodyMaterial = new Material(bodyShader)
            {
                color = new Color(0.86f, 0.72f, 0.5f)
            };
            if (bodyMaterial.HasProperty("_BaseColor")) bodyMaterial.SetColor("_BaseColor", bodyMaterial.color);
            body.GetComponent<Renderer>().sharedMaterial = bodyMaterial;
            ReflectionUtil.Set(movement, "visualsRoot", body.transform);

            return player;
        }

        private static void CreateCamera(GameObject player)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            if (camera.GetComponent<AudioListener>() == null) camera.gameObject.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0f, 4f, -7f);
            camera.transform.rotation = Quaternion.Euler(16f, 0f, 0f);
            camera.fieldOfView = 68f;

            ThirdPersonCameraController orbit = camera.GetComponent<ThirdPersonCameraController>();
            if (orbit == null) orbit = camera.gameObject.AddComponent<ThirdPersonCameraController>();
            ReflectionUtil.Set(orbit, "target", player.transform);
        }
    }
}
