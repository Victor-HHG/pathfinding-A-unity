using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour
{
    Grid grid; //referencia al grid

    //Referencias al personaje que busca caminos y al objetivo
    public Transform seeker;
    public Transform target;

    private void Awake()
    {
        //Se obtiene la referencia al grid que esta cargado en el mismo Game Object. Es decir, se requiere que este script y el de grid estén
        //cargados al mismo objeto.
        grid = GetComponent<Grid>();
    }

    private void Update()
    {
        //sólo se busa el path en el frame en el que se oprime un botón
        if(Input.GetButtonDown("Jump"))
        {
            FindPath(seeker.position, target.position);
        }

    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        //PROVISIONAL###############Se crea un medidor para ver el cambio en eficiencia del heap
        Stopwatch sw = new Stopwatch();
        sw.Start();

        //Se encuentran los nodos inicial y objetivo
        Node startNode = grid.NodeFromWorldPosition(startPos);
        Node targetNode = grid.NodeFromWorldPosition(targetPos);

        //Se crea el conjunto de nodos por visitar y el conjunto de nodos visitados
        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while(openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if(currentNode == targetNode)
            {
                //PROVISIONAL###############Se crea un medidor para ver el cambio en eficiencia del heap
                sw.Stop();
                print("Path found: " + sw.ElapsedMilliseconds + " ms");

                RetracePath(startNode, targetNode);
                return;
            }

            foreach(Node neighbour in grid.GetNeighbours(currentNode))
            {
                //Si no se puede caminar por el nodo, o ya está en la lista de visitados, se lo salta.
                if(!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetNodeDistance(currentNode, neighbour);
                if(newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetNodeDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if(!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
                
            }
        }
    }

    //Metodo para reconstruir el camino a partir de los parents de cada nodo
    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node tracingNode = endNode;

        while(tracingNode != startNode)
        {
            path.Add(tracingNode);
            tracingNode = tracingNode.parent;
        }

        path.Reverse();

        grid.path = path;
    }

    //metodo para calcular la distancia (discreta )entre dos nodos
    int GetNodeDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);

        //Considerando que el jugador puede moverse a las 8 celdas contiguas, el camino más corto implica moverse en diagonal
        //Si la diferencia en posiciones es, por ejemplo (5,2), el camino más directo es moverse 2 veces en diagonal
        //cubriendo los 2 en un eje y dos de los 5 del otro eje. Sólo le quedaría moverse en línea 3 unidades sobre otro eje.
        //Asumimos que el costo de moverse a las celdas no-diagonales es 10, y en diagonal es 14.
        //esto es una aproximación a las distancias del triángulo de lado 1 ( raiz cuadrada de 2 es 1.41 )
        //Entonces, la función de distancias es 14*menor + 10*(mayor-menor)

        if(distX > distZ)
        {
            return 14 * distZ + 10 * (distX - distZ);
        }
        else
        {
            return 14 * distX + 10 * (distZ - distX);
        }
    }
}
