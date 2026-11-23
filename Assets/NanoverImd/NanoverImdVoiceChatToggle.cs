using System;
using UnityEngine;
using UnityEngine.UI;

namespace NanoverImd.UI
{
    public class VoiceChatToggle : MonoBehaviour
    {
        [SerializeField]
        private GameObject offIcon;

        [SerializeField]
        private NanoverImdApplication application;

        public void ToggleVoiceChat()
        {
            application.SetVoiceChat(!application.VoiceChat);
        }

        private void Update()
        {
            offIcon.SetActive(!application.VoiceChat);
        }
    }
}
