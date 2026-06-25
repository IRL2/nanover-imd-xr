using UnityEngine;

public class CleanSavedPanelPosition : MonoBehaviour
{
    void Awake()
    {
        CleanSavedData();
    }


    private void CleanSavedData()
    {
        PlayerPrefs.DeleteKey("UIPanel.position");
        PlayerPrefs.DeleteKey("UIPanel.rotation");
    }


}
