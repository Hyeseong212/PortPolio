using UnityEngine;
using Newtonsoft.Json;
using SharedCode.Model.HttpCommand;
using SharedCode.MessagePack;
using MessagePack;

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
        var responseObject = JsonConvert.DeserializeObject<AccountLoginResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            Global.Instance.standbyInfo.userEntity = JsonConvert.DeserializeObject<UserEntity>(responseObject.Userentity);
            WebSocketController.Instance.Init();
            ViewController.Instance.SetActiveView(VIEWTYPE.LOGIN, false);

            var loginRequestPacket = new LoginRequestPacket
            {
                RequestType = LoginRequestType.LoginRequest,
                UserId = Global.Instance.standbyInfo.userEntity.UserUID
            };

            var packet = new Packet
            {
                Protocol = (byte)Protocol.Login,
                Data = MessagePackSerializer.Serialize(loginRequestPacket)
            };

            WebSocketController.Instance.SendToServer(packet);
        }
        else
        {
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
    }

    public void OnAccountCreateResponse(string response)
    {
        var responseObject = JsonConvert.DeserializeObject<AccountCreateResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            ViewController.Instance.SetActiveView(VIEWTYPE.SIGNUP, false);
            ViewController.Instance.SetActiveView(VIEWTYPE.LOGIN, true);
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
        else
        {
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
    }

    public void OnAccountLogoutResponse(string response)
    {
        var responseObject = JsonConvert.DeserializeObject<AccountCreateResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            ViewController.Instance.SetActiveView(VIEWTYPE.LOGIN, true);
            Global.Instance.standbyInfo.Reset();
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);

            var logoutRequestPacket = new LoginRequestPacket
            {
                RequestType = LoginRequestType.LogoutRequest,
                UserId = Global.Instance.standbyInfo.userEntity.UserUID
            };

            var packet = new Packet
            {
                Protocol = (byte)Protocol.Login,
                Data = MessagePackSerializer.Serialize(logoutRequestPacket)
            };

            WebSocketController.Instance.SendToServer(packet);
            WebSocketController.Instance.Disconnect();
        }
        else
        {
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
    }
}
