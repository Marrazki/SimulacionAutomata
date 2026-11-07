using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(LineRenderer))]

public class CuerdaVerlet : MonoBehaviour
{
    [Header("Cuerda")]
    [Min(2)]
    [SerializeField] int numPuntos = 10;
    [SerializeField] float distanciaEntrePuntos = 0.2f;

    [Header("Simulacion")]
    [SerializeField] Vector3 gravedad = new Vector3(0f, -9.8f, 0f);
    [Range(1, 500)]
    [SerializeField] int numIteraciones = 10;
    [SerializeField] float damping = 0.95f;
    [SerializeField] float radioCast = 0.2f;
    [SerializeField] LayerMask mascaraColisiones;

    [SerializeField] Transform esquina;


    List<VerletParticle> listaParticulas;

    LineRenderer lineRenderer;  
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numPuntos;
        listaParticulas = new List<VerletParticle>();
        
        for (int i = 0; i < numPuntos; i++) {
            Vector3 distanciaAInicio = new Vector3(i * distanciaEntrePuntos, 0, 0);
            listaParticulas.Add(new VerletParticle( transform.position + distanciaAInicio, false));
        }
        listaParticulas[0].puntoFijo = true;
        if (esquina != null) listaParticulas[listaParticulas.Count - 1].puntoFijo = true;
        

    }
    private void Update()
    {
        DibujarLista();
    }

    private void FixedUpdate()
    {
        float fdt = Time.fixedDeltaTime;

        listaParticulas[0].posicion = transform.position;
        if(esquina != null) listaParticulas[listaParticulas.Count -1].posicion = esquina.position;
        //Simular las particulas
        for (int i = 0; i < numPuntos; i++) {
            listaParticulas[i].Simulacion(gravedad, fdt, damping);
        }
        //Aplicar constrains
        for (int iter = 0; iter < numIteraciones; iter++)
        {
            for (int i = 1; i < numPuntos; i++)
            {
                ResolucionConstrainsDistance(listaParticulas[i - 1], listaParticulas[i]);
                ResolucionConstrainFloor(listaParticulas[i]);
                ResolucionColisiones(listaParticulas[i]);
            }
        }

    }




    private void ResolucionConstrainsDistance(VerletParticle particleA, VerletParticle particleB)
    {
        Vector3 vAB = particleB.posicion - particleA.posicion;
        float distancia = vAB.magnitude;
        if (distancia == distanciaEntrePuntos) return;
        float error = distancia - distanciaEntrePuntos;
        Vector3 correccion = vAB.normalized * error;

        if (particleA.puntoFijo)
        {
            particleB.posicion -= correccion;
        }
        else if (particleB.puntoFijo)
        {
            particleA.posicion += correccion;
        }
        else
        {
            particleB.posicion -= correccion * 0.5f;
            particleA.posicion += correccion * 0.5f;
        }
    }
    private void ResolucionConstrainFloor(VerletParticle particleA)
    {
        if(particleA.posicion.y < 0)
        {
            particleA.posicion.y = 0;
        }
    }

    private void ResolucionColisiones(VerletParticle particleA) {
        if (particleA.puntoFijo) return;
        Vector3 movimiento = particleA.posicion - particleA.posicionPasada;
        float distancia = movimiento.magnitude;
        if (distancia < 0.0001f)
        {
            return;
        }
        if (Physics.SphereCast(particleA.posicionPasada, radioCast, movimiento.normalized, out RaycastHit hitInfo, distancia, mascaraColisiones))
        {
            Debug.Log("Collision con " + hitInfo.collider.gameObject.name);
            particleA.posicion = hitInfo.point + hitInfo.normal * (radioCast + 0.005f);
            particleA.posicionPasada = particleA.posicion;
        }
    }


    void DibujarLista()
    {
        for (int i = 0; i < listaParticulas.Count; i++) {
            lineRenderer.SetPosition(i, listaParticulas[i].posicion);
        }
    }


}
