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
        private GameObject currentMenuPrefab;

        [SerializeField]
        private GameObject currentMenu;

        [SerializeField]
        protected GameObject initialMenuPrefab;

        [SerializeField]
        private GameObject simulationMenuPrefab;

        [SerializeField]
        private GameObject sceneUI;

        [SerializeField]
        public GameObject simulation;

        private Stack<GameObject> sceneStack = new Stack<GameObject>();

        [SerializeField]
        public InputDeviceCharacteristics characteristics;

        [SerializeField]
        protected ControllerButton leftControllerButton = ControllerButton.MenuButton;

        [SerializeField]
        protected ControllerButton rightControllerButton = ControllerButton.MenuButton;



        [SerializeField]
        private bool clickOnMenuClosed = true;

        [SerializeField]
        private ControllerManager controllers;

        [SerializeField]
        private UiInputMode uiMode;

        [SerializeField]
        private CanvasGroup canvasGroup;



        public bool SimulationActive => simulation.activeInHierarchy;
        //public bool SimulationMenuActive => sceneUI.activeInHierarchy;
        public bool SimulationMenuActive => simulationMenuPrefab.activeInHierarchy;

        public GameObject SceneUI => sceneUI;

        private void Start()
        {
            if (initialMenuPrefab != null)
                GotoScene(initialMenuPrefab);

            SetupOutOfSimulationMenu();
        }

        protected void SetupOutOfSimulationMenu()
        {
            //SetupControllerButton(InputDeviceCharacteristics.Left, leftControllerButton, () => SimulationActive && SimulationMenuActive, ToggleSimScene);
            //SetupControllerButton(InputDeviceCharacteristics.Right, rightControllerButton, () => SimulationActive && SimulationMenuActive, ToggleSimScene);

            //SetupControllerButton(InputDeviceCharacteristics.Left, leftControllerButton, () => SimulationActive && !SimulationMenuActive, CloseScene);
            //SetupControllerButton(InputDeviceCharacteristics.Right, rightControllerButton, () => SimulationActive && !SimulationMenuActive, CloseScene);

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

            Debug.Log("close scene");
        }



        private void ShowSimMenu()
        {
            Debug.Log("show sim menu");

            canvasGroup.alpha = 1f;

            if (!controllers.WouldBecomeCurrentMode(uiMode))
                return;

            if (currentMenuPrefab != simulationMenuPrefab)
                GotoScene(simulationMenuPrefab);

            uiMode.enabled = true;
        }

        private void FadeSimMenu()
        {
            Debug.Log("fade sim scene");

            if (clickOnMenuClosed)
                WorldSpaceCursorInput.TriggerClick();

            uiMode.enabled = false;

            canvasGroup.alpha = 0.3f;
        }

        private void ToggleSimMenu()
        {
            Debug.Log("trigger toggle menu");

            if (canvasGroup.alpha == 0.3f)
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