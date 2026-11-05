using System.Reflection.PortableExecutable;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Boid : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 2f;
    public float velocidadGiro = 2f;

    [Header("Percepción")]
    public float radio = 4f;
    public float radioSeparacion = 0.5f;
    public float anguloVista = 80f;
    public LayerMask capaObjetos;

    [Header("Pesos")]
    public float pesoCohesion = 1f;
    public float pesoAlineacion = 1f;
    public float pesoSeparacion = 1.5f;
    public float pesoLimite = 4f;
    public float pesoObstaculo = 4f;
    public float pesoObjetivo = 1f;

    BoidManager manager;
    Vector3 direction;

    private void Start()
    {
        manager = BoidManager.Instance;
        direction = transform.forward;
    }

    private void Update()
    {
        Vector3 cohesion = CalcularCohesion();
        Vector3 alineacion = CalcularAlineacion();
        Vector3 separacion = CalcularSeparacion();
        Vector3 limite = CalcularLimite();
        Vector3 obstaculo = CalcularObstaculo();
        Vector3 objetivo = CalcularObjetivo();

        Vector3 dirObjetivo = cohesion*pesoCohesion + alineacion*pesoAlineacion + separacion*pesoSeparacion + limite*pesoLimite + obstaculo*pesoObstaculo + 
            objetivo * pesoObjetivo;
        
        dirObjetivo.Normalize();

        if(dirObjetivo != Vector3.zero)
        {
            direction = Vector3.Lerp(direction, dirObjetivo, velocidadGiro * Time.deltaTime);

        }
        transform.forward = direction; 
        transform.position += direction*velocidad*Time.deltaTime;

    }

    private Vector3 CalcularCohesion() 
    { 
        Vector3 centro = Vector3.zero;
        int numVecinos = 0;

        foreach (Boid actualBoid in manager.boids)
        {
            if(actualBoid == this) continue;
            if (Vector3.SqrMagnitude(transform.position - actualBoid.transform.position) < radio * radio && EnVista(actualBoid)) 
            {
                centro += actualBoid.transform.position;
                numVecinos++;
            }
        }
        if(numVecinos == 0) return Vector3.zero;
        centro /= numVecinos;
        return (centro - transform.position).normalized;
    }
    private Vector3 CalcularAlineacion()
    {
        Vector3 dir = Vector3.zero;
        int numVecinos = 0;
        foreach (Boid actualBoid in manager.boids)
        {
            if (actualBoid == this) continue;
            if (Vector3.SqrMagnitude(transform.position - actualBoid.transform.position) < radio * radio && EnVista(actualBoid))
            {
                dir += actualBoid.transform.forward;
                numVecinos++;
            }
        }
        if (numVecinos == 0) return Vector3.zero;
        dir /= numVecinos;
        return dir.normalized;
    }
    private Vector3 CalcularSeparacion()
    {
        Vector3 dir = Vector3.zero;
        foreach (Boid actualBoid in manager.boids)
        {
            if (actualBoid == this) continue;
            if (Vector3.SqrMagnitude(transform.position - actualBoid.transform.position) < radioSeparacion * radioSeparacion)
            {
                dir += (transform.position - actualBoid.transform.position).normalized;
            }
        }

        return dir.normalized;
    }

    private Vector3 CalcularLimite()
    {
        if (Vector3.SqrMagnitude(transform.position - manager.transform.position) < manager.radioZona * manager.radioZona) return Vector3.zero;

        return (manager.transform.position - transform.position).normalized;


    }

    private Vector3 CalcularObstaculo()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position,transform.forward,out hit, radio, capaObjetos))
        {
            
            Vector3 dirEscape = Vector3.zero;

            dirEscape -= transform.forward;

            if(Physics.Raycast(transform.position, -transform.up, out hit, radioSeparacion, capaObjetos))
            {
                dirEscape += transform.up;
            }
            if (Physics.Raycast(transform.position, transform.up, out hit, radioSeparacion, capaObjetos))
            {
                dirEscape += -transform.up;
            }
            if (Physics.Raycast(transform.position, -transform.right, out hit, radioSeparacion, capaObjetos))
            {
                dirEscape += transform.right;
            }
            if (Physics.Raycast(transform.position, transform.right, out hit, radioSeparacion, capaObjetos))
            {
                dirEscape += -transform.right;
            }

            return dirEscape.normalized;
        }
        return Vector3.zero;
    }

    private Vector3 CalcularObjetivo()
    {
        return (manager.transform.position - transform.position);
    }

    bool EnVista(Boid otro)
    {
        Vector3 dirOtro = otro.transform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, dirOtro);
        if (angle < anguloVista) return true;
        return false;

    }

}
