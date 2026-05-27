using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace NanoverImd.UI
{
    public class ControllerSnackBar : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text text;

        [SerializeField]
        private SpriteRenderer background;

        private Color backgroundColorInitial;
        private Color backgroundColorTransparent;
        private Color textColorInitial;
        private Color textColorTransparent;

        private float strength = 0;

        [SerializeField]
        private float decaySpeed = 1;

        private void Awake()
        {
            Assert.IsNotNull(text);
            Assert.IsNotNull(background);
            backgroundColorInitial = background.color;
            backgroundColorTransparent = new Color(backgroundColorInitial.r, backgroundColorInitial.g, backgroundColorInitial.b, 0);
            textColorInitial = text.color;
            textColorTransparent = new Color(textColorInitial.r, textColorInitial.g, textColorInitial.b, 0);
        }

        private void Update()
        {
            if (strength > 0)
            {
                strength -= Time.deltaTime * decaySpeed;

                if (strength < 0.3f)
                {
                    text.color = Color.Lerp(textColorTransparent, textColorInitial, strength*3);
                    background.color = Color.Lerp(backgroundColorTransparent, backgroundColorInitial, strength*3);
                }

                transform.rotation = Quaternion.LookRotation(- (Camera.main.transform.position - transform.position), Vector3.up);
            }
            else
            {
                text.enabled = false;
                background.enabled = false;
            }
        
        }

        public void PushNotification(string content)
        {
            text.text = CleanIncommingText(content);
            strength = 1;
            text.enabled = true;
            background.enabled = true;
            text.color = textColorInitial;
            background.color = backgroundColorInitial;
        }

        public string CleanIncommingText(string content)
        {
            if (content.StartsWith("[") || content.StartsWith("<") || content.StartsWith("("))
            {
                content.Substring(1, content.Length - 2);
            }
            if (content.EndsWith("]") || content.EndsWith(">") || content.EndsWith(")"))
            {
                content.Substring(0, content.Length - 1);
            }
            return content;
        }
    }
}
