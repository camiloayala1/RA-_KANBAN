using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class BoardPlacementManager : MonoBehaviour
{
    [Header("Prefabs y Referencias")]
    public GameObject boardPrefab;
    public GameObject placementReticle;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private ARAnchorManager anchorManager;

    private GameObject spawnedBoard = null;
    private ARAnchor currentAnchor = null;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool boardPlaced = false;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        anchorManager = GetComponent<ARAnchorManager>();
    }

    void OnEnable()
    {
        if (planeManager != null)
            planeManager.planesChanged += OnPlanesChanged;
    }

    void OnDisable()
    {
        if (planeManager != null)
            planeManager.planesChanged -= OnPlanesChanged;
    }

    // Oculta automáticamente cualquier plano nuevo que ARCore detecte
    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (!boardPlaced) return;

        foreach (var plane in args.added)
            plane.gameObject.SetActive(false);
        foreach (var plane in args.updated)
            plane.gameObject.SetActive(false);
    }

    void Update()
    {
        if (boardPlaced) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            ARRaycastHit hit = hits[0];
            Pose hitPose = hit.pose;

            placementReticle.SetActive(true);
            placementReticle.transform.position = hitPose.position;
            placementReticle.transform.rotation = hitPose.rotation;

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    return;

                Vector3 lookDirection = Camera.main.transform.position - hitPose.position;
                lookDirection.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(-lookDirection);

                ARPlane plane = planeManager.GetPlane(hit.trackableId);
                if (anchorManager != null && plane != null)
                {
                    currentAnchor = anchorManager.AttachAnchor(plane, new Pose(hitPose.position, targetRotation));
                }

                if (currentAnchor != null)
                {
                    spawnedBoard = Instantiate(boardPrefab, currentAnchor.transform.position, currentAnchor.transform.rotation);
                    spawnedBoard.transform.SetParent(currentAnchor.transform, true);
                }
                else
                {
                    spawnedBoard = Instantiate(boardPrefab, hitPose.position, targetRotation);
                }

                if (KanbanBoardManager.Instance != null)
                {
                    KanbanBoardManager.Instance.RegistrarTablero(spawnedBoard.transform);
                }

                boardPlaced = true;
                placementReticle.SetActive(false);

                // Ocultar todos los planos detectados hasta el momento
                foreach (var p in planeManager.trackables)
                {
                    p.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            placementReticle.SetActive(false);
        }
    }

    public void ResetPlacement()
    {
        boardPlaced = false;

        if (spawnedBoard != null)
        {
            Destroy(spawnedBoard);
            spawnedBoard = null;
        }

        if (currentAnchor != null)
        {
            Destroy(currentAnchor.gameObject);
            currentAnchor = null;
        }

        // Reactivar visibilidad de planos para volver a escanear
        foreach (var p in planeManager.trackables)
        {
            p.gameObject.SetActive(true);
        }

        placementReticle.SetActive(false);
    }
}