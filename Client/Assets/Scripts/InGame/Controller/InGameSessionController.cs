using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameSessionController : MonoBehaviour
{
    private static InGameSessionController instance;
    public static InGameSessionController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InGameSessionController>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("InGameSessionControllerSingleton");
                    instance = singletonObject.AddComponent<InGameSessionController>();
                    var singletonParent = FindObjectOfType<InGameSingleton>();
                    Instantiate(singletonObject, singletonParent.transform);
                }
            }
            return instance;
        }
    }

    public InGameSessionInfo thisPlayerInfo;

    public void Init()
    {
        Global.Instance.StaticLog($"{this.ToString()} Init Complete");
        thisPlayerInfo = new InGameSessionInfo();
    }

    public void ProcessSessionPacket(byte[] realData)
    {
        if ((SessionInfo)realData[0] == SessionInfo.SessionSyncOK)
        {
            Global.Instance.StaticLog("SessionSyncOK Packet");

            Packet packet = new Packet();

            int length = 0x01 + Utils.GetLength(Global.Instance.standbyInfo.userEntity.UserUID);

            packet.push((byte)InGameProtocol.SessionInfo);
            packet.push(length);
            packet.push((byte)SessionInfo.PlayerNum);
            packet.push(Global.Instance.standbyInfo.userEntity.UserUID);
            //�ε� 1�ܰ�Ϸ� ó��

            InGameTCPController.Instance.SendToInGameServer(packet);

            thisPlayerInfo.isSyncOK = true;
        }
        else if ((SessionInfo)realData[0] == SessionInfo.PlayerNum)
        {
            //�÷��̾� ���°�÷��̾���
            thisPlayerInfo.playerNum = BitConverter.ToInt32(realData, 1);
            Global.Instance.StaticLog($"PlayerNum : {thisPlayerInfo.playerNum }");
            CharacterTrController.Instance.Init();
            thisPlayerInfo.isPlayerInfoOK = true;
            //�ε� 2�ܰ�Ϸ� ó��
            Packet packet = new Packet();

            int length = 0x01 + Utils.GetLength(thisPlayerInfo.playerNum) + Utils.GetLength(Global.Instance.standbyInfo.SelectedCharacterId);

            packet.push((byte)InGameProtocol.GameInfo);
            packet.push(length);
            packet.push((byte)GameInfo.CharacterData);
            packet.push(thisPlayerInfo.playerNum);
            packet.push(Global.Instance.standbyInfo.SelectedCharacterId);

            InGameTCPController.Instance.SendToInGameServer(packet);
            //�ε� 2�ܰ�Ϸ� ó��

        }
        else if ((SessionInfo)realData[0] == SessionInfo.AllPlayerLoadingOK)
        {
            thisPlayerInfo.isAllPlayerLoadingOK = true;
        }
        else if ((SessionInfo)realData[0] == SessionInfo.GameEnd)
        {
            var EndGameView = FindObjectOfType<GameEndView>(true);
            EndGameView.gameObject.SetActive(true);
        }
        else if ((SessionInfo)realData[0] == SessionInfo.Ban)
        {
            InGameTCPController.Instance.Disconnect();
            SceneManager.LoadScene("Lobby");
        }
    }

}