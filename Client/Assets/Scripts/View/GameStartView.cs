using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartView : MonoBehaviour
{
    [SerializeField] Button StartQueueNormalGame;

    [SerializeField] Button StartQueueRankGame;

    private void Start()
    {
        StartQueueNormalGame.onClick.AddListener(delegate{
            SelectNormalGameStart();
        });
        StartQueueRankGame.onClick.AddListener(delegate {
            SelectRankGameStart();
        });
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            this.gameObject.SetActive(false);
        }
    }
    private void SelectNormalGameStart()
    {
        Global.Instance.standbyInfo.isMatchingNow = true;
        Global.Instance.standbyInfo.gameType = GameType.Normal;

        Packet packet = new Packet();

        int length = 0x01 + 0x01 + Utils.GetLength(Global.Instance.standbyInfo.userEntity.UserUID);

        packet.push((byte)Protocol.Match);
        packet.push(length);
        packet.push((byte)MatchProtocol.MatchStart);
        packet.push((byte)Global.Instance.standbyInfo.gameType);
        packet.push(Global.Instance.standbyInfo.userEntity.UserUID);

        WebSocketController.Instance.SendToServer(packet);

        MainView mainView = FindObjectOfType<MainView>(true);
        mainView.QueueTimerSet();


        this.gameObject.SetActive(false);
    }
    private void SelectRankGameStart()
    {
        Global.Instance.standbyInfo.isMatchingNow = true;
        Global.Instance.standbyInfo.gameType = GameType.Rank;

        Packet packet = new Packet();

        int length = 0x01 + 0x01 + Utils.GetLength(Global.Instance.standbyInfo.userEntity.UserUID);

        packet.push((byte)Protocol.Match);
        packet.push(length);
        packet.push((byte)MatchProtocol.MatchStart);
        packet.push((byte)Global.Instance.standbyInfo.gameType);
        packet.push(Global.Instance.standbyInfo.userEntity.UserUID);

        WebSocketController.Instance.SendToServer(packet);

        MainView mainView = FindObjectOfType<MainView>(true);
        mainView.QueueTimerSet();

        this.gameObject.SetActive(false);
    }

}
