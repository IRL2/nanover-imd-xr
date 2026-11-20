using Nanover.Core;
using Nanover.Frontend.Controllers;
using System;
using UnityEngine;

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
                if (args.GetValueOrDefault<string>("message") is string message)
                {
                    notifiedController.PushNotification(message);
                }

                if (args.GetValueOrDefault<string>("audio") is string resourceId)
                {
                    audioSource.PlayOneShot(remoteAudioManager.GetAudio(resourceId));
                }

                return null;
            }
        }

    }
}