using System;
using UnityEngine;
using Gameplay.Interfaces;

namespace Gameplay.Services
{
    /// <summary>
    /// Stub implementation of the chase controller service.
    /// </summary>
    public class ChaseController : MonoBehaviour, IChaseController
    {
        [Header("Chase Configuration")]
        [SerializeField] private float defaultEscapeTime = 120f;
        [SerializeField] private float boardingDistance = 10f;
        [SerializeField] private float maxBoardingRelativeVelocity = 5f;

        private ChaseState currentState = ChaseState.Idle;
        private string currentTargetShipId;
        private float escapeTimeRemaining;
        private float escapeTimeMax;
        private float distanceToTarget;

        public ChaseState CurrentState => currentState;
        public bool IsChaseActive => currentState != ChaseState.Idle && currentState != ChaseState.Escaped && currentState != ChaseState.Caught;
        public float EscapeTimeRemaining => escapeTimeRemaining;
        public float DistanceToTarget => distanceToTarget;

        public event Action<string> OnChaseStarted;
        public event Action<string, ChaseResult> OnChaseEnded;
        public event Action<ChaseState> OnChaseStateChanged;

        public void Initialize()
        {
            // Initialize chase controller
        }

        public bool StartChase(string targetShipId, float escapeTime = 120f)
        {
            if (IsChaseActive)
            {
                Debug.LogWarning("Chase already active!");
                return false;
            }

            currentTargetShipId = targetShipId;
            escapeTimeMax = escapeTime;
            escapeTimeRemaining = escapeTime;
            currentState = ChaseState.Pursuing;

            OnChaseStarted?.Invoke(targetShipId);
            OnChaseStateChanged?.Invoke(currentState);

            Debug.Log($"Chase started with target: {targetShipId}");
            return true;
        }

        public void EndChase(ChaseResult result)
        {
            if (!IsChaseActive) return;

            OnChaseEnded?.Invoke(currentTargetShipId, result);

            // Reset state
            currentTargetShipId = null;
            escapeTimeRemaining = 0f;
            distanceToTarget = 0f;

            switch (result)
            {
                case ChaseResult.TargetEscaped:
                    currentState = ChaseState.Escaped;
                    break;
                case ChaseResult.BoardingSuccessful:
                case ChaseResult.TargetDestroyed:
                    currentState = ChaseState.Caught;
                    break;
                default:
                    currentState = ChaseState.Idle;
                    break;
            }

            OnChaseStateChanged?.Invoke(currentState);
            Debug.Log($"Chase ended with result: {result}");
        }

        public void UpdateChase(float deltaTime)
        {
            if (!IsChaseActive) return;

            // Update escape timer
            escapeTimeRemaining -= deltaTime;
            if (escapeTimeRemaining <= 0f)
            {
                EndChase(ChaseResult.TimeExpired);
                return;
            }

            // Update distance to target (TODO: Get actual distance from ship manager)
            distanceToTarget = 100f; // Placeholder

            // Update chase state based on conditions
            UpdateChaseState();
        }

        private void UpdateChaseState()
        {
            ChaseState newState = currentState;

            if (distanceToTarget < boardingDistance * 2f)
            {
                newState = ChaseState.Closing;
            }

            if (distanceToTarget < boardingDistance && CanBoard())
            {
                newState = ChaseState.BoardingWindow;
            }

            if (newState != currentState)
            {
                currentState = newState;
                OnChaseStateChanged?.Invoke(currentState);
            }
        }

        public bool CanBoard()
        {
            // TODO: Check actual conditions from ship manager
            return distanceToTarget < boardingDistance && true; // Add velocity check later
        }

        public Vector3 GetBoardingPosition()
        {
            // TODO: Calculate optimal boarding position relative to target
            return Vector3.zero; // Placeholder
        }
    }
}