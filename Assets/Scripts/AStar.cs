using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

public class AStar : MonoBehaviour
{
    public static AStar _Instance;// { get { return _Instance; } }
    void Awake()
    {
        if (_Instance != null) throw new Exception("Instance");
        _Instance = this;
    }


    public static List<Transform> AStarSearch(IntersectionGraph graph, Transform start, Transform goal,Transform behind)
    {
        var openSet = new PriorityQueue<Transform>();
        openSet.Enqueue(start, 0);

        var cameFrom = new Dictionary<Transform, Transform>();
        var gScore = new Dictionary<Transform, float>();
        var fScore = new Dictionary<Transform, float>();

        foreach (var vertex in graph.GetVertices())
        {
            gScore[vertex] = int.MaxValue;
            fScore[vertex] = int.MaxValue;
        }

        gScore[start] = 0;
        fScore[start] = HeuristicCostEstimate(start, goal); // Assuming you have a heuristic function

        while (!openSet.IsEmpty())
        {
            Transform current = openSet.Dequeue();

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            foreach (var neighbor in graph.GetNeighbors(current))
            {

                float tentativeGScore = gScore[current] + graph.GetEdgeWeight(current, neighbor.Key);

                if (current == start && neighbor.Key == behind)
                {
                    tentativeGScore += float.MaxValue;
                }

                if (tentativeGScore < gScore[neighbor.Key])
                {
                    cameFrom[neighbor.Key] = current;
                    gScore[neighbor.Key] = tentativeGScore;
                    fScore[neighbor.Key] = gScore[neighbor.Key] + HeuristicCostEstimate(neighbor.Key, goal);

                    if (!openSet.Contains(neighbor.Key))
                    {
                        openSet.Enqueue(neighbor.Key, fScore[neighbor.Key]);
                    }
                }
            }
        }

        return null; // No path found
    }

    private static float HeuristicCostEstimate(Transform current, Transform goal)
    {
        // Replace this with your actual heuristic calculation
        return UnityEngine.Vector3.Distance(current.position, goal.position);
    }

    public static List<Transform> ReconstructPath(Dictionary<Transform, Transform> cameFrom, Transform current)
    {
        var path = new List<Transform>();
        path.Add(current);

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();

        return path;
    }
}

public class PriorityQueue<T>
{
    private SortedDictionary<float, Queue<T>> data = new SortedDictionary<float, Queue<T>>();

    public void Enqueue(T item, float priority)
    {
        if (!data.ContainsKey(priority))
        {
            data[priority] = new Queue<T>();
        }
        data[priority].Enqueue(item);
    }

    public T Dequeue()
    {
        var firstKey = data.Keys.First();
        var queue = data[firstKey];
        var item = queue.Dequeue();
        if (queue.Count == 0)
        {
            data.Remove(firstKey);
        }
        return item;
    }

    public bool IsEmpty()
    {
        return data.Count == 0;
    }

    public bool Contains(T item)
    {
        return data.Values.Any(queue => queue.Contains(item));
    }
}
