using Cysharp.Threading.Tasks;
using Essd;
using Nanover.Core.Math;
using Nanover.Frontend.Controllers;
using Nanover.Frontend.XR;
using Nanover.Network.Multiplayer;
using Nanover.Visualisation;
using NanoverImd.Interaction;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using WebDiscovery;
using static Nanover.Network.Trajectory.TrajectorySession;

namespace NanoverImd
{
    /// <summary>
    /// The entry point to the application, and central location for accessing
    /// shared resources.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NanoverImdApplication : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NanoverImdSimulation simulation;

        [SerializeField]
        private GameObject boxInteraction;

        [SerializeField]
        private GameObject boxVisualiser;

        [SerializeField]
        public ControllerManager controllerManager;

        [SerializeField]
        private NanoverImdMetaCalibrator metaCalibrator;

        [Header("Passthrough")]
        [SerializeField]
        [Range(0f, 1f)]
        private float passthrough = 1f;

        [SerializeField]
        private OVRPassthroughLayer passthroughLayer;

        [SerializeField]
        private new Camera camera;

        [SerializeField]
        private float[] passthroughCyclingStops;


        [Header("Events")]
        [SerializeField]
        private UnityEvent connectionEstablished;
        [SerializeField]
        private UnityEvent connectionLost;
        [SerializeField]
        private UnityEvent afterCalibration;
#pragma warning restore 0649

        public NanoverImdSimulation Simulation => simulation;

        public bool ManualColocation { get; set; } = false;
        public bool ColocateLighthouses { get; set; } = false;
        public float PlayAreaRotationCorrection { get; set; } = 0;
        public float PlayAreaRadialDisplacementFactor { get; set; } = 0;

        /// <summary>
        /// The route through which simulation space can be manipulated with
        /// gestures to perform translation, rotation, and scaling.
        /// </summary>
        public ManipulableScenePose ManipulableSimulationSpace { get; private set; }

        public PhysicallyCalibratedSpace CalibratedSpace { get; } = new PhysicallyCalibratedSpace();

        private void Awake()
        {
            if (PlayerPrefs.GetFloat("passthrough", 1f) is float savedPassthrough && savedPassthrough >= 0f)
            {
                passthrough = savedPassthrough;
            };

            simulation.SessionOpened += connectionEstablished.Invoke;
            simulation.SessionClosed += connectionLost.Invoke;
        }

        // These methods expose the underlying async methods to Unity for use
        // in the UI so we disable warnings about not awaiting them, and use
        // void return type instead of Task.
#pragma warning disable 4014
        /// <summary>
        /// Connect to the Nanover services described in a given ServiceHub.
        /// </summary>
        public void Connect(ServiceHub hub) => simulation.Connect(hub);

        public void Connect(DiscoveryEntry entry) => simulation.Connect(entry);

        /// <summary>
        /// Connect to the first set of Nanover services found via ESSD.
        /// </summary>
        public void AutoConnect() => simulation.AutoConnect();

        public void AutoConnectWebSocket() => simulation.AutoConnectWebSocket();

        /// <summary>
        /// Disconnect from all Nanover services.
        /// </summary>
        public void Disconnect() => simulation.Close();

        /// <summary>
        /// Called from UI to quit the application.
        /// </summary>
        public void Quit() => Application.Quit();

        public async UniTask<IEnumerable<CommandDefinition>> GetUserCommands()
        {
            var commands = await simulation.Trajectory.UpdateCommands();
            return commands.Values.Where(command => command.Name.StartsWith("user/"));
        }

        private void Update()
        {
            if (ManualColocation)
            {

            }
            else if (ColocateLighthouses)
            {
                CalibratedSpace.CalibrateFromLighthouses();
            }
            else
            {
                CalibrateFromRemote();
            }

            UpdateSuggestedParameters();

            UpdatePlayArea();

            // update passthrough
            passthroughLayer.textureOpacity = passthrough;

            if (passthrough == 0.0f)
            {
                camera.clearFlags = CameraClearFlags.Skybox;
            } else
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
            }
        }

        private void UpdateSuggestedParameters()
        {
            const string scaleKey = "suggested.interaction.scale";
            const string typeKey = "suggested.interaction.type";
            const string passthroughKey = "suggested.passthrough";
            const string boxLockedKey = "suggested.box.locked";
            const string boxHiddenKey = "suggested.box.hidden";
            const string debugColocationKey = "debug.colocation";

            if (simulation.Multiplayer.GetSharedState(scaleKey) is double scale)
            {
                simulation.ManipulableParticles.ForceScale = (float)scale;
            }

            if (simulation.Multiplayer.GetSharedState(typeKey) is string type)
            {
                simulation.ManipulableParticles.ForceType = type;
            }

            if (simulation.Multiplayer.GetSharedState(passthroughKey) is double value)
            {
                passthrough = (float)value;
            }

            if (simulation.Multiplayer.GetSharedState(boxLockedKey) is bool locked)
            {
                boxInteraction.SetActive(!locked);
            }
            else
            {
                boxInteraction.SetActive(true);
            }

            if (simulation.Multiplayer.GetSharedState(boxHiddenKey) is bool hidden)
            {
                boxVisualiser.SetActive(!hidden);
            }
            else
            {
                boxVisualiser.SetActive(true);
            }

            var colocationDebug = false;

            if (simulation.Multiplayer.GetSharedState(debugColocationKey) is bool debug)
                colocationDebug = debug;

            metaCalibrator.referenceAnchor.gameObject.SetActive(colocationDebug);
            metaCalibrator.referencePointA.gameObject.SetActive(colocationDebug);
            metaCalibrator.referencePointB.gameObject.SetActive(colocationDebug);
        }

        private Vector3 playareaSize = Vector3.zero;

        /// <summary>
        /// Determine VR playarea size;
        /// </summary>
        private void UpdatePlayArea()
        {
            var system = InputDeviceCharacteristics.HeadMounted.GetFirstDevice().subsystem;

            if (system == null)
                return;

            var points = new List<Vector3>();
            if (!system.TryGetBoundaryPoints(points) || points.Count != 4)
                return;

            playareaSize.x = (points[0] - points[1]).magnitude;
            playareaSize.z = (points[0] - points[3]).magnitude;

            if (simulation.Multiplayer.AccessToken == null)
                return;

            var area = new PlayArea
            {
                A = TransformCornerPosition(points[0]),
                B = TransformCornerPosition(points[1]),
                C = TransformCornerPosition(points[2]),
                D = TransformCornerPosition(points[3]),
            };

            simulation.Multiplayer.PlayAreas.UpdateValue(simulation.Multiplayer.AccessToken, area);

            Vector3 TransformCornerPosition(Vector3 position)
            {
                var transform = new Transformation(position, Quaternion.identity, Vector3.one);
                return CalibratedSpace.TransformPoseWorldToCalibrated(transform).Position;
            }
        }

        private List<Transformation> poses = new List<Transformation>();

        public void RunCalibration()
        {
            metaCalibrator.Clear();
            poses.Clear();
            ManualColocation = true;

            controllerManager.RightController.PushNotification($"Click over two spatial reference");

            var hand = InputDeviceCharacteristics.Right;
            var button = hand.WrapUsageAsButton(CommonUsages.triggerButton);
            button.Pressed += OnPressed;

            void OnPressed()
            {
                if (controllerManager.RightController.CursorPose.Pose is { } pose)
                {
                    poses.Add(pose);

                    if (poses.Count == 1)
                    {
                        metaCalibrator.referencePointA.position = pose.Position;
                        controllerManager.RightController.PushNotification($"Point A placed");
                    }
                    else
                    {
                        metaCalibrator.referencePointB.position = pose.Position;
                        controllerManager.RightController.PushNotification($"Point B placed");
                    }

                    if (poses.Count >= 2)
                        OnReady();
                }
            }

            void OnReady()
            {
                button.Pressed -= OnPressed;

                var point0 = poses[0].Position;
                var point1 = poses[1].Position;

                // assume that headsets agree on y-axis
                point0.y = 0;
                point1.y = 0;

                //CalibratedSpace.CalibrateFromTwoControlPoints(point0, point1);
                metaCalibrator.Setup(point0, point1);

                afterCalibration.Invoke();
            }
        }

        [SerializeField]
        private BoxVisualiser box;

        [ContextMenu("CENTER BOX")]
        public void RecalibrateToBoxAroundCamera()
        {
            Move();

            async UniTask Move()
            {
                CalibratedSpace.CalibrateFromMatrix(Matrix4x4.identity);

                await Task.Delay(50);

                ManualColocation = true;

                var anchor = box.Box.axesMagnitudes * .5f;
                anchor.z = 0;
                var center = boxVisualiser.transform.TransformPoint(anchor);
                var target = camera.transform.position;

                var direction = camera.transform.forward;
                direction.y = 0;
                direction.Normalize();
                var cameraFacing = Quaternion.LookRotation(direction);

                var boxCenterToOrigin = Matrix4x4.Translate(-center);
                var rotateToCameraFacing = Matrix4x4.Rotate(cameraFacing);
                var originToCamera = Matrix4x4.Translate(camera.transform.position);

                CalibratedSpace.CalibrateFromMatrix(originToCamera * rotateToCameraFacing * boxCenterToOrigin);
            }

            //const float offset = .5f;

            //var cornerToCenter = Matrix4x4.Translate(box.Box.axesMagnitudes * .5f);
            //var cameraToCenter = Matrix4x4.Translate(Vector3.forward * offset);

            //var cornerS = box.transform.localToWorldMatrix;
            //var centerS = cornerS * cornerToCenter;

            //var cameraD = camera.transform.localToWorldMatrix;
            //var centerD = cameraD * cameraToCenter;
            //var cornerD = centerD * cornerToCenter;

            //var delta = cornerD.inverse * cornerS;

            //CalibratedSpace.CalibrateFromMatrix(delta);
        }

        /// <summary>
        /// Calibrate space from suggested origin in state service, defaulting to world origin.
        /// </summary>
        private void CalibrateFromRemote()
        {
            var key = simulation.Multiplayer.AccessToken;
            var origin = simulation.Multiplayer.PlayOrigins.ContainsKey(key)
                       ? simulation.Multiplayer.PlayOrigins.GetValue(key).Transformation
                       : UnitScaleTransformation.identity;

            var longest = Mathf.Max(playareaSize.x, playareaSize.z);
            var offset = longest * PlayAreaRadialDisplacementFactor;
            var playspaceToShared = origin.matrix.inverse;
            var deviceToPlayspace = Matrix4x4.TRS(
                Vector3.zero,
                Quaternion.AngleAxis(PlayAreaRotationCorrection, Vector3.up),
                Vector3.one
            ) * Matrix4x4.TRS(
                Vector3.left * offset,
                Quaternion.identity,
                Vector3.one
            );


            CalibratedSpace.CalibrateFromMatrix(deviceToPlayspace * playspaceToShared);
        }

        /// <summary>
        /// Toggle passthrough on/off.
        /// </summary>
        public void TogglePassthrough()
        {
            passthrough = passthrough > 0f ? 0f : 1f;
            simulation.Multiplayer.SetSharedState("suggested.passthrough", passthrough);
            PlayerPrefs.SetFloat("passthrough", passthrough);
        }


        // Cycle passthrough through the keyframes defined in the AnimationCurve
        public void CyclePassthrough()
        {
            passthrough = GetNextPassthroughStep(passthrough, passthroughCyclingStops);

            simulation.Multiplayer.SetSharedState("suggested.passthrough", passthrough);
            PlayerPrefs.SetFloat("passthrough", passthrough);
        }

        public float GetPassthroughValue()
        {
            return passthrough;
        }

        private float GetNextPassthroughStep(float current, float[] stops)
        {
            if (stops == null || stops.Length == 0) return current;

            int idx = GetPassthroughIndexStep();

            idx = (idx + 1) % stops.Length;

            return stops[idx];
        }

        public int GetPassthroughIndexStep()
        {
            float current = passthrough;
            float[] stops = passthroughCyclingStops;
            float epsilon = 1e-6f;

            // find index of the keyframe closer to the current value
            int idx = -1;
            for (int i = 0; i < stops.Length; i++)
            {
                if (Mathf.Abs(stops[i] - current) <= epsilon)
                {
                    idx = i;
                    break;
                }
            }
            return idx;
        }

    }
}