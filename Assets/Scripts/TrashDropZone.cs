using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrashDropZone : MonoBehaviour
{
    public static TrashDropZone Instance;

    [Header("Indicador Visual Circular")]
    [SerializeField] private Image circuloRojo; // Imagen circular con alpha
    [SerializeField] private Color colorNormal = new Color(1f, 0f, 0f, 0f);       // Invisible
    [SerializeField] private Color colorHover = new Color(1f, 0f, 0f, 0.45f);     // Rojo semitransparente
    [SerializeField] private Color colorEliminado = new Color(1f, 0f, 0f, 0.85f); // Destello al borrar

    [Header("Notificación / Advertencia")]
    [SerializeField] private GameObject panelNotificacion;
    [SerializeField] private TextMeshProUGUI textoNotificacion;
    [SerializeField] private float duracionMensaje = 2.5f;

    private Coroutine corrutinaMensaje;

    void Awake()
    {
        Instance = this;

        if (circuloRojo != null)
            circuloRojo.color = colorNormal;

        if (panelNotificacion != null)
            panelNotificacion.SetActive(false);
    }

    /// <summary>
    /// Llamado cuando la nota entra al radio de la papelera mientras se arrastra.
    /// </summary>
    public void SobrePapelera(bool encima)
    {
        if (circuloRojo == null) return;

        StopAllCoroutines();
        StartCoroutine(AnimarAlphaCirculo(encima ? colorHover : colorNormal, 0.2f));
    }

    /// <summary>
    /// Llamado al soltar la nota dentro de la papelera.
    /// </summary>
    public void ConfirmarEliminacionNota(string textoNota = "")
    {
        // 1. Destello rojo en el círculo
        StopAllCoroutines();
        StartCoroutine(EfectoDestello());

        // 2. Mostrar mensaje de advertencia
        MostrarMensaje("La nota se borró correctamente.");
    }

    private IEnumerator AnimarAlphaCirculo(Color destino, float duracion)
    {
        Color inicial = circuloRojo.color;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            circuloRojo.color = Color.Lerp(inicial, destino, tiempo / duracion);
            yield return null;
        }

        circuloRojo.color = destino;
    }

    private IEnumerator EfectoDestello()
    {
        if (circuloRojo != null)
        {
            circuloRojo.color = colorEliminado;
            yield return new WaitForSeconds(0.25f);
            yield return AnimarAlphaCirculo(colorNormal, 0.35f);
        }
    }

    public void MostrarMensaje(string mensaje)
    {
        if (corrutinaMensaje != null)
            StopCoroutine(corrutinaMensaje);

        corrutinaMensaje = StartCoroutine(RutinaMostrarMensaje(mensaje));
    }

    private IEnumerator RutinaMostrarMensaje(string mensaje)
    {
        if (panelNotificacion != null && textoNotificacion != null)
        {
            textoNotificacion.text = mensaje;
            panelNotificacion.SetActive(true);

            yield return new WaitForSeconds(duracionMensaje);

            panelNotificacion.SetActive(false);
        }
    }

    // --- Detección mediante Trigger (si usas Colliders 2D o 3D) ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NotaKanban"))
            SobrePapelera(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NotaKanban"))
            SobrePapelera(false);
    }
}