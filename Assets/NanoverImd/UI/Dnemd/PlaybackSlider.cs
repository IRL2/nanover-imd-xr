using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PlaybackSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] DnemdUIDataHandler dataHandler;
    [SerializeField] TMP_Text stepText;
    [SerializeField] float seekInterval = 0.05f;

    Slider slider;
    bool isInteracting;
    int lastRequestedFrame = -1;
    float nextSeekTime;

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

        if (slider != null)
            slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnDisable()
    {
        if (dataHandler != null)
            dataHandler.OnPlaybackStepChanged -= UpdateUI;

        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);

        isInteracting = false;
        lastRequestedFrame = -1;
    }

    void UpdateUI(int current, int total)
    {
        if (slider != null && !isInteracting)
        {
            var normalizedStep = total > 1 ? (float)(current - 1) / (total - 1) : 0f;
            slider.SetValueWithoutNotify(normalizedStep);
        }

        if (stepText != null && !isInteracting)
            stepText.text = $"{current} / {total}";
    }

    void OnSliderValueChanged(float value)
    {
        if (!isInteracting || dataHandler == null)
            return;

        var total = dataHandler.TotalSteps;
        if (total > 0)
        {
            var frameIndex = NormalizedValueToFrameIndex(value, total);
            if (stepText != null)
                stepText.text = $"{frameIndex + 1} / {total}";

            RequestSeek(frameIndex, false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isInteracting = true;
        lastRequestedFrame = -1;
        nextSeekTime = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isInteracting)
            return;

        isInteracting = false;

        if (slider == null || dataHandler == null || dataHandler.TotalSteps <= 0)
            return;

        RequestSeek(
            NormalizedValueToFrameIndex(slider.normalizedValue, dataHandler.TotalSteps),
            true
        );
    }

    void RequestSeek(int frameIndex, bool force)
    {
        if (dataHandler == null || (frameIndex == lastRequestedFrame && !force))
            return;

        if (!force && Time.unscaledTime < nextSeekTime)
            return;

        lastRequestedFrame = frameIndex;
        nextSeekTime = Time.unscaledTime + seekInterval;
        dataHandler.RequestPlaybackStep(frameIndex);
    }

    static int NormalizedValueToFrameIndex(float normalizedValue, int total)
    {
        return Mathf.RoundToInt(Mathf.Clamp01(normalizedValue) * Mathf.Max(0, total - 1));
    }
}
