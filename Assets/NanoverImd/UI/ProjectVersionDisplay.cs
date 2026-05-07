using UnityEngine;

public class ProjectVersionDisplay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private string prefix;

    private void Start()
    {
        var TMPLabel = GetComponent<TMPro.TextMeshProUGUI>();
        TMPLabel.text = $"{prefix} {Application.version}";
    }
}