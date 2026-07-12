using Essd;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using WebDiscovery;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NanoverImd
{
    /// <summary>
    /// Unity Immediate Mode GUI for connecting, configuring, etc from the
    /// desktop (without needing VR).
    /// </summary>
    public class DesktopDebugUI : MonoBehaviour
    {
        [SerializeField]
        private NanoverImdApplication application;
        
        [SerializeField]
        private NanoverImdSimulation simulation;

        [SerializeField]
        private GameObject xrSimulatorContainer;

        private bool directConnect;
        private string directConnectAddress = "ws://localhost:38801";

        private bool discovery;
        private ICollection<ServiceHub> knownServiceHubs = new List<ServiceHub>();
        private ICollection<DiscoveryEntry> knownWebSockets = new List<DiscoveryEntry>();
        private IList<string> knownSimulations = new List<string>();
        private IList<NanoverRecordings.DemoListing> knownDemos = new List<NanoverRecordings.DemoListing>();

        public float interactionForceMultiplier = 1000;

        private Vector2 currentGuiAreaOrigin;
        private Vector2 fallbackMouseGuiPosition;
        private bool fallbackMouseWasDown;
        private bool fallbackMousePressedThisFrame;
        private bool fallbackMouseReleasedThisFrame;
        private bool fallbackClickConsumed;
        private int fallbackMouseFrame = -1;
        private int buttonIndex;
        private int fallbackPressedButtonIndex = -1;
        private int pendingFallbackButtonIndex = -1;

        private void OnGUI()
        {
            buttonIndex = 0;
            UpdateFallbackMouseState();

            BeginDebugArea(new Rect(16, 16, 192, 1024));
            GUILayout.Box("Nanover iMD");

            GUILayout.Box("Demos");

            if (DebugButton("Refresh"))
            {
                NanoverRecordings
                .FetchDemosListing()
                .AsUniTask()
                .ContinueWith((listing) =>
                {
                    knownDemos = listing;
                });
            }

            foreach (var entry in knownDemos)
            {
                if (DebugButton(entry.Name))
                {
                    NanoverRecordings.LoadDemo(entry.URL).AsUniTask().ContinueWith(simulation.ConnectRecordingReader);
                }
            }

            GUILayout.Box("Connect");

            if (DebugButton("Manual"))
            {
                directConnect = true;
            }

            if (DebugButton("Discover"))
            {
                discovery = true;
                RefreshServiceDiscovery();
            }

            if (DebugButton("Disconnect"))
            {
                simulation.Disconnect();
            }

            if (simulation.gameObject.activeSelf)
            {
                GUILayout.Box("Interaction");
                GUILayout.Label(
                    $"Force Scale: {simulation.ManipulableParticles.ForceScale:0.}x");
                simulation.ManipulableParticles.ForceScale =
                    GUILayout.HorizontalSlider(simulation.ManipulableParticles.ForceScale, 0, 5000);
                GUILayout.Label(
                    $"Force Type: {simulation.ManipulableParticles.ForceType}");
                simulation.ManipulableParticles.ForceType =
                    GUILayout.TextField(simulation.ManipulableParticles.ForceType);

                GUILayout.Box("Simulation");
                if (DebugButton("Play"))
                    simulation.PlayTrajectory();

                if (DebugButton("Pause"))
                    simulation.PauseTrajectory();

                if (DebugButton("Step"))
                    simulation.StepForwardTrajectory();
                
                if (DebugButton("Reset"))
                    simulation.ResetTrajectory();
                
                if (DebugButton("Reset Box"))
                    simulation.ResetBox();

                GUILayout.Box("Switch Simulation");
                if (DebugButton("Refresh"))
                    simulation.Trajectory.GetSimulationListing().ContinueWith((list) => knownSimulations = list);

                for (int i = 0; i < knownSimulations.Count; ++i)
                {
                    if (DebugButton(knownSimulations[i]))
                        simulation.Trajectory.SetSimulationIndex(i);
                }

                GUILayout.Box("Colocation");
                application.ColocateLighthouses = GUILayout.Toggle(application.ColocateLighthouses, "Colocated Lighthouses");

                if (!application.ColocateLighthouses)
                {
                    if (DebugButton("Reset Radial Orientation"))
                        simulation.RunRadialOrientation();

                    GUILayout.Label("Radial Displacement");
                    application.PlayAreaRadialDisplacementFactor = GUILayout.HorizontalSlider(application.PlayAreaRadialDisplacementFactor, 0f, 1f);
                    GUILayout.Label("Rotation Correction");
                    application.PlayAreaRotationCorrection = GUILayout.HorizontalSlider(application.PlayAreaRotationCorrection, -180f, 180f);
                }

                GUILayout.Box("User Commands");
                if (DebugButton("Refresh List"))
                    simulation.Trajectory.UpdateCommands().Forget();

                foreach (var command in simulation.Trajectory.CommandDefinitions.Values.Where(command => command.Name.StartsWith("user/")))
                {
                    if (DebugButton(command.Name))
                        simulation.Trajectory.RunCommand(command.Name, new Dictionary<string, object>());
                }
            }
            
            GUILayout.Box("Debug");
            xrSimulatorContainer.SetActive(GUILayout.Toggle(xrSimulatorContainer.activeSelf, "Simulate Controllers"));

            GUILayout.Box("Misc");
            if (DebugButton("Quit"))
                application.Quit();

            GUILayout.EndArea();

            if (directConnect)
                ShowDirectConnectWindow();
            if (discovery)
                ShowServiceDiscoveryWindow();
        }

        private void ShowDirectConnectWindow()
        {
            BeginDebugArea(new Rect(192 + 16 * 2, 10, 192, 512));
            GUILayout.Box("Direct Connect");

            GUILayout.Label("Address");
            directConnectAddress = GUILayout.TextField(directConnectAddress);

            if (DebugButton("Connect WebSocket"))
            {
                directConnect = false;
                application.Simulation.ConnectWebSocket(directConnectAddress);
            }

            if (DebugButton("Cancel"))
                directConnect = false;

            GUILayout.EndArea();
        }

        private void ShowServiceDiscoveryWindow()
        {
            BeginDebugArea(new Rect(192 * 2 + 16 * 3, 10, 192, 512));
            GUILayout.Box("Discover Servers");

            if (DebugButton("Search"))
            {
                RefreshServiceDiscovery();
            }

            if (DebugButton("Cancel"))
                discovery = false;

            if (knownWebSockets.Count > 0)
            {
                GUILayout.Box("Found WebSockets");

                foreach (var entry in knownWebSockets)
                {
                    if (DebugButton($"{entry.code}: {entry.info.name} ({entry.info.ws})"))
                    {
                        discovery = false;
                        application.Connect(entry);
                    }
                }
            }

            if (knownServiceHubs.Count > 0)
            {
                GUILayout.Box("Found Services");

                foreach (var hub in knownServiceHubs)
                {
                    if (DebugButton($"{hub.Name} ({hub.Address})"))
                    {
                        discovery = false;
                        knownServiceHubs = new List<ServiceHub>();
                        application.Connect(hub);
                    }
                }
            }

            GUILayout.EndArea();
        }
        
        private int? ParseInt(string text)
        {
            return int.TryParse(text, out int number)
                 ? number
                 : (int?) null;
        }

        private void RefreshServiceDiscovery()
        {
            //WebsocketDiscovery.DiscoverWebsocketServers("").ContinueWith(result => knownWebSockets = result);

            var client = new Client();
            knownServiceHubs = client
                .SearchForServices(500)
                .GroupBy(hub => hub.Id)
                .Select(group => group.First())
                .ToList();
        }

        private void BeginDebugArea(Rect area)
        {
            currentGuiAreaOrigin = area.position;
            GUILayout.BeginArea(area);
        }

        private bool DebugButton(string label)
        {
            var currentButtonIndex = buttonIndex++;
            var clicked = GUILayout.Button(label);
            var rect = GUILayoutUtility.GetLastRect();

            if (clicked)
            {
                ClearFallbackClick(currentButtonIndex);
            }
            else if (ConsumePendingFallbackClick(currentButtonIndex))
            {
                clicked = true;
            }

            QueueFallbackClick(currentButtonIndex, rect);

            return clicked;
        }

        private bool ConsumePendingFallbackClick(int currentButtonIndex)
        {
            if (Event.current.type != EventType.Layout || pendingFallbackButtonIndex != currentButtonIndex)
                return false;

            pendingFallbackButtonIndex = -1;
            return true;
        }

        private void ClearFallbackClick(int currentButtonIndex)
        {
            if (pendingFallbackButtonIndex == currentButtonIndex)
                pendingFallbackButtonIndex = -1;

            if (fallbackPressedButtonIndex == currentButtonIndex)
                fallbackPressedButtonIndex = -1;
        }

        private void UpdateFallbackMouseState()
        {
            if (Event.current.type != EventType.Repaint || fallbackMouseFrame == Time.frameCount)
                return;

            fallbackMouseFrame = Time.frameCount;
            fallbackClickConsumed = false;
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse == null)
            {
                fallbackMousePressedThisFrame = false;
                fallbackMouseReleasedThisFrame = false;
                fallbackMouseWasDown = false;
                fallbackPressedButtonIndex = -1;
                return;
            }

            var isDown = mouse.leftButton.isPressed;
            fallbackMousePressedThisFrame = isDown && !fallbackMouseWasDown;
            fallbackMouseReleasedThisFrame = !isDown && fallbackMouseWasDown;
            fallbackMouseWasDown = isDown;

            var mousePosition = mouse.position.ReadValue();
            fallbackMouseGuiPosition = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
#else
            fallbackMousePressedThisFrame = false;
            fallbackMouseReleasedThisFrame = false;
            fallbackMouseWasDown = false;
            fallbackPressedButtonIndex = -1;
#endif
        }

        private void QueueFallbackClick(int currentButtonIndex, Rect localRect)
        {
            if (Event.current.type != EventType.Repaint || fallbackClickConsumed)
                return;

            var localMousePosition = fallbackMouseGuiPosition - currentGuiAreaOrigin;
            var containsMouse = localRect.Contains(localMousePosition);

            if (fallbackMousePressedThisFrame && containsMouse)
                fallbackPressedButtonIndex = currentButtonIndex;

            if (!fallbackMouseReleasedThisFrame)
                return;

            if (fallbackPressedButtonIndex != currentButtonIndex || !containsMouse)
                return;

            pendingFallbackButtonIndex = currentButtonIndex;
            fallbackPressedButtonIndex = -1;
            fallbackClickConsumed = true;
        }
    }
}
