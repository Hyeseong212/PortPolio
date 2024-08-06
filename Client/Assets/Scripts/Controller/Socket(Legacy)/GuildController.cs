using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using UnityEngine;

public class GuildController : MonoBehaviour
{
    private static GuildController instance;
    public static GuildController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GuildController>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("GuildControllerSingleton");
                    instance = singletonObject.AddComponent<GuildController>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }

    private List<UserEntity> JoinRequestUsers = new List<UserEntity>();

    public void Init()
    {
        Debug.Log("GuildController Init Complete");
    }
    public void ProcessGuildPacket(byte[] data)
    {
        if ((GuildProtocol)data[0] == GuildProtocol.CreateGuild)
        {

        }
        else if ((GuildProtocol)data[0] == GuildProtocol.SelectGuildCrew)
        {

        }
        else if ((GuildProtocol)data[0] == GuildProtocol.SelectGuildName)//길드이름으로 길드찾는 메서드
        {
            SetGuildNameFromServer(data);
        }
        else if ((GuildProtocol)data[0] == GuildProtocol.SelectGuildUid)//길드 uid로 길드찾는메서드
        {
            byte[] guildNameByte = data.Skip(1).ToArray();
            SetGuildInfo(guildNameByte);
        }
        else if ((GuildProtocol)data[0] == GuildProtocol.RequestJoinOK)
        {
            byte[] guildInfoByte = data.Skip(1).ToArray();
            ChangeGuildInfo(guildInfoByte);
        }
        else if ((GuildProtocol)data[0] == GuildProtocol.GuildResign)
        {
            byte[] Response = data.Skip(1).ToArray();
            ProcessResign(Response);
        }
    }
    private void ProcessResign(byte[] Response)
    {
        if(Response[0] == 0x00) //성공할경우
        {
            Global.Instance.standbyInfo.userEntity.guildUID = 0;
            //Global.Instance.standbyInfo.guildInfo = new GuildInfo();
            TCPController.Instance.EnqueueDispatcher(delegate
            {
                GUILDVIEW guildView = FindObjectOfType<GUILDVIEW>(true);
                guildView.GuildResignUpdate();
            });
        }
        else
        {
            Debug.Log("길드탈퇴실패");
        }
    }
    private void ChangeGuildInfo(byte[] guildInfoPacket)
    {
        string strguildInfos = Encoding.UTF8.GetString(guildInfoPacket);

        GuildInfo guildInfo = JsonConvert.DeserializeObject<GuildInfo>(strguildInfos);

        //Global.Instance.standbyInfo.guildInfo = guildInfo;

        TCPController.Instance.EnqueueDispatcher(delegate
        {
            GUILDVIEW guildView = FindObjectOfType<GUILDVIEW>(true);
            //guildView.UpdateGuildInfo(guildInfo);
        });
    }
    private void SetGuildNameFromServer(byte[] data)
    {
        string strguildInfos = Encoding.UTF8.GetString(data.Skip(1).ToArray());

        List<GuildInfo> guildInfos = JsonConvert.DeserializeObject<List<GuildInfo>>(strguildInfos);
        TCPController.Instance.EnqueueDispatcher(delegate
        {
            GUILDVIEW guildView = FindObjectOfType<GUILDVIEW>(true);
            guildView.FindedGuildSort(guildInfos);
        });
    }
    private void SetGuildInfo(byte[] data)
    {
        string guildinfoSTR = Encoding.UTF8.GetString(data);

        //Global.Instance.standbyInfo.guildInfo = JsonConvert.DeserializeObject<GuildInfo>(guildinfoSTR);

        TCPController.Instance.EnqueueDispatcher(delegate
        {
            GUILDVIEW guildView = FindObjectOfType<GUILDVIEW>(true);
            guildView.SetActiveGuildFindPanel(false);
            guildView.SetActiveGuildCrewsPanel(true);
            guildView.SetUserGuildName(Global.Instance.standbyInfo.guildInfo.guildName);
            //guildView.UpdateGuildInfo(Global.Instance.standbyInfo.guildInfo);
        });
    }
}
