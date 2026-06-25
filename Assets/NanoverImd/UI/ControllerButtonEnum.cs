using UnityEngine.XR;

namespace NanoverImd.UI
{
    public enum ControllerButton
    {
        None,
        Trigger,
        Grip,
        PrimaryButton,
        SecondaryButton,
        MenuButton,
        Primary2DAxisClick,
        Secondary2DAxisClick
    }

    public static class ControllerButtonExtensions
    {
        public static bool TryToUsage(this ControllerButton button, out InputFeatureUsage<bool> usage)
        {
            switch (button)
            {
                case ControllerButton.Trigger:
                    usage = CommonUsages.triggerButton;
                    return true;
                case ControllerButton.Grip:
                    usage = CommonUsages.gripButton;
                    return true;
                case ControllerButton.PrimaryButton:
                    usage = CommonUsages.primaryButton;
                    return true;
                case ControllerButton.SecondaryButton:
                    usage = CommonUsages.secondaryButton;
                    return true;
                case ControllerButton.MenuButton:
                    usage = CommonUsages.menuButton;
                    return true;
                case ControllerButton.Primary2DAxisClick:
                    usage = CommonUsages.primary2DAxisClick;
                    return true;
                case ControllerButton.Secondary2DAxisClick:
                    usage = CommonUsages.secondary2DAxisClick;
                    return true;
                default:
                    usage = default;
                    return false;
            }
        }
    }
}