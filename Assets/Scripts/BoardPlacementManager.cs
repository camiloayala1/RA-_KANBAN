using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class BoardPlacementManager : MonoBehaviour
{
    [Header("Prefabs y Referencias")]
    public GameObject boardPrefab;
    public GameObject placementReticle;

    [Header("UI de Instrucciones")]
    public GameObject panelInstrucciones;
    public TMP_Text textoInstrucciones;

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

    void Start()
    {
        ActualizarMensaje("Apunta a una pared o superficie...");
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

            ARPlane plane = planeManager.GetPlane(hit.trackableId);

            placementReticle.SetActive(true);
            placementReticle.transform.position = hitPose.position;
            placementReticle.transform.rotation = hitPose.rotation;

            if (plane != null && plane.alignment == PlaneAlignment.Vertical)
            {
                ActualizarMensaje("Pared detectada. Toca la pantalla para fijar el tablero.");
            }
            else
            {
                ActualizarMensaje("Superficie detectada. Toca la pantalla para fijar.");
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    return;

                Quaternion targetRotation;

                if (plane != null && plane.alignment == PlaneAlignment.Vertical)
                {
                    // Vector que sale de la pared
                    Vector3 normalPared = plane.normal;

                    // Asegurar que apunte hacia el usuario y no hacia el interior de la pared
                    Vector3 haciaCamara = Camera.main.transform.position - hitPose.position;
                    if (Vector3.Dot(normalPared, haciaCamara) < 0)
                    {
                        normalPared = -normalPared;
                    }

                    // Alinear para que la cara frontal del Canvas mire hacia el usuario y mantenga la verticalidad
                    targetRotation = Quaternion.LookRotation(-normalPared, Vector3.up);
                }
                else
                {
                    // En suelo o mesa: orientar de frente a la cámara en el plano horizontal
                    Vector3 lookDirection = Camera.main.transform.position - hitPose.position;
                    lookDirection.y = 0;
                    targetRotation = Quaternion.LookRotation(-lookDirection, Vector3.up);
                }

                // Anclaje AR
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

                // Registrar en el gestor de notas
                if (KanbanBoardManager.Instance != null)
                {
                    KanbanBoardManager.Instance.RegistrarTablero(spawnedBoard.transform);
                }

                boardPlaced = true;
                placementReticle.SetActive(false);

                // Ocultar mallas de planos
                foreach (var p in planeManager.trackables)
                {
                    p.gameObject.SetActive(false);
                }

                // Ocultar mensaje de ayuda
                if (panelInstrucciones != null)
                {
                    panelInstrucciones.SetActive(false);
                }
            }
        }
        else
        {
            placementReticle.SetActive(false);
            ActualizarMensaje("Mueve el celular apuntando a una pared...");
        }
    }

    private void ActualizarMensaje(string mensaje)
    {
        if (boardPlaced) return;

        if (panelInstrucciones != null && !panelInstrucciones.activeSelf)
        {
            panelInstrucciones.SetActive(true);
        }

        if (textoInstrucciones != null)
        {
            if (!textoInstrucciones.gameObject.activeSelf)
                textoInstrucciones.gameObject.SetActive(true);

            textoInstrucciones.text = mensaje;
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

        foreach (var p in planeManager.trackables)
        {
            p.gameObject.SetActive(true);
        }

        placementReticle.SetActive(false);

        if (panelInstrucciones != null)
        {
            panelInstrucciones.SetActive(true);
        }

        ActualizarMensaje("Apunta a una pared o superficie...");
    }
}