using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.U2D;

namespace NanoverImd.UI
{
    public class PassthroughIconRotation : MonoBehaviour
    {

        [SerializeField]
        private UnityEngine.UI.Image buttonIcon;

        [SerializeField]
        private NanoverImdApplication nanoverApp;


        [SerializeField]
        [Tooltip("As many sprites as stops in the passthrough cycling steps")]
        private Sprite[] sprites;

        private void Start()
        {
            buttonIcon = this.gameObject.GetComponent<UnityEngine.UI.Image>();
            if (buttonIcon == null)
            {
                Debug.LogError("PassthroughIconRotation: No Image component found on the GameObject.");
                return;
            }
            UpdatePassthroughIcon();
        }

        public void UpdatePassthroughIcon()
        {
            int spriteIndex = nanoverApp.GetPassthroughIndexStep();
            buttonIcon.sprite = sprites[spriteIndex % sprites.Length];
        }


    }
}
