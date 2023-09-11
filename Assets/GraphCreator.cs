using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphCreator : MonoBehaviour
{
    [SerializeField] private Road start;
    private LinkedList<Road> adjRoads = new();


    void Start()
    {
        //start.GetConnectedRoads();

        while (adjRoads.Count > 0)
        {

        }


    }

}
