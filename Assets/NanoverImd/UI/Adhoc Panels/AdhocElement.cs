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
        private NanoverImdApplication app;
        
        [SerializeField]
        private Text headerElement;

        [SerializeField]
        private UiButton buttonElement;

        [SerializeField]
        private GameObject sliderElement;
        // hack
        [SerializeField]
        private UiButton sliderButtonLeft;
        [SerializeField]
        private UiButton sliderButtonRight;

        private AdhocElementData elementData;

        private void Awake()
        {
            buttonElement.OnClick += OnButtonClick;
            sliderButtonLeft.OnClick += OnSliderLeft;
            sliderButtonRight.OnClick += OnSliderRight;
        }

        private T SetData<T>(object data) where T : AdhocElementData
        {
            var converted = Serialization.FromDataStructure<T>(data);
            elementData = converted;
            return converted;
        }

        private T GetData<T>() where T : AdhocElementData
        {
            return elementData as T;
        }

        public void Configure(AdhocElementData data)
        {
            Clear();

            if (data.Type == "header") { 
                ConfigureAsHeader(SetData<AdhocHeaderData>(data.Other));
            } else if (data.Type == "button") { 
                ConfigureAsButton(SetData<AdhocButtonData>(data.Other));
            } else if (data.Type == "slider") {
                ConfigureAsSlider(SetData<AdhocSliderData>(data.Other));
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

        public void OnButtonClick()
        {
            var data = GetData<AdhocButtonData>();
            var arguments = new Dictionary<string, object>();

            foreach (var pair in data.Arguments)
            {
                if (pair.Value.Literal is { } value)
                    arguments[pair.Key] = value;
                else if (pair.Value.Variable is { } variable)
                    arguments[pair.Key] = app.Simulation.Multiplayer.GetSharedState(variable);
            }

            app.Simulation.RunCommand(data.Command, arguments);
        }

        private float GetSliderValue()
        {
            var data = GetData<AdhocSliderData>();
            var value = Mathf.Lerp(data.Range[0], data.Range[1], .5f);

            if (app.Simulation.Multiplayer.GetSharedState(data.Variable) is { } remote)
            {
                switch (remote)
                {
                    case float number:
                        value = number;
                        break;
                    case int number:
                        value = number;
                        break;
                    case double number:
                        value = (float) number;
                        break;
                }
            }

            if (data.IsInteger)
                return Mathf.Round(value);

            return value;
        }

        private float GetSliderStep()
        {
            var data = GetData<AdhocSliderData>();

            if (data.StepSize is { } step)
                return step;

            if (data.IsInteger)
                return 1;

            return (data.Range[1] - data.Range[0]) * .1f;
        }

        public void ConfigureAsSlider(AdhocSliderData data)
        {
            var value = GetSliderValue();
            var valueText = data.IsInteger ? $"{(int) value}" : $"{value:.2f}";

            sliderElement.gameObject.SetActive(true);
            sliderElement.GetComponentInChildren<Text>().text = $"{data.Label}: {valueText} ({data.Range[0]} - {data.Range[1]})";
        }
        
        private void OnSliderLeft()
        {
            var data = GetData<AdhocSliderData>();
            var value = Mathf.Max(data.Range[0], GetSliderValue() - GetSliderStep());
            app.Simulation.Multiplayer.SetSharedState(data.Variable, value);
        }

        private void OnSliderRight()
        {
            var data = GetData<AdhocSliderData>();
            var value = Mathf.Min(data.Range[1], GetSliderValue() + GetSliderStep());
            app.Simulation.Multiplayer.SetSharedState(data.Variable, value);
        }
    }
}