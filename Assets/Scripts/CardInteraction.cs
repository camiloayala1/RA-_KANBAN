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
    private Transform canvasBoardRoot = null;
    private Plane dragPlane;
    private Vector3 offset;
    private float liftDistance = 0.05f;

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

                    Canvas parentCanvas = selectedCard.GetComponentInParent<Canvas>();
                    if (parentCanvas != null)
                    {
                        canvasBoardRoot = parentCanvas.transform;
                        selectedCard.SetParent(canvasBoardRoot, true);
                        selectedCard.SetAsLastSibling();
                    }

                    selectedCard.position -= arCamera.transform.forward * liftDistance;

                    dragPlane = new Plane(-arCamera.transform.forward, selectedCard.position);

                    if (dragPlane.Raycast(ray, out float enterDistance))
                    {
                        offset = selectedCard.position - ray.GetPoint(enterDistance);
                    }
                }
            }
        }
        // 2. Arrastrar
        else if (touch.phase == TouchPhase.Moved && selectedCard != null)
        {
            Ray ray = arCamera.ScreenPointToRay(touch.position);

            if (dragPlane.Raycast(ray, out float enterDistance))
            {
                selectedCard.position = ray.GetPoint(enterDistance) + offset;
            }
        }
        // 3. Soltar
        else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && selectedCard != null)
        {
            DropCard(touch.position);
        }
    }

    private void DropCard(Vector2 screenPosition)
    {
        // 1. Si cayó sobre la papelera: borrar de la escena y de Firebase
        if (trashZone != null && RectTransformUtility.RectangleContainsScreenPoint(trashZone, screenPosition))
        {
            PostItItem postIt = selectedCard.GetComponent<PostItItem>();
            if (postIt != null && FirebaseManager.TableroActivo != null && FirebaseManager.TableroActivo.notas != null)
            {
                FirebaseManager.TableroActivo.notas.RemoveAll(n => n.id == postIt.notaId);
                FirebaseManager.Instance.GuardarTablero(FirebaseManager.TableroActivo, null);
            }

            Destroy(selectedCard.gameObject);
            selectedCard = null;
            return;
        }

        // 2. Comprobar columna de destino
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

        Transform finalParent = (targetColumn != null) ? targetColumn : originalParent;

        selectedCard.SetParent(finalParent, true);
        selectedCard.SetAsLastSibling();

        RectTransform rt = selectedCard.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            Vector3 pos = rt.localPosition;
            pos.z = -5f;
            rt.localPosition = pos;
        }

        // 3. Si cambió de columna, guardar el cambio en Firebase
        PostItItem itemData = selectedCard.GetComponent<PostItItem>();
        if (itemData != null && finalParent != originalParent && targetColumn != null)
        {
            itemData.columnaActual = finalParent.name;

            if (FirebaseManager.TableroActivo != null && FirebaseManager.TableroActivo.notas != null)
            {
                var notaEncontrada = FirebaseManager.TableroActivo.notas.Find(n => n.id == itemData.notaId);
                if (notaEncontrada != null)
                {
                    notaEncontrada.columna = finalParent.name;
                    FirebaseManager.Instance.GuardarTablero(FirebaseManager.TableroActivo, null);
                }
            }
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