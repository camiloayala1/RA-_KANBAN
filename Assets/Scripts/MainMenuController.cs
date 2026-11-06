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

    [Header("Modal Opciones del Tablero")]
    public GameObject modalOpciones;
    public TextMeshProUGUI textoTituloTableroSeleccionado;
    public Button btnOpcionAbrir;
    public Button btnOpcionEditar;
    public Button btnOpcionEliminar;
    public Button btnCerrarOpciones;

    [Header("Modal Editar Nombre")]
    public GameObject modalEditar;
    public TMP_InputField inputEditarNombre;
    public Button btnConfirmarEdicion;
    public Button btnCancelarEdicion;

    [Header("Modal Confirmar Eliminación")]
    public GameObject modalConfirmarEliminar;
    public TextMeshProUGUI textoAdvertenciaEliminar;
    public Button btnConfirmarEliminarSi;
    public Button btnConfirmarEliminarNo;

    [Header("Panel Login y Usuario")]
    public GameObject panelLogin;
    public TMP_InputField inputCorreo;
    public TMP_InputField inputPassword;
    public TextMeshProUGUI textoErrorLogin;
    public TextMeshProUGUI textoUsuarioConectado;

    [Header("Lista ScrollView")]
    public Transform contentLista;
    public GameObject itemTableroPrefab; // Prefab con TableroItemUI o Button estándar

    private TableroData tableroSeleccionado;

    void Start()
    {
        ConfigurarModales();
        VerificarSesionUsuario();
    }

    private void ConfigurarModales()
    {
        // 1. Modal de Selección de Acción
        if (modalOpciones != null)
            modalOpciones.SetActive(false);

        if (btnOpcionAbrir != null)
        {
            btnOpcionAbrir.onClick.RemoveAllListeners();
            btnOpcionAbrir.onClick.AddListener(EjecutarAbrirTablero);
        }

        if (btnOpcionEditar != null)
        {
            btnOpcionEditar.onClick.RemoveAllListeners();
            btnOpcionEditar.onClick.AddListener(EjecutarAbrirModalEditar);
        }

        if (btnOpcionEliminar != null)
        {
            btnOpcionEliminar.onClick.RemoveAllListeners();
            btnOpcionEliminar.onClick.AddListener(AbrirModalConfirmarEliminar);
        }

        if (btnCerrarOpciones != null)
        {
            btnCerrarOpciones.onClick.RemoveAllListeners();
            btnCerrarOpciones.onClick.AddListener(CerrarModalOpciones);
        }

        // 2. Modal de Edición
        if (modalEditar != null)
            modalEditar.SetActive(false);

        if (btnConfirmarEdicion != null)
        {
            btnConfirmarEdicion.onClick.RemoveAllListeners();
            btnConfirmarEdicion.onClick.AddListener(GuardarNuevoNombreTablero);
        }

        if (btnCancelarEdicion != null)
        {
            btnCancelarEdicion.onClick.RemoveAllListeners();
            btnCancelarEdicion.onClick.AddListener(CerrarModalEditar);
        }

        // 3. Modal de Confirmación de Eliminación
        if (modalConfirmarEliminar != null)
            modalConfirmarEliminar.SetActive(false);

        if (btnConfirmarEliminarSi != null)
        {
            btnConfirmarEliminarSi.onClick.RemoveAllListeners();
            btnConfirmarEliminarSi.onClick.AddListener(EjecutarBorradoFinal);
        }

        if (btnConfirmarEliminarNo != null)
        {
            btnConfirmarEliminarNo.onClick.RemoveAllListeners();
            btnConfirmarEliminarNo.onClick.AddListener(CerrarModalConfirmarEliminar);
        }
    }

    public void VerificarSesionUsuario()
    {
        if (FirebaseAuthREST.Instance == null || string.IsNullOrEmpty(FirebaseAuthREST.Instance.CurrentUserId))
        {
            if (panelLogin != null) panelLogin.SetActive(true);
            if (textoUsuarioConectado != null) textoUsuarioConectado.text = "Inicia sesión";
            if (textoErrorLogin != null) textoErrorLogin.text = "";
        }
        else
        {
            if (panelLogin != null) panelLogin.SetActive(false);
            if (textoUsuarioConectado != null)
                textoUsuarioConectado.text = "Usuario: " + FirebaseAuthREST.Instance.CurrentUserEmail;

            CargarListaTableros();
        }
    }

    public void BotonIniciarSesion()
    {
        if (textoErrorLogin != null) textoErrorLogin.text = "";

        string correo = inputCorreo != null ? inputCorreo.text.Trim() : "";
        string pass = inputPassword != null ? inputPassword.text.Trim() : "";

        if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(pass))
        {
            MostrarErrorLogin("Ingresa tu correo y contraseña.");
            return;
        }

        if (pass.Length < 6)
        {
            MostrarErrorLogin("La contraseña debe tener mínimo 6 caracteres.");
            return;
        }

        if (FirebaseAuthREST.Instance == null)
        {
            MostrarErrorLogin("Error: Falta componente FirebaseAuthREST.");
            return;
        }

        MostrarErrorLogin("Conectando con Firebase...");

        FirebaseAuthREST.Instance.IniciarOSuscribir(correo, pass, (exito, respuesta) =>
        {
            if (exito)
            {
                if (textoErrorLogin != null) textoErrorLogin.text = "";
                if (panelLogin != null) panelLogin.SetActive(false);

                if (textoUsuarioConectado != null)
                    textoUsuarioConectado.text = "Usuario: " + FirebaseAuthREST.Instance.CurrentUserEmail;

                CargarListaTableros();
            }
            else
            {
                MostrarErrorLogin(TraducirErrorFirebase(respuesta));
            }
        });
    }

    private void MostrarErrorLogin(string mensaje)
    {
        if (textoErrorLogin != null)
            textoErrorLogin.text = mensaje;
        else if (textoEstado != null)
            textoEstado.text = mensaje;
    }

    private string TraducirErrorFirebase(string jsonOError)
    {
        if (string.IsNullOrEmpty(jsonOError)) return "Error de conexión.";
        if (jsonOError.Contains("INVALID_PASSWORD") || jsonOError.Contains("INVALID_LOGIN_CREDENTIALS"))
            return "Contraseña incorrecta.";
        if (jsonOError.Contains("EMAIL_NOT_FOUND"))
            return "El correo no está registrado.";
        if (jsonOError.Contains("INVALID_EMAIL"))
            return "Formato de correo no válido.";
        if (jsonOError.Contains("EMAIL_EXISTS"))
            return "Credenciales incorrectas.";
        if (jsonOError.Contains("WEAK_PASSWORD"))
            return "Contraseña demasiado débil (mínimo 6 caracteres).";
        if (jsonOError.Contains("TOO_MANY_ATTEMPTS_TRY_LATER"))
            return "Demasiados intentos. Intenta más tarde.";

        return "Error al autenticar. Revisa tus datos.";
    }

    public void CerrarSesion()
    {
        if (FirebaseAuthREST.Instance != null)
            FirebaseAuthREST.Instance.CerrarSesion();

        if (inputCorreo != null) inputCorreo.text = "";
        if (inputPassword != null) inputPassword.text = "";
        if (textoErrorLogin != null) textoErrorLogin.text = "";

        if (contentLista != null)
        {
            foreach (Transform child in contentLista)
                Destroy(child.gameObject);
        }

        VerificarSesionUsuario();
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
        foreach (Transform child in contentLista)
            Destroy(child.gameObject);

        if (textoEstado != null) textoEstado.text = "Cargando tableros...";

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
                if (textoEstado != null) textoEstado.text = "No se encontraron tableros.";
                return;
            }

            foreach (var t in lista)
            {
                GameObject nuevoBoton = Instantiate(itemTableroPrefab, contentLista);
                nuevoBoton.transform.localScale = Vector3.one;

                TableroData datos = t;

                TableroItemUI itemUI = nuevoBoton.GetComponent<TableroItemUI>();
                if (itemUI != null)
                {
                    itemUI.Configurar(datos, this);
                }
                else
                {
                    TextMeshProUGUI tmp = nuevoBoton.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null)
                        tmp.text = datos.nombre;

                    Button btn = nuevoBoton.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => AbrirModalOpciones(datos));
                    }
                }
            }
        });
    }

    // ---------------------------------------------------------
    // CONTROL DE MODALES (OPCIONES, EDITAR, ELIMINAR)
    // ---------------------------------------------------------

    public void AbrirModalOpciones(TableroData tablero)
    {
        tableroSeleccionado = tablero;

        if (textoTituloTableroSeleccionado != null)
            textoTituloTableroSeleccionado.text = tablero.nombre;

        if (modalOpciones != null)
            modalOpciones.SetActive(true);
    }

    public void CerrarModalOpciones()
    {
        if (modalOpciones != null)
            modalOpciones.SetActive(false);
    }

    private void EjecutarAbrirTablero()
    {
        if (tableroSeleccionado == null) return;

        CerrarModalOpciones();
        FirebaseManager.TableroActivo = tableroSeleccionado;
        SceneManager.LoadScene("ARScene");
    }

    private void EjecutarAbrirModalEditar()
    {
        if (tableroSeleccionado == null) return;

        CerrarModalOpciones();

        if (inputEditarNombre != null)
        {
            inputEditarNombre.text = tableroSeleccionado.nombre;
            inputEditarNombre.ActivateInputField();
        }

        if (modalEditar != null)
            modalEditar.SetActive(true);
    }

    public void CerrarModalEditar()
    {
        if (modalEditar != null)
            modalEditar.SetActive(false);
    }

    private void GuardarNuevoNombreTablero()
    {
        if (tableroSeleccionado == null) return;

        string nuevoNombre = inputEditarNombre != null ? inputEditarNombre.text.Trim() : "";

        if (string.IsNullOrEmpty(nuevoNombre))
        {
            if (textoEstado != null) textoEstado.text = "El nombre no puede estar vacío.";
            return;
        }

        CerrarModalEditar();

        if (textoEstado != null) textoEstado.text = "Actualizando nombre...";

        StartCoroutine(FirebaseManager.Instance.ActualizarNombreTablero(tableroSeleccionado.id, nuevoNombre, (exito) =>
        {
            if (exito)
            {
                if (textoEstado != null) textoEstado.text = "Nombre actualizado.";
                CargarListaTableros();
            }
            else
            {
                if (textoEstado != null) textoEstado.text = "Error al actualizar nombre.";
            }
        }));
    }

    // --- FLUJO DE CONFIRMACIÓN DE ELIMINACIÓN ---

    private void AbrirModalConfirmarEliminar()
    {
        if (tableroSeleccionado == null) return;

        // Cierra el modal de opciones y abre el modal de confirmación
        CerrarModalOpciones();

        if (textoAdvertenciaEliminar != null)
            textoAdvertenciaEliminar.text = $"¿Seguro que deseas eliminar \"{tableroSeleccionado.nombre}\"? Esta acción no se puede deshacer.";

        if (modalConfirmarEliminar != null)
            modalConfirmarEliminar.SetActive(true);
    }

    public void CerrarModalConfirmarEliminar()
    {
        if (modalConfirmarEliminar != null)
            modalConfirmarEliminar.SetActive(false);
    }

    private void EjecutarBorradoFinal()
    {
        if (tableroSeleccionado == null) return;

        string idABorrar = tableroSeleccionado.id;
        CerrarModalConfirmarEliminar();

        if (textoEstado != null) textoEstado.text = "Eliminando tablero...";

        StartCoroutine(FirebaseManager.Instance.EliminarTablero(idABorrar, (exito) =>
        {
            if (exito)
            {
                if (textoEstado != null) textoEstado.text = "Tablero eliminado.";
                CargarListaTableros();
            }
            else
            {
                if (textoEstado != null) textoEstado.text = "Error al eliminar tablero.";
            }
        }));
    }

    // ---------------------------------------------------------
    // PARSER JSON MANUAL
    // ---------------------------------------------------------

    private List<TableroData> ExtraerTablerosDeJson(string json)
    {
        List<TableroData> resultado = new List<TableroData>();

        try
        {
            string marcador = "\"id\"";
            int indiceActual = 0;

            while ((indiceActual = json.IndexOf(marcador, indiceActual)) != -1)
            {
                int inicioObjeto = json.LastIndexOf('{', indiceActual);
                if (inicioObjeto != -1)
                {
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