using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;


public class HoverHaptics : MonoBehaviour
{


    public float amplitude = 0.1f;
    public float duration = 0.01f;

    private Transform interactorTransform;

    private void Awake()
    {
        interactorTransform = this.gameObject.transform;
    }

    public void SendHapticImpuse()
    {
        var device = GetNearestControllerDevice();
        if (device.isValid)
            device.SendHapticImpulse(0u, amplitude, duration);
    }

    public void OnHoverEnter(UIHoverEventArgs args)
    {
        var device = GetNearestControllerDevice();

        if (!device.isValid)
            return;

        if (!args.uiObject.tag.Contains("HasHaptics"))
            return;

        device.SendHapticImpulse(0u, amplitude, duration);
    }

    InputDevice GetNearestControllerDevice()
    {
        var candidates = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller, candidates);

        if (candidates.Count == 0)
            return new InputDevice(); // invalid

        // choose device whose reported position is closest to interactor (best-effort)
        float bestDist = float.MaxValue;
        InputDevice best = new InputDevice();
        foreach (var d in candidates)
        {
            if (d.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
            {
                float dist = Vector3.Distance(interactorTransform.position, pos);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = d;
                }
            }
            else
            {
                // if position not available, prefer it only if we don't have a best yet
                if (!best.isValid) best = d;
            }
        }
        return best;
    }
}
