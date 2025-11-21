using System;
using System.Collections.Generic;
using UnityEngine;
using ShipController = Gameplay.Components.ShipController;

namespace Gameplay.Interfaces
{
    /// <summary>
    /// Service for managing ship spawning, pooling, and lifecycle.
    /// </summary>
    public interface IShipManager
    {
        /// <summary>
        /// Event fired when a ship is spawned.
        /// </summary>
        event Action<ShipController> OnShipSpawned;

        /// <summary>
        /// Event fired when a ship is despawned.
        /// </summary>
        event Action<ShipController> OnShipDespawned;

        /// <summary>
        /// Get all active ships.
        /// </summary>
        IReadOnlyList<ShipController> ActiveShips { get; }

        /// <summary>
        /// Get the player ship.
        /// </summary>
        ShipController PlayerShip { get; }

        /// <summary>
        /// Spawn a ship of the specified type.
        /// </summary>
        /// <param name="shipType">Type of ship to spawn</param>
        /// <param name="position">Spawn position</param>
        /// <param name="rotation">Spawn rotation</param>
        /// <returns>Spawned ship controller</returns>
        ShipController SpawnShip(ShipType shipType, Vector3 position, Quaternion rotation);

        /// <summary>
        /// Despawn a ship.
        /// </summary>
        /// <param name="ship">Ship to despawn</param>
        void DespawnShip(ShipController ship);

        /// <summary>
        /// Find ships within a certain radius.
        /// </summary>
        /// <param name="center">Center position</param>
        /// <param name="radius">Search radius</param>
        /// <returns>Ships within radius</returns>
        List<ShipController> FindShipsInRadius(Vector3 center, float radius);

        /// <summary>
        /// Get the nearest ship to a position.
        /// </summary>
        /// <param name="position">Search position</param>
        /// <param name="maxDistance">Maximum search distance</param>
        /// <returns>Nearest ship or null</returns>
        ShipController GetNearestShip(Vector3 position, float maxDistance = float.MaxValue);

        /// <summary>
        /// Spawn the player ship.
        /// </summary>
        /// <param name="spawnPoint">Spawn position</param>
        /// <returns>Player ship controller</returns>
        ShipController SpawnPlayerShip(Vector3 spawnPoint);

        /// <summary>
        /// Clear all ships.
        /// </summary>
        void ClearAllShips();

        /// <summary>
        /// Initialize the ship manager.
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// Types of ships.
    /// </summary>
    public enum ShipType
    {
        PlayerPoliceSmall,
        PlayerPoliceMedium,
        PlayerPoliceLarge,
        CivilianSmall,
        CivilianMedium,
        CivilianLarge,
        SmugglerSmall,
        SmugglerMedium,
        SmugglerLarge,
        PirateSmall,
        PirateMedium,
        PirateLarge
    }
}