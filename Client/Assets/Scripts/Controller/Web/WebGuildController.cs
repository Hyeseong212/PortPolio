using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using WebServer.Model.HttpCommand;

public class WebGuildController : MonoBehaviour
{
    private static WebGuildController instance;

    public static WebGuildController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<WebGuildController>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("WebGuildControllerSingleton");
                    instance = singletonObject.AddComponent<WebGuildController>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }

    public void Init()
    {
        Debug.Log("WebGuildController Init Complete");
    }
    public void OnGuildCreateResponse(string response)
    {
        var responseObject = JsonConvert.DeserializeObject<GuildCreateResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
        else
        {
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
    }
    public void OnGuildJoinOKResponse(string response)
    {
        var responseObject = JsonConvert.DeserializeObject<GuildJoinOkResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            //길드가입 요청 처리 성공
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
        else
        {
            //길드가입 요청 처리 실패
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
    }
    public void ProcessResignationResponse(string response) // 탈퇴응답 처리
    {
        var responseObject = JsonConvert.DeserializeObject<GuildResignResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            Global.Instance.standbyInfo.userEntity.guildUID = -1;
            Global.Instance.standbyInfo.guildInfo = new MyGuildInfo();

            GUILDVIEW guildView = FindObjectOfType<GUILDVIEW>(true);

            guildView.GuildResignUpdate();
            //길드탈퇴 요청 처리 성공
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
        else
        {
            //길드탈퇴 요청 처리 실패
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
    }
    public void OnGetGuildJoinResponse(string response)
    {
        var responseObject = JsonConvert.DeserializeObject<GetGuildJoinResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            if (!string.IsNullOrEmpty(responseObject.GuildRequestInfo))
            {
                Global.Instance.standbyInfo.requestInfo = JsonConvert.DeserializeObject<GuildJoinRequest>(responseObject.GuildRequestInfo);
                GUILDVIEW guildView = FindObjectOfType<GUILDVIEW>(true);
                guildView.JoinRequestSort();
                GetGuildInfoRequest getGuildInfoRequest = new GetGuildInfoRequest();
                getGuildInfoRequest.GuildId = Global.Instance.standbyInfo.userEntity.guildUID;
                StartCoroutine(WebAPIController.Instance.PostRequest<GetGuildInfoRequest>(Global.Instance.apiRequestUri + "Guild/GetGuildInfo", getGuildInfoRequest, WebGuildController.Instance.UpdateGuildInformationResponse));
            }
        }
        else
        {
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
    }

    public void UpdateGuildInformationResponse(string response) // 길드 데이터 업데이트 응답 처리
    {
        var responseObject = JsonConvert.DeserializeObject<GetGuildInfoResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            var guildInfo = JsonConvert.DeserializeObject<MyGuildInfo>(responseObject.MyGuildInfo);

            Global.Instance.standbyInfo.guildInfo.guildName = guildInfo.guildName;
            Global.Instance.standbyInfo.guildInfo = guildInfo;

            GUILDVIEW guildView = FindObjectOfType<GUILDVIEW>(true);
            guildView.SetActiveGuildFindPanel(false);
            guildView.SetActiveGuildCrewsPanel(true);
            guildView.SetUserGuildName(Global.Instance.standbyInfo.guildInfo.guildName);
            guildView.UpdateGuildInfo(Global.Instance.standbyInfo.guildInfo);
        }
        else
        {
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }


    }

    public void RetrieveGuildNamesFromServerResponse(string response) // 길드 이름들 서버에서 가져오기 응답 처리
    {
        var responseObject = JsonConvert.DeserializeObject<FindGuildResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            GUILDVIEW guildView = FindObjectOfType<GUILDVIEW>(true);
            List<GuildInfo> guildInfos = JsonConvert.DeserializeObject<List<GuildInfo>>(responseObject.GuildInfo);
            guildView.FindedGuildSort(guildInfos);
        }
        else
        {
            // 로그인 실패 처리
            //Debug.Log("응 로그인 실패 ㅅㄱ");
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
    }

    public void On(string response) // 
    {

    }
    public void OnReqeustingJoinGuildResponse(string response) //길드가입 넣은사람데이터 응답
    {
        var responseObject = JsonConvert.DeserializeObject<RequestingJoinGuildResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
        else
        {
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
    }
}
