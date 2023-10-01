using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public string specificTag = "Detecting";
    public List<GameObject> taggedObjects = new List<GameObject>();
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found.");
            return;
        }

        // Find objects with the specific tag.
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(specificTag);

        foreach (GameObject obj in objectsWithTag)
        {
            // Check if the object is within the camera's frustum.
            if (IsObjectVisible(obj))
            {
                taggedObjects.Add(obj);
            }
        }
    }

    // Check if an object is within the camera's frustum.
    bool IsObjectVisible(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

            return GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
        }

        return false;
    }
}
