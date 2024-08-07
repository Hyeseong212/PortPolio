using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace SharedCode.Model
{
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
        public long AccountId { get; set; }
        public int Rating { get; set; }
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
    public class GuildJoinRequest
    {
        public List<RequestUserInfo> RequestUserInfos;

        public GuildJoinRequest()
        {
            RequestUserInfos = new List<RequestUserInfo>();
        }
    }
    [Serializable]
    public class RequestUserInfo
    {
        public long AccountId { get; set; }
        public string UserName { get; set; }
    }
    public class RatingRange
    {
        static readonly float BRONZE_MAX = 1000.0f;
        static readonly float SILVER_MIN = 1000.0f;
        static readonly float SILVER_MAX = 1500.0f;
        static readonly float GOLD_MIN = 1500.0f;
        static readonly float GOLD_MAX = 2000.0f;
        static readonly float PLATINUM_MIN = 2000.0f;
        static readonly float PLATINUM_MAX = 2500.0f;
        static readonly float DIAMOND_MIN = 2500.0f;



        public static Tier GetTier(float rating)
        {
            if (rating < BRONZE_MAX)
            {
                return Tier.BRONZE;
            }
            else if (rating >= SILVER_MIN && rating < SILVER_MAX)
            {
                return Tier.SILVER;
            }
            else if (rating >= GOLD_MIN && rating < GOLD_MAX)
            {
                return Tier.GOLD;
            }
            else if (rating >= PLATINUM_MIN && rating < PLATINUM_MAX)
            {
                return Tier.PLATINUM;
            }
            else if (rating >= DIAMOND_MIN)
            {
                return Tier.DIAMOND;
            }
            else
            {
                throw new ArgumentException("Invalid rating: " + rating);
            }
        }
    }
    public enum Tier
    {
        UNRANKED,
        BRONZE,
        SILVER,
        GOLD,
        PLATINUM,
        DIAMOND
    }
    public class PlayerInfo
    {
        public long UserUID { get; set; }
        public float Rating { get; set; }
        public WebSocket WebSocket { get; set; }
        public Socket Socket { get; set; }
        public bool HasAccepted { get; set; } = false;
    }
    public class PlayerRating
    {
        public long UserUID;
        public float Rating;
        public PlayerRating()
        {
            UserUID = 0;
            Rating = 0;
        }
    }
    //인게임관련 여기에 접속한 플레이어정보 기입 
    public class InGamePlayerInfo
    {
        public long UserUID;
        public int PlayerNumber;
        public bool IsConnected;
        public bool IsLoadingOK;
        public InGamePlayerInfo()
        {
            IsConnected = false;
            IsLoadingOK = false;
        }
    }
    public enum CharacterStatus
    {
        NONE,
        IDLE,
        MOVE,
        ATTACK,
        DAMAGED,
        DIE
    }
}