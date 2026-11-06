using UnityEngine;
using UnityEngine.InputSystem;

public class AgenteFSM : MonoBehaviour
{
    public enum State
    {
        Idle,
        Moverse,
        Detenerse,
        CambioRuta
    }

    [Header("Referencias y Configuración")]
    public listaligada gestorLista;
    public State state = State.Idle;

    [Header("Configuración de Física y Movimiento")]
    public float maxSpeed = 6.0f;
    public float maxForce = 10.0f;
    public float mass = 1.0f;
    public float distanciaLlegada = 0.3f;

    [Header("Configuración de Arrival")]
    [Tooltip("Distancia a la cual el agente comenzará a frenar en el último nodo")]
    public float slowingRadius = 3.0f;

    [Header("Configuración de Obstáculos (Avoidance)")]
    public Transform[] obstacles;
    [Tooltip("Radio de detección de obstáculo")]
    public float AvoidanceRadius = 3.0f;
    [Tooltip("Intensidad de repulsión")]
    public float obstacleWeight = 2.0f;

    private Vector3 velocity;
    private listaligada.NodoWaypoint objetivoActual;

    void Start()
    {
        if (gestorLista == null)
        {
            gestorLista = GetComponent<listaligada>();
        }
    }

    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                OnIdle();
                break;
            case State.Moverse:
                OnMoverse();
                break;
            case State.Detenerse:
                OnDetenerse();
                break;
            case State.CambioRuta:
                OnCambioRuta();
                break;
        }
    }


    void OnIdle()
    {
        velocity = Vector3.zero;

        if (Movimiento())
        {
            state = State.Moverse;
        }
        if (ModificarRuta())
        {
            state = State.CambioRuta;
        }
    }

    void OnMoverse()
    {
        if (objetivoActual == null)
        {
            state = State.Detenerse;
            return;
        }

        
        SteeringBehaviors();

       
        float distanciaAlObjetivo = Vector3.Distance(transform.position, objetivoActual.posicion);
        if (distanciaAlObjetivo <= distanciaLlegada)
        {
            if (objetivoActual.siguiente == null)
            {
                objetivoActual = null;
                state = State.Detenerse;
            }
            else
            {
                objetivoActual = objetivoActual.siguiente;
            }
        }

        if (ModificarRuta())
        {
            state = State.CambioRuta;
        }
    }

    void OnDetenerse()
    {
        velocity = Vector3.zero;
        state = State.Idle;
    }

    void OnCambioRuta()
    {
        if (objetivoActual != null)
        {
            state = State.Moverse;
        }
        else
        {
            state = State.Idle;
        }
    }

   

    bool Movimiento()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (gestorLista != null && gestorLista.inicioLista != null)
            {
                objetivoActual = gestorLista.inicioLista;
                return true;
            }
        }
        return false;
    }

    bool ModificarRuta()
    {
        if (Keyboard.current == null || gestorLista == null) return false;

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            gestorLista.AgregarNodoAleatorio();
            return true;
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            gestorLista.Sacar(ref objetivoActual);
            return true;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            gestorLista.VoltearLista();
            objetivoActual = gestorLista.inicioLista;
            return true;
        }

        return false;
    }

  

    void SteeringBehaviors()
    {
        bool esUltimoNodo = (objetivoActual.siguiente == null);

        
        Vector3 targetOffset = objetivoActual.posicion - transform.position;
        float distance = targetOffset.magnitude;

        if (distance < 0.05f && esUltimoNodo)
        {
            velocity = Vector3.zero;
            return;
        }

        float targetSpeed = maxSpeed;

        
        if (esUltimoNodo)
        {
            float rampedSpeed = maxSpeed * (distance / slowingRadius);
            targetSpeed = Mathf.Min(rampedSpeed, maxSpeed);
        }

        Vector3 desiredVelocity = targetOffset.normalized * targetSpeed;
        Vector3 steering = desiredVelocity - velocity;

        
        Vector3 avoidanceForce = ObstacleAvoidance();
        steering += avoidanceForce;

        
        steering = Vector3.ClampMagnitude(steering, maxForce);
        steering /= mass;

        velocity = Vector3.ClampMagnitude(velocity + steering, maxSpeed);
        transform.position += velocity * Time.deltaTime;
    }

    private Vector3 ObstacleAvoidance()
    {
        Vector3 AvoidanceForce = Vector3.zero;

        if (obstacles == null || obstacles.Length == 0) return AvoidanceForce;

        foreach (Transform obs in obstacles)
        {
            if (obs == null) continue;

            Vector3 toObstacle = obs.position - transform.position;
            float distanceToObstacle = toObstacle.magnitude;

            if (distanceToObstacle < AvoidanceRadius)
            {
                
                Vector3 repulseDir = (transform.position - obs.position).normalized;

                float strength = 1f - (distanceToObstacle / AvoidanceRadius);

                AvoidanceForce += repulseDir * strength * obstacleWeight;
            }
        }

        return AvoidanceForce;
    }

    private void OnDrawGizmos()
    {
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slowingRadius);

      
        if (obstacles != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform obs in obstacles)
            {
                if (obs != null)
                {
                    Gizmos.DrawWireSphere(obs.position, AvoidanceRadius);
                }
            }
        }
    }
}