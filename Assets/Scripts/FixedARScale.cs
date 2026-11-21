using UnityEngine;

/// <summary>
/// Bloquea estrictamente la escala local del tablero en el espacio de Realidad Aumentada,
/// garantizando que al acercarse o alejarse el usuario no cambie de tamaño ni se distorsione,
/// permaneciendo a su medida física establecida en metros (1.0m x 0.6m).
/// </summary>
public class FixedARScale : MonoBehaviour
{
    [Header("Escala Física Constante")]
    [Tooltip("Escala local fija en metros (ej. 0.001 para Canvas de 1000x600 px = 1.0m x 0.6m)")]
    public Vector3 fixedScale = new Vector3(0.001f, 0.001f, 0.001f);

    void Awake()
    {
        transform.localScale = fixedScale;
    }

    void LateUpdate()
    {
        if (transform.localScale != fixedScale)
        {
            transform.localScale = fixedScale;
        }
    }
}
