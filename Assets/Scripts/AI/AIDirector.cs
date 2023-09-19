using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AIDirector : MonoBehaviour
{
    public GameObject carPrefab;
    private IntersectionGraph graph;
    private List<Target> path = new();

    private void Start()
    {
        graph = IntersectionGraph._Instance;
    }

    

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.S))
        {
            SpawnCar();
        }
    }

    public void SpawnCar()
    {
        TrySpawnACar();
    }

    private void TrySpawnACar()
    {
        Transform start = graph.GetRandomVertex(null);
        Transform end = graph.GetRandomVertex(start);

        List<Transform> vertexPath = AStar.AStarSearch(graph, start, end, null);
        path = graph.TransformToTargetPath(vertexPath);

        var car = Instantiate(carPrefab, path[0].transform.position, Quaternion.identity);
        car.GetComponent<CarAI>().SetPath(path);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;


        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i].transform.position, path[i + 1].transform.position);
        }
    }
}
