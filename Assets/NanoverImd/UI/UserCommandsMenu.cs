using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Nanover.Frontend.Controllers;
using Nanover.Frontend.UI;
using UnityEngine;

namespace NanoverImd.UI
{
    public class UserCommandsMenu : MonoBehaviour
    {
        [SerializeField]
        private VrController notifiedController;

        [SerializeField]
        private Sprite commandIcon;

        [SerializeField]
        private NanoverImdSimulation simulation;

        [SerializeField]
        private DynamicMenu menu;

        private void OnEnable()
        {
            RefreshCommands();
        }

        public async void RefreshCommands()
        {
            await simulation.Trajectory.UpdateCommands();
            RefreshButtons();
        }
        
        public void RefreshButtons()
        {
            menu.ClearChildren();
            
            foreach (var command in simulation.Trajectory.CommandDefinitions.Values.Where(command => command.Name.StartsWith("user/")))
            {
                async void RunCommand()
                {
                    notifiedController.PushNotification($"Run {command.Name}");
                    var result = await simulation.Trajectory.RunCommand(command.Name);
                    if (result != null && result.TryGetValue("result", out object notification))
                        notifiedController.PushNotification($"{command.Name}: {notification}");
                    else if (result != null)
                        notifiedController.PushNotification($"Completed {command.Name}");
                }

                string commandName = "";
                foreach (string part in command.Label.Split('/').Skip(1))
                {
                    commandName += part + "\n";
                }

                menu.AddItem(commandName, emoji: command.Icon, RunCommand);
            }

            // activate the rest of the menu, only if there are commands to show
            if (menu.GetButtonCount() > 0)
            {
                ChildrenSetActive(true);
                this.gameObject.transform.parent.transform.position += new Vector3(0, 0.1f, 0);
            }
        }

        private void ChildrenSetActive(bool state)
        {
            int otherChildsCount = this.gameObject.transform.childCount;
            for (int i = 0; i < otherChildsCount; i++)
            {
                this.gameObject.transform.GetChild(i).gameObject.SetActive(state);
            }
        }
    }
}