using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class UIPanelDraggable : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField]
    private Transform panelRoot;

    private Vector3 panelPointerOffset;

    private float panelDistance = 1.5f;
    
    [SerializeField]
    [Tooltip("If true, the panel will remember its latest location before close")]
    private bool permaSaveLocation = false;

    [SerializeField]
    private XRRayInteractor activeRay;

    private Transform pointerTransform;

    private bool isDragging;

    private void Awake()
    {
        panelPointerOffset = panelRoot.position - transform.position;
        RestorePanelLocation(permaSaveLocation);
    }

    private void Update()
    {
        if (isDragging && pointerTransform != null)
        {
            Vector3 targetPosition = (pointerTransform.position + panelPointerOffset ) + (pointerTransform.forward * panelDistance);
            panelRoot.position = Vector3.Lerp(panelRoot.position, targetPosition, 0.2f);
            panelRoot.rotation = Quaternion.LookRotation(panelRoot.position - Camera.main.transform.position, Vector3.up);
        }
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        SavePanelLocation();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        activeRay = FindHoveringRay();

        if (activeRay == null)
        {
            Debug.LogWarning("Could not determine which XR Ray Interactor clicked the handle.");
            return;
        }

        isDragging = true;

        pointerTransform = activeRay.rayOriginTransform;

        panelPointerOffset = panelRoot.position - activeRay.rayEndPoint;

        panelDistance = Vector3.Distance(transform.position, pointerTransform.position);
    }


    private XRRayInteractor FindHoveringRay()
    {
        var rays = FindObjectsByType<XRRayInteractor>();

        foreach (var ray in rays)
        {
            if (ray.TryGetCurrentUIRaycastResult(out var hit) &&
                hit.gameObject == gameObject)
            {
                return ray;
            }
        }
        return null;
    }


    private void SavePanelLocation()
    {
        PlayerPrefs.SetString("UIPanel.position", $"{panelRoot.position.x}|{panelRoot.position.y}|{panelRoot.position.z}");
        PlayerPrefs.SetString("UIPanel.rotation", $"{panelRoot.rotation.x}|{panelRoot.rotation.y}|{panelRoot.rotation.z}|{panelRoot.rotation.w}");
    }

    private void RestorePanelLocation(bool useSaved = false)
    {
        panelRoot.position = Camera.main.transform.position + Vector3.down * 0.2f + Camera.main.transform.forward * 0.8f;
        if (useSaved && PlayerPrefs.HasKey("UIPanel.position"))
        {
            string[] p = PlayerPrefs.GetString("UIPanel.position").Split('|');
            panelRoot.position = new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));
        }

        //panelRoot.forward = -Camera.main.transform.forward;
        panelRoot.rotation = Quaternion.LookRotation(panelRoot.position - Camera.main.transform.position,Vector3.up);
        if (useSaved && PlayerPrefs.HasKey("UIPanel.rotation"))
        {
            string[] r = PlayerPrefs.GetString("UIPanel.rotation").Split('|');
            panelRoot.rotation = new Quaternion(float.Parse(r[0]), float.Parse(r[1]), float.Parse(r[2]), float.Parse(r[3]));
        }
    }

}