using Nanover.Core;
using Nanover.Core.Serialization;
using Nanover.Frontend.Utility;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Text = TMPro.TextMeshProUGUI;

namespace NanoverImd.UI
{
    public class AdhocPanel : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField]
        private NanoverImdApplication app;

        [SerializeField]
        private AdhocPanelsManager manager;
        
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

            manager.RegisterPanel(this);
        }

        private void OnDestroy()
        {
            manager.UnregisterPanel(this);
        }

        private void OnEnable()
        {
            TestConfigure();
        }

        private void TestConfigure()
        {
            var data = app.Simulation.Multiplayer.SharedStateDictionary.GetValueOrDefault<Dictionary<string, object>>("panel.test");
            if (data != null)
                Configure(Serialization.FromDataStructure<AdhocPanelData>(data));
            else
                gameObject.SetActive(false);
        }

        public void Configure()
        {
            titleText.text = "No Panel";
            elementPool.SetActiveInstanceCount(0);
        }

        public void Configure(AdhocPanelData data)
        {
            titleText.text = data.Label ?? "Unnamed Panel";
            elementPool.MapConfig(data.Elements, (data, element) => element.Configure(data));
        }

        public void OnVariableUpdated(string key, object value)
        {
            TestConfigure();
        }
    }

    [DataContract]
    public class AdhocPanelData
    {
        [DataMember(Name="label")]
        public string Label { get; set; }

        [DataMember(Name="elements")]
        public List<AdhocElementData> Elements { get; set; } = new List<AdhocElementData>();
    }

    [DataContract]
    public class AdhocElementData
    {
        [DataMember(Name ="type")]
        public string Type { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Other = new Dictionary<string, object>();
    }

    [DataContract]
    public class AdhocHeaderData : AdhocElementData
    {
        [DataMember(Name="label")]
        public string Label { get; set; }
    }

    [DataContract]
    public class AdhocButtonData : AdhocElementData
    {
        [DataMember(Name = "label")]
        public string Label { get; set; }

        [DataMember(Name = "command")]
        public string Command { get; set; }

        [DataMember(Name = "arguments")]
        public Dictionary<string, ArgumentData> Arguments { get; set; }

        [DataContract]
        public class ArgumentData
        {
            [DataMember(Name = "variable")]
            public string Variable { get; set; }

            [DataMember(Name = "literal")]
            public object Literal { get; set; }
        }
    }

    [DataContract]
    public class AdhocSliderData : AdhocElementData
    {
        [DataMember(Name = "label")]
        public string Label { get; set; }

        [DataMember(Name = "variable")]
        public string Variable { get; set; }

        [DataMember(Name = "range")]
        public List<float> Range { get; set; }

        [DataMember(Name = "integer")]
        public bool IsInteger { get; set; }

        [DataMember(Name = "step")]
        public float? StepSize { get; set; }
    }
}