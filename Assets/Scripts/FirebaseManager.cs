using System;
using System.Collections;
using System.Collections.Generic;
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
}