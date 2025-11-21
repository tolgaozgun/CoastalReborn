using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Interfaces
{
    /// <summary>
    /// Service for managing scene loading and unloading operations.
    /// </summary>
    public interface ISceneLoader
    {
        /// <summary>
        /// Progress of the current loading operation (0-1).
        /// </summary>
        float LoadingProgress { get; }

        /// <summary>
        /// Status message for the current loading operation.
        /// </summary>
        string LoadingStatus { get; }

        /// <summary>
        /// Check if a scene loading operation is in progress.
        /// </summary>
        bool IsLoading { get; }

        /// <summary>
        /// Event fired when loading progress updates.
        /// </summary>
        event Action<float, string> OnLoadingProgress;

        /// <summary>
        /// Event fired when a scene load completes.
        /// </summary>
        event Action<string> OnSceneLoaded;

        /// <summary>
        /// Event fired when a scene unload completes.
        /// </summary>
        event Action<string> OnSceneUnloaded;

        /// <summary>
        /// Load a scene additively.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Async operation handle</returns>
        AsyncOperation LoadSceneAdditive(string sceneName,
            Action<float> progressCallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Unload a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to unload</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Async operation handle</returns>
        AsyncOperation UnloadScene(string sceneName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Load a scene and set it as the active scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Async operation handle</returns>
        AsyncOperation LoadSceneSetActive(string sceneName,
            Action<float> progressCallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a scene is currently loaded.
        /// </summary>
        /// <param name="sceneName">Name of the scene to check</param>
        /// <returns>True if scene is loaded</returns>
        bool IsSceneLoaded(string sceneName);

        /// <summary>
        /// Get the loaded scene by name.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <returns>Scene reference or null if not loaded</returns>
        Scene GetLoadedScene(string sceneName);

        /// <summary>
        /// Unload all scenes except the bootstrap scene.
        /// </summary>
        void UnloadAllGameplayScenes();

        /// <summary>
        /// Preload a scene in the background.
        /// </summary>
        /// <param name="sceneName">Name of the scene to preload</param>
        void PreloadScene(string sceneName);
    }
}