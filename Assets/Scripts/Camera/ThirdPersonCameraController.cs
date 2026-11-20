using UnityEngine;
using SurvivalGame.Core;

namespace SurvivalGame.Cameras
{
    public class ThirdPersonCameraController : MonoBehaviour
    {
        [Header("Target & Offsets")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

        [Header("Distance & Zoom")]
        [SerializeField] private float defaultDistance = 4.5f;
        [SerializeField] private float minDistance = 1.5f;
        [SerializeField] private float maxDistance = 8.0f;
        [SerializeField] private float zoomSpeed = 2.0f;

        [Header("Sensitivity & Limits")]
        [SerializeField] private float mouseSensitivity = 2.5f;
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 70f;
        [SerializeField] private float rotationSmoothTime = 0.05f;

        [Header("Collision Avoidance")]
        [SerializeField] private LayerMask collisionLayers;
        [SerializeField] private float collisionRadius = 0.25f;

        private float _yaw = 0f;
        private float _pitch = 20f;
        private float _targetDistance;
        private float _currentDistance;

        private Vector2 _currentRotationVelocity;
        private Vector2 _currentRotation;

        private void Awake()
        {
            if (collisionLayers == 0) collisionLayers = ~0;
        }

        private void Start()
        {
            _targetDistance = defaultDistance;
            _currentDistance = defaultDistance;
        }

        private void LateUpdate()
        {
            if (target == null) return;
            if (GameLoop.Instance != null && GameLoop.Instance.State != RunState.Running) return;

            HandleInput();
            HandleCollisionAndPosition();
        }

        private void HandleInput()
        {
            GameInput.Poll();
            Vector2 look = GameInput.ConsumeLook();
            _yaw += look.x * mouseSensitivity;
            _pitch -= look.y * mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            float scroll = GameInput.Zoom;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _targetDistance = Mathf.Clamp(_targetDistance - scroll * zoomSpeed, minDistance, maxDistance);
            }
        }

        private void HandleCollisionAndPosition()
        {
            Vector3 focusPoint = target.position + targetOffset;
            Quaternion desiredRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 desiredDirection = desiredRotation * -Vector3.forward;

            // Check if environment blocks camera
            float safeDistance = _targetDistance;
            if (Physics.SphereCast(focusPoint, collisionRadius, desiredDirection, out RaycastHit hit, _targetDistance, collisionLayers))
            {
                safeDistance = Mathf.Clamp(hit.distance - collisionRadius, minDistance, _targetDistance);
            }

            _currentDistance = Mathf.Lerp(_currentDistance, safeDistance, Time.deltaTime * 15f);

            Vector3 finalPosition = focusPoint + desiredDirection * _currentDistance;
            transform.position = finalPosition;
            transform.LookAt(focusPoint);
        }
    }
}
