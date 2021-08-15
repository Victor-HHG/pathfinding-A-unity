using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Transform player; //Contiene la posición en la que se encuentra el player.
    public LayerMask unwalkableMaks; //Es la capa donde se encuentran los obstaculos.
    public Vector3 gridWorldSize; //En el video de Sebastian Langue usa un Vector2, pero luego eso vuelve confusas las referencias a ejes. 
                                  //Se introducen los valores en el editor.
    public float nodeRadius; //Se introduce en el editor.
    Node[,] grid;

    float nodeDiameter; //Tamaño del nodo
    int gridSizeX; //Nos dice el numero de cuadros que caben en el eje X
    int gridSizeZ; //Nos dice el número de cuadros que caben en el eje Z. 

    public List<Node> path;

    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        //Se calcula el número de cuadros que caben en el grid dividiendo lo que mide el grid entre el diametro del nodo (o lado del cuadrito).
        //Se redondea el resultado a números enteros. No tengo claro si conviene hacerlo al entero más cercano, o conviene usar el floor.
        //Creo que con el redondeo simple, los objetos podrían salirse del plano si poco menos de la mitad de un cuadrito se redondeó fuera del plano.
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeZ = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);

        CreateGrid();
    }

    //Este método crea el grid a partir de las medidas y número de nodos estimados previamente en el Start.
    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeZ];
        //Se determina la posición del punto inferior izquierdo. A partir de ahí se empiezan a dibujar los cuadritos.
        //Se parte de que este script está en el centro del grid. Se le resta la mitad del tamaño en x y z. En y no se quita nada
        Vector3 worldBottomLeft = transform.position - new Vector3(gridWorldSize.x / 2, 0, gridWorldSize.z / 2);

        //Se crea un loop doble para iterar en todas las posiciones de los cuadritos en el eje x y el eje z.

        for(int x=0; x < gridSizeX; x++)
        { 
            for (int z = 0; z < gridSizeZ; z++)
            {
               //Se determina el centro de cada cuadrito, partiendo del worldBottomLeft y moviendose el diametro en cada iteración hacia x o z.
               //Además, se suma el radio para asegurar que estamos en el centro y no en una orilla.
                Vector3 worldPoint = worldBottomLeft + new Vector3(x * nodeDiameter + nodeRadius, 0, z * nodeDiameter + nodeRadius);

                //Se crea una booleana que indica si se puede caminar por un nodo,
                //Si hay un collider (en una mascara) en la esfera con centro en el centro del nodo y dentro de su radio,
                //entonces no se puede caminar por ahí (está negado).
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMaks));
                grid[x, z] = new Node(walkable, worldPoint, x, z);
            }
        }
    }

    //Este método encuentra el nodo en el que se encuentra un objeto. En este caso, el jugador.
    public Node NodeFromWorldPosition(Vector3 worldPosition)
    {
        //Se determina la posición a partir de la posición relativa del jugador en el mapa.
        float percentX = (worldPosition.x / gridWorldSize.x) + 0.5f; //Se suma el 0.5 porque el centro (0,0,0) está a la mitad del plano
        float percentZ = (worldPosition.z / gridWorldSize.z) + 0.5f;

        //Se asegura que los porcentajes no se salgan del intervalo [0,1]
        percentX = Mathf.Clamp01(percentX);
        percentZ = Mathf.Clamp01(percentZ);

        //Se determinan los índices del cuadro donde está el player, multiplicando el porcentaje por el número de cuadritos.
        //En la versión de Sebastian Langue al grid size se le restaba 1 y el resultado se redondeaba al entero más cercano.
        //Provocaba comportamientos raros cuando la posición se pasaba tantito de la mitad del cuadro, ya se pasaba al siguiente.
        //Lo cambié para tomar el Floor, sin restar 1. Parece funcionar mejor.
        int x = Mathf.FloorToInt(gridSizeX * percentX);
        int z = Mathf.FloorToInt(gridSizeZ * percentZ);

        return grid[x, z];
    }

    //Este método dibuja líneas guía para que sólo se ven durante el desarrollo. Al parecer eso son los Gizmos.
    private void OnDrawGizmos()
    {
        //Se dibuja un cuadro que permite calibrar el tamaño del plano que representa el piso.
        //Una vez dibujado el gizmo, se cambian las medidas del gridWorldSize en el editor de Unity.
        Gizmos.DrawWireCube(transform.position, gridWorldSize);

        //Se dibujan cuadritos representando cada nodo del grid.
        if(grid != null)
        {
            foreach(Node n in grid)
            {
                //Se determina el color del cuadro, dependiendo de si se puede caminar por el cuador o no.
                if(n.walkable)
                {
                    Gizmos.color = Color.white;
                    //El cuadro del jugador tiene un color distinto al resto.
                    if(path.Contains(n))
                    {
                        Gizmos.color = Color.cyan;
                    }
                }
                else
                {
                    Gizmos.color = Color.red;
                }

                //Se dibuja el cuadro en la posición de cada cuadtrito, con cada lado igual a 1 menos un pequeño espacio entre cubos.
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }

    //Método para encontrar los nodos vecinos de un nodo
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        //Se revisan los vecinos en el pequeño grid de 3x3 alrededor del nodo donde se busca.
        for(int x = -1; x <= 1; x++)
        {
            for(int z = -1; z <= 1; z++)
            {
                //Si se revisa el del centro, se salta
                if(x == 0 && z == 0)
                {
                    continue;
                }

                //La posición del nodo con el offset (alrededor del inicial)
                int checkX = node.gridX + x;
                int checkZ = node.gridZ + z;

                //Si el nodo no se sale del grid (osea, no está en la orilla) se añade a la lista de vecinos.
                if(checkX >= 0 && checkX < gridSizeX && checkZ >= 0 && checkZ < gridSizeZ)
                {
                    neighbours.Add(grid[checkX, checkZ]);
                }
            }
        }
        //Se regresa la lista de vecinos
        return neighbours;
    }
}
