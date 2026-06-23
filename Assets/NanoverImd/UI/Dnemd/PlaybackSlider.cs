using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlaybackSlider : MonoBehaviour
{
    [SerializeField] DnemdUIDataHandler dataHandler;
    [SerializeField] TMP_Text stepText;

    Slider slider;

    void Awake()
    {
        if (dataHandler == null)
            dataHandler = FindAnyObjectByType<DnemdUIDataHandler>();

        slider = GetComponent<Slider>() ?? GetComponentInChildren<Slider>();
    }

    void OnEnable()
    {
        if (dataHandler != null)
            dataHandler.OnPlaybackStepChanged += UpdateUI;
        else
            Debug.LogWarning($"{nameof(PlaybackSlider)} could not find a {nameof(DnemdUIDataHandler)}.", this);
    }

    void OnDisable()
    {
        if (dataHandler != null)
            dataHandler.OnPlaybackStepChanged -= UpdateUI;
    }

    void UpdateUI(int current, int total)
    {
        if (slider != null)
            slider.value = total > 0 ? (float)current / total : 0f;

        if (stepText != null)
            stepText.text = $"{current} / {total}";
    }
}
