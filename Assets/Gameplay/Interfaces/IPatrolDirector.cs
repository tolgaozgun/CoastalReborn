using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Interfaces
{
    /// <summary>
    /// Service for managing patrol operations and traffic spawning.
    /// </summary>
    public interface IPatrolDirector
    {
        /// <summary>
        /// Current patrol state.
        /// </summary>
        PatrolState CurrentState { get; }

        /// <summary>
        /// Event fired when patrol state changes.
        /// </summary>
        event Action<PatrolState> OnPatrolStateChanged;

        /// <summary>
        /// Event fired when patrol waypoint is reached.
        /// </summary>
        event Action<int> OnWaypointReached;

        /// <summary>
        /// Start patrol in a specific region.
        /// </summary>
        /// <param name="region">Patrol region</param>
        void StartPatrol(string region);

        /// <summary>
        /// Stop current patrol.
        /// </summary>
        void StopPatrol();

        /// <summary>
        /// Add a patrol waypoint.
        /// </summary>
        /// <param name="position">Waypoint position</param>
        /// <param name="optional">Whether waypoint is optional</param>
        void AddWaypoint(Vector3 position, bool optional = false);

        /// <summary>
        /// Clear all patrol waypoints.
        /// </summary>
        void ClearWaypoints();

        /// <summary>
        /// Spawn civilian traffic.
        /// </summary>
        /// <param name="count">Number of ships to spawn</param>
        /// <param name="region">Spawn region</param>
        void SpawnTraffic(int count, string region);

        /// <summary>
        /// Get next patrol waypoint.
        /// </summary>
        /// <returns>Waypoint position or null</returns>
        Vector3? GetNextWaypoint();

        /// <summary>
        /// Check if patrol is complete.
        /// </summary>
        /// <returns>True if all waypoints visited</returns>
        bool IsPatrolComplete();

        /// <summary>
        /// Initialize patrol director.
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// Patrol operation states.
    /// </summary>
    public enum PatrolState
    {
        Idle,
        Active,
        Complete,
        Suspended
    }
}