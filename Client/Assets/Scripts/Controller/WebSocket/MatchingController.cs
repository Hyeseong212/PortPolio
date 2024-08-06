using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchingController : MonoBehaviour
{
    private static MatchingController instance;
    public static MatchingController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MatchingController>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("MatchingControllerSingleton");
                    instance = singletonObject.AddComponent<MatchingController>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }
    public void Init()
    {
        Debug.Log($"{this.ToString()} Init Complete");
    }

    public void ProcessMatchingPacket(byte[] realData, int length)
    {
        if ((MatchProtocol)realData[0] == MatchProtocol.GameMatched)
        {
            WebSocketController.Instance.EnqueueDispatcher(() =>
            {
                Action okaction = () =>
                {
                    Packet packet = new Packet();

                    int length = 0x01 + 0x01 + Utils.GetLength(Global.Instance.standbyInfo.userEntity.UserUID);

                    packet.push((byte)Protocol.Match);
                    packet.push(length);
                    packet.push((byte)MatchProtocol.GameAccept);
                    packet.push(Global.Instance.standbyInfo.userEntity.UserUID);

                    WebSocketController.Instance.SendToServer(packet);

                    SceneManager.LoadScene("InGame");
                };
                Action Cancelaction = () =>
                {
                    Global.Instance.standbyInfo.gameType = GameType.Default;

                    Global.Instance.standbyInfo.isMatchingNow = false;

                    MainView mainView = FindObjectOfType<MainView>(true);

                    mainView.ChangeGameQueueStatus();
                };
                PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.OKCANCEL, true, "������ ��Ī �Ǿ����ϴ�!", okaction, Cancelaction);
            });
        }
        else if((MatchProtocol)realData[0] == MatchProtocol.GameRoomIP)
        {
            byte[] ipandport = realData.Skip(1).ToArray();
            SetSessionInfo(ipandport);
        }
    }
    private void SetSessionInfo(byte[] roomIp)
    {
        // IP �ּҴ� ù 4����Ʈ
        byte[] ipBytes = roomIp.Take(4).ToArray();
        string ipAddress = new IPAddress(ipBytes).ToString();

        // ��Ʈ�� ���� 2����Ʈ (16������ ��ȯ)
        byte[] portBytes = roomIp.Skip(4).Take(4).ToArray();
        int port = BitConverter.ToInt32(portBytes, 0);

        // sessionIPEndPoint ����
        Global.Instance.standbyInfo.sessionIPEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        Debug.Log($"Session IP: {ipAddress}, Port: {port}");
        WebSocketController.Instance.EnqueueDispatcher(() =>
        {
            InGameTCPController.Instance.Init(Global.Instance.standbyInfo.sessionIPEndPoint.Address.ToString(), Global.Instance.standbyInfo.sessionIPEndPoint.Port);
        });
    }
}
