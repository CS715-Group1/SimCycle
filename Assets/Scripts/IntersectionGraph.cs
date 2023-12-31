using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

[Serializable]
public class Edge
{

    public List<Target> path;
    public int weight;
    public Transform neighbour;

    public Edge(List<Target> path, int weight, Transform vertex)
    {
        this.path = path;
        this.weight = weight;
        neighbour = vertex;
    }
}


public class IntersectionGraph : MonoBehaviour
{

    private Dictionary<Transform, Dictionary<Transform, Edge>> adjacencyList = new();
    public static IntersectionGraph _Instance;


    void Awake()
    {
        if (_Instance != null) throw new Exception("Instance");
        _Instance = this;
    }

    private void Update()
    {
        
        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            Generate();
        }
    }

    private void Generate()
    {
        foreach (Transform child in transform)
        {
            AddVertex(child);
        }

        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<Intersection>(out Intersection node))
            {
                foreach (Edge item in node.GetEdges())
                {
                    
                    AddEdge(child, item.neighbour, item);
                }
            }
        }
    }

    public void AddVertex(Transform vertex)
    {
        if (!adjacencyList.ContainsKey(vertex))
        {
            adjacencyList[vertex] = new Dictionary<Transform, Edge>();
        }
    }

    public void AddEdge(Transform vertex1, Transform vertex2, Edge edge)
    {
        if (!adjacencyList.ContainsKey(vertex1) || !adjacencyList.ContainsKey(vertex2))
        {
            throw new ArgumentException("One or both vertices do not exist in the graph.");
        }

        adjacencyList[vertex1][vertex2] = edge;
    }

    public Dictionary<Transform, Edge> GetNeighbors(Transform vertex)
    {
        if (!adjacencyList.ContainsKey(vertex))
        {
            throw new ArgumentException("Vertex does not exist in the graph.");
        }

        return adjacencyList[vertex];
    }

    public List<Target> TransformToTargetPath(List<Transform> vertexPath)
    {
        List<Target> path = new List<Target>();

        for (int i = 0; i < vertexPath.Count - 1; i++)
        {

            List<Target> edge = GetEdgePath(vertexPath[i], vertexPath[i + 1]);

            if (path.Count != 0 && path.Last<Target>() == edge[0])
            {
                path.RemoveAt(path.Count - 1);
            }

            path.AddRange(edge);
        }
        return path;
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
                return adjacencyList[vertex1][vertex2].weight;
            }
        }
        throw new Exception("No such edge");
    }

    private List<Target> GetEdgePath(Transform startVertex, Transform endVertext)
    {
        return adjacencyList[startVertex][endVertext].path;
    }

    public Transform GetRandomVertex(Transform exclusion)
    {

        Transform vertex = exclusion;

        while (vertex == exclusion)
        {
            int randomNumber = UnityEngine.Random.Range(0, adjacencyList.Keys.Count);
            vertex = adjacencyList.Keys.ToList()[randomNumber];
        }

        return vertex;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        foreach (var vertex in adjacencyList.Keys)
        {
            Vector3 vertexPos = vertex.position + new Vector3(3.5f, .5f, 3.5f);
            Gizmos.DrawSphere(vertexPos, 1);
            foreach (var neighbor in adjacencyList[vertex])
            {
                Vector3 neighbourPos = neighbor.Key.position + new Vector3(3.5f, .5f, 3.5f);
                Gizmos.DrawLine(vertexPos, neighbourPos);
            }
        }
    }
}
