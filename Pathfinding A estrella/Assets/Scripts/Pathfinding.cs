using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour
{
    PathRequestManager requestManager;
    Grid grid; //referencia al grid

    //Referencias al personaje que busca caminos y al objetivo
    public Transform seeker;
    public Transform target;

    private void Awake()
    {
        //Se obtiene la referencia al grid que esta cargado en el mismo Game Object. Es decir, se requiere que este script y el de grid estén
        //cargados al mismo objeto.
        grid = GetComponent<Grid>();

        //Se obtiene la referencia al Path Manager que se encuentra en el mismo Game Object.
        requestManager = GetComponent<PathRequestManager>();
    }

    private void Update()
    {
        //sólo se busa el path en el frame en el que se oprime un botón
        if(Input.GetButtonDown("Jump"))
        {
            FindPath(seeker.position, target.position);
        }

    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        //PROVISIONAL###############Se crea un medidor para ver el cambio en eficiencia del heap
        Stopwatch sw = new Stopwatch();
        sw.Start();

        //Es la lista de puntos por donde pasa el camino.
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        //Se encuentran los nodos inicial y objetivo
        Node startNode = grid.NodeFromWorldPosition(startPos);
        Node targetNode = grid.NodeFromWorldPosition(targetPos);

        if (startNode.walkable && targetNode.walkable)
        {

            //Se crea el conjunto de nodos por visitar y el conjunto de nodos visitados
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    //PROVISIONAL###############Se crea un medidor para ver el cambio en eficiencia del heap
                    sw.Stop();
                    print("Path found: " + sw.ElapsedMilliseconds + " ms");

                    pathSuccess = true;

                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    //Si no se puede caminar por el nodo, o ya está en la lista de visitados, se lo salta.
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetNodeDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetNodeDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }

                }
            }
        }
        yield return null;

        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    //Metodo para reconstruir el camino a partir de los parents de cada nodo
    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node tracingNode = endNode;

        while(tracingNode != startNode)
        {
            path.Add(tracingNode);
            tracingNode = tracingNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
        
    }

    //Método para simplificar el path. Crea un nuevo path llamado waypoints, donde sólo mantiene los puntos en los que cambia de dirección,
    //en lugar de toda la lista original.
    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for(int i=1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridZ - path[i].gridZ);
            if(directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
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
