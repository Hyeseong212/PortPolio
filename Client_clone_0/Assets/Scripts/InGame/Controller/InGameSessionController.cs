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

            int length = 0x01 + Utils.GetLength(Global.Instance.StandbyInfo.UserEntity.UserUID);

            packet.push((byte)InGameProtocol.SessionInfo);
            packet.push(length);
            packet.push((byte)SessionInfo.PlayerNum);
            packet.push(Global.Instance.StandbyInfo.UserEntity.UserUID);
            //로딩 1단계완료 처리

            InGameTCPController.Instance.SendToInGameServer(packet);

            thisPlayerInfo.IsSyncOK = true;
        }
        else if ((SessionInfo)realData[0] == SessionInfo.PlayerNum)
        {
            //플레이어 몇번째플레이언지
            thisPlayerInfo.PlayerNum = BitConverter.ToInt32(realData, 1);
            Global.Instance.StaticLog($"PlayerNum : {thisPlayerInfo.PlayerNum }");
            CharacterTrController.Instance.Init();
            thisPlayerInfo.IsPlayerInfoOK = true;
            //로딩 2단계완료 처리
            Packet packet = new Packet();

            int length = 0x01 + Utils.GetLength(thisPlayerInfo.PlayerNum) + Utils.GetLength(Global.Instance.StandbyInfo.SelectedCharacterId);

            packet.push((byte)InGameProtocol.GameInfo);
            packet.push(length);
            packet.push((byte)GameInfo.CharacterData);
            packet.push(thisPlayerInfo.PlayerNum);
            packet.push(Global.Instance.StandbyInfo.SelectedCharacterId);

            InGameTCPController.Instance.SendToInGameServer(packet);
            //로딩 2단계완료 처리

        }
        else if ((SessionInfo)realData[0] == SessionInfo.AllPlayerLoadingOK)
        {
            thisPlayerInfo.IsAllPlayerLoadingOK = true;
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
