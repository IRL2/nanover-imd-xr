using Nanover.Core.Math;
using Nanover.Frontend.Controllers;
using Nanover.Frontend.Input;
using Nanover.Frontend.Utility;
using Nanover.Frontend.XR;
using Nanover.Network.Multiplayer;
using NanoverImd.UI;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputTrackingAggregator;

namespace NanoverImd
{
    public class NanoverImdAvatarManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NanoverImdApplication application;
        
        [SerializeField]
        private NanoverImdSimulation nanover;

        [SerializeField]
        private AvatarModel headsetPrefab;

        [SerializeField]
        private AvatarModel controllerPrefab;
#pragma warning restore 0649
        
        private IndexedPool<AvatarModel> headsetObjects;
        private IndexedPool<AvatarModel> controllerObjects;
        
        private Coroutine sendAvatarsCoroutine;
        private Coroutine sendCursorsCoroutine;

        private MultiplayerAvatar LocalAvatar => nanover.Multiplayer.Avatars.LocalAvatar;

        private void Start()
        {
            headsetObjects = new IndexedPool<AvatarModel>(
                () => Instantiate(headsetPrefab),
                transform => transform.gameObject.SetActive(true),
                transform => transform.gameObject.SetActive(false)
            );

            controllerObjects = new IndexedPool<AvatarModel>(
                () => Instantiate(controllerPrefab),
                transform => transform.gameObject.SetActive(true),
                transform => transform.gameObject.SetActive(false)
            );
        }

        private void Update()
        {
            UpdateRendering();
        }

        private void OnEnable()
        {
            sendAvatarsCoroutine = StartCoroutine(UpdateLocalAvatar());
            sendCursorsCoroutine = StartCoroutine(UpdateLocalCursors());
        }

        private void OnDisable()
        {
            StopCoroutine(sendAvatarsCoroutine);
            StopCoroutine(sendCursorsCoroutine);
        }

        private IEnumerator UpdateLocalCursors()
        {
            var buttonUsage = CommonUsages.primaryButton;

            var buttons = new[]
            {
                new { Name = "primary", Usage = CommonUsages.primaryButton },
                new { Name = "secondary", Usage = CommonUsages.secondaryButton },
                new { Name = "trigger", Usage = CommonUsages.triggerButton },
                new { Name = "grip", Usage = CommonUsages.gripButton },
            };

            var leftCursorObject = application.controllerManager.LeftController.CursorPose;
            var rightCursorObject = application.controllerManager.RightController.CursorPose;

            while (true)
            {
                if (nanover.Multiplayer.IsOpen)
                {
                    if (!application.controllerManager.ShouldBroadcastCursors)
                    {
                        nanover.Multiplayer.Cursors.LocalCursorLeft = null;
                        nanover.Multiplayer.Cursors.LocalCursorRight = null;
                    }
                    else
                    {
                        nanover.Multiplayer.Cursors.LocalCursorLeft = MakeCursor(leftCursorObject, InputDeviceCharacteristics.Left);
                        nanover.Multiplayer.Cursors.LocalCursorRight = MakeCursor(rightCursorObject, InputDeviceCharacteristics.Right);
                    }

                    nanover.Multiplayer.Cursors.FlushLocalCursors();
                }

                yield return null;
            }

            MultiplayerCursor MakeCursor(IPosedObject posedObject, InputDeviceCharacteristics characteristic)
            {
                if (posedObject.Pose is not { } pose)
                    return null;

                var device = characteristic.GetFirstDevice();

                return new MultiplayerCursor
                {
                    OwnerID = nanover.Multiplayer.AccessToken,
                    Position = pose.Position,
                    Rotation = pose.Rotation,
                    HeldButtons = buttons.Where((button) => device.GetButtonPressed(button.Usage) ?? false).Select((button) => button.Name).ToList(),
                    Joystick = device.GetJoystickValue(CommonUsages.primary2DAxis) ?? Vector2.zero,
                };
            }
        }

        private IEnumerator UpdateLocalAvatar()
        {
            var leftHand = InputDeviceCharacteristics.Left.WrapAsPosedObject();
            var rightHand = InputDeviceCharacteristics.Right.WrapAsPosedObject();
            var headset = InputDeviceCharacteristics.HeadMounted.WrapAsPosedObject();

            var leftCursor = application.controllerManager.LeftController.CursorPose;
            var rightCursor = application.controllerManager.RightController.CursorPose;

            while (true)
            {
                if (nanover.Multiplayer.IsOpen)
                {
                    LocalAvatar.SetTransformations(
                        TransformPoseWorldToCalibrated(headset.Pose),
                        TransformPoseWorldToCalibrated(leftHand.Pose),
                        TransformPoseWorldToCalibrated(rightHand.Pose));
                    LocalAvatar.Name = PlayerName.GetPlayerName();
                    LocalAvatar.Color = PlayerColor.GetPlayerColor();
                    nanover.Multiplayer.Avatars.FlushLocalAvatar();
                }

                yield return null;
            }
        }

        private void UpdateRendering()
        {
            var headsets = nanover.Multiplayer
                                 .Avatars.OtherPlayerAvatars
                                 .SelectMany(avatar => avatar.Components, (avatar, component) =>
                                                 (Avatar: avatar, Component: component))
                                 .Where(res => res.Component.Name == MultiplayerAvatar.HeadsetName);


            var controllers = nanover.Multiplayer
                                    .Avatars.OtherPlayerAvatars
                                    .SelectMany(avatar => avatar.Components, (avatar, component) =>
                                                    (Avatar: avatar, Component: component))
                                    .Where(res => res.Component.Name == MultiplayerAvatar.LeftHandName
                                               || res.Component.Name == MultiplayerAvatar.RightHandName);

            headsetObjects.MapConfig(headsets, UpdateAvatarComponent);
            controllerObjects.MapConfig(controllers, UpdateAvatarComponent);

            void UpdateAvatarComponent((MultiplayerAvatar Avatar, MultiplayerAvatar.Component Component) value, AvatarModel model)
            {
                var transformed = TransformPoseCalibratedToWorld(value.Component.Transformation).Value;
                model.transform.SetPositionAndRotation(transformed.Position, transformed.Rotation);
                model.SetPlayerColor(value.Avatar.Color);
                model.SetPlayerName(value.Avatar.Name);
            }
        }

        public Transformation? TransformPoseCalibratedToWorld(Transformation? pose)
        {
            if (pose is Transformation calibratedPose)
                return application.CalibratedSpace.TransformPoseCalibratedToWorld(calibratedPose);

            return null;
        }

        public Transformation? TransformPoseWorldToCalibrated(Transformation? pose)
        {
            if (pose is Transformation worldPose)
                return application.CalibratedSpace.TransformPoseWorldToCalibrated(worldPose);

            return null;
        }
    }
}