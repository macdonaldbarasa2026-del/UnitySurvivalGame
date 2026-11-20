using UnityEngine;

namespace SurvivalGame.Core
{
    /// <summary>
    /// Runs before every gameplay script so input state is identical for keyboard and touch.
    /// </summary>
    [DefaultExecutionOrder(-2000)]
    public class GameInputDriver : MonoBehaviour
    {
        private void Update()
        {
            GameInput.Poll();
        }
    }

    /// <summary>
    /// Single input abstraction. Keyboard/mouse and the on-screen mobile controls feed the
    /// same state, so every gameplay script works identically on desktop and Android.
    /// </summary>
    public static class GameInput
    {
        private static int _lastPollFrame = -1;

        private static Vector2 _move;
        private static Vector2 _look;
        private static bool _jump, _attack, _interact, _inventory, _pause;
        private static bool _sprint, _block;
        private static float _zoom;

        private static Vector2 _touchMove;
        private static Vector2 _touchLook;
        private static float _touchZoom;
        private static bool _touchJump, _touchAttack, _touchInteract;
        private static bool _touchInventory, _touchPause;
        private static bool _touchSprint, _touchBlock;

        public static Vector2 Move => _move;
        public static bool Sprint => _sprint;
        public static bool Block => _block;
        public static bool JumpPressed => _jump;
        public static bool AttackPressed => _attack;
        public static bool InteractPressed => _interact;
        public static bool InventoryPressed => _inventory;
        public static bool PausePressed => _pause;
        public static float Zoom => _zoom;
        public static bool IsTouchActive => _touchMove.sqrMagnitude > 0.01f || _touchLook.sqrMagnitude > 0.01f;

        public static void SetTouchMove(Vector2 value) => _touchMove = value;
        public static void AddTouchLook(Vector2 delta) => _touchLook += delta;
        public static void AddTouchZoom(float delta) => _touchZoom += delta;
        public static void PressTouchJump() => _touchJump = true;
        public static void PressTouchAttack() => _touchAttack = true;
        public static void PressTouchInteract() => _touchInteract = true;
        public static void PressTouchInventory() => _touchInventory = true;
        public static void PressTouchPause() => _touchPause = true;
        public static void SetTouchSprint(bool value) => _touchSprint = value;
        public static void SetTouchBlock(bool value) => _touchBlock = value;

        public static Vector2 ConsumeLook()
        {
            Vector2 result = _look;
            _look = Vector2.zero;
            return result;
        }

        public static void Poll()
        {
            if (_lastPollFrame == Time.frameCount) return;
            _lastPollFrame = Time.frameCount;

            Vector2 keyboard = Vector2.zero;
            if (SafeKey(KeyCode.W) || SafeKey(KeyCode.UpArrow)) keyboard.y += 1f;
            if (SafeKey(KeyCode.S) || SafeKey(KeyCode.DownArrow)) keyboard.y -= 1f;
            if (SafeKey(KeyCode.D) || SafeKey(KeyCode.RightArrow)) keyboard.x += 1f;
            if (SafeKey(KeyCode.A) || SafeKey(KeyCode.LeftArrow)) keyboard.x -= 1f;
            if (keyboard.sqrMagnitude > 1f) keyboard.Normalize();

            _move = _touchMove.sqrMagnitude > 0.04f ? _touchMove : keyboard;

            Vector2 mouse = Vector2.zero;
            if (Input.mousePresent)
            {
                mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            }
            _look = mouse + _touchLook;
            _touchLook = Vector2.zero;

            _zoom = SafeAxis("Mouse ScrollWheel") + _touchZoom;
            _touchZoom = 0f;

            _jump = SafeButton("Jump") || SafeKey(KeyCode.Space) || _touchJump;
            _touchJump = false;

            _attack = (Input.mousePresent && Input.GetMouseButtonDown(0)) || SafeKey(KeyCode.F) || _touchAttack;
            _touchAttack = false;

            _interact = SafeKey(KeyCode.E) || _touchInteract;
            _touchInteract = false;

            _inventory = SafeKey(KeyCode.Tab) || SafeKey(KeyCode.I) || _touchInventory;
            _touchInventory = false;

            _pause = SafeKey(KeyCode.Escape) || SafeKey(KeyCode.P) || _touchPause;
            _touchPause = false;

            _sprint = SafeKey(KeyCode.LeftShift) || SafeKey(KeyCode.RightShift) || _touchSprint;
            _block = (Input.mousePresent && Input.GetMouseButton(1)) || SafeKey(KeyCode.Q) || _touchBlock;
        }

        private static bool SafeKey(KeyCode key)
        {
            try { return Input.GetKey(key); }
            catch (System.Exception) { return false; }
        }

        private static bool SafeButton(string button)
        {
            try { return Input.GetButtonDown(button); }
            catch (System.Exception) { return false; }
        }

        private static float SafeAxis(string axis)
        {
            try { return Input.GetAxis(axis); }
            catch (System.Exception) { return 0f; }
        }
    }
}
