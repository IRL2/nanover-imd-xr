using System;
using System.Diagnostics;
using Nanover.Frontend.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class UIPanelDraggable : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField]
    private Transform panelRoot;


    private Vector3 panelPointerOffset;

    private float panelDistance = 1.5f;
    
    [SerializeField]
    [Tooltip("If true, the panel will use remember its latest location before close")]
    private bool restoreFromSaved = true;

    [SerializeField]
    [Tooltip("If true, use the float follower only when not using saved location")]
    private bool fallbackFollowing = false;

    [SerializeField]
    private FollowingUi followGazeUI;


    private XRRayInteractor activeRay;

    private Transform pointerTransform;

    private bool isDragging;

    public bool hasBeenSaved = false;

    private void Awake()
    {
        panelPointerOffset = panelRoot.position - transform.position;

        hasBeenSaved = RestorePanelLocation(restoreFromSaved);
        followGazeUI.enabled = fallbackFollowing && !hasBeenSaved;
    }

    private void Update()
    {
        if (isDragging && pointerTransform != null)
        {
            Vector3 targetPosition = (pointerTransform.position + panelPointerOffset) + (pointerTransform.forward * panelDistance);
            panelRoot.position = Vector3.Lerp(panelRoot.position, targetPosition, 0.2f);
            panelRoot.rotation = Quaternion.LookRotation(panelRoot.position - Camera.main.transform.position, Vector3.up);
        }

        followGazeUI.enabled = fallbackFollowing && !hasBeenSaved;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        SavePanelLocation();
        activeRay.transform.GetComponent<XRInteractorLineVisual>().enabled = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        activeRay = FindHoveringRay();

        if (activeRay == null)
        {
            UnityEngine.Debug.LogWarning("Could not determine which XR Ray Interactor clicked the handle.");
            return;
        }

        isDragging = true;

        pointerTransform = activeRay.rayOriginTransform;

        panelPointerOffset = panelRoot.position - activeRay.rayEndPoint;

        panelDistance = Vector3.Distance(transform.position, pointerTransform.position);

        activeRay.transform.GetComponent<XRInteractorLineVisual>().enabled = false;

        hasBeenSaved = true;
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
        UnityEngine.Debug.Log("Saving panel location");
        PlayerPrefs.SetString("UIPanel.position", $"{panelRoot.position.x}|{panelRoot.position.y}|{panelRoot.position.z}");
        PlayerPrefs.SetString("UIPanel.rotation", $"{panelRoot.rotation.x}|{panelRoot.rotation.y}|{panelRoot.rotation.z}|{panelRoot.rotation.w}");
    }

    private bool RestorePanelLocation(bool useSaved = false)
    {
        bool hasSaved = false;

        panelRoot.position = Camera.main.transform.position + Vector3.down * 0.2f + Camera.main.transform.forward * 0.8f;
        if (useSaved && PlayerPrefs.HasKey("UIPanel.position"))
        {
            string[] p = PlayerPrefs.GetString("UIPanel.position").Split('|');
            panelRoot.position = new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));
            hasSaved = true;
        }

        panelRoot.rotation = Quaternion.LookRotation(panelRoot.position - Camera.main.transform.position,Vector3.up);
        if (useSaved && PlayerPrefs.HasKey("UIPanel.rotation"))
        {
            string[] r = PlayerPrefs.GetString("UIPanel.rotation").Split('|');
            panelRoot.rotation = new Quaternion(float.Parse(r[0]), float.Parse(r[1]), float.Parse(r[2]), float.Parse(r[3]));
            hasSaved = true;
        }

        UnityEngine.Debug.Log("Restoring panel location from saved data: " + hasSaved);
        return hasSaved;
    }
}