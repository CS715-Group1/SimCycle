using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdentifiableDetection : MonoBehaviour
{
    [Header("Vision Parameters")]
    [SerializeField] int maxDistance = 100;
    [SerializeField] float recognizableThreshold = 0.7f;

    [Header("Detection Settings")]
    [SerializeField] int maxDimension = 640;

    [Header("Debug")]
    [SerializeField] DisplayPlane blockedDisplay;
    [SerializeField] DisplayPlane perfectDisplay;

    Camera m_cam;

    private void Start()
    {
        m_cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {
            // TODO: check for multiple
            foreach (IdentifiableObject obj in Resources.FindObjectsOfTypeAll(typeof(IdentifiableObject)) as IdentifiableObject[])
            {
                if (!obj.isActiveAndEnabled) continue;

                CheckVisibility(obj);
            }
        }
    }

    private void CheckVisibility(IdentifiableObject obj)
    {
        // Isolate object for detection
        obj.gameObject.layer = LayerMask.NameToLayer("Detecting");

        // Set the camera's target texture to capture the scene.
        m_cam.Render();

        RenderTexture rt = new(m_cam.pixelWidth, m_cam.pixelHeight, 1);

        // Create a new Texture2D and read the pixels from the RenderTexture.
        Texture2D texture = new(rt.width, rt.height);
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();


        // // TODO: restrict resolution of processed image
        //float scaleFactor = 1;

        //if (m_cam.pixelWidth > maxDimension)
        //{
        //    scaleFactor = (float)maxDimension / m_cam.pixelWidth;
        //}
        //else if (m_cam.pixelHeight > maxDimension)
        //{
        //    scaleFactor = (float)maxDimension / m_cam.pixelWidth;
        //}

        //texture = ScaleTexture(texture, (int)(texture.width * scaleFactor), (int)(texture.height * scaleFactor));

        //Debug.Log(texture.width + "," + texture.height);


        Texture2D blockedVision = new(texture.width, texture.height);
        Texture2D perfectVision = new(texture.width, texture.height);

        // Loop through the pixels and check if the corresponding objects are in the "Identifiable" layer.
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
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
        m_cam.targetTexture = null;
        rt.Release();


        // Output Visibility
        if (blockedDisplay != null) blockedDisplay.ApplyTexture(blockedVision);
        if (perfectDisplay != null) perfectDisplay.ApplyTexture(perfectVision);

        if (IsRecognizable(blockedVision, perfectVision, recognizableThreshold))
        {
            Debug.Log($"Camera '{m_cam.name}' recognized object '{obj.name}'");
        }
        else
        {
            Debug.Log($"Camera '{m_cam.name}' did not recognize object '{obj.name}'");
        }

        // Finished detection, remove object from isolation
        // FIXME: seems to immediately set even before code above has run.
        obj.gameObject.layer = LayerMask.NameToLayer("Identifiable");
    }

    private Texture2D FindBlockedIdentifiable(Texture2D texture, int x, int y, IdentifiableObject obj)
    {
        bool identified = false;

        // Raycast from the camera to the world point to determine the object's layer.
        Ray ray = m_cam.ScreenPointToRay(new Vector3(x, y, 0));

        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance))
        {
            IdentifiableObject identifiableObject = hitInfo.collider.GetComponent<IdentifiableObject>();

            identified = identifiableObject != null && identifiableObject.Equals(obj);
        }

        if (identified)
        {
            texture.SetPixel(x, y, Color.white);
        }
        else
        {
            texture.SetPixel(x, y, Color.black);
        }

        return texture;
    }

    private Texture2D FindAllIdentifiable(Texture2D texture, int x, int y)
    {
        // Raycast from the camera to the world point to determine the object's layer.
        Ray ray = m_cam.ScreenPointToRay(new Vector3(x, y, 0));

        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, LayerMask.GetMask("Detecting")))
        {
            texture.SetPixel(x, y, Color.white);
        }
        else
        {
            texture.SetPixel(x, y, Color.black);
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
            if (perfectPixels[i] != Color.black)
            {
                total++;
            }

            if (blockedPixels[i] != Color.black)
            {
                visible++;
            }
        }

        float ratio = (float)visible / total;

        Debug.Log($"Visible: {visible} / Total: {total} = {ratio}");

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
}