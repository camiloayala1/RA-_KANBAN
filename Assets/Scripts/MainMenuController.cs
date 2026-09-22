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
    public TextMeshProUGUI textoEstado; // Muestra "Cargando tableros...", etc.

    [Header("Panel Login y Usuario")]
    public GameObject panelLogin;
    public TMP_InputField inputCorreo;
    public TMP_InputField inputPassword;
    public TextMeshProUGUI textoErrorLogin;       // Mensajes rojos dentro del panel de login
    public TextMeshProUGUI textoUsuarioConectado; // Muestra el correo arriba del menú

    [Header("Lista ScrollView")]
    public Transform contentLista; // Objeto 'Content' del ScrollView
    public GameObject itemTableroPrefab; // Prefab 'ItemTablero_Btn'

    void Start()
    {
        VerificarSesionUsuario();
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

    /// <summary>
    /// Conectar al evento On Click del botón Ingresar_btn en el Login
    /// </summary>
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
            MostrarErrorLogin("Error: Falta componente FirebaseAuthREST en FirebaseManager.");
            return;
        }

        MostrarErrorLogin("Conectando con Firebase...");

        FirebaseAuthREST.Instance.IniciarOSuscribir(correo, pass, (exito, respuesta) =>
        {
            if (exito)
            {
                if (textoErrorLogin != null) textoErrorLogin.text = "";
                if (panelLogin != null) panelLogin.SetActive(false);

                // Muestra el correo conectado en la interfaz principal
                if (textoUsuarioConectado != null)
                    textoUsuarioConectado.text = "Usuario: " + FirebaseAuthREST.Instance.CurrentUserEmail;

                CargarListaTableros();
            }
            else
            {
                // Traducir respuestas comunes de Firebase Auth REST
                string mensajeTraducido = TraducirErrorFirebase(respuesta);
                MostrarErrorLogin(mensajeTraducido);
            }
        });
    }

    private void MostrarErrorLogin(string mensaje)
    {
        if (textoErrorLogin != null)
        {
            textoErrorLogin.text = mensaje;
        }
        else if (textoEstado != null)
        {
            textoEstado.text = mensaje;
        }
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
        {
            FirebaseAuthREST.Instance.CerrarSesion();
        }

        // 1. Limpiar los campos de texto del login
        if (inputCorreo != null) inputCorreo.text = "";
        if (inputPassword != null) inputPassword.text = "";
        if (textoErrorLogin != null) textoErrorLogin.text = "";

        // 2. Limpiar la lista visual de tableros
        if (contentLista != null)
        {
            foreach (Transform child in contentLista)
            {
                Destroy(child.gameObject);
            }
        }

        // 3. Volver a mostrar el panel de login
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
        {
            Destroy(child.gameObject);
        }

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

                TextMeshProUGUI tmp = nuevoBoton.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = t.nombre;
                }

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