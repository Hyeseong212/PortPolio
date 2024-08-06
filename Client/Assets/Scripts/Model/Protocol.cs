public enum Protocol
{
    ///<summary>
    ///�α��� ���� ���� ��û ��Ŷ
    ///</summary>
    Login = 0x00,
    ///<summary>
    ///ä�� ���� ���� ��û ��Ŷ
    ///</summary>
    Chat = 0x16,
    ///<summary>
    ///��� ���� ���� ��û ��Ŷ
    ///</summary>
    Guild = 0x17,
    ///<summary>
    ///��Ī ���� ���� ��û ��Ŷ
    ///</summary>
    Match = 0x32,
    ///<summary>
    ///�׽�Ʈ ��Ŷ
    ///</summary>
    Test = 0x33,
}
public enum GuildProtocol
{
    ///<summary>
    ///���� uid�� ��� �����˻� �ϴ� ��������
    ///</summary>
    IsUserGuildEnable = 0x00,

    ///<summary>
    ///����̸����� �˻��ϴ� ��������
    ///</summary>
    SelectGuildName = 0x01,

    ///<summary>
    ///���� ������ ��忡�� ���� ��ȸ�ϴ� ��������
    ///</summary>
    SelectGuildCrew = 0x02,

    ///<summary>
    ///��� ���� ��������
    ///</summary>
    CreateGuild = 0x03,
    ///<summary>
    ///���uid�� ��� �˻��ϴ� ��������
    ///</summary>
    SelectGuildUid = 0x04,
    ///<summary>
    ///��� ���Կ�û ��������
    ///</summary>
    RequestJoinGuild = 0x05,
    ///<summary>
    ///��� ���Կ�û ��������� ��������
    ///</summary>
    RequestJoinUsers = 0x06,
    ///<summary>
    ///��� ���Կ�û ���� ��������
    ///</summary>
    RequestJoinOK = 0x07,
    ///<summary>
    ///��� Ż��
    ///</summary>
    GuildResign = 0x08
}
public enum ChatStatus
{
    ///<summary>
    ///��ü ä��
    ///</summary>
    ENTIRE = 0x00,

    ///<summary>
    ///�ӼӸ�
    ///</summary>
    WHISPER = 0x01,

    ///<summary>
    ///���
    ///</summary>
    GUILD = 0x02
}
public enum LoginRequestType
{
    ///<summary>
    /// �ű� ȸ������ ��û
    ///</summary>
    SignupRequest = 0x00,

    ///<summary>
    /// ���� ȸ���� �α��� ��û
    ///</summary>
    LoginRequest = 0x01,

    ///<summary>
    /// ���� ȸ���� �α׾ƿ� ��û
    ///</summary>
    LogoutRequest = 0x02,

    ///<summary>
    /// ȸ�� ���� ��û
    ///</summary>
    DeleteRequest = 0x03,

    ///<summary>
    /// ȸ�� ���� ���� ��û
    ///</summary>
    UpdateRequest = 0x04,

}
public enum MatchProtocol
{
    MatchStart = 0x00,
    MatchStop = 0x01,
    GameAccept = 0x02,
    GameMatched = 0x03,
    GameRoomIP = 0x04,
    GameTestMatched = 0x05,
}

public enum GameType
{
    Default,
    Normal,
    Rank,
}

public enum ResponseType
{
    Success = 0x00,
    Fail = 0x01,
}
public enum InGameProtocol
{
    CharacterTr = 0x00,
    SessionInfo = 0x01,
    GameInfo = 0x02,
}
public enum GameInfo
{
    IsHit = 0x00,
    CharacterData = 0x01,
    CharacterAnimationStatus = 0x02,
    SetPlayerCharacterLevel = 0x03,
}
public enum SessionInfo
{
    SessionSyncOK = 0x00,
    PlayerNum = 0x01,
    TransformInfo = 0x02,
    AllPlayerLoadingOK = 0x03,
    LoadingOK = 0x04,
    GameEnd = 0x05,
    Ban = 0x06,
}