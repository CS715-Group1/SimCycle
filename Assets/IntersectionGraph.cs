using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class IntersectionGraph : MonoBehaviour
{
    
    private Dictionary<Transform, Dictionary<Transform, float>> adjacencyList = new();
    [SerializeField] private List<Transform> goals = new List<Transform>();
    private int counter = 0;

    public static IntersectionGraph _Instance;

    void Awake()
    {
        if (_Instance != null) throw new Exception("Instance");
        _Instance = this;
    }

    void Start()
    {

        adjacencyList = new Dictionary<Transform, Dictionary<Transform, float>>();

        foreach (Transform child in transform)
        {
            AddVertex(child);
        }

        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<Node>(out Node node))
            {
                for (int i = 0; i < node.neighbours.Count; i++)
                {
                    AddEdge(child, node.neighbours[i], node.weights[i]);
                }
            }
        }
    }

    public void AddVertex(Transform vertex)
    {
        if (!adjacencyList.ContainsKey(vertex))
        {
            adjacencyList[vertex] = new Dictionary<Transform, float>();
        }
    }

    public void AddEdge(Transform vertex1, Transform vertex2, int weight)
    {
        if (!adjacencyList.ContainsKey(vertex1) || !adjacencyList.ContainsKey(vertex2))
        {
            throw new ArgumentException("One or both vertices do not exist in the graph.");
        }

        adjacencyList[vertex1][vertex2] = weight;
        adjacencyList[vertex2][vertex1] = weight;
    }

    public Dictionary<Transform, float> GetNeighbors(Transform vertex)
    {
        if (!adjacencyList.ContainsKey(vertex))
        {
            throw new ArgumentException("Vertex does not exist in the graph.");
        }

        return adjacencyList[vertex];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
            
        foreach (var vertex in adjacencyList.Keys)
        {
            Gizmos.DrawSphere(vertex.position, 1);
            foreach (var neighbor in adjacencyList[vertex])
            {
                Gizmos.DrawLine(vertex.position, neighbor.Key.position);
            }
        }
    }

    public List<Transform> GetVertices()
    {
        return adjacencyList.Keys.ToList();
    }

    public float GetEdgeWeight(Transform vertex1, Transform vertex2)
    {
        if (adjacencyList.ContainsKey(vertex1))
        {
            if (adjacencyList[vertex1].ContainsKey(vertex2))
            {
                return adjacencyList[vertex1][vertex2];
            }
        }
        throw new Exception("No such edge");
    }

    public Transform GetRandomVertex(Transform exclusion)
    {
        //int randomNumber = UnityEngine.Random.Range(0, adjacencyList.Keys.Count);
            
        return goals[counter++];
    }
}
