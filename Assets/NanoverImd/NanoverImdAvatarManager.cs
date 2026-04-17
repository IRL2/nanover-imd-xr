using Nanover.Core.Math;
using Nanover.Frontend.Utility;
using Nanover.Frontend.XR;
using Nanover.Network.Multiplayer;
using NanoverImd.UI;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

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

        private MultiplayerAvatar LocalAvatar => nanover.Multiplayer.Avatars.LocalAvatar;

        public bool HideHeads;
        public bool HideHands;

        private void Update()
        {
            UpdateSuggestedParameters();
            UpdateRendering();
        }

        private void UpdateSuggestedParameters()
        {
            const string headsKey = "suggested.hide.heads";
            const string handsKey = "suggested.hide.hands";

            if (nanover.Multiplayer.GetSharedState(headsKey) is bool hideHeads)
            {
                HideHeads = hideHeads;
            }

            if (nanover.Multiplayer.GetSharedState(handsKey) is bool hideHands)
            {
                HideHands = hideHands;
            }
        }

        private void OnEnable()
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
            sendAvatarsCoroutine = StartCoroutine(UpdateLocalAvatar());
        }

        private void OnDisable()
        {
            StopCoroutine(sendAvatarsCoroutine);
        }

        private IEnumerator UpdateLocalAvatar()
        {
            var leftHand = InputDeviceCharacteristics.Left.WrapAsPosedObject();
            var rightHand = InputDeviceCharacteristics.Right.WrapAsPosedObject();
            var headset = InputDeviceCharacteristics.HeadMounted.WrapAsPosedObject();

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

            if (!HideHeads)
            {
                headsetObjects.MapConfig(headsets, UpdateAvatarComponent);
            }
            else
            {
                headsetObjects.SetActiveInstanceCount(0);
            }

            if (!HideHands)
            {
                controllerObjects.MapConfig(controllers, UpdateAvatarComponent);
            }
            else
            {
                controllerObjects.SetActiveInstanceCount(0);
            }

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