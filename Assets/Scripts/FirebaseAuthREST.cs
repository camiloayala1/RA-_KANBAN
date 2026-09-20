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

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
                // Si la cuenta no existe, la crea de inmediato
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
                CurrentUserId = res.localId;
                CurrentUserEmail = res.email;
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
                CurrentUserId = res.localId;
                CurrentUserEmail = res.email;
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
    }
}
