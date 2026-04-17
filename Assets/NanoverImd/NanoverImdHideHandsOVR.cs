using UnityEngine;

namespace NanoverImd
{
    public class NanoverImdHideHandsOVR : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NanoverImdApplication application;

        [SerializeField]
        private NanoverImdSimulation nanover;

        [SerializeField]
        private OVRControllerHelper leftController;
        [SerializeField]
        private OVRControllerHelper rightController;
#pragma warning restore 0649

        public bool HideHands;

        private void Update()
        {
            UpdateSuggestedParameters();

            leftController.gameObject.SetActive(!HideHands);
            rightController.gameObject.SetActive(!HideHands);
        }

        private void UpdateSuggestedParameters()
        {
            const string handsKey = "suggested.hide.hands.self";

            if (nanover.Multiplayer.GetSharedState(handsKey) is bool hideHands)
            {
                HideHands = hideHands;
            }
        }
    }
}