using Nanover.Core.Serialization;
using Nanover.Frontend.UI;
using System.Collections.Generic;
using UnityEngine;
using Text = TMPro.TextMeshProUGUI;

namespace NanoverImd.UI
{
    public class AdhocElement : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField]
        private Text headerElement;

        [SerializeField]
        private UiButton buttonElement;

        [SerializeField]
        private GameObject sliderElement;

        public void Configure(AdhocElementData data)
        {
            Clear();

            if (data.Type == "header") { 
                ConfigureAsHeader(Serialization.FromDataStructure<AdhocHeaderData>(data.Other));
            } else if (data.Type == "button") { 
                ConfigureAsButton(Serialization.FromDataStructure<AdhocButtonData>(data.Other));
            } else if (data.Type == "slider") {
                ConfigureAsSlider(Serialization.FromDataStructure<AdhocSliderData>(data.Other));
            }
        }

        private void Clear()
        {
            headerElement.gameObject.SetActive(false);
            buttonElement.gameObject.SetActive(false);
            sliderElement.gameObject.SetActive(false);
        }

        public void ConfigureAsHeader(AdhocHeaderData data)
        {
            headerElement.gameObject.SetActive(true);
            headerElement.text = data.Label;
        }

        public void ConfigureAsButton(AdhocButtonData data)
        {
            buttonElement.gameObject.SetActive(true);
            buttonElement.Text = data.Label;
        }

        public void ConfigureAsSlider(AdhocSliderData data)
        {
            sliderElement.gameObject.SetActive(true);
        }
    }
}