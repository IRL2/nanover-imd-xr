using System;
using System.Collections.Generic;
using System.Linq;
using Nanover.Frontend.Controllers;

using Nanover.Frontend.UI;
using Nanover.Frontend.XR;
using UnityEngine;
using UnityEngine.XR;

namespace NanoverImd.UI
{
    public class MainUserInterfaceManager : MonoBehaviour
    {
        const float FADE_ALPHA = 0.2f;

        private GameObject currentMenuPrefab;

        private Stack<GameObject> sceneStack = new Stack<GameObject>();

        [Header("UI Panels")]
        [SerializeField]
        private GameObject currentMenu;

        [SerializeField]
        protected GameObject initialMenuPrefab;

        [SerializeField]
        private GameObject simulationMenuPrefab;

        [Header("UI Dependencies")]
        [SerializeField]
        private GameObject sceneUI;

        [SerializeField]
        private GameObject simulation;

        [SerializeField]
        private CanvasGroup canvasGroup;


        [SerializeField]
        private UiInputMode uiMode;

        [SerializeField]
        private ControllerManager controllers;

        [Header("Controller Settings")]
        [SerializeField]
        public InputDeviceCharacteristics characteristics;

        [SerializeField]
        protected ControllerButton leftControllerButton = ControllerButton.MenuButton;

        [SerializeField]
        protected ControllerButton rightControllerButton = ControllerButton.MenuButton;


        public bool SimulationActive => simulation.activeInHierarchy;
        public bool SimulationMenuActive => currentMenu != null && currentMenuPrefab == simulationMenuPrefab;

        private void Start()
        {
            if (initialMenuPrefab != null)
                GotoScene(initialMenuPrefab);

            SetupOutOfSimulationMenu();
        }

        protected void SetupOutOfSimulationMenu()
        {
            SetupControllerButton(InputDeviceCharacteristics.Left, leftControllerButton, () => SimulationActive, LaunchButtonTrigger);
            SetupControllerButton(InputDeviceCharacteristics.Right, rightControllerButton, () => SimulationActive, LaunchButtonTrigger);
        }

        protected void SetupControllerButton(InputDeviceCharacteristics hand, ControllerButton button, Func<bool> predicate, Action pressed)
        {
            if (!button.TryToUsage(out var usage))
                return;

            var menuButton = (characteristics & ~(InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Right) | hand)
                .WrapUsageAsButton(usage, predicate);
            menuButton.Pressed += pressed;
        }

        private void LeaveScene(GameObject scene)
        {
            WorldSpaceCursorInput.ClearSelection();
            Destroy(scene);
        }

        private GameObject EnterScene(GameObject scene)
        {
            if (scene != null)
            {
                var newScene = Instantiate(scene, sceneUI.transform);
                newScene.SetActive(true);
                return newScene;
            }

            return null;
        }

        public void GotoScene(GameObject scene)
        {
            if (currentMenu != null)
                LeaveScene(currentMenu);

            currentMenu = EnterScene(scene);

            if (currentMenu != null)
                currentMenuPrefab = scene;
            else
                currentMenuPrefab = null;

            sceneUI.SetActive(currentMenu != null);
        }

        public void GotoSceneAndAddToStack(GameObject newScene)
        {
            var previousScenePrefab = currentMenuPrefab;
            GotoScene(newScene);
            if (newScene != null && previousScenePrefab != null)
                sceneStack.Push(previousScenePrefab);
        }

        public void GoBack()
        {
            if (sceneStack.Any())
            {
                GotoScene(sceneStack.Pop());
                sceneStack.Clear();
            }
        }

        public void CloseScene()
        {
            sceneStack.Clear();
            GotoScene(null);
        }



        private void ShowSimMenu()
        {

            canvasGroup.alpha = 1f;

            if (!controllers.WouldBecomeCurrentMode(uiMode))
                return;

            // load menu if not already loaded
            if (currentMenuPrefab != simulationMenuPrefab)
                GotoScene(simulationMenuPrefab);

            uiMode.enabled = true;
        }

        private void FadeSimMenu()
        {
            uiMode.enabled = false;

            canvasGroup.alpha = FADE_ALPHA;
        }

        private void ToggleSimMenu()
        {
            if (canvasGroup.alpha == FADE_ALPHA)
                ShowSimMenu();
            else
                FadeSimMenu();
        }

        private void LaunchButtonTrigger()
        {
            if (currentMenu == null)
            {
                ShowSimMenu();
                return;
            }

            if (currentMenu != null)
            {
                if (currentMenuPrefab == simulationMenuPrefab)
                {
                    ToggleSimMenu();
                    return;
                }
                CloseScene();
            }
        }

        public void EnableUIMode()
        {
            uiMode.enabled = true;
        }
        public void DisableUIMode()
        { 
            uiMode.enabled = false;
        }

    }
}