using Nanover.Core.Math;
using Nanover.Frontend.Controllers;
using Nanover.Frontend.Input;
using Nanover.Frontend.XR;
using UnityEngine;
using UnityEngine.XR;

namespace NanoverImd
{
    public class NanoverImdLocalTranslateControls : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NanoverImdApplication application;

        [SerializeField]
        private ControllerManager controllers;
#pragma warning restore 0649

        private void Awake()
        {
            var buttonUsage = CommonUsages.gripButton;
            var leftPoseObject = controllers.LeftController.CursorPose;
            var leftButton = InputDeviceCharacteristics.Left.WrapUsageAsButton(buttonUsage);
            var rightPoseObject = controllers.RightController.CursorPose;
            var rightButton = InputDeviceCharacteristics.Right.WrapUsageAsButton(buttonUsage);

            WrapController(leftPoseObject, leftButton);
            WrapController(rightPoseObject, rightButton);

            void WrapController(IPosedObject posedObject, IButton button)
            {
                Matrix4x4? initialMatrix = null;
                Vector3? initialPosition = null;

                button.Pressed += () =>
                {
                    if (posedObject.Pose is { } pose && application.Simulation.InLocalPlayback)
                    {
                        initialMatrix = application.CalibratedSpace.LocalToWorldMatrix;
                        initialPosition = pose.Position;
                    }
                };

                button.Released += () =>
                {
                    initialMatrix = null;
                    initialPosition = null;
                };

                posedObject.PoseChanged += () =>
                {
                    if (!application.Simulation.InLocalPlayback)
                    {
                        initialMatrix = null;
                        initialPosition = null;
                        return;
                    }

                    if (initialPosition is { } position
                        && initialMatrix is { } matrix
                        && posedObject.Pose is { } current
                    )
                    {
                        var translation = Matrix4x4.Translate(current.Position - position);
                        application.CalibratedSpace.CalibrateFromMatrix(translation * matrix);
                    }
                };
            }
        }
    }
}