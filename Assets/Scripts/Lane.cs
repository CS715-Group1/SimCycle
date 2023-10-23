using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class represents a single lane of a road section
[Serializable]
public class Lane
{
    public List<Target> linkedTargets = new();

    public void AddTarget(Target target)
    {
        linkedTargets.Add(target);
    }

    public Transform lastTarget()
    {
        return linkedTargets[^1].transform;
    }

    public List<Target> GetTargets()
    {
        return linkedTargets;
    }

    public Transform GetLaneStart()
    {
        return linkedTargets[0].transform;
    }

    public void AddToStart(Target target)
    {
        List<Target> targets = new List<Target>
        {
            target
        };
        targets.AddRange(linkedTargets);
        linkedTargets = targets;
    }
    public List<Vector3> ToPath()
    {
        List<Vector3> path = new List<Vector3>();

        foreach (var target in linkedTargets)
        {
            path.Add(target.transform.position);
        }
        return path;
    }
}
