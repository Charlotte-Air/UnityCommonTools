using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SortUtil : MonoBehaviour {
    public int sortOrder = 1;
    void Awake()
    {
        GetComponent<Renderer>().sortingOrder = sortOrder;
    }
}
