using UnityEngine;
using SurvivalGame.Core;

namespace SurvivalGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Speeds")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float sprintSpeed = 8.0f;
        [SerializeField] private float rotationSmoothTime = 0.1f;
        [SerializeField] private float acceleration = 12f;

        [Header("Jumping & Gravity")]
        [SerializeField] private float jumpHeight = 1.4f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedOffset = -0.15f;
        [SerializeField] private float groundedRadius = 0.35f;
        [SerializeField] private LayerMask groundLayers;

        [Header("Stamina Drain")]
        [SerializeField] private float sprintStaminaCostPerSec = 15f;

        [Header("Procedural Motion / Game Feel")]
        [SerializeField] private Transform visualsRoot;
        [SerializeField] private float bobFrequency = 2.5f;
        [SerializeField] private float bobAmplitude = 0.06f;
        [SerializeField] private float leanAmount = 4f;

        private CharacterController _characterController;
        private PlayerSurvivalStats _stats;
        private Camera _mainCamera;

        private Vector3 _velocity;
        private float _currentSpeed;
        private float _targetRotation;
        private float _rotationVelocity;
        private bool _isGrounded;
        private bool _isSprinting;
        private float _bobTimer;
        private float _stepTimer;
        private Vector3 _defaultVisualsLocalPos;
        private Quaternion _defaultVisualsLocalRot;

        public bool IsMoving => _currentSpeed > 0.1f;
        public bool IsSprinting => _isSprinting;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerSurvivalStats>();
            _mainCamera = Camera.main;

            if (visualsRoot != null)
            {
                _defaultVisualsLocalPos = visualsRoot.localPosition;
                _defaultVisualsLocalRot = visualsRoot.localRotation;
            }

            // Ensure Kinematic Rigidbody + trigger SphereCollider so triggers always fire
            var rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            var triggerCol = GetComponent<SphereCollider>();
            if (triggerCol == null)
            {
                triggerCol = gameObject.AddComponent<SphereCollider>();
                triggerCol.isTrigger = true;
                triggerCol.radius = 1.0f;
            }
        }

        private void Update()
        {
            if (GameLoop.Instance?.State != RunState.Running || (_stats != null && _stats.IsDead))
            {
                return;
            }

            GroundCheck();
            HandleMovement();
            HandleJumpAndGravity();
            ApplyProceduralMotion();
        }

        private void GroundCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
            _isGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
        }

        private void HandleMovement()
        {
            GameInput.Poll();
            Vector2 input = GameInput.Move;
            Vector3 inputDirection = new Vector3(input.x, 0f, input.y).normalized;

            bool sprintRequested = GameInput.Sprint;
            bool hasStamina = _stats != null && _stats.StaminaPercent > 0.05f;

            _isSprinting = sprintRequested && hasStamina && inputDirection.magnitude > 0.1f;

            if (_isSprinting && _stats != null)
            {
                _stats.UseStamina(sprintStaminaCostPerSec * Time.deltaTime);
            }

            float targetSpeed = inputDirection.magnitude > 0.1f
                ? (_isSprinting ? sprintSpeed : walkSpeed)
                : 0f;

            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            if (inputDirection.magnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
                if (_mainCamera != null)
                {
                    targetAngle += _mainCamera.transform.eulerAngles.y;
                }

                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _rotationVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                _characterController.Move(moveDir.normalized * (_currentSpeed * Time.deltaTime));
            }
        }

        private void HandleJumpAndGravity()
        {
            if (GameInput.JumpPressed && _isGrounded)
            {
                if (_stats == null || _stats.UseStamina(15f))
                {
                    _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }

            _velocity.y += gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private void ApplyProceduralMotion()
        {
            if (visualsRoot == null) return;

            float speedNormalized = _currentSpeed / sprintSpeed;

            if (_isGrounded && speedNormalized > 0.05f)
            {
                _stepTimer += Time.deltaTime;
                if (_stepTimer >= (_isSprinting ? 0.28f : 0.46f))
                {
                    _stepTimer = 0f;
                    SurvivalGame.Audio.SoundManager.Instance?.PlayFootstep();
                }

                _bobTimer += Time.deltaTime * bobFrequency * Mathf.PI * 2f;
                float bobOffset = Mathf.Sin(_bobTimer) * bobAmplitude * speedNormalized;
                float rollOffset = Mathf.Cos(_bobTimer * 0.5f) * 1.5f * speedNormalized;

                visualsRoot.localPosition = _defaultVisualsLocalPos + new Vector3(0f, bobOffset, 0f);
                visualsRoot.localRotation = _defaultVisualsLocalRot * Quaternion.Euler(speedNormalized * 4f, 0f, rollOffset);
            }
            else
            {
                _bobTimer = 0f;
                visualsRoot.localPosition = Vector3.Lerp(visualsRoot.localPosition, _defaultVisualsLocalPos, Time.deltaTime * 6f);
                visualsRoot.localRotation = Quaternion.Slerp(visualsRoot.localRotation, _defaultVisualsLocalRot, Time.deltaTime * 6f);
            }
        }
    }
}
