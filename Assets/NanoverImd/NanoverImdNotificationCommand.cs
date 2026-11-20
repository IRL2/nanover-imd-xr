using Nanover.Core;
using Nanover.Frontend.Controllers;
using Nanover.Frontend.XR;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using CommandArguments = System.Collections.Generic.Dictionary<string, object>;
using CommandReturn = System.Collections.Generic.Dictionary<string, object>;

namespace NanoverImd
{
    public sealed class NanoverImdNotificationCommand : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NanoverImdSimulation simulation;
        [SerializeField]
        private VrController notifiedController;
        [SerializeField]
        private VrController otherController;
        [SerializeField]
        private NanoverImdRemoteAudioManager remoteAudioManager;
        [SerializeField]
        private AudioSource audioSource;
#pragma warning restore 0649

        private void Start()
        {
            var id = Guid.NewGuid().ToString();

            simulation.RegisterCommand($"notify", Notify);

            CommandReturn Notify(CommandArguments args)
            {
                bool head = false;
                bool left = false;
                bool right = false;

                if (args.GetValueOrDefault<string>("target") is string target)
                {
                    var hands = target == "hands";
                    var all = target == "all";

                    head = target == "head" || all;
                    left = target == "left" || hands || all;
                    right = target == "right" || hands || all;
                }
                else
                {
                    right = true;
                }

                if (args.GetValueOrDefault<string>("message") is string message)
                {
                    if (right) 
                        notifiedController.PushNotification(message);
                    if (left)
                        otherController.PushNotification(message);
                }

                if (args.GetValueOrDefault<string>("audio") is string resourceId)
                {
                    var clip = remoteAudioManager.GetAudioClip(resourceId);

                    if (right)
                        notifiedController.PlayAudioClip(clip);
                    if (left)
                        otherController.PlayAudioClip(clip);
                    if (head)
                        audioSource.PlayOneShot(clip);
                }

                if (args.ContainsKey("haptic"))
                {
                    if (right)
                        InputDeviceCharacteristics.Right.GetFirstDevice().SendHapticImpulse(0, 1, .5f);
                    if (left)
                        InputDeviceCharacteristics.Left.GetFirstDevice().SendHapticImpulse(0, 1, .5f);
                }

                return null;
            }
        }

    }
}