using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPController : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private const int bufferSize = 4096;

    private static TCPController instance;
    public static TCPController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TCPController>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("TCPControllerSingleton");
                    instance = singletonObject.AddComponent<TCPController>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }

    public void Init()
    {
        Debug.Log("TCPController Init Complete");
        ConnectToServer(Global.Instance.serverIP, 9000); // 서버 IP 주소 및 포트
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
            client = new TcpClient(serverIp, serverPort);
            stream = client.GetStream();
            receiveThread = new Thread(new ThreadStart(ReceivePackets));
            receiveThread.Start();
            Debug.Log("Connected to server.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Connection error: {e.Message}");
        }
    }

    public void Disconnect()
    {
        if (receiveThread != null) receiveThread.Abort();
        if (stream != null) stream.Close();
        if (client != null) client.Close();
        Debug.Log("Disconnected from server.");
    }

    private void ReceivePackets()
    {
        byte[] buffer = new byte[Packet.buffersize];

        //try
        //{
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    HandlePacket(buffer); // 수신된 메시지를 분석하는 함수 호출
                }
            }
        //}
        //catch (Exception e)
        //{
        //    Debug.LogError($"Receive error: {e.Message}");
        //}
    }
    private void HandlePacket(byte[] buffer)
    {
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
            case (byte)Protocol.Login:
                LoginController.Instance.ProcessLoginPacket(realData);
                break;
            case (byte)Protocol.Guild:
                GuildController.Instance.ProcessGuildPacket(realData);
                break;
            case (byte)Protocol.Chat:
                ChatController.Instance.ProcessChatPacket(realData, length);
                break;
            case (byte)Protocol.Match:
                MatchingController.Instance.ProcessMatchingPacket(realData, length);
                break;

            default:
                //Debug.Log("Something Come in");
                //string str = "";
                //for (int i = 0; i < buffer.Length; i++)
                //{
                //    if (i != buffer.Length - 1)
                //        str += buffer[i].ToString() + "|";
                //    else
                //        str += buffer[i].ToString();
                //}
                //Debug.Log(str);
                break;
        }
    }


    public void SendToServer(Packet data)
    {
        try
        {
            if (stream != null)
            {
                stream.Write(data.buffer, 0, data.position);
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