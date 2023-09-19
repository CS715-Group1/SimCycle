using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IDetector
{
    public List<IdentifiableObject> GetVisible(IdentifiableObject[] objects);
}
