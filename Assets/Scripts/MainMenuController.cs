using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Paneles")]
    public GameObject modalCrear;
    public TMP_InputField inputNombre;
    public TextMeshProUGUI textoEstado;

    [Header("Lista ScrollView")]
    public Transform contentLista; // Objeto 'Content' del ScrollView
    public GameObject itemTableroPrefab; // Prefab 'ItemTablero_Btn'

    void Start()
    {
        CargarListaTableros();
    }

    public void AbrirModalCrear()
    {
        modalCrear.SetActive(true);
        inputNombre.text = "";
        inputNombre.ActivateInputField();
    }

    public void CerrarModalCrear()
    {
        modalCrear.SetActive(false);
    }

    public void ConfirmarCrearTablero()
    {
        string nombre = string.IsNullOrWhiteSpace(inputNombre.text) ? "Tablero Sin Título" : inputNombre.text.Trim();
        string nuevoId = "tablero_" + DateTime.UtcNow.Ticks;

        TableroData nuevoTablero = new TableroData
        {
            id = nuevoId,
            nombre = nombre,
            notas = new List<NotaData>()
        };

        if (textoEstado != null) textoEstado.text = "Creando en la nube...";

        FirebaseManager.Instance.GuardarTablero(nuevoTablero, (exito) =>
        {
            if (exito)
            {
                FirebaseManager.TableroActivo = nuevoTablero;
                SceneManager.LoadScene("ARScene");
            }
            else
            {
                if (textoEstado != null) textoEstado.text = "Error al guardar tablero.";
            }
        });
    }

    public void CargarListaTableros()
    {
        // Limpiar botones anteriores
        foreach (Transform child in contentLista)
        {
            Destroy(child.gameObject);
        }

        if (textoEstado != null) textoEstado.text = "Cargando tableros desde Firebase...";

        FirebaseManager.Instance.ObtenerTableros((json) =>
        {
            if (string.IsNullOrEmpty(json) || json == "null" || json.Trim() == "{}")
            {
                if (textoEstado != null) textoEstado.text = "No hay tableros guardados. ¡Crea uno nuevo!";
                return;
            }

            if (textoEstado != null) textoEstado.text = "";

            List<TableroData> lista = ExtraerTablerosDeJson(json);

            if (lista.Count == 0)
            {
                if (textoEstado != null) textoEstado.text = "No se pudieron procesar los tableros.";
                return;
            }

            foreach (var t in lista)
            {
                GameObject nuevoBoton = Instantiate(itemTableroPrefab, contentLista);
                
                // Asegurar escala normal en el ScrollView
                nuevoBoton.transform.localScale = Vector3.one;

                // Asignar texto del botón
                TextMeshProUGUI tmp = nuevoBoton.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = t.nombre;
                }

                // Asignar evento al hacer clic
                TableroData tableroSeleccionado = t;
                Button btn = nuevoBoton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() =>
                    {
                        FirebaseManager.TableroActivo = tableroSeleccionado;
                        SceneManager.LoadScene("ARScene");
                    });
                }
            }
        });
    }

    /// <summary>
    /// Extrae cada objeto {"id":..., "nombre":...} del mapa general devuelto por Firebase
    /// sin que JsonUtility falle por las llaves dinámicas.
    /// </summary>
    private List<TableroData> ExtraerTablerosDeJson(string json)
    {
        List<TableroData> resultado = new List<TableroData>();

        try
        {
            // Busca cada aparición del campo "id" dentro del JSON
            string marcador = "\"id\"";
            int indiceActual = 0;

            while ((indiceActual = json.IndexOf(marcador, indiceActual)) != -1)
            {
                // Encontrar el inicio de la llave '{' que contiene este id
                int inicioObjeto = json.LastIndexOf('{', indiceActual);
                if (inicioObjeto != -1)
                {
                    // Encontrar el cierre de llave '}' correspondiente al objeto del tablero
                    int balanceLlaves = 0;
                    int finObjeto = -1;

                    for (int i = inicioObjeto; i < json.Length; i++)
                    {
                        if (json[i] == '{') balanceLlaves++;
                        else if (json[i] == '}')
                        {
                            balanceLlaves--;
                            if (balanceLlaves == 0)
                            {
                                finObjeto = i;
                                break;
                            }
                        }
                    }

                    if (finObjeto != -1)
                    {
                        string jsonObjeto = json.Substring(inicioObjeto, (finObjeto - inicioObjeto) + 1);

                        // Limpiar posibles notas para instanciar la cabecera básica del botón de forma limpia
                        TableroData t = JsonUtility.FromJson<TableroData>(jsonObjeto);
                        if (t != null && !string.IsNullOrEmpty(t.id))
                        {
                            resultado.Add(t);
                        }

                        indiceActual = finObjeto + 1;
                        continue;
                    }
                }

                indiceActual += marcador.Length;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error al procesar JSON de tableros: " + e.Message);
        }

        return resultado;
    }
}