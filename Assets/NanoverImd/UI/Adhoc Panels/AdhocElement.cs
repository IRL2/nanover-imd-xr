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
        
        public void Configure(Dictionary<string, object> data)
        {
            Clear();

            if (data.GetValueOrDefault("type") is not string type)
                return;

            if (type == "header")
                ConfigureAsHeader(data.GetValueOrDefault("label") as string);
            else if (type == "button")
                ConfigureAsButton(data.GetValueOrDefault("label") as string);
            else if (type == "slider")
                ConfigureAsSlider(data.GetValueOrDefault("label") as string);
        }

        private void Clear()
        {
            headerElement.gameObject.SetActive(false);
            buttonElement.gameObject.SetActive(false);
            sliderElement.gameObject.SetActive(false);
        }

        public void ConfigureAsHeader(string label)
        {
            headerElement.gameObject.SetActive(true);
            headerElement.text = label;
        }

        public void ConfigureAsButton(string label)
        {
            buttonElement.gameObject.SetActive(true);
            buttonElement.Text = label;
        }

        public void ConfigureAsSlider(string label)
        {
            sliderElement.gameObject.SetActive(true);
        }
    }
}