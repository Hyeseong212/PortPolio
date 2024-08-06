using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebAPIController : MonoBehaviour
{
    private static WebAPIController instance;

    public static WebAPIController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<WebAPIController>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("WebAPIControllerSingleton");
                    instance = singletonObject.AddComponent<WebAPIController>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }

    public void Init()
    {
        Debug.Log("WebAPIController Init Complete");
    }

    public IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // �ε� �� Ȱ��ȭ
            ViewController.Instance.SetActiveView(VIEWTYPE.LOADING, true);

            // ��û ������
            yield return webRequest.SendWebRequest();

            // �ε� �� ��Ȱ��ȭ
            ViewController.Instance.SetActiveView(VIEWTYPE.LOADING, false);

            // ��û �Ϸ� �� ó��
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                // ���� ��� ���
                Debug.Log(webRequest.downloadHandler.text);
            }
        }
    }

    public IEnumerator PostRequest<T>(string uri, T data, Action<string> callback)
    {
        // �ε� �� Ȱ��ȭ
        ViewController.Instance.SetActiveView(VIEWTYPE.LOADING, true);

        // ��ü�� JSON ���ڿ��� ��ȯ
        string jsonData = JsonConvert.SerializeObject(data);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        //Debug.Log($"Sending POST request to {uri} with data: {jsonData}");

        using (UnityWebRequest webRequest = new UnityWebRequest(uri, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // ��û ������
            yield return webRequest.SendWebRequest();

            string responseText = string.Empty;

            // ��û �Ϸ� �� ó��
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
                Debug.LogError($"Response Code: {webRequest.responseCode}");
                responseText = webRequest.downloadHandler.text;
                Debug.LogError($"Response: {responseText}");
            }
            else
            {
                // ���� ��� ���
                responseText = webRequest.downloadHandler.text;
                //Debug.Log($"Response: {responseText}");
            }

            // �ݹ� ȣ��
            callback?.Invoke(responseText);
        }

        // �ε� �� ��Ȱ��ȭ
        ViewController.Instance.SetActiveView(VIEWTYPE.LOADING, false);
    }
}
