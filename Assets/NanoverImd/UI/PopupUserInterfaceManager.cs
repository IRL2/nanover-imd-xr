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

        private void Start()
        {
            Assert.IsNotNull(menuPrefab, "Missing menu prefab");

            SetupInSimulationMenu();
        }

        private void SetupInSimulationMenu()
        {
            var menuButton = characteristics.WrapUsageAsButton(CommonUsages.menuButton, () => SimulationActive);
            menuButton.Pressed += ToggleMenu;
        }

        private void ShowMenu()
        {
            if (!controllers.WouldBecomeCurrentMode(mode))
                return;

            GotoScene(menuPrefab);
        }

        private void CloseMenu()
        {
            if (clickOnMenuClosed)
                WorldSpaceCursorInput.TriggerClick();
            CloseScene();
        }

        private void ToggleMenu()
        {
            if (!SimulationMenuActive)
                ShowMenu();
            else
                CloseMenu();
        }
    }
}