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
        // 로그인 응답 처리 로직
        //Debug.Log("Login response: " + response);

        var responseObject = JsonConvert.DeserializeObject<AccountLoginResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            // 로그인 성공 처리
            Global.Instance.standbyInfo.userEntity = JsonConvert.DeserializeObject<UserEntity>(responseObject.Userentity);
            //Debug.Log("로긴 성공");
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
            // 로그인 실패 처리
            //Debug.Log("응 로그인 실패 ㅅㄱ");
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
    }

    public void OnAccountCreateResponse(string response)
    {
        // 로그인 응답 처리 로직
        //Debug.Log("Account Create response: " + response);

        var responseObject = JsonConvert.DeserializeObject<AccountCreateResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            // 로그인 성공 처리
            ViewController.Instance.SetActiveView(VIEWTYPE.SIGNUP, false);
            ViewController.Instance.SetActiveView(VIEWTYPE.LOGIN, true);
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
            //Debug.Log("회원가입 성공");
        }
        else
        {
            // 로그인 실패 처리
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
            //Debug.Log("응 회원가입 실패 ㅅㄱ");
        }
    }
    public void OnAccountLogoutResponse(string response)
    {
        var responseObject = JsonConvert.DeserializeObject<AccountCreateResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            // 로그인 성공 처리
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
            //Debug.Log("로그아웃 성공");
        }
        else
        {
            // 로그인 실패 처리
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
            //Debug.Log("응 로그아웃 실패 ㅅㄱ");
        }
    }

}


