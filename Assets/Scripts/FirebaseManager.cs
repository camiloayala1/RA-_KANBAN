using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    public static TableroData TableroActivo = null;

    [Header("Configuración Firebase")]
    [Tooltip("URL base de tu Realtime Database")]
    public string databaseURL = "https://ra-kanban-app-default-rtdb.firebaseio.com/";

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
    /// Genera la ruta aislada en Firebase para el usuario actual (/usuarios/{UID}/).
    /// </summary>
    private string ObtenerRutaUsuario()
    {
        string uid = "invitado";
        if (FirebaseAuthREST.Instance != null && !string.IsNullOrEmpty(FirebaseAuthREST.Instance.CurrentUserId))
        {
            uid = FirebaseAuthREST.Instance.CurrentUserId;
        }
        return $"{databaseURL}usuarios/{uid}/";
    }

    public void GuardarTablero(TableroData tablero, Action<bool> callback)
    {
        StartCoroutine(RutinaGuardar(tablero, callback));
    }

    private IEnumerator RutinaGuardar(TableroData tablero, Action<bool> callback)
    {
        string url = $"{ObtenerRutaUsuario()}tableros/{tablero.id}.json";
        string jsonData = JsonUtility.ToJson(tablero);

        using (UnityWebRequest req = UnityWebRequest.Put(url, jsonData))
        {
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                callback?.Invoke(true);
            }
            else
            {
                Debug.LogError("Error al guardar en Firebase: " + req.error);
                callback?.Invoke(false);
            }
        }
    }

    public void ObtenerTableros(Action<string> callback)
    {
        StartCoroutine(RutinaObtenerTableros(callback));
    }

    private IEnumerator RutinaObtenerTableros(Action<string> callback)
    {
        string url = $"{ObtenerRutaUsuario()}tableros.json";

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                callback?.Invoke(req.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error al obtener tableros: " + req.error);
                callback?.Invoke(null);
            }
        }
    }


    /// <summary>
    /// Actualiza únicamente el título de un tablero en Firebase.
    /// </summary>
    public IEnumerator ActualizarNombreTablero(string tableroId, string nuevoTitulo, System.Action<bool> onComplete)
    {
        string token = PlayerPrefs.GetString("fb_token", "");
        string uid = PlayerPrefs.GetString("fb_uid", "");

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(uid))
        {
            Debug.LogError("FirebaseManager: Sesión no válida para actualizar.");
            onComplete?.Invoke(false);
            yield break;
        }

        string url = $"{databaseURL}/usuarios/{uid}/tableros/{tableroId}/titulo.json?auth={token}";
        
        // Serializamos el string como JSON válido
        string jsonBody = $"\"{nuevoTitulo}\"";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Tablero {tableroId} actualizado exitosamente.");
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError($"Error al actualizar tablero: {request.error} | {request.downloadHandler.text}");
                onComplete?.Invoke(false);
            }
        }
    }

    /// <summary>
    /// Elimina el nodo completo de un tablero en Firebase.
    /// </summary>
    public IEnumerator EliminarTablero(string tableroId, System.Action<bool> onComplete)
    {
        string token = PlayerPrefs.GetString("fb_token", "");
        string uid = PlayerPrefs.GetString("fb_uid", "");

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(uid))
        {
            Debug.LogError("FirebaseManager: Sesión no válida para eliminar.");
            onComplete?.Invoke(false);
            yield break;
        }

        string url = $"{databaseURL}/usuarios/{uid}/tableros/{tableroId}.json?auth={token}";

        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Tablero {tableroId} eliminado de la base de datos.");
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError($"Error al eliminar tablero: {request.error} | {request.downloadHandler.text}");
                onComplete?.Invoke(false);
            }
        }
    }
}