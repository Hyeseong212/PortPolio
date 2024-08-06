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
    public string id;
    public string password;
    public LoginInfo()
    {
        id = string.Empty;
        password = string.Empty;
    }
}
[Serializable]
public class SignUpInfo
{
    public string id;
    public string pw;
    public string name;
    public SignUpInfo()
    {
        id = string.Empty;
        pw = string.Empty;
        name = string.Empty;
    }
}
[Serializable]
public class StandbyInfo
{
    public UserEntity userEntity;
    public MyGuildInfo guildInfo;
    public GuildJoinRequest requestInfo;
    public IPEndPoint sessionIPEndPoint;
    public int SelectedCharacterId;

    public GameType gameType;

    public bool isMatchingNow; 

    public StandbyInfo()
    {
        Reset();
    }
    public void Reset()
    {
        sessionIPEndPoint = new IPEndPoint(0,0);
        userEntity = new UserEntity();
        guildInfo = new MyGuildInfo();
        requestInfo = new GuildJoinRequest();
        isMatchingNow = false;
        gameType = GameType.Default;
        SelectedCharacterId = 0;
    }
}
[Serializable]
public class GuildJoinRequest
{
    public List<RequestUserInfo> requestUserInfos;

    public GuildJoinRequest()
    {
        requestUserInfos = new List<RequestUserInfo>();
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
    public int idx;
    public string message;
}
[Serializable]
public class GuildInfo
{
    public long guildUid;
    public string guildName;
    public GuildInfo()
    {
        guildUid = long.MinValue;
        guildName = string.Empty;
    }
}
[Serializable]
public class MyGuildInfo
{
    public long guildUid;
    public string guildName;
    public List<GuildCrew> Crew;
    public MyGuildInfo()
    {
        guildUid = long.MinValue;
        guildName = string.Empty;
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
    public long guildUID;
    public UserEntity()
    {
        UserUID = long.MinValue;
        UserName = string.Empty;
        SessionId = string.Empty;
        Userid = string.Empty;
        UserPW = string.Empty;
        guildUID = 0;
    }
}
[Serializable]
public class InGameSessionInfo
{
    public int playerNum;
    public bool isPlayerInfoOK;
    public bool isSyncOK;
    public bool isLoadingOK;
    public bool isAllPlayerLoadingOK;
    public List<OpponentInfo> opponentInfos;
    public InGameSessionInfo()
    {
        playerNum = 2;
        isPlayerInfoOK = false;
        isSyncOK = false;
        isLoadingOK = false;
        opponentInfos = new List<OpponentInfo>();
    }
}
[Serializable]
public class OpponentInfo
{
    public long OpponentUid { get; set; }
    public Vector3 pos { get; set; }
    public Quaternion rotation { get; set; }
    public float HP { get; set; }
    public float MP { get; set; }
    public bool isHit { get; set; }
}
public class CharacterStore
{
    public int CharacterId { get; set; }
    public string? CharacterName { get; set; }
    public long Price { get; set; }
}
public class Item
{
    public int ItemId { get; set; }
    public string? ItemName { get; set; }
    public long Price { get; set; }
}
public class MessageModel
{
    public string? TYPE { get; set; }
    public int MessageCode { get; set; }
    public string? Message { get; set; }
}
public class RankRating
{
    public int AccountId { get; set; }
    public int Rating { get; set; }
}