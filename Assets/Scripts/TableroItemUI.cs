using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TableroItemUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TMP_Text txtNombreTablero;
    [SerializeField] private Button btnSeleccionar;

    private TableroData datosTablero;
    private MainMenuController controller;

    public void Configurar(TableroData datos, MainMenuController mainController)
    {
        datosTablero = datos;
        controller = mainController;

        if (txtNombreTablero != null)
            txtNombreTablero.text = datos.nombre;

        if (btnSeleccionar != null)
        {
            btnSeleccionar.onClick.RemoveAllListeners();
            // Al presionar el botón en la lista, abre el modal con las 3 opciones
            btnSeleccionar.onClick.AddListener(() => controller.AbrirModalOpciones(datosTablero));
        }
    }
}