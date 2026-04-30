using UnityEngine;

public class ProjectVersionDisplay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private TMPro.TextMeshProUGUI versionText;
    private void Start()
    {
        versionText.text = $"NanoVer-imd-XR version {Application.version}";
    }
}
