using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SharedCode.Model.HttpCommand;

public class GUILDVIEW : MonoBehaviour
{
    [Header("���")]
    [SerializeField] Text guildNameTxt;
    [Header("��� ã�� �г�")]
    [SerializeField] GameObject guildFindPanel;
    [SerializeField] InputField findGuildNameTextPanel;
    [SerializeField] GameObject guildNameContainer;
    [SerializeField] GameObject guildNameObject;

    [SerializeField] Button GuildFindBtn;

    [SerializeField] InputField createGuildName;
    [SerializeField] Button GuildCreate;

    [Header("���� �г�")]
    [SerializeField] GameObject guildCrewsPanel;
    [SerializeField] GameObject guildCrewsContainer;
    [SerializeField] GameObject guildCrewsNameObject;

    [SerializeField] GameObject joinRequestContainerObject;
    [SerializeField] GameObject joinRequestBtn;

    [SerializeField] Button guildResignBtn;

    [Header("ǲ")]
    [SerializeField] Button GuildFindPanelBtn;
    [SerializeField] Button GuildCrewPanelBtn;

    List<GameObject> guildInfoObject = new List<GameObject>();
    List<GameObject> guildCrewObject = new List<GameObject>();
    List<GameObject> JoinRequestObject = new List<GameObject>();

    //�׽�Ʈ �ڵ�
    public List<GuildInfo> guildInfos = new List<GuildInfo>();
    public List<GuildCrew> guildcrews = new List<GuildCrew>();

    private void Start()
    {
        GuildFindBtn.onClick.AddListener(delegate
        {
            FindGuild();
        });
        GuildCreate.onClick.AddListener(delegate
        {
            GuildCreatePakcetToServer();
        });
        GuildFindPanelBtn.onClick.AddListener(delegate
        {
            guildCrewsPanel.SetActive(false);
            guildFindPanel.SetActive(true);
        });
        GuildCrewPanelBtn.onClick.AddListener(delegate
        {
            guildCrewsPanel.SetActive(true);
            guildFindPanel.SetActive(false);
        });
        guildResignBtn.onClick.AddListener(delegate
        {
            GuildResign();
        });
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            guildFindPanel.SetActive(false);
            gameObject.SetActive(false);
        }
    }
    private void OnEnable()
    {
        findGuildNameTextPanel.text = "";

        for (int i = 0; i < guildInfoObject.Count; i++)
        {
            Destroy(guildInfoObject[i]);
        }
        guildInfoObject.Clear();

        guildFindPanel.SetActive(false);
        guildCrewsPanel.SetActive(false);

        if (Global.Instance.standbyInfo.userEntity.guildUID != -1)//��尡�ԵǾ��������
        {
            GetGuildInfoRequest getGuildInfoRequest = new GetGuildInfoRequest();
            getGuildInfoRequest.GuildId = Global.Instance.standbyInfo.userEntity.guildUID;
            StartCoroutine(WebAPIController.Instance.PostRequest<GetGuildInfoRequest>(Global.Instance.apiRequestUri + "Guild/GetGuildInfo", getGuildInfoRequest, WebGuildController.Instance.UpdateGuildInformationResponse));

            createGuildName.gameObject.SetActive(false);
            GuildCreate.gameObject.SetActive(false);

            guildResignBtn.gameObject.SetActive(true);
        }
        else//���ԾȵǾ��������
        {
            SetUserGuildName("���Ե� ��尡 �����ϴ�");

            createGuildName.gameObject.SetActive(true);
            GuildCreate.gameObject.SetActive(true);

            guildResignBtn.gameObject.SetActive(false);
        }
    }
    public void GuildResignUpdate()
    {
        SetUserGuildName("���Ե� ��尡 �����ϴ�");

        createGuildName.gameObject.SetActive(true);
        GuildCreate.gameObject.SetActive(true);

        guildResignBtn.gameObject.SetActive(false);

        var gcc = guildCrewsContainer.GetComponentsInChildren<Button>();

        for (int i = 0; i < gcc.Length; i++)
        {
            Destroy(gcc[i].gameObject);
        }
        guildCrewObject.Clear();
    }
    private void GuildResign()
    {
        GuildResignRequest guildResignRequest = new GuildResignRequest();
        guildResignRequest.AccountId = Global.Instance.standbyInfo.userEntity.UserUID;
        StartCoroutine(WebAPIController.Instance.PostRequest<GuildResignRequest>(Global.Instance.apiRequestUri + "Guild/GuildResign", guildResignRequest, WebGuildController.Instance.ProcessResignationResponse));
    }
    private void FindGuild()
    {
        for (int i = 0; i < guildInfoObject.Count; i++)
        {
            Destroy(guildInfoObject[i]);
        }
        guildInfoObject.Clear();

        FindGuildRequest guildCreateRequest = new FindGuildRequest();
        guildCreateRequest.GuildName = findGuildNameTextPanel.text;
        StartCoroutine(WebAPIController.Instance.PostRequest<FindGuildRequest>(Global.Instance.apiRequestUri + "Guild/GuildFind", guildCreateRequest, WebGuildController.Instance.RetrieveGuildNamesFromServerResponse));
    }
    public void UpdateGuildInfo(MyGuildInfo guildinfo)
    {
        //ũ���гο� ũ���߰�
        for (int i = 0; i < guildCrewObject.Count; i++)
        {
            Destroy(guildCrewObject[i]);
        }

        for (int i = 0; i < guildinfo.Crew.Count; i++)
        {
            GameObject go = Instantiate(guildCrewsNameObject, guildCrewsContainer.transform);
            go.SetActive(true);
            guildCrewObject.Add(go);
            go.AddComponent<GuildCrewInfo>();
            go.GetComponent<GuildCrewInfo>().guildCrew = guildinfo.Crew[i];
            go.GetComponentInChildren<Text>().text = guildinfo.Crew[i].CrewName;
        }
        GetGuildJoinRequest getGuildJoinRequest = new GetGuildJoinRequest();
        getGuildJoinRequest.GuildId = Global.Instance.standbyInfo.guildInfo.guildUid;

        StartCoroutine(WebAPIController.Instance.PostRequest<GetGuildJoinRequest>(Global.Instance.apiRequestUri + "Guild/GetGuildJoinRequestInfo", getGuildJoinRequest, WebGuildController.Instance.OnGetGuildJoinResponse));
    }
    public void FindedGuildSort(List<GuildInfo> guildInfos)
    {
        for (int i = 0; i < guildInfos.Count; i++)
        {
            GameObject GuildNameObject = Instantiate(guildNameObject, guildNameContainer.transform);
            guildInfoObject.Add(GuildNameObject);
            GuildNameObject.SetActive(true);
            GuildNameObject.GetComponentInChildren<Text>().text = guildInfos[i].guildName;
            GuildNameObject.GetComponent<GuildProfile>().guildinfo = guildInfos[i];
            GuildNameObject.GetComponent<Button>().onClick.AddListener(delegate
            {
                if (Global.Instance.standbyInfo.userEntity.guildUID != -1)//��尡 ���Ե��������
                {
                    PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, "�̹� ������ ��尡 �־� \n ���Խ�û�� ������ �����ϴ�", null, null);
                }
                else//���Ծȵ��������
                {
                    //��� ���� ��û ������
                    Action action = () =>
                    {
                        RequestingJoinGuildRequest requestingJoinGuildRequest = new RequestingJoinGuildRequest();
                        requestingJoinGuildRequest.AccountId = Global.Instance.standbyInfo.userEntity.UserUID;
                        requestingJoinGuildRequest.GuildId = GuildNameObject.GetComponent<GuildProfile>().guildinfo.guildUid;
                        StartCoroutine(WebAPIController.Instance.PostRequest<RequestingJoinGuildRequest>(Global.Instance.apiRequestUri + "Guild/RequestingJoinGuild", requestingJoinGuildRequest, WebGuildController.Instance.OnGuildCreateResponse));
                    };
                    PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.OKCANCEL, true, "���� ��û�� �����ðڽ��ϱ�?", action,null);
                }
            });
        }
    }
    private void GuildCrewsNameSort(List<GuildCrew> guildCrews)
    {
        //for (int i = 0; i < guildInfos.Count; i++)
        //{
        //    GameObject GuildNameObject = Instantiate(guildCrewsNameObject, guildCrewsContainer.transform);
        //    GuildNameObject.SetActive(true);
        //    GuildNameObject.GetComponentInChildren<Text>().text = guildCrews[i].crewName;
        //    GuildNameObject.GetComponent<Button>().onClick.AddListener(delegate
        //    {
        //        //�ӼӸ� ������ �÷��� ����
        //        Debug.Log($"this is {GuildNameObject.GetComponentInChildren<Text>().text}");
        //    });
        //}
    }


    public void JoinRequestSort()
    {
        for (int i = 0; i < JoinRequestObject.Count; i++)
        {
            Destroy(JoinRequestObject[i]);
        }

        if(Global.Instance.standbyInfo.requestInfo.requestUserInfos.Count <= 0)
        {
            return;
        }

        for (int i = 0; i < Global.Instance.standbyInfo.requestInfo.requestUserInfos.Count; i++)
        {
            GameObject GuildNameObject = Instantiate(joinRequestBtn, joinRequestContainerObject.transform);
            JoinRequestObject.Add(GuildNameObject);
            GuildNameObject.SetActive(true);
            GuildNameObject.AddComponent<RequestUserInfo>();
            GuildNameObject.GetComponent<RequestUserInfo>().AccountId = Global.Instance.standbyInfo.requestInfo.requestUserInfos[i].AccountId;
            GuildNameObject.GetComponentInChildren<Text>().text = Global.Instance.standbyInfo.requestInfo.requestUserInfos[i].UserName;
            GuildNameObject.GetComponent<Button>().onClick.AddListener(delegate
            {
                Action action = () =>
                {
                    //���⿡ ��� 
                    GuildJoinOkRequest guildJoinOkRequest = new GuildJoinOkRequest();
                    guildJoinOkRequest.AccountId = GuildNameObject.GetComponent<RequestUserInfo>().AccountId;
                    guildJoinOkRequest.GuildId = Global.Instance.standbyInfo.userEntity.guildUID;
                    StartCoroutine(WebAPIController.Instance.PostRequest<GuildJoinOkRequest>(Global.Instance.apiRequestUri + "Guild/GuildJoinOK", guildJoinOkRequest, WebGuildController.Instance.OnGuildCreateResponse));
                    Destroy(GuildNameObject);
                };
                //��û �˾� ����
                PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.OKCANCEL, true, "��û�� �����Ͻðڽ��ϱ�?", action, null);
            });
        }
    }
    public void GuildCreatePakcetToServer()
    {
        GuildCreateRequest guildCreateRequest = new GuildCreateRequest();
        guildCreateRequest.Creator = Global.Instance.standbyInfo.userEntity.UserUID;
        guildCreateRequest.GuildName = createGuildName.text;
        StartCoroutine(WebAPIController.Instance.PostRequest<GuildCreateRequest>(Global.Instance.apiRequestUri + "Guild/GuildCreate", guildCreateRequest, WebGuildController.Instance.OnGuildCreateResponse));
    }


    public void SetActiveGuildFindPanel(bool isActive)
    {
        guildFindPanel.SetActive(isActive);
    }
    public void SetActiveGuildCrewsPanel(bool isActive)
    {
        guildCrewsPanel.SetActive(isActive);
    }
    public void SetUserGuildName(string guildName)
    {
        guildNameTxt.text = guildName;
    }
}
