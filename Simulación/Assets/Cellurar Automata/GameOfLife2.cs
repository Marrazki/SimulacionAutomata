using UnityEngine;
using System;

public class GameOfLife2 : MonoBehaviour
{
    private bool[,] tableroActual;
    private bool[,] tableroSiguiente;

    private GameObject[,] objetosTablero;
    private MaterialPropertyBlock propertyBlock;

    public int ancho = 10;
    public int alto = 10;

    public float espacioEntreCubos = 1.1f;
    [Range(0f, 1f)]
    public float probabilidadVivos = 0.8f;

    public float tiempoEntreJugadas = 0.8f;

    private float timer = 0f;

    //TERREMOTO
    [Range(0f, 1f)]
    public float probabilidadTerremoto = 0.1f;

    [Min(1)]
    public int anchoTerremoto = 10;

    // Configuración del meteorito
    public float probabilidadMeteorito = 0.3f;
    public int radioMeteorito = 5;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Evitar dimensiones inválidas.
        ancho = Mathf.Max(1, ancho);
        alto = Mathf.Max(1, alto);
        tiempoEntreJugadas = Mathf.Max(0.01f, tiempoEntreJugadas);

        propertyBlock = new MaterialPropertyBlock();

        GenerarTablero();
        RefrescarTablero();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= tiempoEntreJugadas)
        {
            timer -= tiempoEntreJugadas;
            Simulacion();
        }
    }

    void GenerarTablero()
    {
        tableroActual = new bool[alto, ancho];
        tableroSiguiente = new bool[alto, ancho];
        objetosTablero = new GameObject[alto, ancho];

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

                objetosTablero[fila, columna] = nuevoCubo;
                tableroActual[fila, columna] = UnityEngine.Random.value < probabilidadVivos;
            }
        }
    }
    void Simulacion()
    {
        // Calcular la siguiente generación sin modificar la actual.
        for (int fila = 0; fila < alto; fila++)
        {
            for (int columna = 0; columna < ancho; columna++)
            {
                int vecinosVivos = CuentaVecinosVivos(fila, columna);
                bool vivo = tableroActual[fila, columna];

                if (vecinosVivos < 2 || vecinosVivos > 3 && vecinosVivos < 8)
                {
                    Debug.Log("PEIO 2 3");
                    // Muere por soledad o superpoblación.
                    tableroSiguiente[fila, columna] = false;
                }
                else if (vecinosVivos == 3)
                {
                    Debug.Log("PEIO 3");

                    // Una célula nace o sobrevive.
                    tableroSiguiente[fila, columna] = true;
                }

                /*
                // EXPLOSION EN CRUZ ALEATORIA
                else if (vecinosVivos == 8)
                {
                    Debug.Log($"¡EXPLOSION! Centro: {fila},{columna}");

                    bool crearVida = UnityEngine.Random.value < 0.5f;

                    // Toda la fila
                    for (int j = 0; j < ancho; j++)
                    {
                        tableroSiguiente[fila, j] = crearVida;
                    }

                    // Toda la columna
                    for (int i = 0; i < alto; i++)
                    {
                        tableroSiguiente[i, columna] = crearVida;
                    }
                }*/

                // EXPLOSION CIRCULAR ALEATORIA
                
                else if (vecinosVivos == 8)
                {
                    //Debug.Log($"¡EXPLOSION! Centro: {fila},{columna}");

                    bool crearVida = UnityEngine.Random.value < 0.5f;

                    int radio = 10;

                    for (int i = fila - radio; i <= fila + radio; i++)
                    {
                        for (int j = columna - radio; j <= columna + radio; j++)
                        {
                            // Evitar salir de los límites del tablero
                            if (i < 0 || i >= alto || j < 0 || j >= ancho)
                                continue;

                            // Distancia al centro de la explosión
                            int df = i - fila;
                            int dc = j - columna;

                            if (df * df + dc * dc <= radio * radio)
                            {
                                tableroSiguiente[i, j] = crearVida;
                            }
                        }
                    }

                    Debug.Log(crearVida
                        ? "¡La explosión ha creado vida!"
                        : "¡La explosión ha destruido vida!");
                }


                else
                {
                    // Con dos vecinos conserva su estado.
                    tableroSiguiente[fila, columna] = vivo;
                }
            }
        }
        AplicarMeteorito();
        Terremoto();

        // Copiar la siguiente generación a la actual.
        Array.Copy(tableroSiguiente, tableroActual, tableroActual.Length);

        RefrescarTablero();

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
                    if (tableroActual[vecinoFila, vecinoColumna])
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }
    void RefrescarTablero()
    {
        for (int fila = 0; fila < alto; fila++)
        {
            for (int columna = 0; columna < ancho; columna++)
            {
                GameObject cubo = objetosTablero[fila, columna];

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
                Color color = tableroActual[fila, columna]
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
    void Terremoto()
    {
        if (UnityEngine.Random.value >= probabilidadTerremoto)
            return;

        int filasADestruir = Mathf.Clamp(anchoTerremoto, 1, alto);
        int inicio = UnityEngine.Random.Range(0, alto - filasADestruir + 1);

        Debug.Log($"TERREMOTO: filas {inicio} a {inicio + filasADestruir - 1}");

        for (int fila = inicio; fila < inicio + filasADestruir; fila++)
        {
            for (int columna = 0; columna < ancho; columna++)
            {
                tableroSiguiente[fila, columna] = false;
            }
        }
    }
    void AplicarMeteorito()
    {
        // Decidir si cae un meteorito en esta generación
        if (UnityEngine.Random.value >= probabilidadMeteorito)
            return;

        // Elegir una posición aleatoria del tablero
        int centroFila = UnityEngine.Random.Range(0, alto);
        int centroColumna = UnityEngine.Random.Range(0, ancho);

        int radio = Mathf.Max(1, radioMeteorito);
        int radioInterior = radio * radio;
        int radioExterior = (radio + 1) * (radio + 1);

        Debug.Log(
            $"¡METEORITO! Impacto en {centroFila},{centroColumna}"
        );

        // Recorrer la zona afectada
        for (int i = centroFila - radio - 1;
             i <= centroFila + radio + 1; i++)
        {
            for (int j = centroColumna - radio - 1;
                 j <= centroColumna + radio + 1; j++)
            {
                // Comprobar los límites del tablero
                if (i < 0 || i >= alto ||
                    j < 0 || j >= ancho)
                    continue;

                int df = i - centroFila;
                int dc = j - centroColumna;
                int distanciaCuadrada = df * df + dc * dc;

                // Centro del impacto: destruir las células
                if (distanciaCuadrada < radioInterior)
                {
                    tableroSiguiente[i, j] = false;
                }
                // Borde exterior: crear vida nueva
                else if (distanciaCuadrada <= radioExterior)
                {
                    tableroSiguiente[i, j] = true;
                }
            }
        }

        Debug.Log("¡El meteorito ha creado vida alrededor del cráter!");
        Debug.Log(centroFila + " " + centroColumna);
    }
}
