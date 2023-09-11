using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour
{

    [SerializeField] private List<Target> targets = new();
    public List<Edge> edges = new();
    public Transform center;

    //Collider m_Collider;
    //RaycastHit m_Hit;
    //float m_MaxDistance = 3;
    //LayerMask m_LayerMask;

    //private void Awake()
    //{
    //    m_Collider = GetComponent<Collider>();
    //    m_LayerMask = LayerMask.GetMask("Road");
    //}

    

    internal Target GetNearestTarget(Vector3 start)
    {
        float distance = float.MaxValue;
        Target target = null;

        foreach (Target item in targets)
        {
            float distanceFromTarget = Vector3.Distance(item.transform.position, start);

            if (distanceFromTarget < distance){
                distance = distanceFromTarget;
                target = item;
            }        
        }
        return target;
    }

    public void AddEdge(Edge edge)
    {
        edges.Add(edge);
    }

    public List<Edge> GetEdges()
    {
        return edges;
    }

    //public List<Road> GetConnectedRoads()
    //{

    //    List<Road> adgRoads = new List<Road>();

    //    for (int i = 0; i < 360; i+=90)
    //    {
    //        float rad =Mathf.Deg2Rad*i;

    //        Vector3 direction = new(Mathf.Cos(rad), 0, Mathf.Sin(rad));

    //        if (Physics.BoxCast(m_Collider.bounds.center, transform.localScale, direction, out m_Hit, transform.rotation, m_MaxDistance, m_LayerMask))
    //        {
    //            Debug.Log(m_Hit.collider.name);
    //            adgRoads.Add(m_Hit.collider.gameObject.GetComponent<Road>());
    //        }
    //    }

    //    return adgRoads;
    //}


}
