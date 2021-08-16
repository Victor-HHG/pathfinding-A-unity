using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Esta clase es general, no está en monoBehaviour. Cada que se llama, se tiene que especificar el tipo de datos que va a manejar "T".
public class Heap<T> where T : IHeapItem<T>
{
    T[] items;
    int currentItemCount; //Guarda el tamaño del heap. Como índice, apunta a la primera posición "vacía" del arbol

    //Se crea el método Heap. Se tiene que especificar desde el principio el tamaño del heap, pues es complicado cambiarlo después.
    //Para el caso de Pathfinding, es el número de nodos total.
    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];

    }

    //Método para añadir objetos al arbol Heap
    public void Add(T item)
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

        //Método para extraer el nodo menor del arbol
    public T RemoveFirst()
    {
        T firstItem = items[0]; //referencia al nodo más pequeño
        currentItemCount--;     //Se reduce el número de nodos en el arbol

        //Se sube el último elemento a la primera posición.
        //En este momento, currentItemCount apunta al último elemento, no al primer vacío, dado que en el paso anterior se le quitó uno.
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    //Metodo para actualizar el costo de un item
    public void UpdateItem(T item)
    {
        //Cuando se cambia el valor del item (en otro script), se mueve el item hacia arriba en el arbol.
        //Sólo se puede mover hacia arriba, porque los pesos siempre se actializan hacia abajo, no hacia arriba.
        SortUp(item);
    }

    //Metodo para obtener el número de items en el heap
    public int Count
    {
        get
        {
            return currentItemCount;
        }
    }

    //Método para determinar si un item está en el heap
    public bool Contains(T item)
    {
        //Corrobora que el si el indice del item que se está pasando corresponde con item idéntico.
        //Es decir, que en la posición del arbol exista un item con las mismas características
        return Equals(items[item.HeapIndex], item);
    }

    //Reordena los elementos del arbol después de haber añadido un nuevo elemento
    void SortUp(T item)
    {
        //Primero se determina el indice del parent
        int parentIndex = (item.HeapIndex - 1) / 2;

        //Se crea un loop que se mantiene hasta que el código lo detiene (break), cuando ya no está haciendo movimientos.
        while (true)
        {
            //Referencia al padre
            T parentItem = items[parentIndex];

            //Se compara el valor del nodo con el padre
            //Con esta función CompareTo, se obtiene 1 si el de la izquierda es menor que el de la derecha, o -1 lo contrario. 0 si son iguales.
            if (item.CompareTo(parentItem) > 0)
            {
                //Se cambia la posición en el árbol.
                Swap(item, parentItem);
            }
            else
            {
                //Si no es más pequeño, ya llegó a su posición final, y se detiene le while
                break;
            }

            //Se recalcula el indice del padre, y se reejecuta el loop.
            parentIndex = (item.HeapIndex - 1) / 2;

        }
    }

    //Método para ordenar el arbol de arriba hacia abajo.
    void SortDown(T item)
    {
        while(true)
        {
            //Se calculan los indices de los dos hijos
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            //Si tiene al menos un hijo, se asigna ese como el menor
            if(childIndexLeft < currentItemCount)
            {
                swapIndex = childIndexLeft;

                //Si hay un segundo hijo, se determina cuál de los dos tiene la menor prioridad
                if(childIndexRight < currentItemCount)
                {
                    if(items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }

                //Una vez determinado cuál de los dos hijos es menor, se compara contra el padre y se cambia de posición si aplica.
                if(item.CompareTo(items[swapIndex]) < 0)
                {
                    Swap(item, items[swapIndex]);
                }
                else
                {
                    //Si no es necesario cambiarlos, se detiene el método.
                    return;
                }
            }
            else
            {
                //Si el padre no tiene hijos, se detiene el método
                return;
            }
        }
    }

    //Este método intercambia dos elementos y sus indices en el arbol
    void Swap(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;

        int itemAIndex = itemA.HeapIndex;       //Se guarda el indice provisionalmente para hacer el cambio
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }

}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }
}