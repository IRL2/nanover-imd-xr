using UnityEngine;
using Nanover.Visualisation;
using Nanover.Frame;
using Nanover.Frame.Event;
using NanoverImd;
using System;

public class DnemdUIDataHandler : MonoBehaviour
{
    public SynchronisedFrameSource frameSource;

    [SerializeField]
    NanoverImdSimulation simulation;

    public event Action<int, int, float, float, string> OnPlaybackStepChanged;
    public event Action<float[]> OnResidueColourGradientChanged;

    int totalSteps;
    int currentStep;
    float currentSimulationTime = float.NaN;
    float totalSimulationTime = float.NaN;
    string simulationTimeUnit;
    float[] residueColourGradient;

    public int TotalSteps => totalSteps;
    public float CurrentSimulationTime => currentSimulationTime;
    public float TotalSimulationTime => totalSimulationTime;
    public string SimulationTimeUnit => simulationTimeUnit;

    void OnEnable()
    {
        if (frameSource == null)
            frameSource = FindAnyObjectByType<SynchronisedFrameSource>();

        if (simulation == null)
            simulation = FindAnyObjectByType<NanoverImdSimulation>();

        if (frameSource != null)
            frameSource.FrameChanged += OnFrameChanged;
        else
            Debug.LogWarning($"{nameof(DnemdUIDataHandler)} could not find a {nameof(SynchronisedFrameSource)}.", this);
    }

    void OnDisable()
    {
        if (frameSource != null)
            frameSource.FrameChanged -= OnFrameChanged;
    }

    void OnFrameChanged(IFrame frame, FrameChanges changes)
    {
        // ---- playback ----
        bool playbackChanged = false;

        if (frame.Data.TryGetValue("frame.total", out var totalObj))
        {
            int t = Convert.ToInt32(totalObj);
            if (t != totalSteps)
            {
                totalSteps = t;
                playbackChanged = true;
            }
        }

        if (frame.Data.TryGetValue("frame.progress", out var progressObj))
        {
            int p = Convert.ToInt32(progressObj);
            if (p != currentStep)
            {
                currentStep = p;
                playbackChanged = true;
            }
        }

        if (frame.Data.TryGetValue("frame.time", out var timeObj))
        {
            float time = Convert.ToSingle(timeObj);
            if (!Mathf.Approximately(time, currentSimulationTime))
            {
                currentSimulationTime = time;
                playbackChanged = true;
            }
        }

        if (frame.Data.TryGetValue("frame.total_time", out var totalTimeObj))
        {
            float totalTime = Convert.ToSingle(totalTimeObj);
            if (!Mathf.Approximately(totalTime, totalSimulationTime))
            {
                totalSimulationTime = totalTime;
                playbackChanged = true;
            }
        }

        if (frame.Data.TryGetValue("frame.time_unit", out var timeUnitObj))
        {
            string timeUnit = Convert.ToString(timeUnitObj);
            if (timeUnit != simulationTimeUnit)
            {
                simulationTimeUnit = timeUnit;
                playbackChanged = true;
            }
        }

        if (playbackChanged)
        {
            OnPlaybackStepChanged?.Invoke(
                currentStep,
                totalSteps,
                currentSimulationTime,
                totalSimulationTime,
                simulationTimeUnit
            );
        }

        // ---- residue colour gradient ----
        if (frame.Data.TryGetValue("residue.colour_gradient", out var colourObj) &&
            colourObj is float[] gradientArray)
        {
            residueColourGradient = gradientArray;
            OnResidueColourGradientChanged?.Invoke(residueColourGradient);
        }
    }

    public void RequestPlaybackStep(int frameIndex)
    {
        if (simulation == null)
        {
            Debug.LogWarning($"{nameof(DnemdUIDataHandler)} could not find a {nameof(NanoverImdSimulation)}.", this);
            return;
        }

        if (totalSteps <= 0)
            return;

        simulation.SeekTrajectory(Mathf.Clamp(frameIndex, 0, totalSteps - 1));
    }
}
