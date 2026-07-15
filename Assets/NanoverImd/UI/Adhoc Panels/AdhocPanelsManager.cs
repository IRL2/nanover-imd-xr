using Nanover.Core;
using Nanover.Core.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace NanoverImd.UI
{
    public class AdhocPanelsManager : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField]
        private NanoverImdApplication app;

        private HashSet<AdhocPanel> panels = new HashSet<AdhocPanel>();

        private void Awake()
        {
            app.Simulation.Multiplayer.SharedStateDictionaryCleared += OnCleared;
            app.Simulation.Multiplayer.SharedStateDictionaryKeyUpdated += OnKeyUpdated;
            app.Simulation.Multiplayer.SharedStateDictionaryKeyRemoved += OnKeyRemoved;

            void OnCleared()
            {
                foreach (var panel in panels)
                    panel.Configure();
            }

            void OnKeyUpdated(string key, object value)
            {
                if (key == "panel.test")
                    foreach (var panel in panels)
                        panel.Configure();
                else if (key.StartsWith("variable."))
                    foreach (var panel in panels)
                        panel.OnVariableUpdated(key, value);
            }

            void OnKeyRemoved(string key)
            {
                if (key == "panel.test")
                    foreach (var panel in panels)
                        panel.Configure();
            }
        }

        public void RegisterPanel(AdhocPanel panel)
        {
            panels.Add(panel);
        }

        public void UnregisterPanel(AdhocPanel panel)
        {
            panels.Remove(panel);
        }
    }
}