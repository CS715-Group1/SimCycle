using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastDetector : IDetector
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

    public new Camera camera { get; private set; }

    // Calculate from maxDimension and view cone angle instead
    Vector2 cameraSize = new(50, 25);

    private void Start()
    {
        camera = GetComponent<Camera>();
        camera.pixelRect = new Rect(0, 0, cameraSize.x, cameraSize.y);
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
        // ======== Stage 1: Getting camera view as a texture + setting up other textures.


        // Isolate object for detection; raycasts will only affect this object.
        obj.gameObject.layer = LayerMask.NameToLayer("Detecting");

        // Set the camera's target texture to capture the scene.
        camera.Render();

        RenderTexture rt = new(camera.pixelWidth, camera.pixelHeight, 1);

        // Create a new Texture2D and read the pixels from the RenderTexture.
        Texture2D texture = new(rt.width, rt.height);
        RenderTexture.active = rt;

        // Read in exactly what the camera sees as a new texture.
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

        // Create empty textures to be written to.
        Texture2D blockedVision = new(texture.width, texture.height);
        Texture2D perfectVision = new(texture.width, texture.height);



        // ======== Stage 2: Draw pixels using raycasts



        // Loop through the pixels and check if the corresponding objects are in the "Identifiable" layer.
        for (int y = 0; y < texture.height; y += (int)(1 / scaleFactor))
        {
            for (int x = 0; x < texture.width; x += (int)(1 / scaleFactor))
            {
                // Set pixels to black or white
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


        // Drawing to the display planes
        if (blockedDisplay != null) blockedDisplay.ApplyTexture(blockedVision);
        if (perfectDisplay != null) perfectDisplay.ApplyTexture(perfectVision);



        // ======== Stage 3: Check recognisability



        bool isRecognisable = IsRecognizable(blockedVision, perfectVision);

        //ReportVisibility(obj, isRecognisable);

        return isRecognisable; 
    }

    private Texture2D FindBlockedIdentifiable(Texture2D texture, int x, int y, IdentifiableObject obj)
    {
        bool detected = false;

        // Raycast from the camera to the world point to determine the object's layer.
        Ray ray = camera.ScreenPointToRay(new Vector3(x, y, 0));

        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance))
        {
            // Tries to get the IdentifiableObject component
            IdentifiableObject identifiableObject = hitInfo.collider.GetComponent<IdentifiableObject>();

            // If null, this means not an IdentifiableObject. If equals "obj", then raycast has hit the object.
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
        Ray ray = camera.ScreenPointToRay(new Vector3(x, y, 0));

        // Set pixel to black if ray hits the object in the "Detecting" layer
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

    private bool IsRecognizable(Texture2D blockedVision, Texture2D perfectVision)
    {
        Color[] perfectPixels = perfectVision.GetPixels();
        Color[] blockedPixels = blockedVision.GetPixels();

        int recognisable = 0;
        int total = 0;

        for (int i = 0; i < perfectPixels.Length; i++)
        {
            if (perfectPixels[i] == detectedColor)
            {
                total++;
            }

            if (blockedPixels[i] == detectedColor)
            {
                recognisable++;
            }
        }

        float ratio = (float)recognisable / total;

        //Debug.Log($"Recognisable: {recognisable} / Total: {total} = {ratio}");

        if (total == 0)
        {
            return false;
        }

        return ratio >= recognizableThreshold;
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

    private void ReportVisibility(IdentifiableObject obj, bool isRecognisable)
    {
        if (isRecognisable)
        {
            Debug.Log($"Camera '{camera.name}' recognized object '{obj.name}'");
        }
        else
        {
            Debug.Log($"Camera '{camera.name}' did not recognize object '{obj.name}'");
        }
    }
}