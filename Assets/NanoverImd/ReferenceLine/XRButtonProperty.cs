using UnityEngine;
using Nanover.Frontend.XR;
using UnityEngine.XR;

[System.Serializable]
public class XRButtonProperty
{
    public enum ControllerHand
    {
        Left,
        Right
    }

    public enum VRButton
    {
        None,
        TriggerButton,
        GripButton,
        PrimaryButton,
        PrimaryTouch,
        SecondaryButton,
        SecondaryTouch,
        MenuButton
    }

    [Tooltip("Which controller to use")]
    public ControllerHand hand = ControllerHand.Left;

    [Tooltip("Which button on the controller to use")]
    public VRButton button = VRButton.None;

    public InputFeatureUsage<bool> GetFeatureUsage()
    {
        switch (button)
        {
            case VRButton.TriggerButton: return CommonUsages.triggerButton;
            case VRButton.GripButton: return CommonUsages.gripButton;
            case VRButton.PrimaryButton: return CommonUsages.primaryButton;
            case VRButton.PrimaryTouch: return CommonUsages.primaryTouch;
            case VRButton.SecondaryButton: return CommonUsages.secondaryButton;
            case VRButton.SecondaryTouch: return CommonUsages.secondaryTouch;
            case VRButton.MenuButton: return CommonUsages.menuButton;
            case VRButton.None:
            default: return new InputFeatureUsage<bool>();
        }
    }

    public InputDeviceCharacteristics GetCharacteristics()
    {
        return hand == ControllerHand.Left
            ? InputDeviceCharacteristics.Left
            : InputDeviceCharacteristics.Right;
    }

    /// <summary>
    /// Creates the Nanover IButton wrapper based on selected settings.
    /// Returns null if 'None' is selected.
    /// </summary>
    public Nanover.Frontend.Input.IButton CreateButton()
    {
        if (button == VRButton.None)
        {
            return null;
        }

        return GetCharacteristics().WrapUsageAsButton(GetFeatureUsage());
    }
}