using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    private static LoginController instance;
    public static LoginController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LoginController>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("LoginControllerSingleton");
                    instance = singletonObject.AddComponent<LoginController>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }
    public void Init()
    {
        Debug.Log("LoginController Init Complete");
    }
    public void ProcessLoginPacket(byte[] realData) 
    {
        if ((LoginRequestType)realData[0] == LoginRequestType.LoginRequest)
        {
            if ((ResponseType)realData[1] == ResponseType.Success)
            {
                string userData = Encoding.UTF8.GetString(realData.Skip(2).ToArray());
                UserEntity user = JsonConvert.DeserializeObject<UserEntity>(userData);

                Global.Instance.standbyInfo.userEntity = user;
                TCPController.Instance.EnqueueDispatcher(() =>
                {
                    Debug.Log("로긴 성공");
                    ViewController.Instance.SetActiveView(VIEWTYPE.LOGIN, false);
                });
            }
            else if ((ResponseType)realData[1] == ResponseType.Fail)
            {
                Debug.Log("응 로그인 실패 ㅅㄱ");
            }
        }
        else if ((LoginRequestType)realData[0] == LoginRequestType.LogoutRequest)
        {
            if ((ResponseType)realData[1] == ResponseType.Success)
            {
                //Debug.Log(message);
                TCPController.Instance.EnqueueDispatcher(() =>
                {
                    ViewController.Instance.SetActiveView(VIEWTYPE.LOGIN, true);
                    Global.Instance.standbyInfo.Reset();
                    Debug.Log("로그아웃 성공");
                });
            }
            else if ((ResponseType)realData[1] == ResponseType.Fail)
            {
                Debug.Log("응 로그아웃 실패 ㅅㄱ");
            }
        }
        else if ((LoginRequestType)realData[0] == LoginRequestType.SignupRequest)
        {
            if ((ResponseType)realData[1] == ResponseType.Success)
            {
                //Debug.Log(message);
                TCPController.Instance.EnqueueDispatcher(() =>
                {
                    ViewController.Instance.SetActiveView(VIEWTYPE.SIGNUP, false);
                    ViewController.Instance.SetActiveView(VIEWTYPE.LOGIN, true);
                    //PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, 1, null, null);
                    Debug.Log("회원가입 성공");
                });
            }
            else if ((ResponseType)realData[1] == ResponseType.Fail)
            {
                TCPController.Instance.EnqueueDispatcher(() =>
                {
                    //PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, 0, null,null);
                    Debug.Log("응 회원가입 실패 ㅅㄱ");
                });
            }
        }
    }
    public void Logout()
    {

        //Packet packet = new Packet();

        //int length2 = 0x01 + 0x01 + Utils.GetLength(Global.Instance.standbyInfo.userEntity.UserUID);

        //packet.push((byte)Protocol.Match);
        //packet.push(length2);
        //packet.push((byte)MatchProtocol.MatchStop);
        //packet.push((byte)Global.Instance.standbyInfo.gameType);
        //packet.push(Global.Instance.standbyInfo.userEntity.UserUID);

        //TCPController.Instance.SendToServer(packet);


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


        ////////////////소켓(Legacy)//////////////////

        //var message = new Packet();

        //int length = 0x01 + Utils.GetLength(Global.Instance.standbyInfo.userEntity.UserUID);

        //message.push((byte)Protocol.Login);
        //message.push(length);
        //message.push((byte)LoginRequestType.LogoutRequest);
        //message.push(Global.Instance.standbyInfo.userEntity.UserUID);
        //TCPController.Instance.SendToServer(message);

        //mainView.QueueTimerSet();
    }
}
