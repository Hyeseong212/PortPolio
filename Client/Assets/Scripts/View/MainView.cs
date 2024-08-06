using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WebServer.Model.HttpCommand;

public class MainView : MonoBehaviour
{
    Stopwatch stopwatch = new Stopwatch();

    public InputField inputField;

    public GameObject ChatObject;
    public GameObject ChatParentObject;
    public ScrollRect chatScrollbar;
    public Dropdown dropdown;

    private StreamWriter writer;

    [SerializeField] Button sendBtn;
    [SerializeField] Button logoutBtn;
    [SerializeField] Button guildOpenBtn;
    [SerializeField] Button gameStartBtn;
    [SerializeField] Button gameStopBtn;

    [SerializeField] Text queueTimerText;

    ChatStatus ChatStatus = ChatStatus.ENTIRE;

    private void Start()
    {
        sendBtn.onClick.AddListener(delegate
        {
            SendMessage();
        });
        gameStartBtn.onClick.AddListener(delegate
        {
            ViewController.Instance.SetActiveView(VIEWTYPE.GAMESTART, true);
        });
        gameStopBtn.onClick.AddListener(delegate
        {
            //Packet packet = new Packet();

            //int length = 0x01 + 0x01 + Utils.GetLength(Global.Instance.standbyInfo.userEntity.UserUID);

            //packet.push((byte)Protocol.Match);
            //packet.push(length);
            //packet.push((byte)MatchProtocol.MatchStop);
            //packet.push((byte)Global.Instance.standbyInfo.gameType);
            //packet.push(Global.Instance.standbyInfo.userEntity.UserUID);

            //TCPController.Instance.SendToServer(packet);

            Global.Instance.standbyInfo.gameType = GameType.Default;

            Global.Instance.standbyInfo.isMatchingNow = false;

            ChangeGameQueueStatus();
        });
        guildOpenBtn.onClick.AddListener(delegate
        {
            ViewController.Instance.SetActiveView(VIEWTYPE.GUILD, true);
        });
        dropdown.onValueChanged.AddListener(delegate
        {
            switch (dropdown.value)
            {
                case 0:
                    ChatStatus = ChatStatus.ENTIRE;
                    break;
                case 1:
                    ChatStatus = ChatStatus.WHISPER;
                    break;
                case 2:
                    ChatStatus = ChatStatus.GUILD;
                    break;
            }

        });
        logoutBtn.onClick.AddListener(delegate
        {
            Logout();
        });
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            SendMessage();

        queueTimerText.text = string.Format("{0:mm\\:ss}", stopwatch.Elapsed);
    }

    public void SendMessage()
    {
        if (ChatStatus == ChatStatus.ENTIRE)
        {
            var message = new Packet();

            int length = 0x01 + Utils.GetLength(Global.Instance.standbyInfo.userEntity.UserUID) + Utils.GetLength(inputField.text);

            message.push((byte)Protocol.Chat);
            message.push(length);
            message.push((byte)ChatStatus);
            message.push(Global.Instance.standbyInfo.userEntity.UserUID);
            message.push(inputField.text);
            WebSocketController.Instance.SendToServer(message);
            inputField.text = "";
        }
        //else if (ChatStatus == ChatStatus.WHISPER)
        //{
        //    //이제 여기에서 보내는 유저 이름을 알려줘야됨
        //    var message = new Packet();

        //    int length = 0x01 + Utils.GetLength(Global.Instance.standbyInfo.userEntity.UserUID) + Utils.GetLength(ReceiveUID) + Utils.GetLength(inputField.text);

        //    message.push((byte)Protocol.Chat);
        //    message.push(length);
        //    message.push((byte)ChatStatus);
        //    message.push(Global.Instance.standbyInfo.userEntity.UserUID);
        //    message.push(ReceiveUID);
        //    message.push(inputField.text);
        //    TCPController.Instance.SendToServer(message);
        //    inputField.text = "";
        //}
        //else if (ChatStatus == ChatStatus.GUILD)
        //{
        //    //이제 여기에서 보내는 유저 이름을 알려줘야됨
        //    var message = new Packet();

        //    int length = 0x01 + Utils.GetLength(Global.Instance.standbyInfo.userEntity.UserUID) + Utils.GetLength(Global.Instance.standbyInfo.userEntity.guildUID) + Utils.GetLength(inputField.text);

        //    message.push((byte)Protocol.Chat);
        //    message.push(length);
        //    message.push((byte)ChatStatus);
        //    message.push(Global.Instance.standbyInfo.userEntity.UserUID);
        //    message.push(Global.Instance.standbyInfo.userEntity.guildUID);
        //    message.push(inputField.text);
        //    TCPController.Instance.SendToServer(message);
        //    inputField.text = "";
        //}
    }
    public void Logout()
    {
        AccountLogoutRequest accountLogoutRequest = new AccountLogoutRequest();
        accountLogoutRequest.AccountId = Global.Instance.standbyInfo.userEntity.UserUID;
        StartCoroutine(WebAPIController.Instance.PostRequest<AccountLogoutRequest>(Global.Instance.apiRequestUri + "Account/Logout", accountLogoutRequest, WebLoginController.Instance.OnAccountLogoutResponse));

        if (Global.Instance.standbyInfo.isMatchingNow)
        {
            Global.Instance.standbyInfo.gameType = GameType.Default;

            MainView mainView = FindAnyObjectByType<MainView>();

            Global.Instance.standbyInfo.isMatchingNow = false;

            mainView.ChangeGameQueueStatus();
            List<Text> textObjects = mainView.ChatParentObject.GetComponentsInChildren<Text>().ToList();

            for (int i = 0; i < textObjects.Count; i++)
            {
                Destroy(textObjects[i]);
            }
        }
    }
    public void QueueTimerSet()
    {
        ChangeGameQueueStatus();
    }
    public void ChangeGameQueueStatus()
    {
        if (Global.Instance.standbyInfo.isMatchingNow)
        {
            stopwatch.Start();

            queueTimerText.gameObject.SetActive(true);

            gameStartBtn.gameObject.SetActive(false);
            gameStopBtn.gameObject.SetActive(true);
        }
        else
        {
            stopwatch.Reset();
            stopwatch.Stop();

            queueTimerText.gameObject.SetActive(false);

            gameStartBtn.gameObject.SetActive(true);
            gameStopBtn.gameObject.SetActive(false);
        }
    }
}
