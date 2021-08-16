using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    //Esta clase contiene las propiedades de cada nodo. Cada nodo es un cuadrito del grid. Se llama nodo por su representación en teoría de redes.

    public bool walkable; //true si es posible caminar a traves del nodo
    public Vector3 worldPosition; //Posición en las coordenadas del mundo

    //Posición discreta del nodo basado en las coordenadas del grid. Sirve también como índice en el arreglo grid.
    public int gridX;
    public int gridZ;

    //Costos de moverse a este nodo
    public int gCost; //Costo del camino transcurrido hasta este nodo
    public int hCost; //Estimación del costo restante.
    public Node parent;
    int heapIndex;


    public Node(bool _walkable, Vector3 _worldPosition, int _gridX, int _gridZ)
    {
        walkable = _walkable;
        worldPosition = _worldPosition;
        gridX = _gridX;
        gridZ = _gridZ;
    }

    //Este método (?) calcula el costo total del nodo cada que se llama.
    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if(compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return -compare;
    }
}
