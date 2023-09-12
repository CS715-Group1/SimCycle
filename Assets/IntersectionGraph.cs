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

    public List<Vector3> path;
    public int weight;
    public Transform neighbour;

    public Edge(List<Vector3> path, int weight, Transform vertex)
    {
        this.path = path;
        this.weight = weight;
        neighbour = vertex;
    }
}


public class IntersectionGraph : MonoBehaviour
{

    private Dictionary<Transform, Dictionary<Transform, Edge>> adjacencyList = new();

    private List<Vector3> path = new List<Vector3>();

    public static IntersectionGraph _Instance;

    public Transform start;
    public Transform end;

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

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            

        }
    }

    private void Generate()
    {
        //adjacencyList = new Dictionary<Transform, Dictionary<Transform, Edge>>();

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

                //for (int i = 0; i < node.ed.Count; i++)
                //{
                //    AddEdge(child, node.neighbours[i], node.weights[i]);
                //}
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
        //adjacencyList[vertex2][vertex1] = edge;
    }

    public Dictionary<Transform, Edge> GetNeighbors(Transform vertex)
    {
        if (!adjacencyList.ContainsKey(vertex))
        {
            throw new ArgumentException("Vertex does not exist in the graph.");
        }

        return adjacencyList[vertex];
    }

    public List<Vector3> TransformToTargetPath(List<Transform> vertexPath)
    {
        List<Vector3> path = new List<Vector3>();

        for (int i = 0; i < vertexPath.Count - 1; i++)
        {
            path.AddRange(GetEdgePath(vertexPath[i], vertexPath[i + 1]));
        }
        return path;
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

    private List<Vector3> GetEdgePath(Transform startVertex, Transform endVertext)
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
}
