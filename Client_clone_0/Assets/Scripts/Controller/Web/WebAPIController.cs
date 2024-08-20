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
            // 로딩 뷰 활성화
            ViewController.Instance.SetActiveView(VIEWTYPE.LOADING, true);

            // 요청 보내기
            yield return webRequest.SendWebRequest();

            // 로딩 뷰 비활성화
            ViewController.Instance.SetActiveView(VIEWTYPE.LOADING, false);

            // 요청 완료 후 처리
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                // 응답 결과 출력
                Debug.Log(webRequest.downloadHandler.text);
            }
        }
    }

    public IEnumerator PostRequest<T>(string uri, T data, Action<string> callback)
    {
        // 로딩 뷰 활성화
        ViewController.Instance.SetActiveView(VIEWTYPE.LOADING, true);

        // 객체를 JSON 문자열로 변환
        string jsonData = JsonConvert.SerializeObject(data);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        //Debug.Log($"Sending POST request to {uri} with data: {jsonData}");

        using (UnityWebRequest webRequest = new UnityWebRequest(uri, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // 요청 보내기
            yield return webRequest.SendWebRequest();

            string responseText = string.Empty;

            // 요청 완료 후 처리
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
                Debug.LogError($"Response Code: {webRequest.responseCode}");
                responseText = webRequest.downloadHandler.text;
                Debug.LogError($"Response: {responseText}");
            }
            else
            {
                // 응답 결과 출력
                responseText = webRequest.downloadHandler.text;
                //Debug.Log($"Response: {responseText}");
            }

            // 콜백 호출
            callback?.Invoke(responseText);
        }

        // 로딩 뷰 비활성화
        ViewController.Instance.SetActiveView(VIEWTYPE.LOADING, false);
    }
}
