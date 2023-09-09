using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Simulator))]
public class SimulatorEditor : Editor
{
    public string sessionId;

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Test"))
            SendPOSTRequest();
    }

    public void SendPOSTRequest()
    {
        string url = "http://localhost:5454" + "/sim/start";
        string postData = sessionId;

        SendRequest(url, postData);
    }

    IEnumerator SendRequest(string url, string postData)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);
        }
    }
}
