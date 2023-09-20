﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles simul
/// </summary>
public class AgentVisionController : MonoBehaviour
{
    [SerializeField] RaycastDetector detector;

    /// <summary>
    /// Reaction time in seconds
    /// </summary>
    [SerializeField] float reactionTime = 5;

    public List<IdentifiableObject> recognisableObjects { get; private set; }

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
        foreach (IdentifiableObject obj in recognisableObjects)
        {
            Debug.DrawLine(detector.camera.transform.position, obj.transform.position, Color.green);
        }
    }

    // Advance one simulation step
    private void Step()
    {
        Debug.Log($"Step {step}");
        step++;

        if (!detector.isActiveAndEnabled) return;

        IdentifiableObject[] identifiableObjects = Resources.FindObjectsOfTypeAll(
            typeof(IdentifiableObject)) as IdentifiableObject[];

        recognisableObjects = detector.GetRecognisable(identifiableObjects);

        foreach (IdentifiableObject obj in recognisableObjects)
        {
            Debug.Log($"{name} sees: {obj.name}");
        }
    }
}