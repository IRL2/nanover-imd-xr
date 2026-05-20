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

                return null;
            }
        }

    }
}