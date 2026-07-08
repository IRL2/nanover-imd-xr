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
            var data = app.Simulation.Multiplayer.SharedStateDictionary.GetValueOrDefault<Dictionary<string, object>>("panel.test");
            if (data != null)
                Configure(Serialization.FromDataStructure<AdhocPanelData>(data));
        }

        public void Configure(AdhocPanelData data)
        {
            titleText.text = data.Label ?? "Unnamed Panel";
            elementPool.MapConfig(data.Elements, (data, element) => element.Configure(data));
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
        [DataMember(Name="label")]
        public string Label { get; set; }
    }

    [DataContract]
    public class AdhocSliderData : AdhocElementData
    {
        [DataMember(Name="label")]
        public string Label { get; set; }
    }
}