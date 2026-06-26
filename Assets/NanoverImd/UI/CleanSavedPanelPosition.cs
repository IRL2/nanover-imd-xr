using System;
using Nanover.Frontend.UI;
using UnityEngine;

public class CleanSavedPanelPosition : MonoBehaviour
{
    void Awake()
    {
        PlayerPrefs.DeleteKey("UIPanel.position");
        PlayerPrefs.DeleteKey("UIPanel.rotation");
    }
}
