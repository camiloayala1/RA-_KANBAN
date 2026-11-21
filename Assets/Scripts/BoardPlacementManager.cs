using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

/// <summary>
/// Gestiona la detección de planos y la fijación del tablero Kanban en Realidad Aumentada.
/// Implementa anclaje físico persistente (ARAnchor), cálculo cinemático de pose para paredes y mesas,
/// bloqueo estricto de escala para evitar achicamiento/agrandamiento al acercarse,
/// y ocultamiento visual de planos sin desactivar el tracking de ARCore/ARKit.
/// </summary>
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

    // Componentes AR Foundation
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private ARAnchorManager anchorManager;
    private ARPointCloudManager pointCloudManager;

    // Lista reutilizable de raycast hits para evitar recolección de basura
    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>(8);

    // Objetos instanciados
    private GameObject spawnedBoard;
    private ARAnchor currentAnchor;
    private ARPlane currentPlane;

    // Estado de detección y pose
    private bool boardPlaced = false;
    private bool isDetecting = true;
    private Pose currentPose;
    private string ultimoMensaje = "";
    private Camera arCamera;

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        anchorManager = GetComponent<ARAnchorManager>();
        pointCloudManager = GetComponent<ARPointCloudManager>();

        arCamera = Camera.main;

        if (raycastManager == null)
            Debug.LogError("BoardPlacementManager: No se encontró ARRaycastManager.");

        if (planeManager == null)
            Debug.LogError("BoardPlacementManager: No se encontró ARPlaneManager.");

        if (anchorManager == null)
            Debug.LogWarning("BoardPlacementManager: No se encontró ARAnchorManager. El tablero podrá colocarse sin Anchor.");

        if (boardPrefab == null)
            Debug.LogError("BoardPlacementManager: boardPrefab no está asignado.");
    }

    private void Start()
    {
        if (placementReticle != null)
            placementReticle.SetActive(false);

        if (panelBotonesAccion != null)
            panelBotonesAccion.SetActive(false);

        MostrarMensaje("Mueve el celular apuntando a una pared o superficie...");
    }

    private void OnEnable()
    {
        if (planeManager != null)
        {
            planeManager.trackablesChanged.AddListener(OnTrackablesChanged);
        }
    }

    private void OnDisable()
    {
        if (planeManager != null)
        {
            planeManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        }
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        // Si el tablero ya está fijado, ocultar las mallas de nuevos planos detectados
        // SIN desactivar los GameObjects para que el tracking del Anchor continúe funcionando
        if (!boardPlaced) return;

        for (int i = 0; i < args.added.Count; i++)
        {
            SetPlaneVisualsActive(args.added[i], false);
        }
        for (int i = 0; i < args.updated.Count; i++)
        {
            SetPlaneVisualsActive(args.updated[i], false);
        }
    }

    private void Update()
    {
        if (boardPlaced || !isDetecting)
            return;

        DetectarSuperficie();
    }

    // ---------------------------------------------------------
    // DETECCIÓN DE SUPERFICIE
    // ---------------------------------------------------------

    private void DetectarSuperficie()
    {
        if (raycastManager == null) return;

        if (arCamera == null)
        {
            arCamera = Camera.main;
            if (arCamera == null) return;
        }

        Vector2 centroPantalla = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        // Raycast optimizado contra polígonos y límites de planos para mayor fluidez
        bool encontrado = raycastManager.Raycast(
            centroPantalla,
            hits,
            TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds
        );

        if (!encontrado || hits.Count == 0)
        {
            currentPlane = null;
            OcultarReticula();
            MostrarMensaje("Mueve el celular apuntando a una pared o superficie...");
            return;
        }

        // Buscar el hit más adecuado
        ARRaycastHit mejorHit = ObtenerMejorHit();
        if (mejorHit.trackableId == TrackableId.invalidId)
        {
            currentPlane = null;
            OcultarReticula();
            return;
        }

        ARPlane plane = planeManager.GetPlane(mejorHit.trackableId);
        if (plane == null || !EsSuperficieValida(plane))
        {
            currentPlane = null;
            OcultarReticula();
            MostrarMensaje("Apunta a una pared o superficie válida.");
            return;
        }

        if (usarSoloPlanosEstables && plane.trackingState != TrackingState.Tracking)
        {
            currentPlane = null;
            MostrarMensaje("Esperando que la superficie se estabilice...");
            return;
        }

        currentPlane = plane;
        currentPose = CalcularPose(mejorHit, plane);

        MostrarReticula(currentPose);

        if (plane.alignment == PlaneAlignment.Vertical)
        {
            MostrarMensaje("Pared detectada. Toca la pantalla para fijar el tablero.");
        }
        else
        {
            MostrarMensaje("Superficie detectada. Toca la pantalla para fijar el tablero.");
        }

        ProcesarToque();
    }

    private ARRaycastHit ObtenerMejorHit()
    {
        if (hits.Count == 0) return default;
        if (hits.Count == 1) return hits[0];

        ARRaycastHit mejorHit = hits[0];
        float menorDistancia = float.MaxValue;
        Vector3 posicionCamara = (arCamera != null) ? arCamera.transform.position : Vector3.zero;

        for (int i = 0; i < hits.Count; i++)
        {
            float distancia = (hits[i].pose.position - posicionCamara).sqrMagnitude;
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                mejorHit = hits[i];
            }
        }

        return mejorHit;
    }

    private bool EsSuperficieValida(ARPlane plane)
    {
        if (plane == null) return false;

        if (plane.alignment == PlaneAlignment.Vertical)
            return permitirParedes;

        if (plane.alignment == PlaneAlignment.HorizontalUp)
            return permitirSuelo;

        return false;
    }

    // ---------------------------------------------------------
    // CÁLCULO PRECISO DE POSE (Paredes y Mesas)
    // ---------------------------------------------------------

    private Pose CalcularPose(ARRaycastHit hit, ARPlane plane)
    {
        Vector3 posicion = hit.pose.position;

        if (arCamera == null)
            arCamera = Camera.main;

        // PARED (Superficie vertical)
        if (plane.alignment == PlaneAlignment.Vertical)
        {
            Vector3 normalPared = plane.normal;
            Vector3 haciaCamara = arCamera.transform.position - posicion;

            // Asegurar que la normal apunte hacia el usuario
            if (Vector3.Dot(normalPared, haciaCamara) < 0f)
            {
                normalPared = -normalPared;
            }

            // En Unity UI / Quads, el frontal es opuesto a forward (LookRotation hacia adentro)
            Quaternion rotacion = Quaternion.LookRotation(-normalPared, Vector3.up);

            // Separación de 1.5 cm de la pared para evitar z-fighting / parpadeo
            Vector3 posicionAjustada = posicion + normalPared * 0.015f;

            return new Pose(posicionAjustada, rotacion);
        }

        // MESA / SUELO (Superficie horizontal)
        Vector3 direccion = arCamera.transform.position - posicion;
        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.0001f)
            direccion = Vector3.forward;

        direccion.Normalize();

        // El tablero se orienta erguido mirando hacia el usuario
        Quaternion rotacionHorizontal = Quaternion.LookRotation(-direccion, Vector3.up);

        // Elevar 30 cm (la mitad de la altura de 60 cm) para que la base descanse sobre la mesa
        Vector3 posicionMesa = posicion + Vector3.up * 0.30f;

        return new Pose(posicionMesa, rotacionHorizontal);
    }

    // ---------------------------------------------------------
    // RETÍCULA DE COLOCACIÓN CON SUAVIZADO CINEMÁTICO
    // ---------------------------------------------------------

    private void MostrarReticula(Pose pose)
    {
        if (placementReticle == null) return;

        if (!placementReticle.activeSelf)
        {
            placementReticle.SetActive(true);
            placementReticle.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
        else
        {
            placementReticle.transform.position = Vector3.Lerp(placementReticle.transform.position, pose.position, Time.deltaTime * 20f);
            placementReticle.transform.rotation = Quaternion.Slerp(placementReticle.transform.rotation, pose.rotation, Time.deltaTime * 20f);
        }
    }

    private void OcultarReticula()
    {
        if (placementReticle != null && placementReticle.activeSelf)
        {
            placementReticle.SetActive(false);
        }
    }

    // ---------------------------------------------------------
    // PROCESAMIENTO DE TOQUES
    // ---------------------------------------------------------

    private void ProcesarToque()
    {
        // 1. Toque en pantalla táctil móvil (Android / iOS)
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                if (currentPlane == null) return;

                if (usarSoloPlanosEstables && currentPlane.trackingState != TrackingState.Tracking)
                    return;

                ColocarTablero();
                return;
            }
        }

        // 2. Clic en Unity Editor
        #if UNITY_EDITOR
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (currentPlane == null) return;

            ColocarTablero();
        }
        #endif
    }

    // ---------------------------------------------------------
    // COLOCACIÓN Y ANCLAJE DEL TABLERO
    // ---------------------------------------------------------

    private void ColocarTablero()
    {
        if (boardPlaced) return;

        if (boardPrefab == null)
        {
            Debug.LogError("BoardPlacementManager: boardPrefab no está asignado.");
            return;
        }

        if (currentPlane == null) return;

        Pose poseFinal = currentPose;

        // 1. Intentar fijar el Anchor al plano detectado en ARCore / ARKit
        if (anchorManager != null && currentPlane != null)
        {
            currentAnchor = anchorManager.AttachAnchor(currentPlane, poseFinal);
        }

        // 2. Si AttachAnchor no está disponible, crear un Anchor directo por pose en el mundo
        if (currentAnchor == null && anchorManager != null)
        {
            GameObject anchorGo = new GameObject("Board_ARAnchor");
            anchorGo.transform.SetPositionAndRotation(poseFinal.position, poseFinal.rotation);
            currentAnchor = anchorGo.AddComponent<ARAnchor>();
        }

        // 3. Instanciar el tablero como hijo del Anchor para que acompañe el tracking físico
        if (currentAnchor != null)
        {
            spawnedBoard = Instantiate(boardPrefab, currentAnchor.transform);
            spawnedBoard.transform.localPosition = Vector3.zero;
            spawnedBoard.transform.localRotation = Quaternion.identity;
        }
        else
        {
            spawnedBoard = Instantiate(boardPrefab, poseFinal.position, poseFinal.rotation);
            Debug.LogWarning("No se pudo crear ARAnchor. El tablero fue colocado directamente.");
        }

        // 4. Bloquear la escala estrictamente en metros reales (1.0m ancho x 0.6m alto)
        spawnedBoard.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        // Asegurar que el Canvas tenga asignada la cámara AR en World Space
        Canvas boardCanvas = spawnedBoard.GetComponent<Canvas>();
        if (boardCanvas != null)
        {
            boardCanvas.renderMode = RenderMode.WorldSpace;
            if (arCamera == null) arCamera = Camera.main;
            boardCanvas.worldCamera = arCamera;
        }

        // Garantizar que la escala permanezca fija independientemente de la cercanía del usuario
        if (spawnedBoard.GetComponent<FixedARScale>() == null)
        {
            spawnedBoard.AddComponent<FixedARScale>();
        }

        // 5. Registrar el tablero en el manager de Kanban
        if (KanbanBoardManager.Instance != null)
        {
            KanbanBoardManager.Instance.RegistrarTablero(spawnedBoard.transform);
        }

        // 6. Actualizar estado
        boardPlaced = true;
        isDetecting = false;

        OcultarReticula();

        if (panelInstrucciones != null)
            panelInstrucciones.SetActive(false);

        if (panelBotonesAccion != null)
            panelBotonesAccion.SetActive(true);

        // 7. Ocultar las visuales de los planos SIN apagar los GameObjects (mantiene el tracking AR activo)
        OcultarTodosLosPlanos();
        OcultarNubeDePuntos();
    }

    // ---------------------------------------------------------
    // GESTIÓN DE VISUALES DE PLANOS (Sin afectar el tracking)
    // ---------------------------------------------------------

    private void SetPlaneVisualsActive(ARPlane plane, bool visible)
    {
        if (plane == null) return;

        var meshRenderer = plane.GetComponent<MeshRenderer>();
        if (meshRenderer != null) meshRenderer.enabled = visible;

        var lineRenderer = plane.GetComponent<LineRenderer>();
        if (lineRenderer != null) lineRenderer.enabled = visible;

        var visualizer = plane.GetComponent<ARPlaneMeshVisualizer>();
        if (visualizer != null) visualizer.enabled = visible;
    }

    private void OcultarTodosLosPlanos()
    {
        if (planeManager == null) return;

        foreach (ARPlane plane in planeManager.trackables)
        {
            SetPlaneVisualsActive(plane, false);
        }
    }

    private void MostrarPlanosExistentes()
    {
        if (planeManager == null) return;

        foreach (ARPlane plane in planeManager.trackables)
        {
            SetPlaneVisualsActive(plane, true);
        }
    }

    private void OcultarNubeDePuntos()
    {
        if (pointCloudManager == null) return;

        pointCloudManager.enabled = false;

        foreach (ARPointCloud cloud in pointCloudManager.trackables)
        {
            if (cloud != null)
            {
                var ps = cloud.GetComponent<ParticleSystem>();
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var mr = cloud.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;
            }
        }
    }

    private void MostrarNubeDePuntos()
    {
        if (pointCloudManager == null) return;

        pointCloudManager.enabled = true;

        foreach (ARPointCloud cloud in pointCloudManager.trackables)
        {
            if (cloud != null)
            {
                var ps = cloud.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
                var mr = cloud.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = true;
            }
        }
    }

    private void MostrarMensaje(string mensaje)
    {
        if (ultimoMensaje == mensaje) return;

        ultimoMensaje = mensaje;

        if (panelInstrucciones != null && !panelInstrucciones.activeSelf)
            panelInstrucciones.SetActive(true);

        if (textoInstrucciones != null)
            textoInstrucciones.text = mensaje;
    }

    // ---------------------------------------------------------
    // REUBICAR TABLERO
    // ---------------------------------------------------------

    public void ResetPlacement()
    {
        boardPlaced = false;
        isDetecting = true;
        currentPlane = null;

        if (panelBotonesAccion != null)
            panelBotonesAccion.SetActive(false);

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
            panelInstrucciones.SetActive(true);

        MostrarMensaje("Mueve el celular para seleccionar una superficie donde reubicar el tablero.");
    }

    // ---------------------------------------------------------
    // REGRESAR AL MENÚ PRINCIPAL
    // ---------------------------------------------------------

    public void RegresarAlMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
