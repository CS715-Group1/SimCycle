using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastDepthBufferDetector : IDetector
{
    [Header("Vision Parameters")]
    [SerializeField] int maxDistance = 100;
    [SerializeField] float recognizableThreshold = 0.7f;

    [Header("Detection Settings")]
    [SerializeField] private Material postprocessMaterial;
    [SerializeField] int maxDimension = 128;

    [Header("Debug")]
    [SerializeField] DisplayPlane display;

    public new Camera camera { get; private set; }

    Texture2D texture;

    private void Start()
    {
        camera = GetComponent<Camera>();
        camera.cullingMask = LayerMask.GetMask("Detecting");
        camera.depthTextureMode = camera.depthTextureMode | DepthTextureMode.DepthNormals;

        texture = new(camera.pixelWidth, camera.pixelHeight);
    }

    // Automatically called by unity after the camera is done rendering
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Get viewspace to worldspace matrix and pass it to shader
        Matrix4x4 viewToWorld = camera.cameraToWorldMatrix;
        postprocessMaterial.SetMatrix("_viewToWorld", viewToWorld);

        // Draws the pixels from the source texture to the destination texture
        Graphics.Blit(source, destination, postprocessMaterial);

        RenderTexture.active = destination;

        // Read in exactly what the camera sees as a new texture.
        texture.ReadPixels(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), 0, 0);
        texture.Apply();

        RenderTexture.active = null;
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

        // Restrict resolution of processed texture
        // - Not scaling texture as raycasts depend on pixel/screen position
        float scaleFactor = 1;

        //if (camera.pixelWidth > maxDimension)
        //{
        //    scaleFactor = (float)maxDimension / camera.pixelWidth;
        //}
        //else if (camera.pixelHeight > maxDimension)
        //{
        //    scaleFactor = (float)maxDimension / camera.pixelWidth;
        //}



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

        // Clean up
        RenderTexture.active = null;
        //camera.targetTexture = null;

        // Finished detection, remove object from isolation
        //obj.gameObject.layer = LayerMask.NameToLayer("Identifiable");


        // Drawing to the display plane.
        display.ApplyTexture(texture);



        // ======== Stage 3: Check recognisability



        bool isRecognisable = IsRecognizable(visibleCount, totalCount);

        //ReportVisibility(obj, isRecognisable);

        return isRecognisable; 
    }

    //private Texture2D FindBlockedIdentifiable(Texture2D texture, int x, int y, IdentifiableObject obj)
    //{
    //    bool detected = false;

    //    // Raycast from the camera to the world point to determine the object's layer.
    //    Ray ray = camera.ScreenPointToRay(new Vector3(x, y, 0));

    //    if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance))
    //    {
    //        // Tries to get the IdentifiableObject component
    //        IdentifiableObject identifiableObject = hitInfo.collider.GetComponent<IdentifiableObject>();

    //        // If null, this means not an IdentifiableObject. If equals "obj", then raycast has hit the object.
    //        detected = identifiableObject != null && identifiableObject.Equals(obj);
    //    }

    //    if (detected)
    //    {
    //        texture.SetPixel(x, y, detectedColor);
    //    }
    //    else
    //    {
    //        texture.SetPixel(x, y, baseColor);
    //    }

    //    return texture;
    //}

    //private Texture2D FindAllIdentifiable(Texture2D texture, int x, int y)
    //{
    //    Ray ray = camera.ScreenPointToRay(new Vector3(x, y, 0));

    //    // Set pixel to black if ray hits the object in the "Detecting" layer
    //    if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, LayerMask.GetMask("Detecting")))
    //    {
    //        // TODO: check against mesh, not collider
    //        texture.SetPixel(x, y, detectedColor);
    //    }
    //    else
    //    {
    //        texture.SetPixel(x, y, baseColor);
    //    }

    //    return texture;
    //}

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
}