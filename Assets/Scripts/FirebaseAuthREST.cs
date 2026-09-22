using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class AuthResponse
{
    public string idToken;
    public string email;
    public string refreshToken;
    public string expiresIn;
    public string localId; // UID único del usuario en Firebase
}

public class FirebaseAuthREST : MonoBehaviour
{
    public static FirebaseAuthREST Instance;

    [Header("Firebase Config")]
    [Tooltip("Browser key de Google Cloud")]
    public string apiKey = "AIzaSyCDMnoaIRLsS8R24aB7p3LYMpotWQYpqyk";

    public string CurrentUserId { get; private set; }
    public string CurrentUserEmail { get; private set; }

    private const string PREF_UID_KEY = "FB_USER_UID";
    private const string PREF_EMAIL_KEY = "FB_USER_EMAIL";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CargarSesionGuardada();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Recupera la sesión anterior si el usuario no cerró sesión manualmente.
    /// </summary>
    private void CargarSesionGuardada()
    {
        if (PlayerPrefs.HasKey(PREF_UID_KEY))
        {
            CurrentUserId = PlayerPrefs.GetString(PREF_UID_KEY);
            CurrentUserEmail = PlayerPrefs.GetString(PREF_EMAIL_KEY, "");
            Debug.Log($"[FirebaseAuthREST] Sesión recuperada para: {CurrentUserEmail} ({CurrentUserId})");
        }
    }

    private void GuardarSesion(string uid, string email)
    {
        CurrentUserId = uid;
        CurrentUserEmail = email;

        PlayerPrefs.SetString(PREF_UID_KEY, uid);
        PlayerPrefs.SetString(PREF_EMAIL_KEY, email);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Intenta iniciar sesión; si la cuenta no existe en Firebase, la crea automáticamente.
    /// </summary>
    public void IniciarOSuscribir(string email, string password, Action<bool, string> callback)
    {
        StartCoroutine(RutinaLogin(email, password, (exitoLogin, respLogin) =>
        {
            if (exitoLogin)
            {
                callback?.Invoke(true, CurrentUserEmail);
            }
            else
            {
                // Si falla el login (por ejemplo cuenta nueva), intenta el registro
                StartCoroutine(RutinaRegistro(email, password, (exitoRegistro, respRegistro) =>
                {
                    if (exitoRegistro)
                    {
                        callback?.Invoke(true, CurrentUserEmail);
                    }
                    else
                    {
                        callback?.Invoke(false, respRegistro);
                    }
                }));
            }
        }));
    }

    private IEnumerator RutinaLogin(string email, string password, Action<bool, string> callback)
    {
        string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";
        string payload = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                AuthResponse res = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
                GuardarSesion(res.localId, res.email);
                callback?.Invoke(true, CurrentUserId);
            }
            else
            {
                callback?.Invoke(false, req.downloadHandler.text);
            }
        }
    }

    private IEnumerator RutinaRegistro(string email, string password, Action<bool, string> callback)
    {
        string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}";
        string payload = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                AuthResponse res = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
                GuardarSesion(res.localId, res.email);
                callback?.Invoke(true, CurrentUserId);
            }
            else
            {
                callback?.Invoke(false, req.downloadHandler.text);
            }
        }
    }

    public void CerrarSesion()
    {
        CurrentUserId = "";
        CurrentUserEmail = "";

        PlayerPrefs.DeleteKey(PREF_UID_KEY);
        PlayerPrefs.DeleteKey(PREF_EMAIL_KEY);
        PlayerPrefs.Save();

        Debug.Log("[FirebaseAuthREST] Sesión cerrada y eliminada de PlayerPrefs.");
    }
}