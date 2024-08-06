using UnityEngine;
using Newtonsoft.Json;
using SharedCode.Model.HttpCommand;

public class WebLoginController : MonoBehaviour
{
    private static WebLoginController instance;

    public static WebLoginController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<WebLoginController>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("WebLoginControllerSingleton");
                    instance = singletonObject.AddComponent<WebLoginController>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }

    public void Init()
    {
        Debug.Log("WebLoginController Init Complete");
    }

    public void OnLoginResponse(string response)
    {
        // �α��� ���� ó�� ����
        //Debug.Log("Login response: " + response);

        var responseObject = JsonConvert.DeserializeObject<AccountLoginResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            // �α��� ���� ó��
            Global.Instance.standbyInfo.userEntity = JsonConvert.DeserializeObject<UserEntity>(responseObject.Userentity);
            //Debug.Log("�α� ����");
            WebSocketController.Instance.Init();
            ViewController.Instance.SetActiveView(VIEWTYPE.LOGIN, false);

            Packet packet = new Packet();

            int length = 0x01 + Utils.GetLength(Global.Instance.standbyInfo.userEntity.UserUID);

            packet.push((byte)Protocol.Login);
            packet.push(length);
            packet.push((byte)LoginRequestType.LoginRequest);
            packet.push(Global.Instance.standbyInfo.userEntity.UserUID);

            WebSocketController.Instance.SendToServer(packet);
        }
        else
        {
            // �α��� ���� ó��
            //Debug.Log("�� �α��� ���� ����");
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
    }

    public void OnAccountCreateResponse(string response)
    {
        // �α��� ���� ó�� ����
        //Debug.Log("Account Create response: " + response);

        var responseObject = JsonConvert.DeserializeObject<AccountCreateResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            // �α��� ���� ó��
            ViewController.Instance.SetActiveView(VIEWTYPE.SIGNUP, false);
            ViewController.Instance.SetActiveView(VIEWTYPE.LOGIN, true);
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
            //Debug.Log("ȸ������ ����");
        }
        else
        {
            // �α��� ���� ó��
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
            //Debug.Log("�� ȸ������ ���� ����");
        }
    }
    public void OnAccountLogoutResponse(string response)
    {
        var responseObject = JsonConvert.DeserializeObject<AccountCreateResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            // �α��� ���� ó��
            ViewController.Instance.SetActiveView(VIEWTYPE.LOGIN, true);
            Global.Instance.standbyInfo.Reset();
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);

            Packet packet = new Packet();

            int length = 0x01 + Utils.GetLength(Global.Instance.standbyInfo.userEntity.UserUID);

            packet.push((byte)Protocol.Login);
            packet.push(length);
            packet.push((byte)LoginRequestType.LogoutRequest);
            packet.push(Global.Instance.standbyInfo.userEntity.UserUID);

            WebSocketController.Instance.SendToServer(packet);

            WebSocketController.Instance.Disconnect();
            //Debug.Log("�α׾ƿ� ����");
        }
        else
        {
            // �α��� ���� ó��
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
            //Debug.Log("�� �α׾ƿ� ���� ����");
        }
    }

}


