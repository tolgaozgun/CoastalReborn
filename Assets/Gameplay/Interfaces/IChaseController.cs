using System;
using UnityEngine;

namespace Gameplay.Interfaces
{
    /// <summary>
    /// Service for managing chase sequences and escape mechanics.
    /// </summary>
    public interface IChaseController
    {
        /// <summary>
        /// Current chase state.
        /// </summary>
        ChaseState CurrentState { get; }

        /// <summary>
        /// Check if a chase is currently active.
        /// </summary>
        bool IsChaseActive { get; }

        /// <summary>
        /// Time until target escapes (in seconds).
        /// </summary>
        float EscapeTimeRemaining { get; }

        /// <summary>
        /// Distance to target.
        /// </summary>
        float DistanceToTarget { get; }

        /// <summary>
        /// Event fired when chase starts.
        /// </summary>
        event Action<string> OnChaseStarted;

        /// <summary>
        /// Event fired when chase ends.
        /// </summary>
        event Action<string, ChaseResult> OnChaseEnded;

        /// <summary>
        /// Event fired when chase state changes.
        /// </summary>
        event Action<ChaseState> OnChaseStateChanged;

        /// <summary>
        /// Start a chase with a target ship.
        /// </summary>
        /// <param name="targetShipId">Target ship ID</param>
        /// <param name="escapeTime">Time until target escapes</param>
        /// <returns>True if chase started successfully</returns>
        bool StartChase(string targetShipId, float escapeTime = 120f);

        /// <summary>
        /// End the current chase.
        /// </summary>
        /// <param name="result">Chase result</param>
        void EndChase(ChaseResult result);

        /// <summary>
        /// Update chase parameters.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        void UpdateChase(float deltaTime);

        /// <summary>
        /// Check if boarding is possible.
        /// </summary>
        /// <returns>True if boarding conditions are met</returns>
        bool CanBoard();

        /// <summary>
        /// Get boarding position relative to target.
        /// </summary>
        /// <returns>Optimal boarding position</returns>
        Vector3 GetBoardingPosition();

        /// <summary>
        /// Initialize chase controller.
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// Chase operation states.
    /// </summary>
    public enum ChaseState
    {
        Idle,
        Pursuing,
        Closing,
        BoardingWindow,
        Escaped,
        Caught
    }

    /// <summary>
    /// Chase outcome results.
    /// </summary>
    public enum ChaseResult
    {
        TargetEscaped,
        BoardingSuccessful,
        TargetDestroyed,
        Abandoned,
        TimeExpired
    }
}