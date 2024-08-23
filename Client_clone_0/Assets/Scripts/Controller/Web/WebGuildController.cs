using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using SharedCode.Model.HttpCommand;

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
            //��尡�� ��û ó�� ����
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
        else
        {
            //��尡�� ��û ó�� ����
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
    }
    public void ProcessResignationResponse(string response) // Ż������ ó��
    {
        var responseObject = JsonConvert.DeserializeObject<GuildResignResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            Global.Instance.StandbyInfo.UserEntity.GuildUID = -1;
            Global.Instance.StandbyInfo.GuildInfo = new MyGuildInfo();

            GUILDVIEW guildView = FindObjectOfType<GUILDVIEW>(true);

            guildView.GuildResignUpdate();
            //���Ż�� ��û ó�� ����
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
        else
        {
            //���Ż�� ��û ó�� ����
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
                Global.Instance.StandbyInfo.RequestInfo = JsonConvert.DeserializeObject<GuildJoinRequest>(responseObject.GuildRequestInfo);
                GUILDVIEW guildView = FindObjectOfType<GUILDVIEW>(true);
                guildView.JoinRequestSort();
                GetGuildInfoRequest getGuildInfoRequest = new GetGuildInfoRequest();
                getGuildInfoRequest.GuildId = Global.Instance.StandbyInfo.UserEntity.GuildUID;
                //StartCoroutine(WebAPIController.Instance.PostRequest<GetGuildInfoRequest>(Global.Instance.ApiRequestUri + "Guild/GetGuildInfo", getGuildInfoRequest, WebGuildController.Instance.UpdateGuildInformationResponse));
            }
        }
        else
        {
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
    }

    public void UpdateGuildInformationResponse(string response) // ��� ������ ������Ʈ ���� ó��
    {
        var responseObject = JsonConvert.DeserializeObject<GetGuildInfoResponse>(response);

        if (responseObject != null && responseObject.IsSuccess)
        {
            var guildInfo = JsonConvert.DeserializeObject<MyGuildInfo>(responseObject.MyGuildInfo);

            Global.Instance.StandbyInfo.GuildInfo.GuildName = guildInfo.GuildName;
            Global.Instance.StandbyInfo.GuildInfo = guildInfo;

            GUILDVIEW guildView = FindObjectOfType<GUILDVIEW>(true);
            guildView.SetActiveGuildFindPanel(false);
            guildView.SetActiveGuildCrewsPanel(true);
            guildView.SetUserGuildName(Global.Instance.StandbyInfo.GuildInfo.GuildName);
            guildView.UpdateGuildInfo(Global.Instance.StandbyInfo.GuildInfo);
        }
        else
        {
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }


    }

    public void RetrieveGuildNamesFromServerResponse(string response) // ��� �̸��� �������� �������� ���� ó��
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
            // �α��� ���� ó��
            //Debug.Log("�� �α��� ���� ����");
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, responseObject.Message, null, null);
        }
    }

    public void On(string response) // 
    {

    }
    public void OnReqeustingJoinGuildResponse(string response) //��尡�� ������������� ����
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