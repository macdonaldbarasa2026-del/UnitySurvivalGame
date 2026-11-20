using UnityEngine;
using UnityEngine.Rendering;
using SurvivalGame.Core;
using SurvivalGame.Inventory;
using SurvivalGame.Enemies;
using SurvivalGame.Player;

namespace SurvivalGame.Environment
{
    /// <summary>
    /// Builds the complete 3D world at runtime from code: no external models, textures or
    /// scene assets are required, which keeps the Android build small and fully offline.
    /// </summary>
    public static class WorldGenerator
    {
        public const float WorldSize = 160f;

        private static Material _groundMat;
        private static Material _waterMat;
        private static Material _trunkMat;
        private static Material _leafMat;
        private static Material _rockMat;
        private static Material _plantMat;
        private static Material _berryMat;
        private static Material _wallMat;
        private static Material _roofMat;
        private static Material _fireMat;
        private static Material _deerMat;
        private static Material _wolfMat;
        private static Material _beastMat;

        public static Light Generate(Transform root, GameObject player)
        {
            EnsureMaterials();

            GameObject world = new GameObject("=== PROCEDURAL WORLD ===");
            world.transform.SetParent(root, false);

            BuildTerrain(world.transform);
            BuildWater(world.transform);
            BuildVegetation(world.transform);
            BuildSettlement(world.transform);
            BuildWildlife(world.transform);
            BuildEnemies(world.transform, player);
            BuildPickups(world.transform);

            return BuildLighting(world.transform);
        }

        public static float Height(float x, float z)
        {
            float baseHeight = Mathf.PerlinNoise(x * 0.024f + 11f, z * 0.024f + 11f) * 5.5f;
            float ridge = Mathf.PerlinNoise(x * 0.011f + 47f, z * 0.011f + 47f);
            float edge = Mathf.Clamp01(((Mathf.Abs(x) + Mathf.Abs(z)) / WorldSize) * 1.7f - 0.45f);
            float height = baseHeight + ridge * 17f * edge * edge;

            float riverX = Mathf.Sin(z * 0.045f) * 13f;
            float riverDistance = Mathf.Abs(x - riverX);
            height = Mathf.Lerp(0.02f, height, Mathf.SmoothStep(0f, 7f, riverDistance));

            float lakeDistance = Vector2.Distance(new Vector2(x, z), new Vector2(-36f, -30f));
            height = Mathf.Lerp(-0.7f, height, Mathf.SmoothStep(9f, 20f, lakeDistance));

            return height;
        }

        private static void BuildTerrain(Transform parent)
        {
            const int resolution = 72;
            Vector3[] vertices = new Vector3[(resolution + 1) * (resolution + 1)];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[resolution * resolution * 6];

            for (int z = 0; z <= resolution; z++)
            {
                for (int x = 0; x <= resolution; x++)
                {
                    float wx = (x / (float)resolution - 0.5f) * WorldSize;
                    float wz = (z / (float)resolution - 0.5f) * WorldSize;
                    int index = z * (resolution + 1) + x;
                    vertices[index] = new Vector3(wx, Height(wx, wz), wz);
                    uvs[index] = new Vector2(x / (float)resolution * 10f, z / (float)resolution * 10f);
                }
            }

            int t = 0;
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int i = z * (resolution + 1) + x;
                    triangles[t++] = i;
                    triangles[t++] = i + resolution + 1;
                    triangles[t++] = i + 1;
                    triangles[t++] = i + 1;
                    triangles[t++] = i + resolution + 1;
                    triangles[t++] = i + resolution + 2;
                }
            }

            Mesh mesh = new Mesh { name = "SurvivalTerrain" };
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject terrain = new GameObject("Terrain");
            terrain.transform.SetParent(parent, false);
            terrain.AddComponent<MeshFilter>().sharedMesh = mesh;
            terrain.AddComponent<MeshRenderer>().sharedMaterial = _groundMat;
            terrain.AddComponent<MeshCollider>().sharedMesh = mesh;
        }

        private static void BuildWater(Transform parent)
        {
            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = "RiverAndLake";
            water.transform.SetParent(parent, false);
            water.transform.position = new Vector3(0f, 0.14f, 0f);
            water.transform.localScale = new Vector3(WorldSize / 10f, 1f, WorldSize / 10f);
            water.GetComponent<Renderer>().sharedMaterial = _waterMat;
            Collider collider = water.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
        }

        private static void BuildVegetation(Transform parent)
        {
            for (int i = 0; i < 52; i++)
            {
                float x = Random.Range(-72f, 72f);
                float z = Random.Range(-72f, 72f);
                float h = Height(x, z);
                if (h < 0.4f) continue;

                float scale = Random.Range(0.85f, 1.35f);
                GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = "Tree_" + i;
                trunk.transform.SetParent(parent, false);
                trunk.transform.position = new Vector3(x, h + 1.5f * scale, z);
                trunk.transform.localScale = new Vector3(0.42f * scale, 1.5f * scale, 0.42f * scale);
                trunk.GetComponent<Renderer>().sharedMaterial = _trunkMat;

                GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopy.name = "Canopy";
                canopy.transform.SetParent(trunk.transform, false);
                canopy.transform.localPosition = new Vector3(0f, 1.9f, 0f);
                canopy.transform.localScale = new Vector3(1.7f, 1.5f, 1.7f);
                canopy.GetComponent<Renderer>().sharedMaterial = _leafMat;
                Collider canopyCollider = canopy.GetComponent<Collider>();
                if (canopyCollider != null) Object.Destroy(canopyCollider);

                HarvestableResource resource = trunk.AddComponent<HarvestableResource>();
                ReflectionUtil.Set(resource, "resourceName", "Oak Tree");
                ReflectionUtil.Set(resource, "maxHealth", 55f);
                ReflectionUtil.Set(resource, "dropItem", ItemDatabase.Wood);
                ReflectionUtil.Set(resource, "minDropAmount", 3);
                ReflectionUtil.Set(resource, "maxDropAmount", 6);
            }

            for (int i = 0; i < 26; i++)
            {
                float x = Random.Range(-70f, 70f);
                float z = Random.Range(-70f, 70f);
                float h = Height(x, z);
                if (h < 0.3f) continue;

                float s = Random.Range(0.7f, 1.5f);
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = "RockNode_" + i;
                rock.transform.SetParent(parent, false);
                rock.transform.position = new Vector3(x, h + 0.25f * s, z);
                rock.transform.localScale = new Vector3(1.1f * s, 0.85f * s, 1.0f * s);
                rock.GetComponent<Renderer>().sharedMaterial = _rockMat;

                HarvestableResource resource = rock.AddComponent<HarvestableResource>();
                ReflectionUtil.Set(resource, "resourceName", "Stone Outcrop");
                ReflectionUtil.Set(resource, "maxHealth", 70f);
                ReflectionUtil.Set(resource, "dropItem", ItemDatabase.Stone);
                ReflectionUtil.Set(resource, "minDropAmount", 2);
                ReflectionUtil.Set(resource, "maxDropAmount", 5);
            }

            for (int i = 0; i < 30; i++)
            {
                float x = Random.Range(-60f, 60f);
                float z = Random.Range(-60f, 60f);
                float h = Height(x, z);
                if (h < 0.3f) continue;

                GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                plant.name = "FiberPlant_" + i;
                plant.transform.SetParent(parent, false);
                plant.transform.position = new Vector3(x, h + 0.35f, z);
                plant.transform.localScale = new Vector3(0.12f, 0.35f, 0.12f);
                plant.GetComponent<Renderer>().sharedMaterial = _plantMat;

                HarvestableResource resource = plant.AddComponent<HarvestableResource>();
                ReflectionUtil.Set(resource, "resourceName", "Fiber Plant");
                ReflectionUtil.Set(resource, "maxHealth", 15f);
                ReflectionUtil.Set(resource, "dropItem", ItemDatabase.Fiber);
                ReflectionUtil.Set(resource, "minDropAmount", 2);
                ReflectionUtil.Set(resource, "maxDropAmount", 4);
            }

            for (int i = 0; i < 16; i++)
            {
                float x = Random.Range(-55f, 55f);
                float z = Random.Range(-55f, 55f);
                float h = Height(x, z);
                if (h < 0.3f) continue;

                GameObject bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bush.name = "BerryBush_" + i;
                bush.transform.SetParent(parent, false);
                bush.transform.position = new Vector3(x, h + 0.55f, z);
                bush.transform.localScale = new Vector3(1.1f, 0.9f, 1.1f);
                bush.GetComponent<Renderer>().sharedMaterial = _berryMat;

                HarvestableResource resource = bush.AddComponent<HarvestableResource>();
                ReflectionUtil.Set(resource, "resourceName", "Berry Bush");
                ReflectionUtil.Set(resource, "maxHealth", 20f);
                ReflectionUtil.Set(resource, "dropItem", ItemDatabase.Berry);
                ReflectionUtil.Set(resource, "minDropAmount", 2);
                ReflectionUtil.Set(resource, "maxDropAmount", 4);
            }
        }

        private static void BuildSettlement(Transform parent)
        {
            float h = Height(24f, 24f);
            GameObject cabin = new GameObject("Shelter_Cabin");
            cabin.transform.SetParent(parent, false);
            cabin.transform.position = new Vector3(24f, h, 24f);

            Box("Floor", cabin.transform, new Vector3(0f, 0.15f, 0f), new Vector3(8f, 0.3f, 7f), _wallMat);
            Box("WallBack", cabin.transform, new Vector3(0f, 1.7f, -3.4f), new Vector3(8f, 3f, 0.3f), _wallMat);
            Box("WallLeft", cabin.transform, new Vector3(-3.85f, 1.7f, 0f), new Vector3(0.3f, 3f, 7f), _wallMat);
            Box("WallRight", cabin.transform, new Vector3(3.85f, 1.7f, 0f), new Vector3(0.3f, 3f, 7f), _wallMat);
            Box("WallFrontLeft", cabin.transform, new Vector3(-2.3f, 1.7f, 3.4f), new Vector3(3.4f, 3f, 0.3f), _wallMat);
            Box("WallFrontRight", cabin.transform, new Vector3(2.3f, 1.7f, 3.4f), new Vector3(3.4f, 3f, 0.3f), _wallMat);

            GameObject roofLeft = Box("RoofLeft", cabin.transform, new Vector3(-2.1f, 3.7f, 0f), new Vector3(5f, 0.3f, 8f), _roofMat);
            roofLeft.transform.localRotation = Quaternion.Euler(0f, 0f, 22f);
            GameObject roofRight = Box("RoofRight", cabin.transform, new Vector3(2.1f, 3.7f, 0f), new Vector3(5f, 0.3f, 8f), _roofMat);
            roofRight.transform.localRotation = Quaternion.Euler(0f, 0f, -22f);

            float fireH = Height(28f, 32f);
            GameObject fireBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fireBase.name = "Campfire";
            fireBase.transform.SetParent(parent, false);
            fireBase.transform.position = new Vector3(28f, fireH + 0.16f, 32f);
            fireBase.transform.localScale = new Vector3(1.3f, 0.16f, 1.3f);
            fireBase.GetComponent<Renderer>().sharedMaterial = _rockMat;

            GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.name = "Flame";
            flame.transform.SetParent(fireBase.transform, false);
            flame.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            flame.transform.localScale = new Vector3(0.65f, 1f, 0.65f);
            flame.GetComponent<Renderer>().sharedMaterial = _fireMat;
            Collider flameCollider = flame.GetComponent<Collider>();
            if (flameCollider != null) Object.Destroy(flameCollider);

            Light fireLight = flame.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.color = new Color(1f, 0.55f, 0.2f);
            fireLight.intensity = 2.2f;
            fireLight.range = 12f;

            Campfire campfire = fireBase.AddComponent<Campfire>();
            ReflectionUtil.Set(campfire, "fireVfx", flame);
            ReflectionUtil.Set(campfire, "fireLight", fireLight);
            ReflectionUtil.Set(campfire, "heatRadius", 7f);
            ReflectionUtil.Set(campfire, "fuelTimeRemaining", 420f);
        }

        private static void BuildWildlife(Transform parent)
        {
            for (int i = 0; i < 5; i++)
            {
                float x = Random.Range(-50f, 50f);
                float z = Random.Range(-50f, 50f);
                float h = Height(x, z);
                if (h < 0.3f) continue;

                GameObject deer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                deer.name = "Deer_" + i;
                deer.transform.SetParent(parent, false);
                deer.transform.position = new Vector3(x, h + 0.9f, z);
                deer.transform.localScale = new Vector3(0.55f, 0.75f, 0.55f);
                deer.GetComponent<Renderer>().sharedMaterial = _deerMat;
                deer.AddComponent<AnimalAI>();

                HarvestableResource resource = deer.AddComponent<HarvestableResource>();
                ReflectionUtil.Set(resource, "resourceName", "Deer");
                ReflectionUtil.Set(resource, "maxHealth", 30f);
                ReflectionUtil.Set(resource, "dropItem", ItemDatabase.Meat);
                ReflectionUtil.Set(resource, "minDropAmount", 1);
                ReflectionUtil.Set(resource, "maxDropAmount", 3);
            }

            for (int i = 0; i < 3; i++)
            {
                float x = Random.Range(-55f, 55f);
                float z = Random.Range(-55f, 55f);
                float h = Height(x, z);
                if (h < 0.3f) continue;

                GameObject wolf = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                wolf.name = "Wolf_" + i;
                wolf.transform.SetParent(parent, false);
                wolf.transform.position = new Vector3(x, h + 0.75f, z);
                wolf.transform.localScale = new Vector3(0.6f, 0.65f, 0.6f);
                wolf.GetComponent<Renderer>().sharedMaterial = _wolfMat;
                wolf.AddComponent<AnimalAI>();
            }
        }

        private static void BuildEnemies(Transform parent, GameObject player)
        {
            Vector3[] spawns =
            {
                new Vector3(14f, 0f, 16f),
                new Vector3(-22f, 0f, 14f),
                new Vector3(8f, 0f, -28f),
                new Vector3(-14f, 0f, -18f)
            };

            for (int i = 0; i < spawns.Length; i++)
            {
                Vector3 spawn = spawns[i];
                float h = Height(spawn.x, spawn.z);
                GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = "WildBeast_" + i;
                enemy.transform.SetParent(parent, false);
                enemy.transform.position = new Vector3(spawn.x, h + 1f, spawn.z);
                enemy.transform.localScale = new Vector3(0.9f, 1.05f, 0.9f);
                enemy.GetComponent<Renderer>().sharedMaterial = _beastMat;

                EnemyHealth health = enemy.AddComponent<EnemyHealth>();
                ReflectionUtil.Set(health, "maxHealth", 55f);
                ReflectionUtil.Set(health, "lootItem", ItemDatabase.Meat);

                EnemyAI ai = enemy.AddComponent<EnemyAI>();
                if (player != null) ReflectionUtil.Set(ai, "targetPlayer", player.transform);
                ReflectionUtil.Set(ai, "detectionMask", ~0);
                ReflectionUtil.Set(ai, "sightRadius", 14f);
                ReflectionUtil.Set(ai, "patrolRadius", 10f);
            }
        }

        private static void BuildPickups(Transform parent)
        {
            SpawnPickup(parent, ItemDatabase.Water, 2, new Vector3(4f, 0f, 4f));
            SpawnPickup(parent, ItemDatabase.Berry, 3, new Vector3(-5f, 0f, 6f));
            SpawnPickup(parent, ItemDatabase.Fiber, 3, new Vector3(7f, 0f, -4f));
        }

        private static void SpawnPickup(Transform parent, ItemData item, int amount, Vector3 position)
        {
            float h = Height(position.x, position.z);
            GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pickup.name = "Pickup_" + item.itemId;
            pickup.transform.SetParent(parent, false);
            pickup.transform.position = new Vector3(position.x, h + 0.6f, position.z);
            pickup.transform.localScale = Vector3.one * 0.45f;
            pickup.GetComponent<Renderer>().sharedMaterial = item.itemType == ItemType.Drink ? _waterMat : _berryMat;

            WorldItemPickup worldItem = pickup.AddComponent<WorldItemPickup>();
            ReflectionUtil.Set(worldItem, "itemData", item);
            ReflectionUtil.Set(worldItem, "quantity", amount);
        }

        private static Light BuildLighting(Transform parent)
        {
            GameObject sunObject = new GameObject("Sun");
            sunObject.transform.SetParent(parent, false);
            sunObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.15f;
            sun.color = new Color(1f, 0.96f, 0.86f);
            sun.shadows = LightShadows.Soft;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.5f, 0.25f), 0f),
                    new GradientColorKey(new Color(1f, 0.92f, 0.78f), 0.25f),
                    new GradientColorKey(new Color(1f, 1f, 1f), 0.5f),
                    new GradientColorKey(new Color(1f, 0.85f, 0.6f), 0.78f),
                    new GradientColorKey(new Color(1f, 0.5f, 0.25f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                });

            AnimationCurve intensity = new AnimationCurve(
                new Keyframe(0f, 0.05f),
                new Keyframe(0.18f, 0.45f),
                new Keyframe(0.32f, 1.1f),
                new Keyframe(0.62f, 1.15f),
                new Keyframe(0.82f, 0.4f),
                new Keyframe(1f, 0.05f));

            DayNightCycle cycle = sunObject.AddComponent<DayNightCycle>();
            ReflectionUtil.Set(cycle, "sunLight", sun);
            ReflectionUtil.Set(cycle, "sunColorGradient", gradient);
            ReflectionUtil.Set(cycle, "sunIntensityCurve", intensity);
            ReflectionUtil.Set(cycle, "dayDurationInSeconds", 150f);
            ReflectionUtil.Set(cycle, "dayTemperature", 26f);
            ReflectionUtil.Set(cycle, "nightTemperature", 6f);

            RenderSettings.ambientLight = new Color(0.42f, 0.47f, 0.55f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0065f;
            RenderSettings.fogColor = new Color(0.62f, 0.71f, 0.8f);

            return sun;
        }

        private static GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
            return box;
        }

        private static void EnsureMaterials()
        {
            if (_groundMat != null) return;
            _groundMat = MakeMaterial("Ground", new Color(0.29f, 0.46f, 0.24f));
            _waterMat = MakeMaterial("Water", new Color(0.18f, 0.45f, 0.68f, 0.72f), true);
            _trunkMat = MakeMaterial("Trunk", new Color(0.36f, 0.24f, 0.13f));
            _leafMat = MakeMaterial("Leaves", new Color(0.18f, 0.42f, 0.18f));
            _rockMat = MakeMaterial("Rock", new Color(0.43f, 0.43f, 0.46f));
            _plantMat = MakeMaterial("Fiber", new Color(0.45f, 0.66f, 0.25f));
            _berryMat = MakeMaterial("Berries", new Color(0.62f, 0.15f, 0.2f));
            _wallMat = MakeMaterial("WoodWalls", new Color(0.55f, 0.4f, 0.25f));
            _roofMat = MakeMaterial("Roof", new Color(0.32f, 0.22f, 0.16f));
            _fireMat = MakeMaterial("Fire", new Color(1f, 0.52f, 0.12f));
            _deerMat = MakeMaterial("Deer", new Color(0.68f, 0.5f, 0.32f));
            _wolfMat = MakeMaterial("Wolf", new Color(0.4f, 0.42f, 0.45f));
            _beastMat = MakeMaterial("Beast", new Color(0.46f, 0.16f, 0.16f));
        }

        private static Material MakeMaterial(string name, Color color, bool transparent = false)
        {
            bool urp = GraphicsSettings.currentRenderPipeline != null;
            Shader shader = urp ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Standard") ?? Shader.Find("Unlit/Color");

            Material material = new Material(shader) { name = name, color = color };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);

            if (transparent)
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)RenderQueue.Transparent;
            }

            return material;
        }
    }
}
