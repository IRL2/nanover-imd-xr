using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Text = TMPro.TextMeshProUGUI;

namespace NanoverImd
{
    public sealed class NanoverImdConnectionHealthUpdater : MonoBehaviour
    {
        [SerializeField]
        private NanoverImdConnectionHealth health;
        [SerializeField]
        private NanoverImdVoiceChat voice;
        [SerializeField]
        private Text label;

        private void Update()
        {
            label.text = $"Frame: {health.Frame}/s, State: {health.State}/s, RTT: {health.RTT}ms\nVoice: {voice.Stage}";
        }
    }
}