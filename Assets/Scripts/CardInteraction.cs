using UnityEngine;
using UnityEngine.EventSystems;

public class CardInteraction : MonoBehaviour
{
    [Header("Cámara AR")]
    public Camera arCamera;

    [Header("Zona de Papelera en HUD (Opcional)")]
    public RectTransform trashZone;

    private Transform selectedCard = null;
    private Transform originalParent = null;
    private Plane dragPlane;
    private Vector3 offset;
    private float liftDistance = 0.04f;

    void Awake()
    {
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
    }

    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        // 1. Iniciar toque
        if (touch.phase == TouchPhase.Began)
        {
            if (IsPointerOverScreenUI(touch.fingerId)) return;

            Ray ray = arCamera.ScreenPointToRay(touch.position);

            if (Physics.Raycast(ray, out RaycastHit hit, 30f))
            {
                Transform hitTransform = hit.transform;

                if (hitTransform.CompareTag("PostIt") || hitTransform.name.Contains("PostIt"))
                {
                    selectedCard = hitTransform;
                    originalParent = selectedCard.parent;

                    // Pasar la tarjeta al frente de su contenedor para que no la tape nada
                    selectedCard.SetAsLastSibling();

                    // Separarla físicamente hacia la cámara
                    selectedCard.position -= arCamera.transform.forward * liftDistance;

                    dragPlane = new Plane(-arCamera.transform.forward, selectedCard.position);

                    if (dragPlane.Raycast(ray, out float enterDistance))
                    {
                        offset = selectedCard.position - ray.GetPoint(enterDistance);
                    }
                }
            }
        }
        // 2. Arrastrar con el dedo
        else if (touch.phase == TouchPhase.Moved && selectedCard != null)
        {
            Ray ray = arCamera.ScreenPointToRay(touch.position);

            if (dragPlane.Raycast(ray, out float enterDistance))
            {
                selectedCard.position = ray.GetPoint(enterDistance) + offset;
            }
        }
        // 3. Soltar tarjeta
        else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && selectedCard != null)
        {
            DropCard(touch.position);
        }
    }

    private void DropCard(Vector2 screenPosition)
    {
        // 1. Destruir si cae en papelera
        if (trashZone != null && RectTransformUtility.RectangleContainsScreenPoint(trashZone, screenPosition))
        {
            Destroy(selectedCard.gameObject);
            selectedCard = null;
            return;
        }

        // 2. Identificar la columna de destino
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 30f);

        Transform targetColumn = null;

        foreach (var h in hits)
        {
            if (h.transform.name.StartsWith("Columna_"))
            {
                targetColumn = h.transform;
                break;
            }
        }

        if (targetColumn != null)
        {
            selectedCard.SetParent(targetColumn, false);
        }
        else
        {
            selectedCard.SetParent(originalParent, false);
        }

        // Poner la tarjeta en primer plano dentro de la columna y asegurar Z negativa
        selectedCard.SetAsLastSibling();

        RectTransform rt = selectedCard.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            Vector3 pos = rt.localPosition;
            pos.z = -15f; // Despegado hacia el frente para evitar solapamiento con el fondo
            rt.localPosition = pos;
        }

        selectedCard = null;
    }

    private bool IsPointerOverScreenUI(int fingerId)
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.GetTouch(fingerId).position
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            if (r.gameObject.transform.root.name == "ScreenHUD")
                return true;
        }

        return false;
    }
}