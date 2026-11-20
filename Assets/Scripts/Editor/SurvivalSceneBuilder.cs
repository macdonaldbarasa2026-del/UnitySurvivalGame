#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivalGame.Core;
using SurvivalGame.Player;
using SurvivalGame.Cameras;
using SurvivalGame.Inventory;
using SurvivalGame.Environment;
using SurvivalGame.Enemies;
using SurvivalGame.UI;
using SurvivalGame.Audio;

namespace SurvivalGame.Editor
{
    public static class SurvivalSceneBuilder
    {
        [MenuItem("Tools/Survival Game/Setup Complete Starter Scene")]
        public static void BuildStarterScene()
        {
            // 1. Core Systems Manager
            var managersObj = new GameObject("=== MANAGERS ===");
            managersObj.AddComponent<GameLoop>();
            managersObj.AddComponent<InventorySystem>();
            managersObj.AddComponent<CraftingSystem>();
            managersObj.AddComponent<SoundManager>();

            // 2. Lighting & Environment
            var envObj = new GameObject("=== ENVIRONMENT ===");
            var lightObj = new GameObject("Sun");
            lightObj.transform.SetParent(envObj.transform);
            var sun = lightObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.2f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var dayNight = lightObj.AddComponent<DayNightCycle>();
            var sunField = typeof(DayNightCycle).GetField("sunLight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            sunField?.SetValue(dayNight, sun);

            // Ground Plane
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(envObj.transform);
            ground.transform.localScale = new Vector3(10f, 1f, 10f); // 100x100m
            ground.layer = LayerMask.NameToLayer("Default");

            // 3. Player Rig
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1.0f, 0f);

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            var stats = player.AddComponent<PlayerSurvivalStats>();
            var pController = player.AddComponent<PlayerController>();
            var combat = player.AddComponent<PlayerCombat>();
            var interaction = player.AddComponent<PlayerInteraction>();

            // Visual capsule for player
            var visuals = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visuals.name = "Visuals";
            visuals.transform.SetParent(player.transform);
            visuals.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            visuals.transform.localScale = new Vector3(0.8f, 0.9f, 0.8f);
            Object.DestroyImmediate(visuals.GetComponent<Collider>()); // Avoid duplicate collider

            var visField = typeof(PlayerController).GetField("visualsRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            visField?.SetValue(pController, visuals.transform);

            // 4. Main Camera Rig
            var mainCam = Camera.main;
            if (mainCam == null)
            {
                var camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            var camControl = mainCam.gameObject.GetComponent<ThirdPersonCameraController>();
            if (camControl == null) camControl = mainCam.gameObject.AddComponent<ThirdPersonCameraController>();

            var targetField = typeof(ThirdPersonCameraController).GetField("target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            targetField?.SetValue(camControl, player.transform);

            // 5. Sample Harvestable Resources (Trees & Rocks)
            for (int i = 0; i < 6; i++)
            {
                var tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tree.name = $"Tree_{i + 1}";
                tree.transform.SetParent(envObj.transform);
                tree.transform.position = new Vector3(Random.Range(-25f, 25f), 2.5f, Random.Range(5f, 35f));
                tree.transform.localScale = new Vector3(0.8f, 2.5f, 0.8f);
                tree.AddComponent<HarvestableResource>();
            }

            for (int i = 0; i < 4; i++)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = $"RockNode_{i + 1}";
                rock.transform.SetParent(envObj.transform);
                rock.transform.position = new Vector3(Random.Range(-20f, 20f), 0.6f, Random.Range(-25f, -5f));
                rock.transform.localScale = new Vector3(1.4f, 1.2f, 1.4f);
                rock.AddComponent<HarvestableResource>();
            }

            // 6. Campfire
            var campfireObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            campfireObj.name = "Campfire";
            campfireObj.transform.SetParent(envObj.transform);
            campfireObj.transform.position = new Vector3(3f, 0.15f, 3f);
            campfireObj.transform.localScale = new Vector3(1.2f, 0.3f, 1.2f);
            campfireObj.AddComponent<Campfire>();

            // 7. Enemy
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "WildBeast_Enemy";
            enemy.transform.SetParent(envObj.transform);
            enemy.transform.position = new Vector3(12f, 1.0f, 15f);
            enemy.AddComponent<EnemyHealth>();
            enemy.AddComponent<EnemyAI>();

            // 8. Basic Canvas & HUD setup
            var canvasObj = new GameObject("HUD_Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            canvasObj.AddComponent<HUDController>();
            canvasObj.AddComponent<GameShellUI>();

            Selection.activeGameObject = player;
            Debug.Log("<color=green><b>Survival Game Starter Scene successfully built!</b> Press Play to test.</color>");
        }
    }
}
#endif
