using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdentifiableDetection : MonoBehaviour
{
    [SerializeField] LayerMask identifiableLayer;

    [Header("Vision Parameters")]
    [SerializeField] int maxDistance = 100;
    [SerializeField] float recognizableThreshold = 0.7f;

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

            CheckVisibility();
            // FIXME: why does this only take the bottom left corner?

            //m_cam.cullingMask = LayerMask.GetMask("Car");
            //Texture2D visionClear = ScreenCapture.CaptureScreenshotAsTexture();

            //m_cam.cullingMask = LayerMask.GetMask("Car") | LayerMask.GetMask("Wall");
            //Texture2D visionBlocked = ScreenCapture.CaptureScreenshotAsTexture();


            //ApplyTextureToPlane(visionBlocked, blockedDisplay);
            //ApplyTextureToPlane(visionClear, clear);


            

        }
    }

    private void CheckVisibility()
    {
        RenderTexture rt = new(m_cam.pixelWidth, m_cam.pixelHeight, 1);

        // Set the camera's target texture to capture the scene.
        m_cam.Render();

        // Create a new Texture2D and read the pixels from the RenderTexture.
        Texture2D texture = new(rt.width, rt.height);
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        Texture2D blockedVision = new(texture.width, texture.height);
        Texture2D perfectVision = new(texture.width, texture.height);

        // Loop through the pixels and check if the corresponding objects are in the "Identifiable" layer.
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                blockedVision = FindBlockedIdentifiable(blockedVision, x, y);
                perfectVision = FindAllIdentifiable(perfectVision, x, y);
            }
        }

        // Apply the changes to the result texture.
        blockedVision.Apply();

        // Clean up
        RenderTexture.active = null;
        rt.Release();


        // Output Visibility
        if (blockedDisplay != null) blockedDisplay.ApplyTexture(blockedVision);
        if (perfectDisplay != null) perfectDisplay.ApplyTexture(perfectVision);

        if (IsRecognizable(blockedVision, perfectVision, recognizableThreshold))
        {
            Debug.Log(m_cam.name + ": Recognized");
        }
        else
        {
            Debug.Log(m_cam.name + ": Not Recognized");
        }
    }

    private Texture2D FindBlockedIdentifiable(Texture2D texture, int x, int y)
    {
        bool identified = false;

        // Raycast from the camera to the world point to determine the object's layer.
        Ray ray = m_cam.ScreenPointToRay(new Vector3(x, y, 0));

        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance))
        {
            IdentifiableObject identifiableObject = hitInfo.collider.GetComponent<IdentifiableObject>();

            identified = identifiableObject != null;
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

        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, identifiableLayer))
        {
            texture.SetPixel(x, y, Color.white);
        }
        else
        {
            texture.SetPixel(x, y, Color.black);
        }

        return texture;
    }

    private bool IsRecognizable(Texture2D blockedDisplay, Texture2D clear, float threshold)
    {
        Color[] firstPix = clear.GetPixels();
        Color[] secondPix = blockedDisplay.GetPixels();
        int visible = 0;
        int total = 0;
        for (int i = 0; i < firstPix.Length; i++)
        {

            if (firstPix[i] != Color.black)
            {
                total++;
            }

            if (secondPix[i] != Color.black)
            {
                visible++;
            }
        }

        if (total == 0)
        {
            return false;
        }

        float percentage = (float)visible / total;
        Debug.Log(percentage);

        return percentage >= threshold;
    }
}