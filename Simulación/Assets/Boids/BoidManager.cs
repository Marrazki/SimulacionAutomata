using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public static BoidManager Instance { get; private set; }
    [SerializeField]
    GameObject boidPrefab;
    public float radioZona;

    [Header("Spawn info")]
    public int cantidad = 10;
    public int radioSpawn = 10;

    public List<Boid> boids = new List<Boid>();



    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        for (int i = 0; i < cantidad; i++) { 
        
            GameObject go = Instantiate(boidPrefab, this.transform.position + Random.insideUnitSphere * radioSpawn, Random.rotation );
            boids.Add(go.GetComponent<Boid>());

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radioZona);
    }

}
