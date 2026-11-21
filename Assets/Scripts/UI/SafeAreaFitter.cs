using UnityEngine;

/// <summary>
/// Componente de ajuste automático para Notch y Áreas Seguras (Safe Area) en dispositivos móviles.
/// Compatible con Android y iOS (pantallas con agujero de cámara, muescas, dynamic islands y barras de navegación).
/// </summary>
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class SafeAreaFitter : MonoBehaviour
{
    public enum SafeAreaMode
    {
        FullSafeArea,       // Ajusta los 4 lados (Top, Bottom, Left, Right)
        TopOnly,            // Solo protege contra el Notch / Barra de estado superior
        TopAndBottom,       // Protege Notch superior y barra de gestos inferior
        BottomOnly          // Solo protege la barra de gestos inferior
    }

    [Header("Configuración de Ajuste")]
    [Tooltip("Define qué bordes se protegen contra notch/barras del sistema.")]
    public SafeAreaMode mode = SafeAreaMode.TopAndBottom;

    [Header("Márgenes Adicionales (Canvas Units)")]
    [Tooltip("Margen extra de resguardo en la parte superior (además del notch físico).")]
    public float extraTopPadding = 20f;
    [Tooltip("Margen extra de resguardo en la parte inferior.")]
    public float extraBottomPadding = 15f;

    [Header("Simulación en Editor")]
    [Tooltip("Margen superior simulado en el Editor de Unity si no hay notch real.")]
    public float simulatedEditorTopNotch = 90f;

    private RectTransform rectTransform;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);
    private Vector2Int lastScreenSize = new Vector2Int(0, 0);
    private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        RefreshIfNeeded();
    }

    private void RefreshIfNeeded()
    {
        Rect currentSafeArea = Screen.safeArea;
        Vector2Int currentScreenSize = new Vector2Int(Screen.width, Screen.height);
        ScreenOrientation currentOrientation = Screen.orientation;

        if (currentSafeArea != lastSafeArea || currentScreenSize != lastScreenSize || currentOrientation != lastOrientation)
        {
            ApplySafeArea();
        }
    }

    [ContextMenu("Forzar Aplicación Safe Area")]
    public void ApplySafeArea()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null) return;

        Rect safeArea = Screen.safeArea;

        #if UNITY_EDITOR
        // En el editor de Unity, si safeArea abarca toda la pantalla, simulamos un notch para previsualizar
        if (safeArea.yMax >= Screen.height && simulatedEditorTopNotch > 0)
        {
            float simulatedPixels = (simulatedEditorTopNotch / 1920f) * Screen.height;
            safeArea = new Rect(0, 0, Screen.width, Screen.height - simulatedPixels);
        }
        #endif

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        float screenW = Screen.width;
        float screenH = Screen.height;

        if (screenW <= 0 || screenH <= 0) return;

        anchorMin.x /= screenW;
        anchorMin.y /= screenH;
        anchorMax.x /= screenW;
        anchorMax.y /= screenH;

        // Según el modo seleccionado, limitamos qué anclas se modifican
        switch (mode)
        {
            case SafeAreaMode.TopOnly:
                rectTransform.anchorMin = new Vector2(0f, 0f);
                rectTransform.anchorMax = new Vector2(1f, anchorMax.y);
                rectTransform.offsetMax = new Vector2(0f, -extraTopPadding);
                rectTransform.offsetMin = Vector2.zero;
                break;

            case SafeAreaMode.TopAndBottom:
                rectTransform.anchorMin = new Vector2(0f, anchorMin.y);
                rectTransform.anchorMax = new Vector2(1f, anchorMax.y);
                rectTransform.offsetMax = new Vector2(0f, -extraTopPadding);
                rectTransform.offsetMin = new Vector2(0f, extraBottomPadding);
                break;

            case SafeAreaMode.BottomOnly:
                rectTransform.anchorMin = new Vector2(0f, anchorMin.y);
                rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.offsetMin = new Vector2(0f, extraBottomPadding);
                break;

            case SafeAreaMode.FullSafeArea:
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
                rectTransform.offsetMax = new Vector2(0f, -extraTopPadding);
                rectTransform.offsetMin = new Vector2(0f, extraBottomPadding);
                break;
        }

        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        lastOrientation = Screen.orientation;
    }
}
