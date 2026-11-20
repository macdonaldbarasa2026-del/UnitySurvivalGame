// Minimal Unity API stubs used ONLY by scripts/compile_check.sh (never shipped in Assets).
// They let the Mono C# compiler type-check the gameplay code without a Unity Editor.
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public class Object
    {
        public string name;
        public static void Destroy(Object o) { }
        public static void Destroy(Object o, float delay) { }
        public static void DestroyImmediate(Object o) { }
        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object => original;
        public static T Instantiate<T>(T original, Transform parent) where T : Object => original;
        public static T FindFirstObjectByType<T>() where T : Object => null;
        public static T FindObjectOfType<T>() where T : Object => null;
        public static implicit operator bool(Object o) => o != null;
    }

    public class Component : Object
    {
        public Transform transform => null;
        public GameObject gameObject => null;
        public T GetComponent<T>() where T : class => null;
        public bool TryGetComponent<T>(out T component) where T : class { component = null; return false; }
        public T GetComponentInChildren<T>() where T : class => null;
        public T[] GetComponents<T>() where T : class => new T[0];
    }

    public class Behaviour : Component { public bool enabled; }

    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(IEnumerator routine) => null;
        public void StopCoroutine(Coroutine routine) { }
    }

    public class Coroutine { }

    public class GameObject : Object
    {
        public GameObject() { }
        public GameObject(string n) { name = n; }
        public GameObject(string n, params Type[] components) { name = n; }
        public string tag;
        public int layer;
        public bool activeInHierarchy = true;
        public Transform transform => null;
        public void SetActive(bool value) { }
        public T AddComponent<T>() where T : Component, new() => new T();
        public Component AddComponent(Type type) => null;
        public T GetComponent<T>() where T : class => null;
        public T GetComponentInChildren<T>() where T : class => null;
        public bool TryGetComponent<T>(out T component) where T : class { component = null; return false; }
        public static GameObject CreatePrimitive(PrimitiveType type) => new GameObject();
        public static GameObject FindGameObjectWithTag(string tagName) => null;
    }

    public enum PrimitiveType { Sphere, Capsule, Cylinder, Cube, Plane, Quad }

    public class Transform : Component, IEnumerable
    {
        public Vector3 position;
        public Vector3 localPosition;
        public Vector3 localScale = Vector3.one;
        public Vector3 eulerAngles;
        public Vector3 localEulerAngles;
        public Quaternion rotation = Quaternion.identity;
        public Quaternion localRotation = Quaternion.identity;
        public Vector3 forward => new Vector3(0, 0, 1);
        public Vector3 up => new Vector3(0, 1, 0);
        public Transform parent;
        public int childCount;
        public void SetParent(Transform p) { }
        public void SetParent(Transform p, bool worldPositionStays) { }
        public void LookAt(Vector3 target) { }
        public void Rotate(Vector3 axis, float degrees) { }
        public Transform GetChild(int index) => null;
        public Transform Find(string name) => null;
        public IEnumerator GetEnumerator() => new Transform[0].GetEnumerator();
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero => new Vector3();
        public static Vector3 one => new Vector3(1, 1, 1);
        public static Vector3 up => new Vector3(0, 1, 0);
        public static Vector3 down => new Vector3(0, -1, 0);
        public static Vector3 forward => new Vector3(0, 0, 1);
        public static Vector3 right => new Vector3(1, 0, 0);
        public float magnitude => (float)Math.Sqrt(x * x + y * y + z * z);
        public float sqrMagnitude => x * x + y * y + z * z;
        public Vector3 normalized => this;
        public void Normalize() { }
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.x, -a.y, -a.z);
        public static Vector3 operator *(Vector3 a, float s) => new Vector3(a.x * s, a.y * s, a.z * s);
        public static Vector3 operator *(float s, Vector3 a) => a * s;
        public static Vector3 operator /(Vector3 a, float s) => new Vector3(a.x / s, a.y / s, a.z / s);
        public static bool operator ==(Vector3 a, Vector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
        public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);
        public override bool Equals(object o) => false;
        public override int GetHashCode() => 0;
        public static float Dot(Vector3 a, Vector3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        public static float Distance(Vector3 a, Vector3 b) => (a - b).magnitude;
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a + (b - a) * t;
        public static Vector3 MoveTowards(Vector3 a, Vector3 b, float d) => a;
        public static Vector3 ClampMagnitude(Vector3 v, float m) => v;
        public static Vector3 Scale(Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public struct Rect
    {
        public float width; public float height;
        public Rect(float x, float y, float width, float height) { this.width = width; this.height = height; }
    }

    public class Sprite : Object { }

    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero => new Vector2();
        public static Vector2 one => new Vector2(1, 1);
        public float magnitude => (float)Math.Sqrt(x * x + y * y);
        public float sqrMagnitude => x * x + y * y;
        public Vector2 normalized => this;
        public void Normalize() { }
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
        public static Vector2 operator *(Vector2 a, float s) => new Vector2(a.x * s, a.y * s);
        public static Vector2 operator *(float s, Vector2 a) => a * s;
        public static Vector2 operator /(Vector2 a, float s) => new Vector2(a.x / s, a.y / s);
        public static float Distance(Vector2 a, Vector2 b) => (float)Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
        public static Vector2 ClampMagnitude(Vector2 v, float m) => v;
        public static Vector2 insideUnitCircle => new Vector2(1, 1);
        public static implicit operator Vector3(Vector2 v) => new Vector3(v.x, 0, v.y);
        public static implicit operator Vector2(Vector3 v) => new Vector2(v.x, v.y);
    }

    public struct Quaternion
    {
        public float x, y, z, w;
        public static Quaternion identity => new Quaternion { w = 1 };
        public static Quaternion Euler(float x, float y, float z) => identity;
        public static Quaternion Euler(Vector3 e) => identity;
        public static Quaternion AngleAxis(float a, Vector3 axis) => identity;
        public static Quaternion LookRotation(Vector3 forward) => identity;
        public static Quaternion Slerp(Quaternion a, Quaternion b, float t) => a;
        public Vector3 eulerAngles => Vector3.zero;
        public static Quaternion operator *(Quaternion a, Quaternion b) => a;
        public static Vector3 operator *(Quaternion q, Vector3 v) => v;
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a = 1f) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white => new Color(1, 1, 1);
        public static Color black => new Color(0, 0, 0);
        public static Color clear => new Color(0, 0, 0, 0);
        public static Color red => new Color(1, 0, 0);
        public static Color yellow => new Color(1, 1, 0);
        public static Color green => new Color(0, 1, 0);
        public static Color cyan => new Color(0, 1, 1);
    }

    public static class Mathf
    {
        public const float PI = 3.14159265f;
        public const float Rad2Deg = 57.29578f;
        public const float Deg2Rad = 0.017453292f;
        public static float Clamp01(float v) => v;
        public static float Clamp(float v, float min, float max) => v;
        public static float Lerp(float a, float b, float t) => a;
        public static float SmoothStep(float a, float b, float t) => t;
        public static float MoveTowards(float a, float b, float d) => b;
        public static float Sin(float f) => 0f;
        public static float Cos(float f) => 1f;
        public static float Exp(float f) => 1f;
        public static float Sqrt(float f) => 1f;
        public static float Abs(float f) => f;
        public static float Pow(float a, float b) => 1f;
        public static float Max(float a, float b) => a;
        public static float Min(float a, float b) => a;
        public static int Max(int a, int b) => a;
        public static int Min(int a, int b) => a;
        public static float Repeat(float t, float l) => 0f;
        public static float Atan2(float y, float x) => 0f;
        public static float PerlinNoise(float x, float y) => 0.5f;
        public static float SmoothDampAngle(float current, float target, ref float velocity, float time) => target;
        public static float DeltaAngle(float a, float b) => 0f;
    }

    public static class Random
    {
        public static float Range(float min, float max) => min;
        public static int Range(int min, int max) => min;
        public static Vector2 insideUnitCircle => Vector2.zero;
        public static Vector3 insideUnitSphere => Vector3.zero;
        public static Quaternion rotation => Quaternion.identity;
    }

    public static class Time
    {
        public static float deltaTime = 0.016f;
        public static float time;
        public static int frameCount;
        public static float timeScale;
    }

    public enum KeyCode { W, A, S, D, UpArrow, DownArrow, LeftArrow, RightArrow, LeftShift, RightShift, Space, F, E, Tab, I, Escape, P, Q }

    public struct Touch
    {
        public Vector2 position;
        public Vector2 delta;
        public int phase;
    }

    public static class Input
    {
        public static bool mousePresent = true;
        public static int touchCount;
        public static Touch GetTouch(int index) => new Touch();
        public static bool GetKey(KeyCode key) => false;
        public static bool GetKeyDown(KeyCode key) => false;
        public static bool GetButtonDown(string button) => false;
        public static float GetAxis(string axis) => 0f;
        public static float GetAxisRaw(string axis) => 0f;
        public static bool GetMouseButton(int button) => false;
        public static bool GetMouseButtonDown(int button) => false;
        public static Vector3 mousePosition;
    }

    public static class Application
    {
        public static bool isMobilePlatform;
        public static bool isEditor = true;
        public static void Quit() { }
    }

    public enum CursorLockMode { None, Locked, Confined }
    public static class Cursor { public static CursorLockMode lockState; public static bool visible; }
    public enum ScreenOrientation { Portrait, PortraitUpsideDown, LandscapeLeft, LandscapeRight, AutoRotation }

    public class Camera : Behaviour
    {
        public static Camera main;
        public float fieldOfView;
        public float nearClipPlane;
        public float farClipPlane;
    }

    public class AudioListener : Behaviour { }
    public class AudioClip : Object
    {
        public static AudioClip Create(string name, int length, int channels, int frequency, bool stream) => new AudioClip();
        public bool SetData(float[] data, int offset) => true;
    }

    public class AudioSource : Behaviour
    {
        public AudioClip clip;
        public bool loop;
        public float volume;
        public float pitch;
        public void Play() { }
        public void PlayOneShot(AudioClip clip, float volumeScale) { }
    }

    public enum LightType { Directional, Point, Spot, Area }
    public enum LightShadows { None, Hard, Soft }
    public class Light : Behaviour
    {
        public LightType type;
        public LightShadows shadows;
        public float intensity;
        public float range;
        public float spotAngle;
        public float innerSpotAngle;
        public Color color;
    }

    public enum BlendMode { SrcAlpha, OneMinusSrcAlpha }
    public enum RenderQueue { Transparent = 3000 }
    public class Shader : Object { public static Shader Find(string name) => new Shader(); }
    public class Material : Object
    {
        public Material(Shader shader) { }
        public Color color;
        public int renderQueue;
        public bool HasProperty(string name) => true;
        public void SetColor(string name, Color value) { }
        public void SetFloat(string name, float value) { }
        public void EnableKeyword(string keyword) { }
    }

    public enum IndexFormat { UInt16, UInt32 }
    public class Mesh : Object
    {
        public IndexFormat indexFormat;
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uv;
        public void RecalculateNormals() { }
        public void RecalculateBounds() { }
    }

    public class Renderer : Component { public Material sharedMaterial; public Material material; }
    public class MeshRenderer : Renderer { }
    public class MeshFilter : Component { public Mesh sharedMesh; }

    public class Collider : Component
    {
        public Vector3 ClosestPoint(Vector3 position) => position;
    }
    public class MeshCollider : Collider { public Mesh sharedMesh; }
    public class SphereCollider : Collider { public float radius; public bool isTrigger; }
    public class Rigidbody : Component { public bool isKinematic; public bool useGravity; }
    public class CharacterController : Collider
    {
        public float height; public float radius; public Vector3 center;
        public float stepOffset; public float slopeLimit;
        public void Move(Vector3 motion) { }
    }

    public struct Ray
    {
        public Ray(Vector3 origin, Vector3 direction) { }
    }

    public struct RaycastHit { public Collider collider; public float distance; }

    public class LayerMask
    {
        public int value;
        public static int NameToLayer(string name) => 0;
        public static implicit operator LayerMask(int v) => new LayerMask { value = v };
        public static implicit operator int(LayerMask m) => m.value;
    }

    public static class Physics
    {
        public static bool CheckSphere(Vector3 p, float r, LayerMask mask, QueryTriggerInteraction q) => true;
        public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hit, float distance, LayerMask mask) { hit = new RaycastHit(); return false; }
        public static bool Raycast(Ray ray, out RaycastHit hit, float distance, LayerMask mask) { hit = new RaycastHit(); return false; }
        public static Collider[] OverlapSphere(Vector3 p, float r, LayerMask mask) => new Collider[0];
        public static Collider[] OverlapSphere(Vector3 p, float r) => new Collider[0];
    }

    public enum QueryTriggerInteraction { UseGlobal, Ignore, Collide }
    public enum FogMode { Linear, Exponential, ExponentialSquared }

    public static class RenderSettings
    {
        public static bool fog;
        public static FogMode fogMode;
        public static float fogDensity;
        public static Color fogColor;
        public static Color ambientLight;
    }

    public struct GradientColorKey { public GradientColorKey(Color color, float time) { } }
    public struct GradientAlphaKey { public GradientAlphaKey(float alpha, float time) { } }
    public class Gradient
    {
        public void SetKeys(GradientColorKey[] colors, GradientAlphaKey[] alphas) { }
        public Color Evaluate(float time) => Color.white;
    }
    public struct Keyframe { public Keyframe(float time, float value) { } }
    public class AnimationCurve { public AnimationCurve(params Keyframe[] keys) { } public float Evaluate(float t) => 1f; }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject, new() => new T();
    }

    [AttributeUsage(AttributeTargets.Field)] public class SerializeField : Attribute { }
    [AttributeUsage(AttributeTargets.Class)] public class CreateAssetMenuAttribute : Attribute
    {
        public string fileName; public string menuName;
    }
    [AttributeUsage(AttributeTargets.Field)] public class HeaderAttribute : Attribute { public HeaderAttribute(string s) { } }
    [AttributeUsage(AttributeTargets.Field)] public class TooltipAttribute : Attribute { public TooltipAttribute(string s) { } }
    [AttributeUsage(AttributeTargets.Field)] public class RangeAttribute : Attribute { public RangeAttribute(float a, float b) { } }
    [AttributeUsage(AttributeTargets.Field)] public class TextAreaAttribute : Attribute { public TextAreaAttribute(int a, int b) { } }
    [AttributeUsage(AttributeTargets.Class)] public class RequireComponent : Attribute { public RequireComponent(Type t) { } }
    [AttributeUsage(AttributeTargets.Class)] public class DefaultExecutionOrder : Attribute { public DefaultExecutionOrder(int order) { } }

    public static class Resources { public static T GetBuiltinResource<T>(string path) where T : Object => null; }

    public class Font : Object { public static Font CreateDynamicFontFromOSFont(string name, int size) => new Font(); }
    public static class Debug { public static void Log(object message) { } public static void LogError(object message) { } public static void LogWarning(object message) { } }

    public static class Gizmos
    {
        public static Color color;
        public static void DrawWireSphere(Vector3 center, float radius) { }
    }

    public class ParticleSystem : Component
    {
        public struct MainModule
        {
            public bool loop; public float startLifetime; public float startSpeed; public float startSize;
            public int maxParticles; public ParticleSystemSimulationSpace simulationSpace;
            public float gravityModifier; public bool playOnAwake;
        }
        public struct ShapeModule { public ParticleSystemShapeType shapeType; public Vector3 scale; }
        public struct EmissionModule { public float rateOverTime; }
        public MainModule main;
        public ShapeModule shape;
        public EmissionModule emission;
    }

    public enum ParticleSystemSimulationSpace { Local, World }
    public enum ParticleSystemShapeType { Sphere, Box }
    public enum ParticleSystemRenderMode { Billboard, Stretch }
    public class ParticleSystemRenderer : Renderer
    {
        public ParticleSystemRenderMode renderMode;
        public float lengthScale;
    }
}

namespace UnityEngine.Rendering
{
    public static class GraphicsSettings { public static object currentRenderPipeline; }
}

namespace UnityEngine.UI
{
    using UnityEngine;
    using UnityEngine.Events;

    public enum TextAnchor { UpperLeft, UpperCenter, UpperRight, MiddleLeft, MiddleCenter, MiddleRight, LowerLeft, LowerCenter, LowerRight }
    public enum HorizontalWrapMode { Wrap, Overflow }
    public enum VerticalWrapMode { Truncate, Overflow }

    public class Graphic : Behaviour { public Color color; public bool raycastTarget; }
    public class Image : Graphic
    {
        public enum Type { Simple, Sliced, Filled };
        public enum FillMethod { Horizontal, Vertical };
        public Type type; public FillMethod fillMethod; public float fillAmount; public Sprite sprite;
    }
    public class Text : Graphic
    {
        public string text; public Font font; public int fontSize; public TextAnchor alignment;
        public HorizontalWrapMode horizontalOverflow; public VerticalWrapMode verticalOverflow;
    }
    public class Selectable : Behaviour { public Graphic targetGraphic; }
    public class Button : Selectable
    {
        public ButtonClickedEvent onClick = new ButtonClickedEvent();
        public class ButtonClickedEvent : UnityEvent { }
    }
    public class Slider : Selectable { public float value; public UnityEvent<float> onValueChanged; }
    public class GraphicRaycaster : Behaviour { }
    public class Canvas : Behaviour { public RenderMode renderMode; public int sortingOrder; }
    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }

    public class CanvasScaler : Behaviour
    {
        public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize }
        public ScaleMode uiScaleMode;
        public Vector2 referenceResolution;
        public float matchWidthOrHeight;
    }

    public class RectTransform : Transform
    {
        public Vector2 anchorMin; public Vector2 anchorMax; public Vector2 pivot;
        public Vector2 sizeDelta; public Vector2 anchoredPosition;
        public Vector2 offsetMin; public Vector2 offsetMax;
        public Rect rect;
    }

    public class GridLayoutGroup : Behaviour
    {
        public enum Constraint { Flexible, FixedColumnCount, FixedRowCount }
        public Vector2 cellSize; public Vector2 spacing; public Constraint constraint;
        public int constraintCount; public TextAnchor childAlignment;
    }

    public class VerticalLayoutGroup : Behaviour
    {
        public float spacing; public bool childControlHeight; public bool childForceExpandHeight;
    }
}

namespace UnityEngine.Events
{
    public delegate void UnityAction();
    public class UnityEvent
    {
        public void AddListener(UnityAction action) { }
        public void RemoveListener(UnityAction action) { }
    }
    public class UnityEvent<T>
    {
        public void AddListener(Action<T> action) { }
        public void RemoveListener(Action<T> action) { }
    }
}

namespace UnityEngine.EventSystems
{
    using UnityEngine;
    using UnityEngine.UI;

    public class PointerEventData
    {
        public Vector2 position;
        public Vector2 delta;
        public Camera pressEventCamera;
    }

    public interface IPointerDownHandler { void OnPointerDown(PointerEventData eventData); }
    public interface IPointerUpHandler { void OnPointerUp(PointerEventData eventData); }
    public interface IDragHandler { void OnDrag(PointerEventData eventData); }

    public class EventSystem : MonoBehaviour { public static EventSystem current; }
    public class StandaloneInputModule : MonoBehaviour { }

    public static class RectTransformUtility
    {
        public static bool ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera camera, out Vector2 localPoint)
        {
            localPoint = screenPoint;
            return true;
        }
    }
}

namespace UnityEngine.AI
{
    using UnityEngine;
    public class NavMeshAgent : Behaviour
    {
        public bool isOnNavMesh; public bool isStopped; public float speed;
        public void SetDestination(Vector3 destination) { }
    }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene { public int buildIndex; }
    public static class SceneManager
    {
        public static Scene GetActiveScene() => new Scene();
        public static void LoadScene(int index) { }
    }
}

namespace TMPro
{
    public class TextMeshProUGUI : UnityEngine.UI.Graphic { public string text; }
}

namespace UnityEditor.Build.Reporting
{
    public enum BuildStatus { Succeeded, Failed }
    public class BuildSummary
    {
        public BuildResult result; public int totalErrors; public long totalSize;
    }
    public class BuildReport
    {
        public BuildSummary summary = new BuildSummary();
    }
}

namespace UnityEditor
{
    using System;
    using UnityEngine;
    using UnityEditor.Build.Reporting;

    public static class EditorApplication { public static bool isPlaying; }
    public static class Selection { public static UnityEngine.Object activeGameObject; }
    [AttributeUsage(AttributeTargets.Method)] public class MenuItem : Attribute { public MenuItem(string s) { } }

    public enum BuildResult { Succeeded, Failed, Cancelled }
    [Flags] public enum BuildOptions { None = 0, Development = 1 }
    public enum BuildTarget { Android, StandaloneWindows64, WebGL }
    public enum NamedBuildTarget { Android, Standalone, WebGL, iPhone }
    public enum ScriptingImplementation { Mono2x, IL2CPP }
    public enum AndroidArchitecture { ARMv7 = 1, ARM64 = 2 }
    public enum AndroidSdkVersions { AndroidApiLevel23 = 23, AndroidApiLevel35 = 35 }
    public enum AndroidBuildSystem { Internal, Gradle, ADT }

    public struct EditorBuildSettingsScene
    {
        public bool enabled; public string path;
    }

    public static class EditorBuildSettings
    {
        public static EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[0];
    }

    public static class EditorUserBuildSettings
    {
        public static bool development;
        public static AndroidBuildSystem androidBuildSystem;
    }

    public static class PlayerSettings
    {
        public static string companyName; public static string productName; public static string bundleVersion;
        public static ScreenOrientation defaultScreenOrientation;
        public static void SetApplicationIdentifier(NamedBuildTarget target, string id) { }
        public static void SetScriptingBackend(NamedBuildTarget target, ScriptingImplementation backend) { }
        public static class Android
        {
            public static AndroidArchitecture targetArchitectures;
            public static AndroidSdkVersions minSdkVersion;
            public static AndroidSdkVersions targetSdkVersion;
            public static int bundleVersionCode;
            public static bool forceInternetPermission;
        }
    }

    public struct BuildPlayerOptions
    {
        public string[] scenes; public string locationPathName; public BuildTarget target; public BuildOptions options;
    }

    public static class BuildPipeline
    {
        public static BuildReport BuildPlayer(BuildPlayerOptions options) => new BuildReport();
    }
}
