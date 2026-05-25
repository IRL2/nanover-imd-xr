using System;
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
        private Color backgroundColor;
        private Color backgroundColorTransparent;

        private float strength = 0;

        [SerializeField]
        private float decaySpeed = 1;

        private void Awake()
        {
            Assert.IsNotNull(text);
            backgroundColor = background.color;
            backgroundColorTransparent = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0);
        }

        private void Update()
        {
            if (strength > 0)
            {
                strength -= Time.deltaTime * decaySpeed;

                text.color = new Color(1, 1, 1, strength * strength * (3 - 2 * strength));
                background.color = Color.Lerp(backgroundColorTransparent, backgroundColor, strength);

                var forwards = -(Camera.main.transform.position - transform.position);
                var horizontal = Vector3.Cross(forwards, Vector3.up);
                var up = Vector3.Cross(horizontal, forwards);

                transform.rotation =
                    Quaternion.LookRotation(forwards, up);
            }
            else
            {
                text.enabled = false;
                background.enabled = false;
            }
        
        }

        public void PushNotification(string text)
        {
            this.text.text = text;
            strength = 1;
            this.text.enabled = true;
            this.background.enabled = true;
            this.background.color = backgroundColor;
        }
    }
}
