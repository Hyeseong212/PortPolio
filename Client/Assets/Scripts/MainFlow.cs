using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

public class MainFlow : MonoBehaviour
{
    private void Awake()
    {
        Global.Instance.Init();
        ViewController.Instance.Init();

        ChatController.Instance.Init();
        PopupController.Instance.Init();
        MatchingController.Instance.Init();
        WebAPIController.Instance.Init();
        WebGuildController.Instance.Init();
        ///////////╪рдо©К/////////////
        //GuildController.Instance.Init();
        //LoginController.Instance.Init();
        //TCPController.Instance.Init();
    }
}
