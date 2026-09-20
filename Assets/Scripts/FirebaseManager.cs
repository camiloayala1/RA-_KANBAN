using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    public static TableroData TableroActivo = null;

    // Reemplaza por la URL de tu base de datos copiada de Firebase Console (incluyendo la diagonal final)
    private string databaseURL = "https://ra-kanban-app-default-rtdb.firebaseio.com/";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void GuardarTablero(TableroData tablero, Action<bool> callback)
    {
        StartCoroutine(RutinaGuardar(tablero, callback));
    }

    private IEnumerator RutinaGuardar(TableroData tablero, Action<bool> callback)
    {
        string url = $"{databaseURL}tableros/{tablero.id}.json";
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
        string url = $"{databaseURL}tableros.json";

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