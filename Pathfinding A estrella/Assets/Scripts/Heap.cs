using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Esta clase es general, no est� en monoBehaviour. Cada que se llama, se tiene que especificar el tipo de datos que va a manejar "T".
public class Heap<T> where T : IHeapItem<T>
{
    T[] items;
    int currentItemCount; //Guarda el tama�o del heap. Como �ndice, apunta a la primera posici�n "vac�a" del arbol

    //Se crea el m�todo Heap. Se tiene que especificar desde el principio el tama�o del heap, pues es complicado cambiarlo despu�s.
    //Para el caso de Pathfinding, es el n�mero de nodos total.
    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];

    }

    //M�todo para a�adir objetos al arbol Heap
    public void Add(T item)
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

        //M�todo para extraer el nodo menor del arbol
    public T RemoveFirst()
    {
        T firstItem = items[0]; //referencia al nodo m�s peque�o
        currentItemCount--;     //Se reduce el n�mero de nodos en el arbol

        //Se sube el �ltimo elemento a la primera posici�n.
        //En este momento, currentItemCount apunta al �ltimo elemento, no al primer vac�o, dado que en el paso anterior se le quit� uno.
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    //Metodo para actualizar el costo de un item
    public void UpdateItem(T item)
    {
        //Cuando se cambia el valor del item (en otro script), se mueve el item hacia arriba en el arbol.
        //S�lo se puede mover hacia arriba, porque los pesos siempre se actializan hacia abajo, no hacia arriba.
        SortUp(item);
    }

    //Metodo para obtener el n�mero de items en el heap
    public int Count
    {
        get
        {
            return currentItemCount;
        }
    }

    //M�todo para determinar si un item est� en el heap
    public bool Contains(T item)
    {
        //Corrobora que el si el indice del item que se est� pasando corresponde con item id�ntico.
        //Es decir, que en la posici�n del arbol exista un item con las mismas caracter�sticas
        return Equals(items[item.HeapIndex], item);
    }

    //Reordena los elementos del arbol despu�s de haber a�adido un nuevo elemento
    void SortUp(T item)
    {
        //Primero se determina el indice del parent
        int parentIndex = (item.HeapIndex - 1) / 2;

        //Se crea un loop que se mantiene hasta que el c�digo lo detiene (break), cuando ya no est� haciendo movimientos.
        while (true)
        {
            //Referencia al padre
            T parentItem = items[parentIndex];

            //Se compara el valor del nodo con el padre
            //Con esta funci�n CompareTo, se obtiene 1 si el de la izquierda es menor que el de la derecha, o -1 lo contrario. 0 si son iguales.
            if (item.CompareTo(parentItem) > 0)
            {
                //Se cambia la posici�n en el �rbol.
                Swap(item, parentItem);
            }
            else
            {
                //Si no es m�s peque�o, ya lleg� a su posici�n final, y se detiene le while
                break;
            }

            //Se recalcula el indice del padre, y se reejecuta el loop.
            parentIndex = (item.HeapIndex - 1) / 2;

        }
    }

    //M�todo para ordenar el arbol de arriba hacia abajo.
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

                //Si hay un segundo hijo, se determina cu�l de los dos tiene la menor prioridad
                if(childIndexRight < currentItemCount)
                {
                    if(items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }

                //Una vez determinado cu�l de los dos hijos es menor, se compara contra el padre y se cambia de posici�n si aplica.
                if(item.CompareTo(items[swapIndex]) < 0)
                {
                    Swap(item, items[swapIndex]);
                }
                else
                {
                    //Si no es necesario cambiarlos, se detiene el m�todo.
                    return;
                }
            }
            else
            {
                //Si el padre no tiene hijos, se detiene el m�todo
                return;
            }
        }
    }

    //Este m�todo intercambia dos elementos y sus indices en el arbol
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