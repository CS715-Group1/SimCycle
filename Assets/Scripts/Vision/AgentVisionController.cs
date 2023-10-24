using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles simul
/// </summary>
public class AgentVisionController : MonoBehaviour
{
    [SerializeField] public bool useVision = true;
    [SerializeField] IDetector detector;

    /// <summary>
    /// Reaction time in seconds
    /// </summary>
    [SerializeField] float reactionTime = 5;

    public List<IdentifiableObject> recognisableObjects { get; private set; }

    [field: SerializeField]

    public UnityEvent<List<CarAI>> PublishSeenCars { get; set; }

    int step = 0;

    /// <summary>
    /// In seconds. Change to reduce performance issues
    /// </summary>
    readonly float MIN_REACTION_TIME = 1;


    // Use this for initialization
    private void Start()
    {
        // Keep reaction time above minimum
        reactionTime = Mathf.Max(reactionTime, MIN_REACTION_TIME);

        recognisableObjects = new();

        InvokeRepeating(nameof(Step), 0, reactionTime);  // Start after 1s, repeat every 1s
    }

    private void FixedUpdate()
    {
        List<CarAI> carsSeen = new();

        foreach (IdentifiableObject obj in recognisableObjects)
        {
            Debug.DrawLine(detector.GetComponent<Camera>().transform.position, obj.transform.position, Color.green);

            if(obj.TryGetComponent<CarAI>(out CarAI car))
            {
                carsSeen.Add(car);
            }

        }

        if(carsSeen.Count > 0)
        {
            PublishSeenCars?.Invoke(carsSeen);
        }
    }

    // Advance one simulation step
    private void Step()
    {
        if (!useVision) return;

        //Debug.Log($"Step {step}");
        step++;

        if (!detector.isActiveAndEnabled) return;

        CheckAllObjects();

        foreach (IdentifiableObject obj in recognisableObjects)
        {
            Debug.Log($"{name} sees: {obj.name}");
        }
    }
        

    private void CheckAllObjects()
    {
        IdentifiableObject[] allIdentifiableObjects = Resources.FindObjectsOfTypeAll(
            typeof(IdentifiableObject)) as IdentifiableObject[];

        // Create a list to store objects excluding the current object.
        var identifiableObjects = new List<IdentifiableObject>();

        foreach (IdentifiableObject obj in allIdentifiableObjects)
        {
            // Check if the object's instance ID is not the same as the current object's instance ID.
            if (obj.gameObject != this.gameObject && obj.isActiveAndEnabled)
            {
                identifiableObjects.Add(obj);
            }
        }

        recognisableObjects = detector.GetRecognisable(identifiableObjects.ToArray());
    }
}
