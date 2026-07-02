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

    void UpdateUI(int current, int total, float currentTime, float totalTime, string timeUnit)
    {
        if (slider != null && !isInteracting)
        {
            var normalizedStep = total > 1 ? (float)(current - 1) / (total - 1) : 0f;
            slider.SetValueWithoutNotify(normalizedStep);
        }

        if (stepText != null && !isInteracting)
            stepText.text = FormatPlaybackText(current, total, currentTime, totalTime, timeUnit);
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
                stepText.text = FormatPlaybackText(
                    frameIndex + 1,
                    total,
                    InterpolateSimulationTime(frameIndex, total),
                    dataHandler.TotalSimulationTime,
                    dataHandler.SimulationTimeUnit
                );

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

    float InterpolateSimulationTime(int frameIndex, int total)
    {
        if (dataHandler == null ||
            float.IsNaN(dataHandler.TotalSimulationTime) ||
            total <= 1)
        {
            return float.NaN;
        }

        return dataHandler.TotalSimulationTime * frameIndex / (total - 1);
    }

    static string FormatPlaybackText(int current, int total, float currentTime, float totalTime, string timeUnit)
    {
        if (float.IsNaN(currentTime) || float.IsNaN(totalTime))
            return $"{current} / {total}";

        var unitSuffix = string.IsNullOrEmpty(timeUnit) ? string.Empty : $" {timeUnit}";
        return $"{FormatTime(currentTime)} / {FormatTime(totalTime)}{unitSuffix}";
    }

    static string FormatTime(float time)
    {
        return Mathf.Approximately(time, Mathf.Round(time))
            ? Mathf.RoundToInt(time).ToString()
            : time.ToString("0.###");
    }
}
