using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T, C> where C: IComparer<T>, new()
{
    // minHeap
    // private int size;
    public List<T> arr;
    C comp;
    public PriorityQueue()
    {
        arr = new List<T>();
        comp = new C();
    }
    public void push(T item)
    {
        int i = arr.Count;
        arr.Add(item);
        for (; i >= 1 && comp.Compare(item, arr[(i - 1) / 2])>0; i = (i-1) / 2)
        {
            arr[i] = arr[(i-1) / 2];
        }
        arr[i] = item;
    }

    public T top()
    {
        return arr[0];
    }
    public void pop()
    {
        int parent=0, child;
        T X = arr[arr.Count - 1];
        for(parent=0; parent * 2 + 1 < arr.Count-1; parent = child)
        {
            child = parent * 2 + 1;
            if(child+1<arr.Count-1 && comp.Compare(arr[child], arr[child + 1])<0)
            {
                child++;
            }
            if (comp.Compare(X, arr[child]) > 0) break;
            arr[parent] = arr[child];
        }
        arr[parent] = X;
        arr.RemoveAt(arr.Count - 1);
    }
    public bool empty()
    {
        return arr.Count == 0;
    }
    public int size()
    {
        return arr.Count;
    }

 }
