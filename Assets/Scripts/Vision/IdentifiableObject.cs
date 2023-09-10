using System;
using UnityEngine;

[ExecuteInEditMode]
public class IdentifiableObject: MonoBehaviour
{
    void Awake()
    {
        int LayerIdentifiable = LayerMask.NameToLayer("Identifiable");
        gameObject.layer = LayerIdentifiable;
    }

    private void Update()
    {
        
    }
}
