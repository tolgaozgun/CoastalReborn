using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.Interfaces;

namespace Core.Services
{
    /// <summary>
    /// Implementation of the UI service.
    /// </summary>
    public class UIService : MonoBehaviour, IUIService
    {
        [Header("UI Configuration")]
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private Canvas tabletCanvas;
        [SerializeField] private Canvas modalCanvas;

        private Dictionary<string, Canvas> registeredCanvases = new Dictionary<string, Canvas>();
        private Stack<string> modalStack = new Stack<string>();
        private string currentInputContext = "Navigation";

        public bool IsTabletOpen { get; private set; }
        public bool IsModalOpen => modalStack.Count > 0;
        public string CurrentInputContext => currentInputContext;

        public event Action OnTabletOpened;
        public event Action OnTabletClosed;
        public event Action<string> OnInputContextChanged;
        public event Action<string> OnModalOpened;
        public event Action<string> OnModalClosed;

        private void Awake()
        {
            RegisterCanvas("HUD", hudCanvas);
            RegisterCanvas("Tablet", tabletCanvas);
            RegisterCanvas("Modal", modalCanvas);

            // Initialize canvas states
            if (tabletCanvas != null) tabletCanvas.gameObject.SetActive(false);
            if (modalCanvas != null) modalCanvas.gameObject.SetActive(false);
            if (hudCanvas != null) hudCanvas.gameObject.SetActive(true);
        }

        public void Initialize()
        {
            SetInputContext("Navigation");
        }

        public void RegisterCanvas(string canvasName, Canvas canvas)
        {
            if (canvas != null)
            {
                registeredCanvases[canvasName] = canvas;
            }
        }

        public void UnregisterCanvas(string canvasName)
        {
            registeredCanvases.Remove(canvasName);
        }

        public void SetCanvasVisibility(string canvasName, bool show)
        {
            if (registeredCanvases.TryGetValue(canvasName, out Canvas canvas))
            {
                canvas.gameObject.SetActive(show);
            }
        }

        public void OpenTablet()
        {
            if (!IsTabletOpen)
            {
                SetCanvasVisibility("Tablet", true);
                IsTabletOpen = true;
                OnTabletOpened?.Invoke();
                SetInputContext("Tablet");
            }
        }

        public void CloseTablet()
        {
            if (IsTabletOpen)
            {
                SetCanvasVisibility("Tablet", false);
                IsTabletOpen = false;
                OnTabletClosed?.Invoke();
                SetInputContext("Navigation");
            }
        }

        public void ToggleTablet()
        {
            if (IsTabletOpen)
            {
                CloseTablet();
            }
            else
            {
                OpenTablet();
            }
        }

        public void OpenModal(string modalName, object data = null)
        {
            modalStack.Push(modalName);
            SetCanvasVisibility("Modal", true);
            OnModalOpened?.Invoke(modalName);
        }

        public void CloseModal(string modalName)
        {
            if (modalStack.Count > 0 && modalStack.Peek() == modalName)
            {
                modalStack.Pop();
                if (modalStack.Count == 0)
                {
                    SetCanvasVisibility("Modal", false);
                }
                OnModalClosed?.Invoke(modalName);
            }
        }

        public void CloseAllModals()
        {
            while (modalStack.Count > 0)
            {
                string modalName = modalStack.Pop();
                OnModalClosed?.Invoke(modalName);
            }
            SetCanvasVisibility("Modal", false);
        }

        public void SetInputContext(string context)
        {
            if (currentInputContext != context)
            {
                currentInputContext = context;
                OnInputContextChanged?.Invoke(context);
            }
        }

        public Canvas GetCanvas(string canvasName)
        {
            registeredCanvases.TryGetValue(canvasName, out Canvas canvas);
            return canvas;
        }
    }
}