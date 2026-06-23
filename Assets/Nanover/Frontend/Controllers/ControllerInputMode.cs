using UnityEngine;
using UnityEngine.XR;

namespace Nanover.Frontend.Controllers
{
    /// <summary>
    /// Define the mode of the controller as 'interaction', setting up the correct
    /// gizmo.
    /// </summary>
    /// <remarks>
    /// This is a temporary object until a more sophisticated multi-mode tool setup is
    /// completed.
    /// </remarks>
    public abstract class ControllerInputMode : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private ControllerManager controllers;

        [SerializeField]
        private bool broadcastCursors;
#pragma warning restore 0649

        protected ControllerManager Controllers => controllers;

        protected bool IsCurrentInputMode => controllers.CurrentInputMode == this;

        public bool ShouldBroadcastCursors => broadcastCursors;

        public abstract int Priority { get; }

        private void OnEnable()
        {
            controllers.AddInputMode(this);
        }

        private void OnDisable()
        {
            controllers.RemoveInputMode(this);
        }

        public abstract void OnModeStarted();
        public abstract void OnModeEnded();

        public abstract void SetupController(VrController controller, InputDeviceCharacteristics inputSource);
    }
}