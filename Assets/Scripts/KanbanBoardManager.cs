using UnityEngine;
using TMPro;

public class KanbanBoardManager : MonoBehaviour
{
    public static KanbanBoardManager Instance;

    [Header("Prefab de la Nota")]
    public GameObject postItPrefab;

    [Header("Referencias de UI (HUD)")]
    public GameObject modalCrearNota;
    public TMP_InputField inputTextoNota;

    private Transform columnaPendiente;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegistrarTablero(Transform tableroTransform)
    {
        // Buscar Columna_Pendiente en toda la jerarquia del tablero
        Transform[] allChildren = tableroTransform.GetComponentsInChildren<Transform>(true);
        foreach (var t in allChildren)
        {
            if (t.name == "Columna_Pendiente")
            {
                columnaPendiente = t;
                Debug.Log("Columna_Pendiente registrada con éxito.");
                break;
            }
        }
    }

    public void AbrirModalCrear()
    {
        if (modalCrearNota != null)
        {
            modalCrearNota.SetActive(true);
            if (inputTextoNota != null)
            {
                inputTextoNota.text = "";
                inputTextoNota.ActivateInputField();
            }
        }
    }

    public void CerrarModalCrear()
    {
        if (modalCrearNota != null)
        {
            modalCrearNota.SetActive(false);
        }
    }

    public void ConfirmarCrearNota()
    {
        // Respaldo de busqueda si aun no esta asignada
        if (columnaPendiente == null)
        {
            GameObject col = GameObject.Find("Columna_Pendiente");
            if (col != null) columnaPendiente = col.transform;
        }

        if (columnaPendiente == null)
        {
            Debug.LogError("Error: No se encontró 'Columna_Pendiente'. Asegúrate de haber colocado el tablero primero.");
            CerrarModalCrear();
            return;
        }

        string texto = (inputTextoNota != null && !string.IsNullOrWhiteSpace(inputTextoNota.text)) 
            ? inputTextoNota.text 
            : "Nueva Tarea";

        // Instanciar nota dentro de Columna_Pendiente
        GameObject nuevaNota = Instantiate(postItPrefab, columnaPendiente, false);
        nuevaNota.name = "PostIt_Prefab";

        // Normalizar transform dentro del Canvas
        RectTransform rt = nuevaNota.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            Vector3 pos = rt.localPosition;
            pos.z = -1f; // Ligeramente despegado del fondo
            rt.localPosition = pos;
        }

        // Asignar texto
        TMP_Text tmp = nuevaNota.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = texto;
        }

        CerrarModalCrear();
    }
}