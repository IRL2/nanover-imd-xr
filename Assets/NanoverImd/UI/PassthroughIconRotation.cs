using System;
using NanoverImd;
using UnityEngine;

public class PassthroughIconRotation : MonoBehaviour
{

    [SerializeField]
    private UnityEngine.UI.Image buttonIcon;

    [SerializeField]
    private NanoverImdApplication nanoverApp;


    [SerializeField]
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

    // From a percentage 0.0 to 1.0, update the button icon using the loaded sprites
    public void UpdatePassthroughIcon()
    {
        float passthroughPercentage = nanoverApp.GetPassthroughValue();
        int spriteIndex = Mathf.Clamp(Mathf.FloorToInt(passthroughPercentage * sprites.Length), 0, sprites.Length - 1);
        buttonIcon.sprite = sprites[spriteIndex];
    }


}
