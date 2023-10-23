using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Road : MonoBehaviour
{

    public virtual void HandleEdge(Edge edge)
    {
        throw new NotImplementedException();
    }

    public virtual List<Road> GetConnectedRoads()
    {
        throw new NotImplementedException();
    }

}
