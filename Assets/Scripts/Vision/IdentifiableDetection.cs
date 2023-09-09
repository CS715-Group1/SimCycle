using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdentifiableDetection : MonoBehaviour
{
    Camera m_cam;
    [SerializeField] LayerMask identifiableLayer;

    [Header("Debug")]
    [SerializeField] GameObject blocked;
    [SerializeField] GameObject clear;
    [SerializeField] int maxDistance = 100;
    [SerializeField] float scaleFactor = 1.0f; 

    private void Start()
    {
        m_cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {

            ApplyTextureToPlane(DrawVisible(), blocked);
            // FIXME: why does this only take the bottom left corner?

            //m_cam.cullingMask = LayerMask.GetMask("Car");
            //Texture2D visionClear = ScreenCapture.CaptureScreenshotAsTexture();

            //m_cam.cullingMask = LayerMask.GetMask("Car") | LayerMask.GetMask("Wall");
            //Texture2D visionBlocked = ScreenCapture.CaptureScreenshotAsTexture();


            //ApplyTextureToPlane(visionBlocked, blocked);
            //ApplyTextureToPlane(visionClear, clear);


            //if (CompareTexture(visionBlocked, visionClear, 0.7f))
            //{
            //    Debug.Log("Seen");
            //}
            //else
            //{
            //    Debug.Log("Not seen");
            //}

        }
    }

    private Texture2D DrawVisible()
    {
        RenderTexture rt = new(m_cam.pixelWidth, m_cam.pixelHeight, 1);

        // Set the camera's target texture to capture the scene.
        m_cam.Render();

        // Create a new Texture2D and read the pixels from the RenderTexture.
        Texture2D texture = new(rt.width, rt.height);
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        // Create a new Texture2D to store the final result.
        Texture2D resultTexture = new(texture.width, texture.height);

        // Loop through the pixels and check if the corresponding objects are in the "Identifiable" layer.
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                bool identified = false;
                Color pixelColor = texture.GetPixel(x, y);

                // Raycast from the camera to the world point to determine the object's layer.
                Ray ray = m_cam.ScreenPointToRay(new Vector3(x, y, 0));

                if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance))
                {
                    IdentifiableObject identifiableObject = hitInfo.collider.GetComponent<IdentifiableObject>();
                    identified = identifiableObject != null;
                }

                if (identified)
                {
                    resultTexture.SetPixel(x, y, Color.white);
                }
                else
                {
                    // Make non-identifiable objects black
                    resultTexture.SetPixel(x, y, Color.black);
                }
            }
        }

        // Apply the changes to the result texture.
        resultTexture.Apply();

        // Clean up
        RenderTexture.active = null;
        rt.Release();

        return resultTexture;
    }

    private bool CompareTexture(Texture2D blocked, Texture2D clear, float percentage)
    {
        Color[] firstPix = clear.GetPixels();
        Color[] secondPix = blocked.GetPixels();
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
        Debug.Log(total);
        Debug.Log(visible);

        return ((visible / total) >= percentage);
    }

    private void ApplyTextureToPlane(Texture2D texture, GameObject obj)
    {
        if (obj != null)
        {
            // Set the material's main texture
            Material planeMaterial = obj.GetComponent<Renderer>().material;
            planeMaterial.mainTexture = texture;

            // Calculate the aspect ratio and adjust the plane's scale
            float aspectRatio = (float)texture.width / texture.height;
            obj.transform.localScale = new Vector3(scaleFactor * aspectRatio, 1, scaleFactor);




            // Define the file path where you want to save the image
            string filePath = Application.dataPath + "/Images/" + obj.name + ".png";

            SaveImage(texture, filePath);
        }
    }

    private void SaveImage(Texture2D texture, string filePath)
    {
        // Convert the texture to a byte array in PNG format
        byte[] bytes = texture.EncodeToPNG();

        // Write the byte array to a file
        System.IO.File.WriteAllBytes(filePath, bytes);
    }
}