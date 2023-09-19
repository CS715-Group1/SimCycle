using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdentifiableDetector : MonoBehaviour
{
    [Header("Vision Parameters")]
    [SerializeField] int maxDistance = 100;
    [SerializeField] float recognizableThreshold = 0.7f;

    [Header("Detection Settings")]
    [SerializeField] Color detectedColor = Color.black;
    [SerializeField] Color baseColor = Color.white;
    [SerializeField] int maxDimension = 128;

    [Header("Debug")]
    [SerializeField] DisplayPlane blockedDisplay;
    [SerializeField] DisplayPlane perfectDisplay;

    new Camera camera;

    private void Start()
    {
        camera = GetComponent<Camera>();
    }

    public List<IdentifiableObject> GetVisible(IdentifiableObject[] objects)
    {
        List<IdentifiableObject> visibleObjects = new();

        foreach (IdentifiableObject obj in objects)
        {
            if (obj == null || !obj.isActiveAndEnabled) continue;

            if (CheckVisibility(obj)) visibleObjects.Add(obj);
        }
        return visibleObjects;
    }

    private bool CheckVisibility(IdentifiableObject obj)
    {
        // Isolate object for detection
        obj.gameObject.layer = LayerMask.NameToLayer("Detecting");

        // Set the camera's target texture to capture the scene.
        camera.Render();

        RenderTexture rt = new(camera.pixelWidth, camera.pixelHeight, 1);

        // Create a new Texture2D and read the pixels from the RenderTexture.
        Texture2D texture = new(rt.width, rt.height);
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();


        // Restrict resolution of processed texture
        // - Not scaling texture as raycasts depend on pixel/screen position
        float scaleFactor = 1;

        if (camera.pixelWidth > maxDimension)
        {
            scaleFactor = (float)maxDimension / camera.pixelWidth;
        }
        else if (camera.pixelHeight > maxDimension)
        {
            scaleFactor = (float)maxDimension / camera.pixelWidth;
        }


        Texture2D blockedVision = new(texture.width, texture.height);
        Texture2D perfectVision = new(texture.width, texture.height);

        // Loop through the pixels and check if the corresponding objects are in the "Identifiable" layer.
        for (int y = 0; y < texture.height; y += (int)(1 / scaleFactor))
        {
            for (int x = 0; x < texture.width; x += (int)(1 / scaleFactor))
            {
                blockedVision = FindBlockedIdentifiable(blockedVision, x, y, obj);
                perfectVision = FindAllIdentifiable(perfectVision, x, y);
            }
        }

        // Apply the changes to the result texture.
        blockedVision.Apply();
        perfectVision.Apply();

        // Clean up
        RenderTexture.active = null;
        camera.targetTexture = null;
        rt.Release();

        // Finished detection, remove object from isolation
        obj.gameObject.layer = LayerMask.NameToLayer("Identifiable");


        // Output Visibility
        if (blockedDisplay != null) blockedDisplay.ApplyTexture(blockedVision);
        if (perfectDisplay != null) perfectDisplay.ApplyTexture(perfectVision);

        bool isVisible = IsRecognizable(blockedVision, perfectVision, recognizableThreshold);

        //ReportVisibility(obj, isVisible);

        return isVisible;
    }

    private Texture2D FindBlockedIdentifiable(Texture2D texture, int x, int y, IdentifiableObject obj)
    {
        bool detected = false;

        // Raycast from the camera to the world point to determine the object's layer.
        Ray ray = camera.ScreenPointToRay(new Vector3(x, y, 0));

        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance))
        {
            IdentifiableObject identifiableObject = hitInfo.collider.GetComponent<IdentifiableObject>();

            // TODO: check against mesh, not collider
            detected = identifiableObject != null && identifiableObject.Equals(obj);
        }

        if (detected)
        {
            texture.SetPixel(x, y, detectedColor);
        }
        else
        {
            texture.SetPixel(x, y, baseColor);
        }

        return texture;
    }

    private Texture2D FindAllIdentifiable(Texture2D texture, int x, int y)
    {
        // Raycast from the camera to the world point to determine the object's layer.
        Ray ray = camera.ScreenPointToRay(new Vector3(x, y, 0));

        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, LayerMask.GetMask("Detecting")))
        {
            // TODO: check against mesh, not collider
            texture.SetPixel(x, y, detectedColor);
        }
        else
        {
            texture.SetPixel(x, y, baseColor);
        }

        return texture;
    }

    private bool IsRecognizable(Texture2D blockedVision, Texture2D perfectVision, float threshold)
    {
        Color[] perfectPixels = perfectVision.GetPixels();
        Color[] blockedPixels = blockedVision.GetPixels();

        int visible = 0;
        int total = 0;

        for (int i = 0; i < perfectPixels.Length; i++)
        {
            if (perfectPixels[i] == detectedColor)
            {
                total++;
            }

            if (blockedPixels[i] == detectedColor)
            {
                visible++;
            }
        }

        float ratio = (float)visible / total;

        //Debug.Log($"Visible: {visible} / Total: {total} = {ratio}");

        if (total == 0)
        {
            return false;
        }

        return ratio >= threshold;
    }

    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = (1.0f / (float)targetWidth);
        float incY = (1.0f / (float)targetHeight);
        for (int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
        }
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }

    private void ReportVisibility(IdentifiableObject obj, bool isVisible)
    {
        if (isVisible)
        {
            Debug.Log($"Camera '{camera.name}' recognized object '{obj.name}'");
        }
        else
        {
            Debug.Log($"Camera '{camera.name}' did not recognize object '{obj.name}'");
        }
    }
}