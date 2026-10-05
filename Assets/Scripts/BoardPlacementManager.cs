using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class BoardPlacementManager : MonoBehaviour
{
    [Header("Prefab y referencias")]
    [SerializeField] private GameObject boardPrefab;
    [SerializeField] private GameObject placementReticle;

    [Header("UI de instrucciones")]
    [SerializeField] private GameObject panelInstrucciones;
    [SerializeField] private TMP_Text textoInstrucciones;

    [Header("UI de acciones (Botones del tablero)")]
    [Tooltip("Panel que agrupa Btn_NuevaNota, Papelera y Btn_Reubicar")]
    [SerializeField] private GameObject panelBotonesAccion;

    [Header("Superficies permitidas")]
    [SerializeField] private bool permitirParedes = true;
    [SerializeField] private bool permitirSuelo = true;

    [Header("Configuración de colocación")]
    [SerializeField] private bool usarSoloPlanosEstables = true;

    // ---------------------------------------------------------
    // COMPONENTES AR FOUNDATION
    // ---------------------------------------------------------

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private ARAnchorManager anchorManager;
    private ARPointCloudManager pointCloudManager;

    // Lista reutilizable para evitar crear memoria cada frame.
    private static readonly List<ARRaycastHit> hits =
        new List<ARRaycastHit>(8);

    // ---------------------------------------------------------
    // OBJETOS
    // ---------------------------------------------------------

    private GameObject spawnedBoard;
    private ARAnchor currentAnchor;
    private ARPlane currentPlane;

    // ---------------------------------------------------------
    // ESTADO
    // ---------------------------------------------------------

    private bool boardPlaced = false;
    private bool isDetecting = true;

    // Pose actual del tablero.
    private Pose currentPose;

    // Último mensaje mostrado.
    private string ultimoMensaje = "";

    // Cámara cacheada.
    private Camera arCamera;

    // ---------------------------------------------------------
    // AWAKE
    // ---------------------------------------------------------

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        anchorManager = GetComponent<ARAnchorManager>();
        pointCloudManager = GetComponent<ARPointCloudManager>();

        arCamera = Camera.main;

        if (raycastManager == null)
        {
            Debug.LogError(
                "BoardPlacementManager: " +
                "No se encontró ARRaycastManager."
            );
        }

        if (planeManager == null)
        {
            Debug.LogError(
                "BoardPlacementManager: " +
                "No se encontró ARPlaneManager."
            );
        }

        if (anchorManager == null)
        {
            Debug.LogWarning(
                "BoardPlacementManager: " +
                "No se encontró ARAnchorManager. " +
                "El tablero podrá colocarse sin Anchor."
            );
        }

        if (boardPrefab == null)
        {
            Debug.LogError(
                "BoardPlacementManager: " +
                "boardPrefab no está asignado."
            );
        }
    }

    // ---------------------------------------------------------
    // START
    // ---------------------------------------------------------

    private void Start()
    {
        if (placementReticle != null)
            placementReticle.SetActive(false);

        // Los botones de nueva nota, papelera y reubicar inician apagados
        if (panelBotonesAccion != null)
            panelBotonesAccion.SetActive(false);

        MostrarMensaje(
            "Mueve el celular para detectar una pared o superficie."
        );
    }

    // ---------------------------------------------------------
    // ENABLE
    // ---------------------------------------------------------

    private void OnEnable()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }
    }

    // ---------------------------------------------------------
    // DISABLE
    // ---------------------------------------------------------

    private void OnDisable()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }
    }

    // ---------------------------------------------------------
    // CAMBIO DE PLANOS
    // ---------------------------------------------------------

    private void OnPlanesChanged(
        ARPlanesChangedEventArgs args)
    {
        if (!boardPlaced)
            return;

        OcultarPlanos(args.added);
        OcultarPlanos(args.updated);
    }

    private void OcultarPlanos(List<ARPlane> planes)
    {
        if (planes == null)
            return;

        for (int i = 0; i < planes.Count; i++)
        {
            if (planes[i] != null)
            {
                planes[i].gameObject.SetActive(false);
            }
        }
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    private void Update()
    {
        if (boardPlaced)
            return;

        if (!isDetecting)
            return;

        DetectarSuperficie();
    }

    // ---------------------------------------------------------
    // DETECTAR SUPERFICIE
    // ---------------------------------------------------------

    private void DetectarSuperficie()
    {
        if (raycastManager == null)
            return;

        if (arCamera == null)
        {
            arCamera = Camera.main;

            if (arCamera == null)
                return;
        }

        // Centro de la pantalla.
        Vector2 centroPantalla = new Vector2(
            Screen.width * 0.5f,
            Screen.height * 0.5f
        );

        // Raycast contra planos.
        bool encontrado = raycastManager.Raycast(
            centroPantalla,
            hits,
            TrackableType.PlaneWithinPolygon
        );

        if (!encontrado || hits.Count == 0)
        {
            currentPlane = null;

            OcultarReticula();

            MostrarMensaje(
                "Mueve el celular apuntando a una superficie..."
            );

            return;
        }

        // -----------------------------------------------------
        // BUSCAR EL MEJOR PLANO
        // -----------------------------------------------------

        ARRaycastHit mejorHit = ObtenerMejorHit();

        if (mejorHit.trackableId ==
            TrackableId.invalidId)
        {
            currentPlane = null;
            OcultarReticula();
            return;
        }

        ARPlane plane =
            planeManager.GetPlane(
                mejorHit.trackableId
            );

        if (plane == null)
        {
            currentPlane = null;
            OcultarReticula();
            return;
        }

        // -----------------------------------------------------
        // COMPROBAR TIPO DE SUPERFICIE
        // -----------------------------------------------------

        if (!EsSuperficieValida(plane))
        {
            currentPlane = null;

            OcultarReticula();

            MostrarMensaje(
                "Apunta a una pared o superficie válida."
            );

            return;
        }

        // -----------------------------------------------------
        // COMPROBAR TRACKING
        // -----------------------------------------------------

        if (usarSoloPlanosEstables)
        {
            if (plane.trackingState !=
                TrackingState.Tracking)
            {
                currentPlane = null;

                MostrarMensaje(
                    "Esperando que la superficie se estabilice..."
                );

                return;
            }
        }

        // -----------------------------------------------------
        // GUARDAR PLANO
        // -----------------------------------------------------

        currentPlane = plane;

        // -----------------------------------------------------
        // CALCULAR POSE
        // -----------------------------------------------------

        currentPose = CalcularPose(
            mejorHit,
            plane
        );

        // -----------------------------------------------------
        // ACTUALIZAR RETÍCULA
        // -----------------------------------------------------

        MostrarReticula(currentPose);

        // -----------------------------------------------------
        // MENSAJE
        // -----------------------------------------------------

        if (plane.alignment ==
            PlaneAlignment.Vertical)
        {
            MostrarMensaje(
                "Pared detectada. Toca para colocar el tablero."
            );
        }
        else
        {
            MostrarMensaje(
                "Superficie detectada. Toca para colocar el tablero."
            );
        }

        // -----------------------------------------------------
        // COMPROBAR TOQUE
        // -----------------------------------------------------

        ProcesarToque();
    }

    // ---------------------------------------------------------
    // OBTENER MEJOR HIT
    // ---------------------------------------------------------

    private ARRaycastHit ObtenerMejorHit()
    {
        if (hits.Count == 0)
            return default;

        if (hits.Count == 1)
            return hits[0];

        ARRaycastHit mejorHit = hits[0];

        float menorDistancia = float.MaxValue;

        Vector3 posicionCamara =
            arCamera != null
                ? arCamera.transform.position
                : Vector3.zero;

        for (int i = 0; i < hits.Count; i++)
        {
            float distancia =
                (hits[i].pose.position -
                 posicionCamara).sqrMagnitude;

            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                mejorHit = hits[i];
            }
        }

        return mejorHit;
    }

    // ---------------------------------------------------------
    // VALIDAR SUPERFICIE
    // ---------------------------------------------------------

    private bool EsSuperficieValida(ARPlane plane)
    {
        if (plane == null)
            return false;

        // Pared.
        if (plane.alignment ==
            PlaneAlignment.Vertical)
        {
            return permitirParedes;
        }

        // Suelo o mesa.
        if (plane.alignment ==
            PlaneAlignment.HorizontalUp)
        {
            return permitirSuelo;
        }

        return false;
    }

    // ---------------------------------------------------------
    // CALCULAR POSE
    // ---------------------------------------------------------

    private Pose CalcularPose(
        ARRaycastHit hit,
        ARPlane plane)
    {
        Vector3 posicion = hit.pose.position;

        if (arCamera == null)
            arCamera = Camera.main;

        // -----------------------------------------------------
        // PARED
        // -----------------------------------------------------

        if (plane.alignment ==
            PlaneAlignment.Vertical)
        {
            Vector3 normalPared =
                plane.normal;

            Vector3 haciaCamara =
                arCamera.transform.position -
                posicion;

            if (Vector3.Dot(
                    normalPared,
                    haciaCamara) < 0f)
            {
                normalPared = -normalPared;
            }

            Quaternion rotacion =
                Quaternion.LookRotation(
                    -normalPared,
                    Vector3.up
                );

            return new Pose(
                posicion,
                rotacion
            );
        }

        // -----------------------------------------------------
        // SUELO / MESA
        // -----------------------------------------------------

        Vector3 direccion =
            arCamera.transform.position -
            posicion;

        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.0001f)
        {
            direccion = Vector3.forward;
        }

        direccion.Normalize();

        Quaternion rotacionHorizontal =
            Quaternion.LookRotation(
                -direccion,
                Vector3.up
            );

        return new Pose(
            posicion,
            rotacionHorizontal
        );
    }

    // ---------------------------------------------------------
    // MOSTRAR RETÍCULA
    // ---------------------------------------------------------

    private void MostrarReticula(Pose pose)
    {
        if (placementReticle == null)
            return;

        if (!placementReticle.activeSelf)
        {
            placementReticle.SetActive(true);
        }

        placementReticle.transform.SetPositionAndRotation(
            pose.position,
            pose.rotation
        );
    }

    // ---------------------------------------------------------
    // OCULTAR RETÍCULA
    // ---------------------------------------------------------

    private void OcultarReticula()
    {
        if (placementReticle == null)
            return;

        if (placementReticle.activeSelf)
        {
            placementReticle.SetActive(false);
        }
    }

    // ---------------------------------------------------------
    // TOQUE
    // ---------------------------------------------------------

    private void ProcesarToque()
    {
        // 1. Toque en Celular (Android)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current != null)
                {
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        return;
                    }
                }

                if (currentPlane == null)
                    return;

                if (usarSoloPlanosEstables)
                {
                    if (currentPlane.trackingState != TrackingState.Tracking)
                    {
                        return;
                    }
                }

                ColocarTablero();
                return;
            }
        }

        // 2. Clic en Editor de Unity
        #if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (currentPlane == null)
                return;

            ColocarTablero();
        }
        #endif
    }

    // ---------------------------------------------------------
    // COLOCAR TABLERO
    // ---------------------------------------------------------

    private void ColocarTablero()
    {
        if (boardPlaced)
            return;

        if (boardPrefab == null)
        {
            Debug.LogError(
                "BoardPlacementManager: " +
                "boardPrefab no está asignado."
            );

            return;
        }

        if (currentPlane == null)
            return;

        Pose poseFinal = currentPose;

        // -----------------------------------------------------
        // CREAR ANCHOR
        // -----------------------------------------------------

        if (anchorManager != null)
        {
            currentAnchor =
                anchorManager.AttachAnchor(
                    currentPlane,
                    poseFinal
                );
        }

        // -----------------------------------------------------
        // CREAR TABLERO
        // -----------------------------------------------------

        if (currentAnchor != null)
        {
            spawnedBoard = Instantiate(
                boardPrefab,
                currentAnchor.transform
            );

            spawnedBoard.transform.localPosition =
                Vector3.zero;

            spawnedBoard.transform.localRotation =
                Quaternion.identity;
        }
        else
        {
            spawnedBoard = Instantiate(
                boardPrefab,
                poseFinal.position,
                poseFinal.rotation
            );

            Debug.LogWarning(
                "No se pudo crear ARAnchor. " +
                "El tablero fue colocado directamente."
            );
        }

        // -----------------------------------------------------
        // REGISTRAR TABLERO
        // -----------------------------------------------------

        if (KanbanBoardManager.Instance != null)
        {
            KanbanBoardManager.Instance
                .RegistrarTablero(
                    spawnedBoard.transform
                );
        }

        // -----------------------------------------------------
        // CAMBIAR ESTADO
        // -----------------------------------------------------

        boardPlaced = true;
        isDetecting = false;

        // -----------------------------------------------------
        // LIMPIAR INTERFAZ Y MOSTRAR BOTONES DE ACCIÓN
        // -----------------------------------------------------

        OcultarReticula();

        if (panelInstrucciones != null)
        {
            panelInstrucciones.SetActive(false);
        }

        // ACTIVAR BOTONES (Nueva Nota, Papelera, Reubicar)
        if (panelBotonesAccion != null)
        {
            panelBotonesAccion.SetActive(true);
        }

        // -----------------------------------------------------
        // OCULTAR PLANOS Y NUBE DE PUNTOS
        // -----------------------------------------------------

        OcultarTodosLosPlanos();
        OcultarNubeDePuntos();
    }

    // ---------------------------------------------------------
    // OCULTAR TODOS LOS PLANOS
    // ---------------------------------------------------------

    private void OcultarTodosLosPlanos()
    {
        if (planeManager == null)
            return;

        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane != null)
            {
                plane.gameObject.SetActive(false);
            }
        }
    }

    // ---------------------------------------------------------
    // OCULTAR NUBE DE PUNTOS
    // ---------------------------------------------------------

    private void OcultarNubeDePuntos()
    {
        if (pointCloudManager == null)
            return;

        pointCloudManager.enabled = false;

        foreach (ARPointCloud cloud in pointCloudManager.trackables)
        {
            if (cloud != null)
            {
                cloud.gameObject.SetActive(false);
            }
        }
    }

    // ---------------------------------------------------------
    // MOSTRAR NUBE DE PUNTOS
    // ---------------------------------------------------------

    private void MostrarNubeDePuntos()
    {
        if (pointCloudManager == null)
            return;

        pointCloudManager.enabled = true;

        foreach (ARPointCloud cloud in pointCloudManager.trackables)
        {
            if (cloud != null)
            {
                cloud.gameObject.SetActive(true);
            }
        }
    }

    // ---------------------------------------------------------
    // MOSTRAR PLANOS EXISTENTES
    // ---------------------------------------------------------

    private void MostrarPlanosExistentes()
    {
        if (planeManager == null)
            return;

        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane != null)
            {
                plane.gameObject.SetActive(true);
            }
        }
    }

    // ---------------------------------------------------------
    // MENSAJE
    // ---------------------------------------------------------

    private void MostrarMensaje(string mensaje)
    {
        if (ultimoMensaje == mensaje)
            return;

        ultimoMensaje = mensaje;

        if (panelInstrucciones != null &&
            !panelInstrucciones.activeSelf)
        {
            panelInstrucciones.SetActive(true);
        }

        if (textoInstrucciones != null)
        {
            textoInstrucciones.text = mensaje;
        }
    }

    // ---------------------------------------------------------
    // REUBICAR
    // ---------------------------------------------------------

    public void ResetPlacement()
    {
        boardPlaced = false;
        isDetecting = true;
        currentPlane = null;

        // OCULTAR LOS BOTONES DE ACCIÓN HASTA VOLVER A FIJAR EL TABLERO
        if (panelBotonesAccion != null)
        {
            panelBotonesAccion.SetActive(false);
        }

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

        MostrarPlanosExistentes();
        MostrarNubeDePuntos();
        OcultarReticula();

        if (panelInstrucciones != null)
        {
            panelInstrucciones.SetActive(true);
        }

        MostrarMensaje(
            "Selecciona una superficie para reubicar el tablero."
        );
    }

    // ---------------------------------------------------------
    // REGRESAR AL MENÚ PRINCIPAL
    // ---------------------------------------------------------

    public void RegresarAlMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}