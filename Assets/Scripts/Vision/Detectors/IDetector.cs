using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public abstract class IDetector : MonoBehaviour
{
    public abstract bool IsObjectVisible(IdentifiableObject obj);
    public abstract List<IdentifiableObject> GetVisible(IdentifiableObject[] objects);

    public abstract bool IsObjectRecognisable(IdentifiableObject obj);
    public abstract List<IdentifiableObject> GetRecognisable(IdentifiableObject[] objects);


	/// <summary>
	/// https://discussions.unity.com/t/is-there-an-easy-way-to-get-on-screen-render-size-bounds/15884/3
	/// </summary>
	/// <param name="go"></param>
	/// <returns></returns>
	public static Rect GUIRectWithObject(GameObject go)
	{
		Vector3 cen = go.GetComponent<Renderer>().bounds.center;
		Vector3 ext = go.GetComponent<Renderer>().bounds.extents;
		Vector2[] extentPoints = new Vector2[8]
		{
		HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
		HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
		HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
		HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),

		HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
		HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
		HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
		HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
		};

		Vector2 min = extentPoints[0];
		Vector2 max = extentPoints[0];

		foreach (Vector2 v in extentPoints)
		{
			min = Vector2.Min(min, v);
			max = Vector2.Max(max, v);
		}

		return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
	}
}
