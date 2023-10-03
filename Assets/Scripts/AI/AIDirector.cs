using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class AIRequest
{
    public Transform start;
    public Transform end;
    public int delay;
}

public class AIDirector : MonoBehaviour
{
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject bikePrefab;
    private IntersectionGraph graph;
    private List<Target> path = new();


    [SerializeField] private List<AIRequest> requests = new();

    [SerializeField] private List<Transform> interections = new();

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

        if (Input.GetKeyUp(KeyCode.D))
        {
            TrySpawnACarFromIntersections();
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            RunScenario();
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            Transform start = graph.GetRandomVertex(null);
            Transform end = graph.GetRandomVertex(start);

            List<Transform> vertexPath = AStar.AStarSearch(graph, start, end, null);
            path = graph.TransformToTargetPath(vertexPath);

            var bike = Instantiate(bikePrefab, path[0].transform.position, Quaternion.identity);
            bike.GetComponent<CarAI>().SetPath(path);
        }
    }

    private void RunScenario()
    {
        foreach (var request in requests)
        {
            System.Threading.Thread.Sleep(request.delay*1000);
            List<Transform> vertexPath = AStar.AStarSearch(graph, request.start, request.end, null);
            path = graph.TransformToTargetPath(vertexPath);

            var car = Instantiate(carPrefab, path[0].transform.position, Quaternion.identity);
            car.GetComponent<CarAI>().SetPath(path);
        }
    }

    private Transform getRandomTransform(Transform exclusion)
    {
        Transform vertex = exclusion;

        while (vertex == exclusion)
        {
            int randomNumber = UnityEngine.Random.Range(0, interections.Count);
            vertex = interections[randomNumber];
        }

        return vertex;
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

    private void TrySpawnACarFromIntersections()
    {

        Transform start = getRandomTransform(null);
        Transform end = getRandomTransform(start);

        List<Transform> vertexPath = AStar.AStarSearch(graph, start, end, null);
        path = graph.TransformToTargetPath(vertexPath);

        var car = Instantiate(carPrefab, path[0].transform.position, Quaternion.identity);
        car.GetComponent<CarAI>().SetPath(path);
    }

}
