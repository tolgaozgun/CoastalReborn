using UnityEngine;
using UnityEngine.InputSystem;
using Core.Interfaces;
using Core.Services;
using Gameplay.Interfaces;

namespace Gameplay.Components
{
    /// <summary>
    /// Player-controlled boat controller.
    /// </summary>
    public class PlayerBoatController : ShipController
    {
        [Header("Player Configuration")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float maxTurnSpeed = 120f;
        [SerializeField] private float dragMultiplier = 2f;
        [SerializeField] private float brakeForce = 10f;
        [SerializeField] private float playerAcceleration = 10f;

        [Header("Input Configuration")]
        [SerializeField] private InputActionAsset inputActions;

        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction boostAction;
        private InputAction brakeAction;

        private IInputService inputService;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool isBoosting;
        private bool isBraking;
        private bool isEnabled = true;

        // Player-specific properties
        public bool IsEnabled => isEnabled;
        public float CurrentThrottle { get; private set; }

        // Events
        public event System.Action<float> OnThrottleChanged;
        public event System.Action OnBoostActivated;
        public event System.Action OnBoostDeactivated;

        protected override void Awake()
        {
            base.Awake();

            // Set ship type
            shipType = ShipType.PlayerPoliceSmall;
            isPlayerControlled = true;

            SetupInput();
        }

        private void Start()
        {
            Debug.Log("[PlayerBoatController] Start - Setting up input...");

            // Try to get input service from DI container
            inputService = FindObjectOfType<InputService>();
            Debug.Log($"[PlayerBoatController] InputService found: {inputService != null}");

            if (inputService != null)
            {
                Debug.Log($"[PlayerBoatController] InputService InputActions asset: {inputService.InputActions != null}");

                // Use the InputService's input actions instead of our own
                if (inputService.InputActions != null)
                {
                    Debug.Log("[PlayerBoatController] Using InputService's InputActions asset");

                    // Find actions under the Navigation action map
                    var navigationMap = inputService.InputActions.FindActionMap("Navigation");
                    if (navigationMap != null)
                    {
                        Debug.Log("[PlayerBoatController] Found Navigation action map");

                        moveAction = navigationMap.FindAction("Move");
                        lookAction = navigationMap.FindAction("Look");
                        boostAction = navigationMap.FindAction("Boost");
                        brakeAction = navigationMap.FindAction("Brake");

                        Debug.Log($"[PlayerBoatController] Move action found: {moveAction != null}");
                        Debug.Log($"[PlayerBoatController] Look action found: {lookAction != null}");
                        Debug.Log($"[PlayerBoatController] Boost action found: {boostAction != null}");
                        Debug.Log($"[PlayerBoatController] Brake action found: {brakeAction != null}");
                    }
                    else
                    {
                        Debug.LogError("[PlayerBoatController] Navigation action map not found in InputService!");
                    }
                }

                if (moveAction != null)
                {
                    moveAction.performed += OnMovePerformed;
                    moveAction.canceled += OnMoveCanceled;
                    moveAction.Enable();
                    Debug.Log("[PlayerBoatController] Move action enabled with callbacks");
                }

                if (lookAction != null)
                {
                    lookAction.performed += OnLookPerformed;
                    lookAction.canceled += OnLookCanceled;
                    lookAction.Enable();
                    Debug.Log("[PlayerBoatController] Look action enabled with callbacks");
                }

                if (boostAction != null)
                {
                    boostAction.started += OnBoostStarted;
                    boostAction.canceled += OnBoostCanceled;
                    boostAction.Enable();
                    Debug.Log("[PlayerBoatController] Boost action enabled with callbacks");
                }

                if (brakeAction != null)
                {
                    brakeAction.started += OnBrakeStarted;
                    brakeAction.canceled += OnBrakeCanceled;
                    brakeAction.Enable();
                    Debug.Log("[PlayerBoatController] Brake action enabled with callbacks");
                }
            }
            else
            {
                Debug.LogWarning("[PlayerBoatController] InputService not found! Using legacy input setup.");
                SetupLegacyInput();
            }
        }

        private void SetupInput()
        {
            if (inputActions == null)
            {
                // Try to find InputActions asset
                inputActions = Resources.Load<InputActionAsset>("InputActions");
            }
        }

        private void SetupLegacyInput()
        {
            // Fallback setup if DI container isn't ready yet
            if (inputActions == null)
            {
                inputActions = Resources.Load<InputActionAsset>("InputActions");
            }

            SetupInput();
        }

        protected override void UpdateShip(float deltaTime)
        {
            if (!isEnabled || !isActive)
                return;

            HandleInput();
            UpdateThrottle();
        }

        protected override void PhysicsUpdate(float deltaTime)
        {
            if (!isEnabled || !isActive)
                return;

            ApplyMovement(deltaTime);
        }

        private void HandleInput()
        {
            // Input is handled through event callbacks now
        }

        private void UpdateThrottle()
        {
            float targetThrottle = Mathf.Abs(moveInput.y);

            if (isBraking)
            {
                targetThrottle = 0f;
            }

            CurrentThrottle = Mathf.Lerp(CurrentThrottle, targetThrottle, Time.deltaTime * 2f);
            OnThrottleChanged?.Invoke(CurrentThrottle);
        }

        private void ApplyMovement(float deltaTime)
        {
            if (rigidbody == null) return;

            // Simple forward/backward movement
            if (Mathf.Abs(moveInput.y) > 0.1f)
            {
                Vector3 force = transform.forward * moveInput.y * playerAcceleration;
                rigidbody.AddForce(force, ForceMode.Force);
                Debug.Log($"[PlayerBoatController] Applying simple force: {force:F2}");
            }

            // Simple turning
            if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                float turnSpeed = rotationSpeed * 2f; // Direct turn speed
                transform.Rotate(0, moveInput.x * turnSpeed * deltaTime, 0);
                Debug.Log($"[PlayerBoatController] Turning by: {moveInput.x * turnSpeed * deltaTime:F2} degrees");
            }

            // Simple speed limit
            if (rigidbody.linearVelocity.magnitude > maxSpeed)
            {
                rigidbody.linearVelocity = rigidbody.linearVelocity.normalized * maxSpeed;
            }
        }

        // Input event callbacks
        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
            Debug.Log($"[PlayerBoatController] OnMovePerformed: moveInput = {moveInput}");
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            moveInput = Vector2.zero;
            Debug.Log("[PlayerBoatController] OnMoveCanceled: moveInput reset to zero");
        }

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
            // Don't spam the log with mouse movement
        }

        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            lookInput = Vector2.zero;
            Debug.Log("[PlayerBoatController] OnLookCanceled: lookInput reset to zero");
        }

        private void OnBoostStarted(InputAction.CallbackContext context)
        {
            isBoosting = true;
            OnBoostActivated?.Invoke();
        }

        private void OnBoostCanceled(InputAction.CallbackContext context)
        {
            isBoosting = false;
            OnBoostDeactivated?.Invoke();
        }

        private void OnBrakeStarted(InputAction.CallbackContext context)
        {
            isBraking = true;
        }

        private void OnBrakeCanceled(InputAction.CallbackContext context)
        {
            isBraking = false;
        }

        // Public methods for external control
        public void EnableControl()
        {
            isEnabled = true;

            if (inputActions != null)
            {
                inputActions.Enable();
            }
        }

        public void DisableControl()
        {
            isEnabled = false;
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            isBoosting = false;
            isBraking = false;

            if (inputActions != null)
            {
                inputActions.Disable();
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            EnableControl();
        }

        public override void Destroy()
        {
            DisableControl();
            base.Destroy();
        }

        private void OnDestroy()
        {
            // Clean up input actions
            if (moveAction != null)
            {
                moveAction.performed -= OnMovePerformed;
                moveAction.canceled -= OnMoveCanceled;
                moveAction.Disable();
            }

            if (lookAction != null)
            {
                lookAction.performed -= OnLookPerformed;
                lookAction.canceled -= OnLookCanceled;
                lookAction.Disable();
            }

            if (boostAction != null)
            {
                boostAction.started -= OnBoostStarted;
                boostAction.canceled -= OnBoostCanceled;
                boostAction.Disable();
            }

            if (brakeAction != null)
            {
                brakeAction.started -= OnBrakeStarted;
                brakeAction.canceled -= OnBrakeCanceled;
                brakeAction.Disable();
            }
        }

        // Helper methods
        public float GetSpeedPercentage()
        {
            if (maxSpeed <= 0) return 0f;
            return Mathf.Clamp01(CurrentSpeed / maxSpeed);
        }

        public Vector3 GetVelocity()
        {
            return rigidbody != null ? rigidbody.linearVelocity : Vector3.zero;
        }

        public Vector3 GetForwardDirection()
        {
            return transform.forward;
        }

        public bool IsMoving()
        {
            return CurrentSpeed > 0.5f;
        }

        public bool IsTurning()
        {
            return Mathf.Abs(moveInput.x) > 0.1f || Mathf.Abs(lookInput.x) > 0.1f;
        }
    }
}