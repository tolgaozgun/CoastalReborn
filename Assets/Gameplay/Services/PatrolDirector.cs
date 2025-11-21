using System;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Interfaces;

namespace Gameplay.Services
{
    /// <summary>
    /// Stub implementation of the patrol director service.
    /// </summary>
    public class PatrolDirector : MonoBehaviour, IPatrolDirector
    {
        [Header("Patrol Configuration")]
        [SerializeField] private List<Transform> patrolWaypoints = new List<Transform>();
        [SerializeField] private float waypointRadius = 50f;

        private PatrolState currentState = PatrolState.Idle;
        private List<Vector3> waypoints = new List<Vector3>();
        private int currentWaypointIndex = -1;
        private string currentRegion = "Harbor";

        public PatrolState CurrentState => currentState;

        public event Action<PatrolState> OnPatrolStateChanged;
        public event Action<int> OnWaypointReached;

        public void Initialize()
        {
            // Convert transforms to positions
            waypoints.Clear();
            foreach (var transform in patrolWaypoints)
            {
                if (transform != null)
                {
                    waypoints.Add(transform.position);
                }
            }
        }

        public void StartPatrol(string region)
        {
            currentRegion = region;
            currentWaypointIndex = 0;
            currentState = PatrolState.Active;
            OnPatrolStateChanged?.Invoke(currentState);
            Debug.Log($"Patrol started in region: {region}");
        }

        public void StopPatrol()
        {
            currentState = PatrolState.Idle;
            currentWaypointIndex = -1;
            OnPatrolStateChanged?.Invoke(currentState);
            Debug.Log("Patrol stopped");
        }

        public void AddWaypoint(Vector3 position, bool optional = false)
        {
            waypoints.Add(position);
            Debug.Log($"Added waypoint at {position}");
        }

        public void ClearWaypoints()
        {
            waypoints.Clear();
            currentWaypointIndex = -1;
            Debug.Log("Cleared all waypoints");
        }

        public void SpawnTraffic(int count, string region)
        {
            // TODO: Implement traffic spawning
            Debug.Log($"Spawning {count} civilian ships in {region}");
        }

        public Vector3? GetNextWaypoint()
        {
            if (currentState != PatrolState.Active || waypoints.Count == 0)
            {
                return null;
            }

            if (currentWaypointIndex >= waypoints.Count)
            {
                return null;
            }

            return waypoints[currentWaypointIndex];
        }

        public bool IsPatrolComplete()
        {
            return currentState == PatrolState.Complete ||
                   (currentState == PatrolState.Active && currentWaypointIndex >= waypoints.Count);
        }
    }
}