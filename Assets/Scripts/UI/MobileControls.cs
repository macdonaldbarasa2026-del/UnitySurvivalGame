using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SurvivalGame.Core;

namespace SurvivalGame.UI
{
    /// <summary>
    /// Creates an on-screen joystick, look pad and action buttons at runtime.
    /// Every control feeds GameInput, the same abstraction used by keyboard and mouse.
    /// </summary>
    public class MobileControls : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color(0.06f, 0.07f, 0.1f, 0.45f);
        private static readonly Color ButtonColor = new Color(0.16f, 0.19f, 0.26f, 0.72f);

        private float _lastPinchDistance;

        public static void Create(Transform parent)
        {
            GameObject go = new GameObject("MobileControls");
            go.transform.SetParent(parent, false);
            go.AddComponent<MobileControls>();
        }

        private void Awake()
        {
            if (!Application.isMobilePlatform && !Application.isEditor)
            {
                gameObject.SetActive(false);
                return;
            }

            EnsureEventSystem();
            BuildInterface();
        }

        private void Update()
        {
            if (Input.touchCount == 2)
            {
                float distance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                if (_lastPinchDistance > 0f)
                {
                    GameInput.AddTouchZoom((distance - _lastPinchDistance) * 0.012f);
                }
                _lastPinchDistance = distance;
            }
            else
            {
                _lastPinchDistance = 0f;
            }
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private void BuildInterface()
        {
            GameObject canvasObject = new GameObject("TouchCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            BuildJoystick(canvasObject.transform);
            BuildLookPad(canvasObject.transform);
            BuildButtons(canvasObject.transform);
        }

        private static void BuildJoystick(Transform canvas)
        {
            RectTransform baseRect = MakePanel("MoveJoystick", canvas, new Vector2(0f, 0f), new Vector2(300f, 300f), new Vector2(46f, 46f));
            RectTransform knob = MakeImage("Knob", baseRect, new Vector2(122f, 122f), Vector2.zero, new Color(0.75f, 0.82f, 0.92f, 0.8f));
            VirtualJoystick joystick = baseRect.gameObject.AddComponent<VirtualJoystick>();
            joystick.Setup(baseRect, knob, false);
        }

        private static void BuildLookPad(Transform canvas)
        {
            RectTransform pad = MakePanel("LookPad", canvas, new Vector2(1f, 0f), new Vector2(760f, 720f), new Vector2(-20f, 20f));
            Image image = pad.GetComponent<Image>();
            image.color = new Color(0.2f, 0.25f, 0.35f, 0.05f);
            RectTransform knob = MakeImage("LookKnob", pad, new Vector2(10f, 10f), Vector2.zero, Color.clear);
            VirtualJoystick look = pad.gameObject.AddComponent<VirtualJoystick>();
            look.Setup(pad, knob, true);
        }

        private static void BuildButtons(Transform canvas)
        {
            float x = -270f;
            float y = 470f;

            MakeActionButton(canvas, "ATTACK", new Vector2(x, y), new Vector2(190f, 120f), GameInput.PressTouchAttack);

            bool sprint = false;
            MakeActionButton(canvas, "RUN", new Vector2(x - 200f, y), new Vector2(150f, 110f), () =>
            {
                sprint = !sprint;
                GameInput.SetTouchSprint(sprint);
            });

            MakeActionButton(canvas, "JUMP", new Vector2(x, y - 150f), new Vector2(190f, 110f), GameInput.PressTouchJump);
            MakeActionButton(canvas, "USE", new Vector2(x, y - 290f), new Vector2(190f, 110f), GameInput.PressTouchInteract);

            bool block = false;
            MakeActionButton(canvas, "BLOCK", new Vector2(x, y - 430f), new Vector2(190f, 110f), () =>
            {
                block = !block;
                GameInput.SetTouchBlock(block);
            });

            MakeActionButton(canvas, "BAG", new Vector2(x - 205f, y - 150f), new Vector2(145f, 105f), GameInput.PressTouchInventory);
            MakeActionButton(canvas, "MENU", new Vector2(x - 205f, y - 290f), new Vector2(145f, 105f), GameInput.PressTouchPause);
        }

        private static void MakeActionButton(Transform parent, string label, Vector2 anchoredPosition, Vector2 size, Action onClick)
        {
            RectTransform rect = MakePanel(label + "Button", parent, new Vector2(1f, 0.5f), size, anchoredPosition);
            Image image = rect.GetComponent<Image>();
            image.color = ButtonColor;

            Text text = MakeText(label, rect, new Vector2(size.x - 8f, size.y - 8f), 30, TextAnchor.MiddleCenter);
            text.color = Color.white;

            TouchButton button = rect.gameObject.AddComponent<TouchButton>();
            button.Setup(onClick);
        }

        private static RectTransform MakePanel(string name, Transform parent, Vector2 anchor, Vector2 size, Vector2 position)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            go.GetComponent<Image>().color = PanelColor;
            return rect;
        }

        private static RectTransform MakeImage(string name, Transform parent, Vector2 size, Vector2 position, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            go.GetComponent<Image>().color = color;
            return rect;
        }

        private static Text MakeText(string value, Transform parent, Vector2 size, int fontSize, TextAnchor alignment)
        {
            GameObject go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.sizeDelta = size;

            Text text = go.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }
    }

    /// <summary>Joystick or look-pad touch surface.</summary>
    public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        private bool _isLook;
        private RectTransform _baseRect;
        private RectTransform _knobRect;

        public void Setup(RectTransform baseRect, RectTransform knobRect, bool isLook)
        {
            _baseRect = baseRect;
            _knobRect = knobRect;
            _isLook = isLook;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_isLook) OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isLook)
            {
                GameInput.AddTouchLook(new Vector2(eventData.delta.x, -eventData.delta.y) * 0.24f);
                return;
            }

            if (_baseRect == null) return;
            Camera eventCamera = eventData.pressEventCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_baseRect, eventData.position, eventCamera, out Vector2 local)) return;

            float radius = _baseRect.rect.width * 0.5f;
            Vector2 direction = Vector2.ClampMagnitude(local / radius, 1f);
            if (_knobRect != null) _knobRect.anchoredPosition = direction * radius * 0.58f;
            GameInput.SetTouchMove(direction);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_knobRect != null) _knobRect.anchoredPosition = Vector2.zero;
            if (!_isLook) GameInput.SetTouchMove(Vector2.zero);
        }
    }

    /// <summary>Simple touch button that reports press (and optional release).</summary>
    public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Action _onDown;
        private Action _onUp;

        public void Setup(Action onDown, Action onUp = null)
        {
            _onDown = onDown;
            _onUp = onUp;
        }

        public void OnPointerDown(PointerEventData eventData) => _onDown?.Invoke();
        public void OnPointerUp(PointerEventData eventData) => _onUp?.Invoke();
    }
}
