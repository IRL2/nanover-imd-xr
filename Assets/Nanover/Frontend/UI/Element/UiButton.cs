using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nanover.Frontend.UI
{
    /// <summary>
    /// Component to abstract different button prefabs to give a common interface for
    /// controlling the icon, label and callback.
    /// </summary>
    public class UiButton : MonoBehaviour
    {
        [Tooltip("The image component which will display the icon.")]
        [SerializeField]
        private Image icon;

        [Tooltip("The text component that will display the emoji.")]
        [SerializeField]
        private TMP_Text emoji;

        [Tooltip("The text component that will display the label.")]
        [SerializeField]
        private TMP_Text text;
        
        [Tooltip("The text component that will display the sublabel.")]
        [SerializeField]
        private TMP_Text subtext;

        [Tooltip("The button component that controls the callback.")]
        [SerializeField]
        private Button button;

        /// <summary>
        /// Callback when the button is clicked.
        /// </summary>
        public event Action OnClick
        {
            add { button?.onClick.AddListener(() => value()); }
            remove { button?.onClick.RemoveListener(() => value()); }
        }

        /// <summary>
        /// The sprite that will display as an icon for the button.
        /// </summary>
        public Sprite Image
        {
            set
            {
                if (value == null)
                {
                    icon.enabled = false;
                }
                else
                {
                    icon.enabled = true;
                    icon.sprite = value;
                }
            }
        }

        /// <summary>
        /// The sprite that will display as an icon for the button.
        /// </summary>
        public string Emoji
        {
            set
            {
                if (value == null)
                {
                    emoji.enabled = false;
                }
                else
                {
                    icon.enabled = false;
                    emoji.enabled = true;
                    emoji.text = value;
                }
            }
        }

        /// <summary>
        /// The label of the button.
        /// </summary>
        public string Text
        {
            set => text.text = value;
        }
        
        /// <summary>
        /// The sublabel of the button.
        /// </summary>
        public string Subtext
        {
            set
            {
                if(subtext != null)
                    subtext.text = value;
            }
        }
    }
}