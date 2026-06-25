using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace Nanover.Frontend.Controllers
{
    /// <summary>
    /// Manager class for accessing the left and right controller.
    /// </summary>
    public class ControllerManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private VrController leftController;

        [SerializeField]
        private VrController rightController;

        [SerializeField]
        private InputDeviceCharacteristics dominantHand;
#pragma warning restore 0649

        public InputDeviceCharacteristics DominantHand => dominantHand;

        public event Action DominantHandChanged;

        public bool ShouldBroadcastCursors => CurrentInputMode?.ShouldBroadcastCursors ?? false;

        private void OnEnable()
        {
            leftController.ControllerReset += SetupLeftController;
            rightController.ControllerReset += SetupRightController;
        }

        private void SetupRightController()
        {
            if(CurrentInputMode != null)
                CurrentInputMode.SetupController(rightController, InputDeviceCharacteristics.Right);
        }

        private void SetupLeftController()
        {
            if(CurrentInputMode != null)
                CurrentInputMode.SetupController(leftController, InputDeviceCharacteristics.Left);
        }

        /// <summary>
        /// The left <see cref="VrController" />.
        /// </summary>
        public VrController LeftController => leftController;

        /// <summary>
        /// The right <see cref="VrController" />.
        /// </summary>
        public VrController RightController => rightController;

        public VrController GetController(InputDeviceCharacteristics characteristics)
        {
            if (characteristics == InputDeviceCharacteristics.Left)
                return LeftController;
            if (characteristics == InputDeviceCharacteristics.Right)
                return RightController;
            throw new ArgumentOutOfRangeException(
                $"Cannot get VR Controller corresponding to {characteristics}");
        }

        /// <summary>
        /// Set the dominant hand, which is the one which is currently in control of UI.
        /// </summary>
        public void SetDominantHand(InputDeviceCharacteristics characteristics)
        {
            dominantHand = characteristics;
            DominantHandChanged?.Invoke();
        }

        /// <summary>
        /// A list of Input Modes which could be active. Only the first item of the list is considered to be active.
        /// </summary>
        private List<ControllerInputMode> modes = new List<ControllerInputMode>();

        public ControllerInputMode CurrentInputMode => modes.FirstOrDefault();
        
        public void AddInputMode(ControllerInputMode mode)
        {
            var current = CurrentInputMode;
            var inserted = false;
            for(var i = 0; i < modes.Count && !inserted; i++)
                if (mode.Priority > modes[i].Priority)
                {
                    modes.Insert(i, mode);
                    inserted = true;
                }
            if(!inserted)
                modes.Add(mode);
            if (CurrentInputMode != current)
            {
                if (current != null)
                {
                    current.OnModeEnded();
                }
                if (CurrentInputMode != null)
                {
                    CurrentInputMode.OnModeStarted();
                    SetupLeftController();
                    SetupRightController();
                }
            }
        }

        public void RemoveInputMode(ControllerInputMode mode)
        {
            var current = CurrentInputMode;
            modes.Remove(mode);
            if (CurrentInputMode != current)
            {
                if (current != null)
                {
                    current.OnModeEnded();
                }
                if (CurrentInputMode != null)
                {
                    CurrentInputMode.OnModeStarted();
                    SetupLeftController();
                    SetupRightController();
                }
            }
        }

        /// <summary>
        /// If the given <see cref="ControllerInputMode"/> were to be added with
        /// <see cref="AddInputMode"/>, would its priority mean that it would become the
        /// current input?
        /// </summary>
        public bool WouldBecomeCurrentMode(ControllerInputMode mode)
        {
            return CurrentInputMode == null || mode.Priority > CurrentInputMode.Priority;
        }
    }
}