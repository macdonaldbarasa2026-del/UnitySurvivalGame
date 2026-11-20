using System;
using UnityEngine;
using SurvivalGame.Core;

namespace SurvivalGame.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float interactionDistance = 2.5f;
        [SerializeField] private LayerMask interactableLayers;

        private Camera _camera;
        private PlayerController _playerController;
        private IInteractable _currentInteractable;

        public IInteractable CurrentInteractable => _currentInteractable;
        public event Action<string> OnPromptUpdated;

        private void Awake()
        {
            _camera = Camera.main;
            _playerController = GetComponent<PlayerController>();
            if (interactableLayers == 0) interactableLayers = ~0;
        }

        private void Update()
        {
            if (GameLoop.Instance?.State != RunState.Running) return;

            DetectInteractables();
            HandleInput();
        }

        private void DetectInteractables()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;

            Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayers))
            {
                if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                {
                    if (_currentInteractable != interactable)
                    {
                        _currentInteractable = interactable;
                        OnPromptUpdated?.Invoke(_currentInteractable.GetInteractionPrompt());
                    }
                    return;
                }
            }

            if (_currentInteractable != null)
            {
                _currentInteractable = null;
                OnPromptUpdated?.Invoke(string.Empty);
            }
        }

        private void HandleInput()
        {
            if (_currentInteractable != null && GameInput.InteractPressed)
            {
                _currentInteractable.Interact(_playerController);
                // Refresh prompt
                if (_currentInteractable != null)
                {
                    OnPromptUpdated?.Invoke(_currentInteractable.GetInteractionPrompt());
                }
            }
        }
    }
}
