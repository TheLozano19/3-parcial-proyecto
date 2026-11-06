using UnityEngine;

public class listaligada : MonoBehaviour
{
    // Clase interna pública sin corchetes extra
    public class NodoWaypoint
    {
        public string nombreWaypoint;
        public Vector3 posicion;
        public GameObject objeto;
        public NodoWaypoint siguiente;

        public NodoWaypoint(string nombre, Vector3 pos, GameObject obj)
        {
            nombreWaypoint = nombre;
            posicion = pos;
            objeto = obj;
            siguiente = null;
        }
    }

    [Header("Configuración Visual")]
    public GameObject Waypoint;

    [Header("Generación Aleatoria")]
    public int nodosIniciales = 3;
    public float rangoGeneracion = 10.0f;

    [Header("Estado de la Lista")]
    public NodoWaypoint inicioLista;
    public int contadorNodos = 1;

    void Start()
    {
        for (int i = 0; i < nodosIniciales; i++)
        {
            AgregarNodoAleatorio();
        }
    }

    public void AgregarNodoAleatorio()
    {
       
        Vector3 Aleatoria = new Vector3(
            Random.Range(-rangoGeneracion, rangoGeneracion), 
            Random.Range(-rangoGeneracion, rangoGeneracion), 
            0f                                              
        );

        Agregar(Aleatoria);
    }
    public void Agregar(Vector3 posicion)
    {
        GameObject objVisual = null;
        if (Waypoint != null)
        {
            objVisual = Instantiate(Waypoint, posicion, Quaternion.identity);
            objVisual.name = "Waypoint_" + contadorNodos;
        }

        NodoWaypoint nuevoNodo = new NodoWaypoint("Punto " + contadorNodos, posicion, objVisual);
        contadorNodos++;

        if (inicioLista == null)
        {
            inicioLista = nuevoNodo;
        }
        else
        {
            NodoWaypoint actual = inicioLista;
            while (actual.siguiente != null)
            {
                actual = actual.siguiente;
            }
            actual.siguiente = nuevoNodo;
        }
    }

    public void Sacar(ref NodoWaypoint objetivoActual)
    {
        if (inicioLista == null) return;

        if (inicioLista.siguiente == null)
        {
            if (inicioLista.objeto != null) Destroy(inicioLista.objeto);
            inicioLista = null;
            objetivoActual = null;
            return;
        }

        NodoWaypoint actual = inicioLista;
        while (actual.siguiente.siguiente != null)
        {
            actual = actual.siguiente;
        }

        if (actual.siguiente.objeto != null)
        {
            Destroy(actual.siguiente.objeto);
        }

        if (objetivoActual == actual.siguiente)
        {
            objetivoActual = actual;
        }

        actual.siguiente = null;
    }

    public void VoltearLista()
    {
        if (inicioLista == null || inicioLista.siguiente == null) return;

        NodoWaypoint previo = null;
        NodoWaypoint actual = inicioLista;
        NodoWaypoint siguiente = null;

        while (actual != null)
        {
            siguiente = actual.siguiente;
            actual.siguiente = previo;
            previo = actual;
            actual = siguiente;
        }

        inicioLista = previo;
    }

    private void OnDrawGizmos()
    {

        if (inicioLista == null) return;

        Gizmos.color = Color.cyan;
        NodoWaypoint actual = inicioLista;
        while (actual != null && actual.siguiente != null)
        {
            Gizmos.DrawLine(actual.posicion, actual.siguiente.posicion);
            actual = actual.siguiente;
        }
    }
}