using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class AuthController : MonoBehaviour
{
    public static AuthController Instance;

    [Header("Paneles de Navegación")]
    public GameObject subPanelLogin;
    public GameObject subPanelRegistro;

    [Header("Tabs Superiores")]
    public Button tabBtnLogin;
    public Button tabBtnRegistro;
    public TextMeshProUGUI tabTextoLogin;
    public TextMeshProUGUI tabTextoRegistro;
    public Image tabFondoLogin;
    public Image tabFondoRegistro;

    [Header("Inputs - Iniciar Sesión")]
    public TMP_InputField inputLoginEmail;
    public TMP_InputField inputLoginPassword;
    public TextMeshProUGUI txtErrorLogin;
    public Button btnSubmitLogin;
    public Button btnIrARegistroAbajo;

    [Header("Inputs - Registrarse")]
    public TMP_InputField inputRegEmail;
    public TMP_InputField inputRegPassword;
    public TMP_InputField inputRegConfirmPassword;
    public TextMeshProUGUI txtErrorRegistro;
    public TextMeshProUGUI txtExitoRegistro;
    public Button btnSubmitRegistro;
    public Button btnIrALoginAbajo;

    [Header("Estado de Carga / Spinner")]
    public GameObject panelCargando;
    public TextMeshProUGUI txtEstadoCarga;

    // Colores 2027 FUTURE
    private readonly Color colorTabActivo = new Color(0.00f, 0.95f, 1.00f, 1f);     // Cian Neón
    private readonly Color colorTabInactivo = new Color(0.58f, 0.65f, 0.74f, 1f);   // Gris Muted
    private readonly Color colorFondoActivo = new Color(0.08f, 0.18f, 0.28f, 0.95f);
    private readonly Color colorFondoInactivo = new Color(0.05f, 0.08f, 0.13f, 0.60f);

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ConfigurarBotones();
        VerificarSesionAutomatica();
    }

    private void ConfigurarBotones()
    {
        if (tabBtnLogin != null)
        {
            tabBtnLogin.onClick.RemoveAllListeners();
            tabBtnLogin.onClick.AddListener(MostrarTabLogin);
        }

        if (tabBtnRegistro != null)
        {
            tabBtnRegistro.onClick.RemoveAllListeners();
            tabBtnRegistro.onClick.AddListener(MostrarTabRegistro);
        }

        if (btnSubmitLogin != null)
        {
            btnSubmitLogin.onClick.RemoveAllListeners();
            btnSubmitLogin.onClick.AddListener(EjecutarLogin);
        }

        if (btnSubmitRegistro != null)
        {
            btnSubmitRegistro.onClick.RemoveAllListeners();
            btnSubmitRegistro.onClick.AddListener(EjecutarRegistro);
        }

        if (btnIrARegistroAbajo != null)
        {
            btnIrARegistroAbajo.onClick.RemoveAllListeners();
            btnIrARegistroAbajo.onClick.AddListener(MostrarTabRegistro);
        }

        if (btnIrALoginAbajo != null)
        {
            btnIrALoginAbajo.onClick.RemoveAllListeners();
            btnIrALoginAbajo.onClick.AddListener(MostrarTabLogin);
        }
    }

    public void VerificarSesionAutomatica()
    {
        // Si ya hay sesión guardada en PlayerPrefs, ingresar directo al MainMenu
        if (FirebaseAuthREST.Instance != null && !string.IsNullOrEmpty(FirebaseAuthREST.Instance.CurrentUserId))
        {
            Debug.Log($"[AuthController] Sesión activa detectada ({FirebaseAuthREST.Instance.CurrentUserEmail}). Entrando a MainMenu...");
            if (panelCargando != null) panelCargando.SetActive(true);
            if (txtEstadoCarga != null) txtEstadoCarga.text = "Iniciando sesión...";
            SceneManager.LoadScene("MainMenu");
            return;
        }

        // Si no hay sesión, iniciar en la pestaña Login
        MostrarTabLogin();
    }

    public void MostrarTabLogin()
    {
        if (subPanelLogin != null) subPanelLogin.SetActive(true);
        if (subPanelRegistro != null) subPanelRegistro.SetActive(false);

        // Feedback visual en Tabs
        if (tabTextoLogin != null)
        {
            tabTextoLogin.color = colorTabActivo;
            tabTextoLogin.fontStyle = FontStyles.Bold;
        }
        if (tabTextoRegistro != null)
        {
            tabTextoRegistro.color = colorTabInactivo;
            tabTextoRegistro.fontStyle = FontStyles.Normal;
        }

        if (tabFondoLogin != null) tabFondoLogin.color = colorFondoActivo;
        if (tabFondoRegistro != null) tabFondoRegistro.color = colorFondoInactivo;

        LimpiarMensajes();
    }

    public void MostrarTabRegistro()
    {
        if (subPanelLogin != null) subPanelLogin.SetActive(false);
        if (subPanelRegistro != null) subPanelRegistro.SetActive(true);

        // Feedback visual en Tabs
        if (tabTextoLogin != null)
        {
            tabTextoLogin.color = colorTabInactivo;
            tabTextoLogin.fontStyle = FontStyles.Normal;
        }
        if (tabTextoRegistro != null)
        {
            tabTextoRegistro.color = colorTabActivo;
            tabTextoRegistro.fontStyle = FontStyles.Bold;
        }

        if (tabFondoLogin != null) tabFondoLogin.color = colorFondoInactivo;
        if (tabFondoRegistro != null) tabFondoRegistro.color = colorFondoActivo;

        LimpiarMensajes();
    }

    private void LimpiarMensajes()
    {
        if (txtErrorLogin != null) txtErrorLogin.text = "";
        if (txtErrorRegistro != null) txtErrorRegistro.text = "";
        if (txtExitoRegistro != null) txtExitoRegistro.text = "";
    }

    public void EjecutarLogin()
    {
        LimpiarMensajes();

        string email = inputLoginEmail != null ? inputLoginEmail.text.Trim() : "";
        string pass = inputLoginPassword != null ? inputLoginPassword.text.Trim() : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            MostrarErrorLogin("Ingresa tu correo y contraseña.");
            return;
        }

        if (!email.Contains("@") || !email.Contains("."))
        {
            MostrarErrorLogin("Ingresa un formato de correo válido.");
            return;
        }

        if (pass.Length < 6)
        {
            MostrarErrorLogin("La contraseña debe tener mínimo 6 caracteres.");
            return;
        }

        if (FirebaseAuthREST.Instance == null)
        {
            MostrarErrorLogin("Error interno: FirebaseAuthREST no encontrado.");
            return;
        }

        SetCargando(true, "AUTENTICANDO // CONECTANDO...");

        FirebaseAuthREST.Instance.IniciarSesion(email, pass, (exito, respuesta) =>
        {
            SetCargando(false);
            if (exito)
            {
                Debug.Log("[AuthController] Login exitoso. Cargando MainMenu...");
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                MostrarErrorLogin(TraducirErrorFirebase(respuesta));
            }
        });
    }

    public void EjecutarRegistro()
    {
        LimpiarMensajes();

        string email = inputRegEmail != null ? inputRegEmail.text.Trim() : "";
        string pass = inputRegPassword != null ? inputRegPassword.text.Trim() : "";
        string confirmPass = inputRegConfirmPassword != null ? inputRegConfirmPassword.text.Trim() : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            MostrarErrorRegistro("Completa todos los campos requeridos.");
            return;
        }

        if (!email.Contains("@") || !email.Contains("."))
        {
            MostrarErrorRegistro("Ingresa un correo electrónico válido.");
            return;
        }

        if (pass.Length < 6)
        {
            MostrarErrorRegistro("La contraseña debe tener mínimo 6 caracteres.");
            return;
        }

        if (pass != confirmPass)
        {
            MostrarErrorRegistro("Las contraseñas no coinciden.");
            return;
        }

        if (FirebaseAuthREST.Instance == null)
        {
            MostrarErrorRegistro("Error interno: FirebaseAuthREST no encontrado.");
            return;
        }

        SetCargando(true, "CREANDO CUENTA // REGISTRANDO...");

        FirebaseAuthREST.Instance.Registrarse(email, pass, (exito, respuesta) =>
        {
            SetCargando(false);
            if (exito)
            {
                Debug.Log("[AuthController] Registro exitoso. Entrando a MainMenu...");
                if (txtExitoRegistro != null) txtExitoRegistro.text = "¡Cuenta creada exitosamente!";
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                MostrarErrorRegistro(TraducirErrorFirebase(respuesta));
            }
        });
    }

    private void SetCargando(bool cargando, string mensaje = "")
    {
        if (panelCargando != null)
            panelCargando.SetActive(cargando);

        if (txtEstadoCarga != null)
            txtEstadoCarga.text = mensaje;
    }

    private void MostrarErrorLogin(string mensaje)
    {
        if (txtErrorLogin != null) txtErrorLogin.text = mensaje;
    }

    private void MostrarErrorRegistro(string mensaje)
    {
        if (txtErrorRegistro != null) txtErrorRegistro.text = mensaje;
    }

    private string TraducirErrorFirebase(string jsonOError)
    {
        if (string.IsNullOrEmpty(jsonOError)) return "Error de conexión con el servidor.";
        if (jsonOError.Contains("INVALID_PASSWORD") || jsonOError.Contains("INVALID_LOGIN_CREDENTIALS"))
            return "Contraseña o credenciales incorrectas.";
        if (jsonOError.Contains("EMAIL_NOT_FOUND"))
            return "El correo ingresado no está registrado.";
        if (jsonOError.Contains("INVALID_EMAIL"))
            return "Formato de correo no válido.";
        if (jsonOError.Contains("EMAIL_EXISTS"))
            return "Este correo ya está registrado. Por favor inicia sesión.";
        if (jsonOError.Contains("WEAK_PASSWORD"))
            return "Contraseña muy débil (mínimo 6 caracteres).";
        if (jsonOError.Contains("TOO_MANY_ATTEMPTS_TRY_LATER"))
            return "Demasiados intentos erróneos. Intenta más tarde.";
        if (jsonOError.Contains("USER_DISABLED"))
            return "Esta cuenta ha sido deshabilitada.";

        return "Error al autenticar. Revisa tus datos.";
    }
}
