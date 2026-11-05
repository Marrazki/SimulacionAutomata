
using System;
using UnityEngine;

public class GOL : MonoBehaviour
{
    private bool[,] gridActual;
    private bool[,] gridExpected;
    private GameObject[,] gridObjects;
    private MaterialPropertyBlock propertyBlock;

    public int ancho = 20;
    public int alto = 20;

    public float espacioEntreCubos = 1.1f;
    [Range(0f, 1f)]
    public float life = 0.6f;

    public float updateInterval = 0.2f;

    private float timer = 0f;

    void Start()
    {
        // Evitar dimensiones inválidas.
        ancho = Mathf.Max(1, ancho);
        alto = Mathf.Max(1, alto);
        updateInterval = Mathf.Max(0.01f, updateInterval);

        propertyBlock = new MaterialPropertyBlock();

        GenerarGrid();
        RefrescarGrid();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            timer -= updateInterval;
            Simulation();
        }
    }

    void GenerarGrid()
    {
        gridActual = new bool[alto, ancho];
        gridExpected = new bool[alto, ancho];
        gridObjects = new GameObject[alto, ancho];

        for (int fila = 0; fila < alto; fila++)
        {
            for (int columna = 0; columna < ancho; columna++)
            {
                GameObject nuevoCubo =
                    GameObject.CreatePrimitive(PrimitiveType.Cube);

                nuevoCubo.name = $"Celda_{fila}_{columna}";

                nuevoCubo.transform.SetParent(transform);
                nuevoCubo.transform.localPosition = new Vector3(
                    columna * espacioEntreCubos,
                    0f,
                    fila * espacioEntreCubos
                );

                gridObjects[fila, columna] = nuevoCubo;
                gridActual[fila, columna] = UnityEngine.Random.value < life;
            }
        }
    }

    void Simulation()
    {
        // Calcular la siguiente generación sin modificar la actual.
        for (int fila = 0; fila < alto; fila++)
        {
            for (int columna = 0; columna < ancho; columna++)
            {
                int vecinosVivos = CuentaVecinosVivos(fila, columna);
                bool vivo = gridActual[fila, columna];

                if (vecinosVivos < 2 || vecinosVivos > 3)
                {
                    // Muere por soledad o superpoblación.
                    gridExpected[fila, columna] = false;
                }
                else if (vecinosVivos == 3)
                {
                    // Una célula nace o sobrevive.
                    gridExpected[fila, columna] = true;
                }
                else
                {
                    // Con dos vecinos conserva su estado.
                    gridExpected[fila, columna] = vivo;
                }
            }
        }

        // Copiar la siguiente generación a la actual.
        Array.Copy(gridExpected, gridActual, gridActual.Length);

        RefrescarGrid();
    }

    int CuentaVecinosVivos(int fila, int columna)
    {
        int count = 0;

        for (int desplazamientoFila = -1;
             desplazamientoFila <= 1;
             desplazamientoFila++)
        {
            for (int desplazamientoColumna = -1;
                 desplazamientoColumna <= 1;
                 desplazamientoColumna++)
            {
                // No contar la propia celda.
                if (desplazamientoFila == 0 &&
                    desplazamientoColumna == 0)
                {
                    continue;
                }

                int vecinoFila = fila + desplazamientoFila;
                int vecinoColumna = columna + desplazamientoColumna;

                // Ignorar posiciones fuera de la cuadrícula.
                if (vecinoFila >= 0 && vecinoFila < alto &&
                    vecinoColumna >= 0 && vecinoColumna < ancho)
                {
                    if (gridActual[vecinoFila, vecinoColumna])
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }

    void RefrescarGrid()
    {
        for (int fila = 0; fila < alto; fila++)
        {
            for (int columna = 0; columna < ancho; columna++)
            {
                GameObject cubo = gridObjects[fila, columna];

                if (cubo == null)
                {
                    continue;
                }

                MeshRenderer meshRenderer =
                    cubo.GetComponent<MeshRenderer>();

                if (meshRenderer == null)
                {
                    continue;
                }

                // Verde = viva; rojo = muerta.
                Color color = gridActual[fila, columna]
                    ? Color.green
                    : Color.red;

                // Cambiar el color sin crear un material por cubo.
                meshRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_Color", color);
                propertyBlock.SetColor("_BaseColor", color);
                meshRenderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}
