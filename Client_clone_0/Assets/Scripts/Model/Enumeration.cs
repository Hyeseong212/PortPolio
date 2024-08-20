using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public enum VIEWTYPE
{
    LOGIN,
    SIGNUP,
    GUILD,
    GAMESTART,
    LOADING
}

public enum POPUPTYPE
{
    MESSAGE,
    OKCANCEL
}
public enum GUILDCLASS
{
    NONE = -1,
    Master = 0,
    SubMaster = 1,
    Bishop = 2,
    Knight = 3,
    Crew = 4,
}
[Serializable]
public class LoginInfo
{
    public string Id;
    public string Password;
    public LoginInfo()
    {
        Id = string.Empty;
        Password = string.Empty;
    }
}
[Serializable]
public class SignUpInfo
{
    public string Id;
    public string Pw;
    public string Name;
    public SignUpInfo()
    {
        Id = string.Empty;
        Pw = string.Empty;
        Name = string.Empty;
    }
}
[Serializable]
public class StandbyInfo
{
    public UserEntity UserEntity;
    public MyGuildInfo GuildInfo;
    public GuildJoinRequest RequestInfo;
    public IPEndPoint SessionIPEndPoint;
    public int SelectedCharacterId;

    public GameType GameType;

    public bool IsMatchingNow; 

    public StandbyInfo()
    {
        Reset();
    }
    public void Reset()
    {
        SessionIPEndPoint = new IPEndPoint(0,0);
        UserEntity = new UserEntity();
        GuildInfo = new MyGuildInfo();
        RequestInfo = new GuildJoinRequest();
        IsMatchingNow = false;
        GameType = GameType.Default;
        SelectedCharacterId = 0;
    }
}
[Serializable]
public class GuildJoinRequest
{
    public List<RequestUserInfo> RequestUserInfos;

    public GuildJoinRequest()
    {
        RequestUserInfos = new List<RequestUserInfo>();
    }
}
[Serializable]
public class RequestUserInfo : MonoBehaviour
{
    public long AccountId { get; set; }
    public string UserName { get; set; }
}
public class MessageInfo
{
    public int Idx;
    public string Message;
}
[Serializable]
public class GuildInfo
{
    public long GuildUid;
    public string GuildName;
    public GuildInfo()
    {
        GuildUid = long.MinValue;
        GuildName = string.Empty;
    }
}
[Serializable]
public class MyGuildInfo
{
    public long GuildUid;
    public string GuildName;
    public List<GuildCrew> Crew;
    public MyGuildInfo()
    {
        GuildUid = long.MinValue;
        GuildName = string.Empty;
        Crew = new List<GuildCrew>();
    }
}
[Serializable]
public class GuildCrew
{
    public long CrewUid;
    public string CrewName;
    public GUILDCLASS GuildClass;
    public GuildCrew()
    {
        CrewUid = long.MinValue;
        CrewName = string.Empty;
        GuildClass = GUILDCLASS.NONE;
    }
}
[Serializable]
public class UserEntity
{
    public long UserUID;
    public string UserName;
    public string SessionId;
    public string Userid;
    public string UserPW;
    public long GuildUID;
    public UserEntity()
    {
        UserUID = long.MinValue;
        UserName = string.Empty;
        SessionId = string.Empty;
        Userid = string.Empty;
        UserPW = string.Empty;
        GuildUID = 0;
    }
}
[Serializable]
public class InGameSessionInfo
{
    public int PlayerNum;
    public bool IsPlayerInfoOK;
    public bool IsSyncOK;
    public bool IsLoadingOK;
    public bool IsAllPlayerLoadingOK;
    public List<OpponentInfo> OpponentInfos;
    public InGameSessionInfo()
    {
        PlayerNum = 2;
        IsPlayerInfoOK = false;
        IsSyncOK = false;
        IsLoadingOK = false;
        OpponentInfos = new List<OpponentInfo>();
    }
}
[Serializable]
public class OpponentInfo
{
    public long OpponentUid { get; set; }
    public Vector3 Pos { get; set; }
    public Quaternion Rotation { get; set; }
    public float HP { get; set; }
    public float MP { get; set; }
    public bool IsHit { get; set; }
}
public class CharacterStore
{
    public int CharacterId { get; set; }
    public string CharacterName { get; set; }
    public long Price { get; set; }
}
public class Item
{
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public long Price { get; set; }
}
public class MessageModel
{
    public string TYPE { get; set; }
    public int MessageCode { get; set; }
    public string Message { get; set; }
}
public class RankRating
{
    public int AccountId { get; set; }
    public int Rating { get; set; }
}