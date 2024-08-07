using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using SharedCode.MessagePack;
using MessagePack;

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

    public void ProcessMatchingPacket(byte[] realData)
    {
        var packet = MessagePackSerializer.Deserialize<Packet>(realData);

        if ((MatchProtocol)packet.Data[0] == MatchProtocol.GameMatched)
        {
            WebSocketController.Instance.EnqueueDispatcher(() =>
            {
                Action okaction = () =>
                {
                    var acceptPacket = new Packet
                    {
                        Protocol = (byte)Protocol.Match,
                        Data = new byte[] { (byte)MatchProtocol.GameAccept }
                            .Concat(BitConverter.GetBytes(Global.Instance.standbyInfo.userEntity.UserUID))
                            .ToArray()
                    };

                    WebSocketController.Instance.SendToServer(acceptPacket);
                    SceneManager.LoadScene("InGame");
                };
                Action Cancelaction = () =>
                {
                    Global.Instance.standbyInfo.gameType = GameType.Default;
                    Global.Instance.standbyInfo.isMatchingNow = false;
                    MainView mainView = FindObjectOfType<MainView>(true);
                    mainView.ChangeGameQueueStatus();
                };
                PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.OKCANCEL, true, "게임이 매칭 되었습니다!", okaction, Cancelaction);
            });
        }
        else if ((MatchProtocol)packet.Data[0] == MatchProtocol.GameRoomIP)
        {
            var ipPacket = MessagePackSerializer.Deserialize<GameRoomIPPacket>(packet.Data);
            SetSessionInfo(ipPacket);
        }
    }

    private void SetSessionInfo(GameRoomIPPacket roomIp)
    {
        string ipAddress = new IPAddress(roomIp.IP).ToString();
        int port = roomIp.Port;
        Global.Instance.standbyInfo.sessionIPEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        Debug.Log($"Session IP: {ipAddress}, Port: {port}");
        WebSocketController.Instance.EnqueueDispatcher(() =>
        {
            InGameTCPController.Instance.Init(Global.Instance.standbyInfo.sessionIPEndPoint.Address.ToString(), Global.Instance.standbyInfo.sessionIPEndPoint.Port);
        });
    }
}
