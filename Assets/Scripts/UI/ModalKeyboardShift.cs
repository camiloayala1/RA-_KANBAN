using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Eleva automáticamente los modales/tarjetas cuando el teclado táctil móvil (TouchScreenKeyboard)
/// se activa o cuando cualquier campo de texto (TMP_InputField) dentro del modal recibe foco.
/// Garantiza que los campos de entrada y los botones de acción (+ CREAR, GUARDAR, + NOTA)
/// nunca queden tapados por el teclado en dispositivos móviles.
/// </summary>
public class ModalKeyboardShift : MonoBehaviour
{
    [Header("Contenedor a Elevar")]
    [Tooltip("El RectTransform que se desplazará verticalmente (ej. Caja_Crear, Caja_CrearNota). Si es nulo, se busca automáticamente.")]
    public RectTransform containerToShift;

    [Header("Ajustes de Elevación")]
    [Tooltip("Distancia en píxeles de Canvas que se eleva la tarjeta al abrirse el teclado.")]
    public float shiftAmount = 400f;
    [Tooltip("Tiempo de suavizado cinemático (SmoothDamp).")]
    public float shiftSmoothTime = 0.16f;
    [Tooltip("Posición Y de reposo cuando no hay teclado activo.")]
    public float restY = 0f;

    [Header("Campos Monitoreados")]
    public TMP_InputField[] inputFields;

    private float currentY;
    private float targetY;
    private float currentVelocityY;
    private bool isKeyboardActive = false;
    private TMP_InputField activeField = null;

    void Awake()
    {
        BuscarContenedorSiEsNulo();
        if (containerToShift != null)
        {
            restY = containerToShift.anchoredPosition.y;
            currentY = restY;
            targetY = restY;
        }
    }

    void OnEnable()
    {
        BuscarContenedorSiEsNulo();
        ConfigurarInputs();

        // Al abrir el modal, asegurarse de iniciar en posición de reposo
        isKeyboardActive = false;
        activeField = null;
        if (containerToShift != null)
        {
            currentY = restY;
            targetY = restY;
            Vector2 pos = containerToShift.anchoredPosition;
            pos.y = restY;
            containerToShift.anchoredPosition = pos;
        }
    }

    void OnDisable()
    {
        isKeyboardActive = false;
        activeField = null;
        if (containerToShift != null)
        {
            Vector2 pos = containerToShift.anchoredPosition;
            pos.y = restY;
            containerToShift.anchoredPosition = pos;
        }
    }

    private void BuscarContenedorSiEsNulo()
    {
        if (containerToShift != null) return;

        // Buscar primero hijos con nombre que empiece por "Caja" o "Card" o "Panel"
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Caja") || child.name.StartsWith("Card") || child.name.StartsWith("Panel"))
            {
                containerToShift = child.GetComponent<RectTransform>();
                break;
            }
        }

        // Si no se encontró un hijo, usar su propio RectTransform
        if (containerToShift == null)
        {
            containerToShift = GetComponent<RectTransform>();
        }
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

            EventTrigger trigger = input.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = input.gameObject.AddComponent<EventTrigger>();
            else trigger.triggers.Clear();

            // OnSelect / Foco
            EventTrigger.Entry selectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Select };
            selectEntry.callback.AddListener((data) => { OnInputFocused(input); });
            trigger.triggers.Add(selectEntry);

            // OnDeselect / Pérdida de foco
            EventTrigger.Entry deselectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Deselect };
            deselectEntry.callback.AddListener((data) => { OnInputUnfocused(input); });
            trigger.triggers.Add(deselectEntry);

            // PointerClick también asegura foco
            EventTrigger.Entry clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            clickEntry.callback.AddListener((data) => { OnInputFocused(input); });
            trigger.triggers.Add(clickEntry);
        }
    }

    public void OnInputFocused(TMP_InputField field)
    {
        activeField = field;
        isKeyboardActive = true;
        targetY = restY + shiftAmount;
    }

    public void OnInputUnfocused(TMP_InputField field)
    {
        if (activeField == field)
        {
            activeField = null;
        }

        #if UNITY_ANDROID || UNITY_IOS
        if (!TouchScreenKeyboard.visible)
        {
            isKeyboardActive = false;
            targetY = restY;
        }
        #else
        // En editor/PC
        isKeyboardActive = false;
        targetY = restY;
        #endif
    }

    void Update()
    {
        // Monitorear teclado táctil nativo en dispositivos móviles
        #if UNITY_ANDROID || UNITY_IOS
        if (TouchScreenKeyboard.visible)
        {
            if (!isKeyboardActive)
            {
                isKeyboardActive = true;
                targetY = restY + shiftAmount;
            }
        }
        else if (isKeyboardActive && activeField == null)
        {
            isKeyboardActive = false;
            targetY = restY;
        }
        #endif

        // Comprobación adicional por EventSystem (selección actual)
        if (EventSystem.current != null)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            bool selectedIsOurField = false;
            if (selected != null && inputFields != null)
            {
                foreach (var inField in inputFields)
                {
                    if (inField != null && (selected == inField.gameObject || selected.transform.IsChildOf(inField.transform)))
                    {
                        selectedIsOurField = true;
                        break;
                    }
                }
            }

            if (selectedIsOurField && !isKeyboardActive)
            {
                isKeyboardActive = true;
                targetY = restY + shiftAmount;
            }
        }

        // Interpolación cinematográfica suave a la posición objetivo
        if (containerToShift != null && Mathf.Abs(currentY - targetY) > 0.1f)
        {
            currentY = Mathf.SmoothDamp(currentY, targetY, ref currentVelocityY, shiftSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            Vector2 pos = containerToShift.anchoredPosition;
            pos.y = currentY;
            containerToShift.anchoredPosition = pos;
        }
    }

    [ContextMenu("Probar Elevación (Simular Teclado)")]
    public void TestElevate()
    {
        targetY = restY + shiftAmount;
        isKeyboardActive = true;
    }

    [ContextMenu("Probar Reposo (Ocultar Teclado)")]
    public void TestRest()
    {
        targetY = restY;
        isKeyboardActive = false;
        activeField = null;
    }
}
