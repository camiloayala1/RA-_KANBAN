using System;
using System.Collections.Generic;
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

    // Referencias a las 3 columnas
    private Transform colPendiente;
    private Transform colEnProceso;
    private Transform colTerminado;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegistrarTablero(Transform tableroTransform)
    {
        // Encontrar las tres columnas dentro del tablero recién instanciado
        Transform[] allChildren = tableroTransform.GetComponentsInChildren<Transform>(true);
        foreach (var t in allChildren)
        {
            if (t.name == "Columna_Pendiente") colPendiente = t;
            else if (t.name == "Columna_EnProceso") colEnProceso = t;
            else if (t.name == "Columna_Terminado") colTerminado = t;
        }

        // Si entramos con un tablero seleccionado desde el menú, cargar sus notas existentes
        CargarNotasDesdeFirebase();
    }

    private void CargarNotasDesdeFirebase()
    {
        if (FirebaseManager.TableroActivo == null || FirebaseManager.TableroActivo.notas == null)
            return;

        foreach (var notaData in FirebaseManager.TableroActivo.notas)
        {
            Transform columnaDestino = ObtenerColumnaPorNombre(notaData.columna);
            if (columnaDestino != null)
            {
                InstanciarNotaVisual(notaData.id, notaData.texto, columnaDestino);
            }
        }
    }

    public Transform ObtenerColumnaPorNombre(string nombreColumna)
    {
        if (nombreColumna == "Columna_Pendiente") return colPendiente;
        if (nombreColumna == "Columna_EnProceso") return colEnProceso;
        if (nombreColumna == "Columna_Terminado") return colTerminado;
        return colPendiente; // Respaldo por defecto
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
        if (colPendiente == null)
        {
            Debug.LogError("Error: Ancla el tablero a la pared antes de crear notas.");
            CerrarModalCrear();
            return;
        }

        string texto = (inputTextoNota != null && !string.IsNullOrWhiteSpace(inputTextoNota.text)) 
            ? inputTextoNota.text.Trim() 
            : "Nueva Tarea";

        string nuevoId = "nota_" + DateTime.UtcNow.Ticks;
        string colNombre = "Columna_Pendiente";

        // 1. Crear visualmente la nota en la columna Pendiente
        InstanciarNotaVisual(nuevoId, texto, colPendiente);

        // 2. Guardar en la estructura de datos y sincronizar con Firebase
        if (FirebaseManager.TableroActivo != null)
        {
            if (FirebaseManager.TableroActivo.notas == null)
            {
                FirebaseManager.TableroActivo.notas = new List<NotaData>();
            }

            NotaData nuevaNota = new NotaData
            {
                id = nuevoId,
                texto = texto,
                columna = colNombre
            };

            FirebaseManager.TableroActivo.notas.Add(nuevaNota);

            // Subir cambio a Firebase Realtime Database
            FirebaseManager.Instance.GuardarTablero(FirebaseManager.TableroActivo, null);
        }

        CerrarModalCrear();
    }

    private void InstanciarNotaVisual(string id, string texto, Transform columnaPadre)
    {
        GameObject nuevaNota = Instantiate(postItPrefab, columnaPadre, false);
        nuevaNota.name = "PostIt_" + id;

        // Configurar el identificador en el script de la nota
        PostItItem item = nuevaNota.GetComponent<PostItItem>();
        if (item != null)
        {
            item.notaId = id;
            item.textoNota = texto;
            item.columnaActual = columnaPadre.name;
        }

        // Posicionar correctamente dentro del Canvas del tablero
        RectTransform rt = nuevaNota.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            Vector3 pos = rt.localPosition;
            pos.z = -5f; // Despegado hacia adelante para evitar oclusión
            rt.localPosition = pos;
        }

        TMP_Text tmp = nuevaNota.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = texto;
        }
    }
}