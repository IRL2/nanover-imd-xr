using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public sealed class TMP_HyperlinkHandler : MonoBehaviour, ISerializationCallbackReceiver, IPointerClickHandler
{
    [SerializeField]
    private TMP_Text textComponent;

    public void OnBeforeSerialize()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TMP_Text>();
        }
    }

    public void OnAfterDeserialize() { }

    public void OnPointerClick(PointerEventData eventData)
    {
        //int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, eventData.position, null);

        //if (linkIndex == -1)
        //{
        //    return;
        //}

        var linkInfo = textComponent.textInfo.linkInfo[0];
        string link = linkInfo.GetLink();

        if (link.StartsWith("https://") || link.StartsWith("www."))
        {
            Debug.Log($"Opening hyperlink: {link}");
            Application.OpenURL(link);
        }
    }
}
