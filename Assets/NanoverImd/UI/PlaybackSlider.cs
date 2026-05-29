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
        slider = GetComponent<Slider>();
    }

    void OnEnable()
    {
        dataHandler.OnPlaybackStepChanged += UpdateUI;
    }

    void OnDisable()
    {
        dataHandler.OnPlaybackStepChanged -= UpdateUI;
    }

    void UpdateUI(int current, int total)
    {
        slider.value = total > 0 ? (float)current / total : 0f;
        if (stepText != null)
            stepText.text = $"{current} / {total}";
    }
}
