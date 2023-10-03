using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleRaycastDetector : IDetector
{
    new Camera camera;

    private void Start()
    {
        camera = GetComponent<Camera>();
    }

    public override List<IdentifiableObject> GetRecognisable(IdentifiableObject[] objects)
    {
        List<IdentifiableObject> recognisableObjects = new();

        foreach (IdentifiableObject obj in objects)
        {
            if (obj == null || !obj.isActiveAndEnabled) continue;

            if (IsObjectRecognisable(obj)) recognisableObjects.Add(obj);
        }
        return recognisableObjects;
    }

    public override bool IsObjectRecognisable(IdentifiableObject obj)
    {
        Vector3 start = camera.transform.position;
        Vector3 dir = obj.transform.position - camera.transform.position;

        Ray ray = new(start, dir);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
        {
            // Tries to get the IdentifiableObject component
            IdentifiableObject identifiableObject = hitInfo.collider.GetComponent<IdentifiableObject>();

            // If null, this means not an IdentifiableObject. If equals "obj", then raycast has hit the object.
            if (identifiableObject != null && identifiableObject.Equals(obj))
            {
                return true;
            }
        }
        return false;
    }
}
