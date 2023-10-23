using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

public class DriverLogic : MonoBehaviour
{
    private List<Transform> nodes = new();
    [SerializeField] private Transform start;
    [SerializeField] private Transform goal;
    [SerializeField] private Transform temp;
    private int counter = 0;

    private void Start()
    {
        nodes.Add(temp);
        nodes.Add(start);
    }

    public UnityEngine.Vector3 getNextNode()
    {
        if (counter == nodes.Count)
        {
            Debug.Log("Finished");
            IntersectionGraph graph = IntersectionGraph._Instance;
            nodes = AStar.AStarSearch(graph, nodes[counter-1], graph.GetRandomVertex(nodes[counter - 1]), nodes[counter-2]);
            counter = 1;
        }

        UnityEngine.Vector3 nextNode = nodes[counter].position;
        counter++;
        return nextNode;
    }
}