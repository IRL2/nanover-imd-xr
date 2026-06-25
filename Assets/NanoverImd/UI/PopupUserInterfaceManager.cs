using System.ComponentModel;
using Cysharp.Threading.Tasks;
using Nanover.Frontend.Controllers;
using Nanover.Frontend.Input;
using Nanover.Frontend.UI;
using Nanover.Frontend.XR;
using OVR.OpenVR;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR;

namespace NanoverImd.UI
{
    /// <summary>
    /// A <see cref="UserInterfaceManager"/> that only shows the UI while a cursor is held down.
    /// </summary>
    public class PopupUserInterfaceManager : UserInterfaceManager
    {
        [SerializeField]
        private GameObject menuPrefab;

        [SerializeField]
        private bool clickOnMenuClosed = true;

        [SerializeField]
        private ControllerManager controllers;
        
        [SerializeField]
        private UiInputMode mode;

        [SerializeField]
        private CanvasGroup canvasGroup;

        private void Start()
        {
            if (this.initialScene != null)
                GotoScene(initialScene);

            //this.SetupOutOfSimulationMenu();
        }

        private void Awake()
        {
            Assert.IsNotNull(menuPrefab, "Missing menu prefab");

            SetupInSimulationMenu();

            ShowMenu();
        }

        private void SetupInSimulationMenu()
        {
            SetupControllerButton(InputDeviceCharacteristics.Left, leftControllerButton, () => SimulationActive, ToggleMenu);
            SetupControllerButton(InputDeviceCharacteristics.Right, rightControllerButton, () => SimulationActive, ToggleMenu);
        }

        private void ShowMenu()
        {
            canvasGroup.alpha = 1f;

            if (!controllers.WouldBecomeCurrentMode(mode))
                return;

            GotoScene(menuPrefab);

            mode.enabled = true;
        }

        private void CloseMenu()
        {
            if (clickOnMenuClosed)
                WorldSpaceCursorInput.TriggerClick();

            //CloseScene();

            mode.enabled = false;

            canvasGroup.alpha = 0.3f;
        }

        private void ToggleMenu()
        {
            Debug.Log("trigger toggle menu");

            if (canvasGroup.alpha == 0.3f)
                ShowMenu();
            else
                CloseMenu();
        }
    }
}