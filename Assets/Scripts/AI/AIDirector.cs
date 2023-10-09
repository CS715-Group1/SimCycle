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
    public float delay;
    public AgentType type;
}

public enum AgentType
{
    CAR, CYCLIST, TARGET_CAR
}

public class AIDirector : MonoBehaviour
{
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject bikePrefab;
    [SerializeField] private GameObject greenCarPrefab;
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
            StartCoroutine(RunScenario());
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            Transform start = graph.GetRandomVertex(null);
            Transform end = graph.GetRandomVertex(start);

            List<Transform> vertexPath = AStar.AStarSearch(graph, start, end, null);
            path = graph.TransformToTargetPath(vertexPath);

            var bike = Instantiate(bikePrefab, path[0].transform.position, Quaternion.identity);
            bike.GetComponent<CarAI>().SetPath(path, vertexPath);
        }
    }

    private IEnumerator RunScenario()
    {
        foreach (var request in requests)
        {
            yield return new WaitForSeconds(request.delay);
            List<Transform> vertexPath = AStar.AStarSearch(graph, request.start, request.end, null);
            path = graph.TransformToTargetPath(vertexPath);

            GameObject agent;

            switch (request.type)
            {
                case AgentType.CYCLIST:
                    agent = Instantiate(bikePrefab, path[0].transform.position, Quaternion.identity);
                    break;
                case AgentType.TARGET_CAR:
                    agent = Instantiate(greenCarPrefab, path[0].transform.position, Quaternion.identity);
                    break;
                default:
                    agent = Instantiate(carPrefab, path[0].transform.position, Quaternion.identity);
                    break;
            }

            agent.GetComponent<CarAI>().SetPath(path, vertexPath);
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
        car.GetComponent<CarAI>().SetPath(path, vertexPath);
    }

    private void TrySpawnACarFromIntersections()
    {

        Transform start = getRandomTransform(null);
        Transform end = getRandomTransform(start);

        List<Transform> vertexPath = AStar.AStarSearch(graph, start, end, null);
        path = graph.TransformToTargetPath(vertexPath);

        var car = Instantiate(carPrefab, path[0].transform.position, Quaternion.identity);
        car.GetComponent<CarAI>().SetPath(path, vertexPath);
    }

}
