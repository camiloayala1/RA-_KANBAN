using System;
using System.Collections.Generic;

[Serializable]
public class NotaData
{
    public string id;
    public string texto;
    public string columna; // "Columna_Pendiente", "Columna_EnProceso", "Columna_Terminado"
}

[Serializable]
public class TableroData
{
    public string id;
    public string nombre;
    public List<NotaData> notas = new List<NotaData>();
}