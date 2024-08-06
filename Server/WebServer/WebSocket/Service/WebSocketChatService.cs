using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WebServer.Model;
using WebServer.Repository.Interface;

public class WebSocketChatService
{
    private readonly IAccountRepository _accountRepository;

    // 생성자에서 IAccountRepository 주입
    public WebSocketChatService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }
    public async Task ProcessChatPacket(WebSocket webSocket, byte[] data)
    {
        if (data.Length < 1)
        {
            return;
        }
        // uid걸러내기
        ChatStatus chatstatus = (ChatStatus)data[0];

        if (chatstatus == ChatStatus.ENTIRE)
        {
            byte[] uidandText = data.Skip(1).ToArray();
            await EntireChat(uidandText);
        }
        else if (chatstatus == ChatStatus.WHISPER) //로그인 시스템 필요 현재 소켓과 IP로 구분하는 구조를 소켓과 해당 클라이언트에서 접속한 유저의 유저UID로 바꾸어야됨
        {
            byte[] uidandText = data.Skip(1).ToArray();
            await WhisperChat(uidandText);
        }
        else if (chatstatus == ChatStatus.GUILD)
        {
            byte[] uidandText = data.Skip(1).ToArray();
            await GuildChat(uidandText);
        }
        else
        {
        }
    }

    public async Task EntireChat(byte[] uidandText)
    {
        byte[] uid = new byte[8];
        Array.Copy(uidandText, 0, uid, 0, 8);
        long uidval = BitConverter.ToInt64(uid, 0);

        var user = await _accountRepository.GetNickName(uidval);

        if (user != null)
        {
            string receivedText = Encoding.UTF8.GetString(uidandText, uid.Length, uidandText.Length - uid.Length);

            if (user.AccountNickName == null) return;

            int length = 0x01 + Encoding.UTF8.GetBytes(user.AccountNickName).Length + Encoding.UTF8.GetBytes(" : ").Length + (uidandText.Length - uid.Length);

            var sendData = new Packet();
            sendData.push((byte)Protocol.Chat);
            sendData.push(length);
            sendData.push((byte)ChatStatus.ENTIRE);
            sendData.push(user.AccountNickName);
            sendData.push(" : ");
            sendData.push(receivedText);

            // 모든 클라이언트에게 메시지 브로드캐스트
            await ClientManager.Broadcast(sendData);
        }
    }
    public async Task WhisperChat(byte[] uidandText)
    {
        byte[] senduid = new byte[8];
        byte[] receivinguid = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            senduid[i] = uidandText[i];
        }
        for (int i = 8; i < 16; i++)
        {
            receivinguid[i - 8] = uidandText[i];
        }
        long sendUseruidval = BitConverter.ToInt64(senduid, 0);
        long receivingUseruidval = BitConverter.ToInt64(receivinguid, 0);

        // DB에서 발신자의 닉네임을 가져옵니다.
        var senderNickname = await _accountRepository.GetNickName(sendUseruidval);
        if (senderNickname == null)
        {
            // 발신자 정보가 없으면 처리 (로그 기록 또는 예외 처리)
            Logger.SetLogger(LOGTYPE.INFO, "Sender user not found.");
            return;
        }
        if (senderNickname.AccountNickName == null) return;

        string receivedText = Encoding.UTF8.GetString(uidandText, senduid.Length + receivinguid.Length, uidandText.Length - senduid.Length - receivinguid.Length);

        int length = 0x01 + Utils.GetLength(senderNickname.AccountNickName + " : " + receivedText);

        var sendData = new Packet();
        sendData.push((byte)Protocol.Chat);
        sendData.push(length);
        sendData.push((byte)ChatStatus.WHISPER);
        sendData.push(senderNickname.AccountNickName);
        sendData.push(" : ");
        sendData.push(receivedText);

        Logger.SetLogger(LOGTYPE.INFO, $"{Encoding.UTF8.GetString(sendData.Buffer, 6, sendData.Position)}");

        // 수신자의 접속 상태 확인 및 메시지 전송
        if (ClientManager.IsClientConnected(receivingUseruidval))
        {
            await ClientManager.SendToClient(receivingUseruidval, sendData);
        }
        else
        {
            // 수신자가 접속해 있지 않으면 처리 (로그 기록 또는 다른 처리)
            Logger.SetLogger(LOGTYPE.INFO, "Receiver user is not connected.");
        }
    }


    public async Task GuildChat(byte[] uidandText)
    {
        byte[] senduid = new byte[8];
        byte[] guilduid = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            senduid[i] = uidandText[i];
        }
        for (int i = 8; i < 16; i++)
        {
            guilduid[i - 8] = uidandText[i];
        }
        long sendUseruidval = BitConverter.ToInt64(senduid, 0);
        long Guilduidval = BitConverter.ToInt64(guilduid, 0);

        // DB에서 발신자의 닉네임을 가져옵니다.
        var senderNickname = await _accountRepository.GetNickName(sendUseruidval);
        if (senderNickname == null)
        {
            // 발신자 정보가 없으면 처리 (로그 기록 또는 예외 처리)
            Logger.SetLogger(LOGTYPE.INFO, "Sender user not found.");
            return;
        }

        string receivedText = Encoding.UTF8.GetString(uidandText, senduid.Length + guilduid.Length, uidandText.Length - senduid.Length - guilduid.Length);

        var (success, guildUsers) = await _accountRepository.GetAllGuildUsers(Guilduidval);
        if (!success)
        {
            // 길드 유저를 가져오는데 실패한 경우 처리
            Logger.SetLogger(LOGTYPE.INFO, "Failed to get guild users.");
            return;
        }
        if (senderNickname.AccountNickName == null) return;
        int length = 0x01 + Utils.GetLength(senderNickname.AccountNickName + " : " + receivedText);

        var sendData = new Packet();
        sendData.push((byte)Protocol.Chat);
        sendData.push(length);
        sendData.push((byte)ChatStatus.GUILD);
        sendData.push(senderNickname.AccountNickName);
        sendData.push(" : ");
        sendData.push(receivedText);

        Logger.SetLogger(LOGTYPE.INFO, $"{Encoding.UTF8.GetString(sendData.Buffer, 6, sendData.Position)}");

        // 길드 유저 중 온라인 상태인 유저에게 메시지 전송
        if (guildUsers == null) return;
        var tasks = guildUsers
            .Where(userId => ClientManager.IsClientConnected(userId))  // 연결 상태 확인
            .Select(userId => ClientManager.SendToClient(userId, sendData));

        await Task.WhenAll(tasks);
    }
}