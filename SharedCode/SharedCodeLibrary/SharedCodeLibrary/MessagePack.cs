using MessagePack;
using SharedCode.Model;
using System.Numerics;

namespace SharedCode.MessagePack
{
    [MessagePackObject]
    public class MatchResponsePacket
    {
        [Key(0)]
        public Protocol Protocol { get; set; }

        [Key(1)]
        public MatchProtocol MatchProtocol { get; set; }
    }

    [MessagePackObject]
    public class GameRoomIPPacket
    {
        [Key(0)]
        public Protocol Protocol { get; set; }

        [Key(1)]
        public MatchProtocol MatchProtocol { get; set; }

        [Key(2)]
        public byte[] IP { get; set; }

        [Key(3)]
        public int Port { get; set; }
    }
    [MessagePackObject]
    public class Packet
    {
        [Key(0)]
        public byte Protocol { get; set; }

        [Key(1)]
        public byte[] Data { get; set; }
    }

    [MessagePackObject]
    public class LoginRequestPacket
    {
        [Key(0)]
        public LoginRequestType RequestType { get; set; }

        [Key(1)]
        public long UserId { get; set; }
    }

    [MessagePackObject]
    public class LoginResponsePacket
    {
        [Key(0)]
        public bool Success { get; set; }

        [Key(1)]
        public string Message { get; set; }
    }
    [MessagePackObject]
    public class ChatPacket
    {
        [Key(0)]
        public ChatStatus Status { get; set; }

        [Key(1)]
        public long SenderId { get; set; }

        [Key(2)]
        public long? ReceiverId { get; set; }

        [Key(3)]
        public long? GuildId { get; set; }

        [Key(4)]
        public string Message { get; set; }
    }
    [MessagePackObject]
    public class CharacterTRPacket
    {
        [Key(0)]
        public long UserUID { get; set; }

        [Key(1)]
        public int PlayerNumber { get; set; }

        [Key(2)]
        public Vector3 Position { get; set; }

        [Key(3)]
        public Quaternion Rotation { get; set; }
    }
    [MessagePackObject]
    public class PlayerNumberPacket
    {
        [Key(0)]
        public int PlayerNumber { get; set; }
    }
    [MessagePackObject]
    public class CharacterDataPacket
    {
        [Key(0)]
        public int PlayerNumber { get; set; }

        [Key(1)]
        public int CharacterId { get; set; }
    }

    [MessagePackObject]
    public class DamagePacket
    {
        [Key(0)]
        public int PlayerNumber { get; set; }

        [Key(1)]
        public float CurrentHP { get; set; }
    }

    [MessagePackObject]
    public class AnimationStatusPacket
    {
        [Key(0)]
        public CharacterStatus CharacterStatus { get; set; }

        [Key(1)]
        public int PlayerNumber { get; set; }

        [Key(2)]
        public int? TargetPlayerNumber { get; set; }
    }

    [MessagePackObject]
    public class PlayerLevelPacket
    {
        [Key(0)]
        public int PlayerNumber { get; set; }

        [Key(1)]
        public int NewLevel { get; set; }
    }
    [MessagePackObject]
    public class SessionSyncPacket
    {
        [Key(0)]
        public SessionInfo SessionInfo { get; set; }

        [Key(1)]
        public long UserUID { get; set; }
    }
    [MessagePackObject]
    public class SetPlayerCharacterLevelPacket
    {
        [Key(0)]
        public int PlayerNum { get; set; }
    }
}
