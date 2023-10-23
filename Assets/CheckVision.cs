using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckVision : MonoBehaviour
{

    Camera m_cam;
    [SerializeField] RenderTexture blocked;
    [SerializeField] RenderTexture clear;


    private void Start()
    {
        m_cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {
            Texture2D VisonBlocked = ScreenCapture.CaptureScreenshotAsTexture();
            
            m_cam.cullingMask = LayerMask.GetMask("Car");
            Texture2D visionClear = ScreenCapture.CaptureScreenshotAsTexture();

            m_cam.cullingMask = LayerMask.GetMask("Car") | LayerMask.GetMask("Wall");


            if (CompareTexture(VisonBlocked, visionClear, 0.7f))
            {
                Debug.Log("Seen");
            }
            else
            {
                Debug.Log("Not seen");
            }
        }
    }

    private bool CompareTexture(Texture2D blocked, Texture2D clear, float percentage)
    {
        Color[] firstPix = clear.GetPixels();
        Color[] secondPix =  blocked.GetPixels();
        int visible = 0;
        int total = 0;
        for (int i = 0; i < firstPix.Length; i++)
        {

            if(firstPix[i] != Color.black)
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
}
