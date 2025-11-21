using System;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Interfaces;
using ShipController = Gameplay.Components.ShipController;
using PlayerBoatController = Gameplay.Components.PlayerBoatController;
using SimpleBoat = Gameplay.Components.SimpleBoat;

namespace Gameplay.Services
{
    /// <summary>
    /// Implementation of the ship manager service.
    /// </summary>
    public class ShipManager : MonoBehaviour, IShipManager
    {
        [Header("Ship Prefabs")]
        [SerializeField] private GameObject playerBoatPrefab;
        [SerializeField] private GameObject[] civilianShipPrefabs;
        [SerializeField] private GameObject[] smugglerShipPrefabs;

        private List<ShipController> activeShips = new List<ShipController>();
        private ShipController playerShip;

        public IReadOnlyList<ShipController> ActiveShips => activeShips.AsReadOnly();
        public ShipController PlayerShip => playerShip;

        public event Action<ShipController> OnShipSpawned;
        public event Action<ShipController> OnShipDespawned;

        public void Initialize()
        {
            // Initialize ship pools and spawning systems
        }

        public ShipController SpawnShip(ShipType shipType, Vector3 position, Quaternion rotation)
        {
            GameObject shipObject = CreateShipObject(shipType, position, rotation);
            ShipController controller = shipObject.GetComponent<ShipController>();

            if (controller == null)
            {
                // Add the appropriate controller based on ship type
                controller = shipObject.AddComponent<PlayerBoatController>();
            }

            if (controller != null)
            {
                activeShips.Add(controller);
                controller.Initialize();
                OnShipSpawned?.Invoke(controller);
            }

            return controller;
        }

        private GameObject CreateShipObject(ShipType shipType, Vector3 position, Quaternion rotation)
        {
            GameObject prefab = GetPrefabForShipType(shipType);
            if (prefab != null)
            {
                return Instantiate(prefab, position, rotation);
            }

            // Create a basic boat if no prefab exists
            GameObject boatObject = CreateBasicBoat(shipType);
            boatObject.transform.position = position;
            boatObject.transform.rotation = rotation;
            return boatObject;
        }

        private GameObject CreateBasicBoat(ShipType shipType)
        {
            GameObject boatObject = new GameObject($"{shipType}_Boat");

            // Add basic boat shape
            GameObject boatBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boatBody.transform.SetParent(boatObject.transform);
            boatBody.transform.localScale = new Vector3(2f, 0.5f, 4f);
            boatBody.transform.localPosition = Vector3.zero;

            // Add SimpleBoat component for AI behavior
            SimpleBoat simpleBoat = boatObject.AddComponent<SimpleBoat>();

            // Configure based on ship type
            switch (shipType)
            {
                case ShipType.PlayerPoliceSmall:
                    // Player boats will be handled by PlayerBoatController
                    break;
                case ShipType.CivilianSmall:
                    simpleBoat.MoveSpeed = 8f;
                    break;
                case ShipType.SmugglerSmall:
                    simpleBoat.MoveSpeed = 15f;
                    break;
                default:
                    simpleBoat.MoveSpeed = 10f;
                    break;
            }

            return boatObject;
        }

        public void DespawnShip(ShipController ship)
        {
            if (ship != null && activeShips.Contains(ship))
            {
                activeShips.Remove(ship);
                OnShipDespawned?.Invoke(ship);
                Destroy(ship.gameObject);
            }
        }

        public ShipController SpawnPlayerShip(Vector3 spawnPoint)
        {
            if (playerShip != null)
            {
                Debug.LogWarning("Player ship already exists!");
                return playerShip;
            }

            playerShip = SpawnShip(ShipType.PlayerPoliceSmall, spawnPoint, Quaternion.identity);
            if (playerShip != null)
            {
                playerShip.SetPlayerControlled(true);
            }

            return playerShip;
        }

        public List<ShipController> FindShipsInRadius(Vector3 center, float radius)
        {
            List<ShipController> shipsInRadius = new List<ShipController>();
            foreach (var ship in activeShips)
            {
                if (Vector3.Distance(center, ship.transform.position) <= radius)
                {
                    shipsInRadius.Add(ship);
                }
            }
            return shipsInRadius;
        }

        public ShipController GetNearestShip(Vector3 position, float maxDistance = float.MaxValue)
        {
            ShipController nearest = null;
            float nearestDistance = maxDistance;

            foreach (var ship in activeShips)
            {
                float distance = Vector3.Distance(position, ship.transform.position);
                if (distance < nearestDistance)
                {
                    nearest = ship;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        public void ClearAllShips()
        {
            for (int i = activeShips.Count - 1; i >= 0; i--)
            {
                DespawnShip(activeShips[i]);
            }
        }

        private GameObject GetPrefabForShipType(ShipType shipType)
        {
            switch (shipType)
            {
                case ShipType.PlayerPoliceSmall:
                    return playerBoatPrefab;
                case ShipType.CivilianSmall:
                case ShipType.CivilianMedium:
                case ShipType.CivilianLarge:
                    return civilianShipPrefabs.Length > 0 ? civilianShipPrefabs[0] : null;
                case ShipType.SmugglerSmall:
                case ShipType.SmugglerMedium:
                case ShipType.SmugglerLarge:
                    return smugglerShipPrefabs.Length > 0 ? smugglerShipPrefabs[0] : null;
                default:
                    return null;
            }
        }
    }
}