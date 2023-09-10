using UnityEngine;
using System.Collections;

public class DisplayPlane : MonoBehaviour
{

    [SerializeField] float scaleFactor = 1.0f;

    public void ApplyTexture(Texture2D texture)
    {
        // Set the material's main texture
        Material planeMaterial = GetComponent<Renderer>().material;
        planeMaterial.mainTexture = texture;

        // Calculate the aspect ratio and adjust the plane's scale
        float aspectRatio = (float)texture.width / texture.height;
        transform.localScale = new Vector3(scaleFactor * aspectRatio, 1, scaleFactor);




        // Define the file path where you want to save the image
        string filePath = Application.dataPath + "/Images/" + name + ".png";

        SaveImage(texture, filePath);
    }

    private void SaveImage(Texture2D texture, string filePath)
    {
        // Convert the texture to a byte array in PNG format
        byte[] bytes = texture.EncodeToPNG();

        // Write the byte array to a file
        System.IO.File.WriteAllBytes(filePath, bytes);
    }
}
