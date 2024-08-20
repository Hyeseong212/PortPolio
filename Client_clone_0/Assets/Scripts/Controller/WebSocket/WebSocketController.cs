using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WebSocketSharp;

public class WebSocketController : MonoBehaviour
{
    private WebSocket ws;
    private static WebSocketController instance;

    public static WebSocketController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<WebSocketController>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("WebSocketControllerSingleton");
                    instance = singletonObject.AddComponent<WebSocketController>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }

    public void Init()
    {
        Debug.Log("WebSocketController Init Complete");
        ConnectToServer(Global.Instance.ServerIP, 5000); // 서버 IP 주소 및 포트
    }

    void OnApplicationQuit()
    {
        // 애플리케이션 종료 시 클라이언트 종료
        Disconnect();
    }

    public void ConnectToServer(string serverIp, int serverPort)
    {
        try
        {
            string serverAddress = $"ws://{serverIp}:{serverPort}/ws";
            ws = new WebSocket(serverAddress);

            ws.OnOpen += (sender, e) =>
            {
                Debug.Log("Connected to server.");
            };

            ws.OnMessage += (sender, e) =>
            {
                EnqueueDispatcher(() => HandlePacket(e.RawData)); // 수신된 메시지를 분석하는 함수 호출
            };

            ws.OnError += (sender, e) =>
            {
                Debug.LogError($"WebSocket error: {e.Message}");
            };

            ws.OnClose += (sender, e) =>
            {
                Debug.Log("WebSocket Closed: " + e.Reason);
            };

            ws.Connect();
        }
        catch (Exception e)
        {
            Debug.LogError($"Connection error: {e.Message}");
        }
    }

    public void Disconnect()
    {
        if (ws != null)
        {
            ws.Close();
            Debug.Log("Disconnected from server.");
        }
    }

    private void HandlePacket(byte[] buffer)
    {
        if (buffer.Length < 1)
        {
            return;
        }
        byte protocol = buffer[0];
        byte[] lengthBytes = new byte[4];

        try
        {
            for (int i = 0; i < 4; i++)
            {
                lengthBytes[i] = buffer[i + 1];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        int length = BitConverter.ToInt32(lengthBytes, 0);
        byte[] realData = new byte[length];

        try
        {
            for (int i = 0; i < length; i++)
            {
                realData[i] = buffer[i + 5];
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        switch (protocol)
        {
            case (byte)Protocol.Chat:
                ChatController.Instance.ProcessChatPacket(realData, length);
                break;
            case (byte)Protocol.Match:
                MatchingController.Instance.ProcessMatchingPacket(realData, length);
                break;
            default:
                break;
        }
    }

    public void SendToServer(Packet data)
    {
        byte[] sendData = new byte[data.Position];
        for(int i = 0; i < sendData.Length; i++)
        {
            sendData[i] = data.Buffer[i];
        }
        try
        {
            if (ws != null && ws.IsAlive)
            {
                ws.Send(sendData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Send error: {e.Message}");
        }
    }

    private static readonly Queue<Action> ExecutionQueue = new Queue<Action>();

    // 메인 스레드에서 작업을 예약하는 메서드
    public void EnqueueDispatcher(Action action)
    {
        lock (ExecutionQueue)
        {
            ExecutionQueue.Enqueue(action);
        }
    }

    // 매 프레임마다 큐에 있는 모든 작업을 실행하는 메서드
    public void ExecutePending()
    {
        lock (ExecutionQueue)
        {
            while (ExecutionQueue.Count > 0)
            {
                ExecutionQueue.Dequeue().Invoke();
            }
        }
    }

    private void Update()
    {
        ExecutePending();
    }
}
