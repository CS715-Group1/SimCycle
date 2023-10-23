using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastDepthBufferDetector : IDetector
{
    [Header("Vision Parameters")]
    [SerializeField] int maxDistance = 100;
    [SerializeField] float recognizableThreshold = 0.7f;

    [Header("Detection Settings")]
    [SerializeField] Material postprocessMaterial;
    [SerializeField] int maxDimension = 128;

    [Header("Debug")]
    [SerializeField] DisplayPlane display;

    public new Camera camera { get; private set; }

    private void Start()
    {
        camera = GetComponent<Camera>();
        camera.cullingMask = LayerMask.GetMask("Detecting");
        camera.depthTextureMode = camera.depthTextureMode | DepthTextureMode.DepthNormals;
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
        SetLayerRecursive(obj.gameObject, "Detecting");



        //// Get viewspace to worldspace matrix and pass it to shader
        Matrix4x4 viewToWorld = camera.cameraToWorldMatrix;
        postprocessMaterial.SetMatrix("_viewToWorld", viewToWorld);

        // Create a temporary RenderTexture for the capture
        RenderTexture renderTexture = new(camera.pixelWidth, camera.pixelHeight, 1);
        renderTexture.Create();

        camera.targetTexture = renderTexture;
        camera.Render();

        RenderTexture resultTexture = new(camera.pixelWidth, camera.pixelHeight, 0);

        // Render the current camera view to the temporary RenderTexture
        Graphics.Blit(renderTexture, resultTexture, postprocessMaterial);


        // Read the contents of the temporary RenderTexture
        Texture2D texture = new(camera.pixelWidth, camera.pixelHeight);
        RenderTexture.active = resultTexture;
        texture.ReadPixels(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), 0, 0);
        texture.Apply();

        // Clean up
        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
        Destroy(resultTexture);




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



        // ======== Stage 2: Draw pixels using raycasts


        int totalCount;
        int visibleCount;

        totalCount = 0;
        visibleCount = 0;

        // Loop through the pixels and check if the corresponding objects are in the "Identifiable" layer.
        for (int y = 0; y < texture.height; y += (int)(1 / scaleFactor))
        {
            for (int x = 0; x < texture.width; x += (int)(1 / scaleFactor))
            {
                // Ignore all white pixels (not the target object)
                if (texture.GetPixel(x, y).grayscale == 1) continue;

                totalCount++;

                // Ray cast to each black pixel to see if the object is visible
                Ray ray = camera.ScreenPointToRay(new Vector3(x, y, 0));

                if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance))
                {
                    // Tries to get the IdentifiableObject component
                    IdentifiableObject identifiableObject = hitInfo.collider.GetComponent<IdentifiableObject>();

                    // If null, this means not an IdentifiableObject. If equals "obj", then raycast has hit the object.
                    if (identifiableObject != null && identifiableObject.Equals(obj))
                    {
                        visibleCount++;
                    }
                }
            }
        }


        // Drawing to the display plane.
        if (display != null) display.ApplyTexture(texture);

        SetLayerRecursive(obj.gameObject, "Identifiable");

        // ======== Stage 3: Check recognisability


        bool isRecognisable = IsRecognizable(visibleCount, totalCount);

        //ReportVisibility(obj, isRecognisable);

        return isRecognisable; 
    }

    private bool IsRecognizable(int visibleCount, int totalCount)
    {

        float ratio = (float)visibleCount / totalCount;

        Debug.Log($"Recognisable: {visibleCount} / Total: {totalCount} = {ratio}");

        if (totalCount == 0)
        {
            return false;
        }

        return ratio >= recognizableThreshold;
    }

    /// <summary>
    /// Necessary to set recursive layers as models may contain multiple components
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="layer"></param>
    private void SetLayerRecursive(GameObject obj, string layer)
    {
        obj.layer = LayerMask.NameToLayer(layer);
        foreach (Transform child in obj.transform)
        {
            // Set the layer for each child
            child.gameObject.layer = LayerMask.NameToLayer(layer);
            SetLayerRecursive(child.gameObject, layer);
        }
    }
}