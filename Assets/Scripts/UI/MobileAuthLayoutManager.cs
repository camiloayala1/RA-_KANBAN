using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Gestiona la adaptabilidad de la pantalla de autenticación en móviles:
/// 1. Detección automática del teclado virtual (TouchScreenKeyboard / TMP_InputField Focus).
/// 2. Desplazamiento suave cinemático (Spring / SmoothDamp) para que los campos no queden tapados.
/// 3. Resplandor interactivo de enfoque (Glow Pulse) en el campo activo.
/// 4. Animaciones táctiles de pulsación en botones (Micro-interacciones 2027).
/// </summary>
public class MobileAuthLayoutManager : MonoBehaviour
{
    public static MobileAuthLayoutManager Instance;

    [Header("Contenedor Principal Desplazable")]
    [Tooltip("El RectTransform que se eleva cuando el teclado táctil se activa.")]
    public RectTransform containerToShift;

    [Header("Ajustes de Desplazamiento")]
    public float keyboardShiftAmount = 380f;     // Desplazamiento en píxeles del Canvas
    public float shiftSmoothTime = 0.18f;        // Suavizado cinemático
    public float targetRestY = 0f;               // Posición Y de reposo

    [Header("Campos de Entrada Interactivos")]
    public TMP_InputField[] inputFields;

    [Header("Estilo de Enfoque Neón 2027")]
    public Color colorBordeDefault = new Color(0.14f, 0.28f, 0.44f, 0.85f);
    public Color colorBordeFocus = new Color(0.00f, 0.95f, 1.00f, 1.00f);
    public Color colorBgFocus = new Color(0.10f, 0.16f, 0.26f, 0.98f);
    public Color colorBgDefault = new Color(0.08f, 0.12f, 0.19f, 0.98f);

    [Header("Botones Táctiles Interactivos")]
    public Button[] interactiveButtons;

    private float currentVelocityY;
    private float currentY;
    private float targetY;
    private bool isKeyboardActive = false;
    private TMP_InputField currentlyFocusedField = null;

    private Dictionary<TMP_InputField, Image> fieldBackgrounds = new Dictionary<TMP_InputField, Image>();

    void Awake()
    {
        Instance = this;
        if (containerToShift != null)
        {
            currentY = containerToShift.anchoredPosition.y;
            targetY = currentY;
            targetRestY = currentY;
        }
    }

    void Start()
    {
        ConfigurarInputs();
        ConfigurarBotonesInteractivos();
    }

    private void ConfigurarInputs()
    {
        if (inputFields == null || inputFields.Length == 0)
        {
            inputFields = GetComponentsInChildren<TMP_InputField>(true);
        }

        foreach (var input in inputFields)
        {
            if (input == null) continue;

            Image bg = input.GetComponent<Image>();
            if (bg != null) fieldBackgrounds[input] = bg;

            // Enlazar eventos de foco y pérdida de foco
            EventTrigger trigger = input.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = input.gameObject.AddComponent<EventTrigger>();

            // OnSelect
            EventTrigger.Entry selectEntry = new EventTrigger.Entry();
            selectEntry.eventID = EventTriggerType.Select;
            selectEntry.callback.AddListener((data) => { OnFieldFocused(input); });
            trigger.triggers.Add(selectEntry);

            // OnDeselect
            EventTrigger.Entry deselectEntry = new EventTrigger.Entry();
            deselectEntry.eventID = EventTriggerType.Deselect;
            deselectEntry.callback.AddListener((data) => { OnFieldUnfocused(input); });
            trigger.triggers.Add(deselectEntry);
        }
    }

    private void ConfigurarBotonesInteractivos()
    {
        if (interactiveButtons == null || interactiveButtons.Length == 0)
        {
            interactiveButtons = GetComponentsInChildren<Button>(true);
        }

        foreach (var btn in interactiveButtons)
        {
            if (btn == null) continue;

            EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();

            // PointerDown -> Micro-escala (0.96)
            EventTrigger.Entry down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener((data) => { StartCoroutine(AnimateScale(btn.transform, 0.96f, 0.08f)); });
            trigger.triggers.Add(down);

            // PointerUp -> Rebote elástico (1.0)
            EventTrigger.Entry up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener((data) => { StartCoroutine(AnimateScale(btn.transform, 1.00f, 0.12f)); });
            trigger.triggers.Add(up);
        }
    }

    private IEnumerator AnimateScale(Transform target, float targetScale, float duration)
    {
        if (target == null) yield break;
        Vector3 start = target.localScale;
        Vector3 end = Vector3.one * targetScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (target != null)
                target.localScale = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        if (target != null) target.localScale = end;
    }

    public void OnFieldFocused(TMP_InputField field)
    {
        currentlyFocusedField = field;
        isKeyboardActive = true;

        // Calcular desplazamiento inteligente según la posición del campo en pantalla
        if (field != null && containerToShift != null)
        {
            // Cuanto más abajo esté el campo, más elevamos el contenedor
            float fieldLocalY = field.GetComponent<RectTransform>().anchoredPosition.y;
            float customShift = Mathf.Abs(fieldLocalY) * 0.70f + 160f;
            targetY = targetRestY + Mathf.Clamp(customShift, 250f, 580f);
        }
        else
        {
            targetY = targetRestY + keyboardShiftAmount;
        }

        // Iluminar campo enfocado (Cian Neón)
        if (fieldBackgrounds.TryGetValue(field, out Image bg) && bg != null)
        {
            StopAllCoroutinesForField(field);
            StartCoroutine(PulseFieldGlow(bg, true));
        }
    }

    public void OnFieldUnfocused(TMP_InputField field)
    {
        if (currentlyFocusedField == field)
        {
            currentlyFocusedField = null;
        }

        // Si no hay teclado visible de TouchScreenKeyboard, retornar
        if (!TouchScreenKeyboard.visible)
        {
            isKeyboardActive = false;
            targetY = targetRestY;
        }

        if (field != null && fieldBackgrounds.TryGetValue(field, out Image bg) && bg != null)
        {
            StopAllCoroutinesForField(field);
            StartCoroutine(PulseFieldGlow(bg, false));
        }
    }

    private void StopAllCoroutinesForField(TMP_InputField field)
    {
        // Limpieza de rutinas auxiliares
    }

    private IEnumerator PulseFieldGlow(Image bg, bool focus)
    {
        if (bg == null) yield break;
        Color startColor = bg.color;
        Color targetColor = focus ? colorBgFocus : colorBgDefault;
        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (bg != null) bg.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        if (bg != null) bg.color = targetColor;
    }

    void Update()
    {
        // Soporte nativo para teclados táctiles Android / iOS
        if (TouchScreenKeyboard.visible)
        {
            if (!isKeyboardActive)
            {
                isKeyboardActive = true;
                targetY = targetRestY + keyboardShiftAmount;
            }
        }
        else if (isKeyboardActive && currentlyFocusedField == null)
        {
            isKeyboardActive = false;
            targetY = targetRestY;
        }

        // Interpolación cinemática fluida (Spring SmoothDamp)
        if (containerToShift != null && Mathf.Abs(currentY - targetY) > 0.05f)
        {
            currentY = Mathf.SmoothDamp(currentY, targetY, ref currentVelocityY, shiftSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            Vector2 pos = containerToShift.anchoredPosition;
            pos.y = currentY;
            containerToShift.anchoredPosition = pos;
        }
    }

    /// <summary>
    /// Llamar cuando se cambie entre Tab Login y Tab Registro para reposicionar con fluidez.
    /// </summary>
    public void ResetPositionToRest()
    {
        targetY = targetRestY;
        isKeyboardActive = false;
        currentlyFocusedField = null;
    }
}
