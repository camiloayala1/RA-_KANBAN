using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class GoogleAuthResponse
{
    public string idToken;
    public string localId; // UID único del usuario
    public string email;
    public string displayName;
}

public class FirebaseAuthREST : MonoBehaviour
{
    public static FirebaseAuthREST Instance;

    [Header("Firebase Config")]
    [Tooltip("Pega aquí la Web API Key de Configuración del Proyecto en Firebase")]
    public string apiKey = "AIzaSyCDMnoaIRLsS8R24aB7p3LYMpotWQYpqyk";

    public string CurrentUserId { get; private set; }
    public string CurrentIdToken { get; private set; }
    public string CurrentUserName { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Intercambia un ID Token de Google por credenciales de Firebase Auth.
    /// </summary>
    public void IniciarSesionConGoogleIdToken(string googleIdToken, Action<bool, string> callback)
    {
        StartCoroutine(RutinaLoginGoogle(googleIdToken, callback));
    }

    private IEnumerator RutinaLoginGoogle(string googleIdToken, Action<bool, string> callback)
    {
        string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key={apiKey}";

        // Estructura requerida por Firebase Auth REST API
        string payload = "{\"postBody\":\"id_token=" + googleIdToken + "&providerId=google.com\",\"requestUri\":\"http://localhost\",\"returnIdpCredential\":true,\"returnSecureToken\":true}";

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                GoogleAuthResponse resp = JsonUtility.FromJson<GoogleAuthResponse>(req.downloadHandler.text);
                CurrentUserId = resp.localId;
                CurrentIdToken = resp.idToken;
                CurrentUserName = resp.displayName;

                callback?.Invoke(true, resp.displayName);
            }
            else
            {
                Debug.LogError("Error en Firebase Auth: " + req.downloadHandler.text);
                callback?.Invoke(false, req.error);
            }
        }
    }

    /// <summary>
    /// Modo de simulación rápida en editor (para probar la separación de usuarios sin compilar APK cada vez).
    /// </summary>
    public void IniciarSesionSimulada(string customUserId, string nombreVisible)
    {
        CurrentUserId = customUserId;
        CurrentUserName = nombreVisible;
    }
}