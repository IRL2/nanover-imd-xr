using UnityEngine;
using Nanover.Visualisation;
using Nanover.Frame;
using Nanover.Frame.Event;
using System;

public class DnemdUIDataHandler : MonoBehaviour
{
    public SynchronisedFrameSource frameSource;


    public event Action<int, int> OnPlaybackStepChanged;
    public event Action<float[]> OnResidueColourGradientChanged;

    int totalSteps;
    int currentStep;
    float[] residueColourGradient;

    void OnEnable()
    {
        if (frameSource == null)
            frameSource = FindAnyObjectByType<SynchronisedFrameSource>();

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

        if (playbackChanged)
        {
            OnPlaybackStepChanged?.Invoke(currentStep, totalSteps);
        }

        // ---- residue colour gradient ----
        if (frame.Data.TryGetValue("residue.colour_gradient", out var colourObj) &&
            colourObj is float[] gradientArray)
        {
            residueColourGradient = gradientArray;
            OnResidueColourGradientChanged?.Invoke(residueColourGradient);
        }
    }
}
