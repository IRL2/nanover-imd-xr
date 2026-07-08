using Nanover.Frontend.Utility;
using System.Collections.Generic;
using UnityEngine;
using Text = TMPro.TextMeshProUGUI;
using Nanover.Core;

namespace NanoverImd.UI
{
    public class AdhocPanel : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField]
        private NanoverImdApplication app;
        
        [SerializeField]
        private Text titleText;

        [SerializeField]
        private AdhocElement elementTemplate;

        private IndexedPool<AdhocElement> elementPool;

        private void Awake()
        {
            elementPool = new IndexedPool<AdhocElement>
                (() => Instantiate(elementTemplate, parent: elementTemplate.transform.parent),
                transform => transform.gameObject.SetActive(true),
                transform => transform.gameObject.SetActive(false)
            );
        }

        private void Start()
        {
            var panel = app.Simulation.Multiplayer.SharedStateDictionary.GetValueOrDefault<Dictionary<string, object>>("panel.test");
            if (panel != null)
                Configure(panel);

            //Configure(new Dictionary<string, object>()
            //{
            //    { "label", "COOL PANEL" },
            //    { "elements", new Dictionary<string, object>[] {
            //        new Dictionary<string, object> { { "type", "header" }, { "label", "nice controls" } },
            //        new Dictionary<string, object> { { "type", "slider" }, { "label", "no label" } },
            //        new Dictionary<string, object> { { "type", "button" }, { "label", "epic action" } },
            //    }},
            //});
        }

        public void Configure(Dictionary<string, object> data)
        {
            titleText.text = data.GetValueOrDefault<string>("label") ?? "Unnamed Panel";
            
            var elements = data.GetValueOrDefault<object[]>("elements") ?? new object[] { };
            elementPool.MapConfig(elements, (data, element) => element.Configure(data as Dictionary<string, object>));
        }
    }
}