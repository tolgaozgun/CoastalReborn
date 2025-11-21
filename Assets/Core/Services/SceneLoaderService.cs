using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Interfaces;

namespace Core.Services
{
    /// <summary>
    /// Implementation of the scene loader service.
    /// </summary>
    public class SceneLoaderService : MonoBehaviour, ISceneLoader
    {
        [Header("Loading Configuration")]
        [SerializeField] private bool showLoadingProgress = true;
        [SerializeField] private float minLoadTime = 1f;

        private float currentProgress = 0f;
        private string currentStatus = "Ready";
        private bool isLoading = false;

        public float LoadingProgress => currentProgress;
        public string LoadingStatus => currentStatus;
        public bool IsLoading => isLoading;

        public event Action<float, string> OnLoadingProgress;
        public event Action<string> OnSceneLoaded;
        public event Action<string> OnSceneUnloaded;

        public AsyncOperation LoadSceneAdditive(string sceneName, Action<float> progressCallback = null, CancellationToken cancellationToken = default)
        {
            StartCoroutine(LoadSceneAsync(sceneName, LoadSceneMode.Additive, progressCallback, cancellationToken));
            return null; // Return null for now, will be improved later
        }

        public AsyncOperation UnloadScene(string sceneName, CancellationToken cancellationToken = default)
        {
            StartCoroutine(UnloadSceneAsync(sceneName, cancellationToken));
            return null; // Return null for now, will be improved later
        }

        public AsyncOperation LoadSceneSetActive(string sceneName, Action<float> progressCallback = null, CancellationToken cancellationToken = default)
        {
            StartCoroutine(LoadSceneAsync(sceneName, LoadSceneMode.Additive, progressCallback, cancellationToken, true));
            return null; // Return null for now, will be improved later
        }

        private IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode loadMode, Action<float> progressCallback, CancellationToken cancellationToken, bool setActive = false)
        {
            if (isLoading)
            {
                Debug.LogWarning("Scene loading already in progress!");
                yield break;
            }

            isLoading = true;
            currentStatus = $"Loading {sceneName}...";
            currentProgress = 0f;
            OnLoadingProgress?.Invoke(currentProgress, currentStatus);

            float startTime = Time.time;

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, loadMode);
            if (asyncLoad == null)
            {
                Debug.LogError($"Failed to load scene '{sceneName}'. Scene may not exist or is not added to build settings.");
                isLoading = false;
                currentStatus = "Failed to load scene";
                OnLoadingProgress?.Invoke(0f, currentStatus);
                progressCallback?.Invoke(0f);
                yield break;
            }

            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone && !cancellationToken.IsCancellationRequested)
            {
                currentProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                currentStatus = $"Loading {sceneName}... {currentProgress:P0}";
                OnLoadingProgress?.Invoke(currentProgress, currentStatus);
                progressCallback?.Invoke(currentProgress);

                if (currentProgress >= 0.9f)
                {
                    // Ensure minimum load time for better UX
                    float elapsedTime = Time.time - startTime;
                    if (elapsedTime < minLoadTime)
                    {
                        yield return new WaitForSeconds(minLoadTime - elapsedTime);
                    }

                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                isLoading = false;
                yield break;
            }

            // Wait one more frame for scene to fully load
            yield return null;

            if (setActive)
            {
                Scene loadedScene = SceneManager.GetSceneByName(sceneName);
                if (loadedScene.IsValid())
                {
                    SceneManager.SetActiveScene(loadedScene);
                }
            }

            currentProgress = 1f;
            currentStatus = $"Loaded {sceneName}";
            OnLoadingProgress?.Invoke(currentProgress, currentStatus);
            OnSceneLoaded?.Invoke(sceneName);

            isLoading = false;
        }

        private IEnumerator UnloadSceneAsync(string sceneName, CancellationToken cancellationToken)
        {
            currentStatus = $"Unloading {sceneName}...";
            currentProgress = 0f;
            OnLoadingProgress?.Invoke(currentProgress, currentStatus);

            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
            if (asyncUnload == null)
            {
                Debug.LogWarning($"Scene '{sceneName}' is not loaded or cannot be unloaded.");
                OnSceneUnloaded?.Invoke(sceneName);
                yield break;
            }

            while (!asyncUnload.isDone && !cancellationToken.IsCancellationRequested)
            {
                currentProgress = asyncUnload.progress;
                OnLoadingProgress?.Invoke(currentProgress, currentStatus);
                yield return null;
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                OnSceneUnloaded?.Invoke(sceneName);
            }
        }

        public bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName)
                {
                    return true;
                }
            }
            return false;
        }

        public Scene GetLoadedScene(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName)
                {
                    return scene;
                }
            }
            return new Scene(); // Returns invalid scene
        }

        public void UnloadAllGameplayScenes()
        {
            // Keep only Bootstrap scene loaded
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name != "Bootstrap" && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        public void PreloadScene(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName, LoadSceneMode.Additive, null, CancellationToken.None));
        }
    }
}