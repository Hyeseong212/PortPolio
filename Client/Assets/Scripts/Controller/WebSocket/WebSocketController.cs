using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WebSocketSharp;
using SharedCode.MessagePack;
using MessagePack;

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
        ConnectToServer(Global.Instance.serverIP, 5000); // ���� IP �ּ� �� ��Ʈ
    }

    void OnApplicationQuit()
    {
        // ���ø����̼� ���� �� Ŭ���̾�Ʈ ����
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
                EnqueueDispatcher(() => HandlePacket(e.RawData)); // ���ŵ� �޽����� �м��ϴ� �Լ� ȣ��
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

        var packet = MessagePackSerializer.Deserialize<Packet>(buffer);

        switch (packet.Protocol)
        {
            case (byte)Protocol.Chat:
                ChatController.Instance.ProcessChatPacket(packet.Data);
                break;
            case (byte)Protocol.Match:
                MatchingController.Instance.ProcessMatchingPacket(packet.Data);
                break;
            default:
                break;
        }
    }

    public void SendToServer<T>(T data)
    {
        try
        {
            if (ws != null && ws.IsAlive)
            {
                byte[] serializedData = MessagePackSerializer.Serialize(data);
                ws.Send(serializedData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Send error: {e.Message}");
        }
    }

    private static readonly Queue<Action> ExecutionQueue = new Queue<Action>();

    // ���� �����忡�� �۾��� �����ϴ� �޼���
    public void EnqueueDispatcher(Action action)
    {
        lock (ExecutionQueue)
        {
            ExecutionQueue.Enqueue(action);
        }
    }

    // �� �����Ӹ��� ť�� �ִ� ��� �۾��� �����ϴ� �޼���
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
