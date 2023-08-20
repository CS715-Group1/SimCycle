using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Node : MonoBehaviour
{
    public List<Transform> neighbours = new();
    public List<int> weights = new();

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(transform.position, 1);
    //    foreach (Transform t in neighbours)
    //    {
    //        Gizmos.DrawLine(transform.position, t.position);
    //    }
    //}
}


