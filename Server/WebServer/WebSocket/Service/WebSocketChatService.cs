using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WebServer.Model;
using WebServer.Repository.Interface;

public class WebSocketChatService
{
    private readonly IAccountRepository _accountRepository;

    // �����ڿ��� IAccountRepository ����
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
        // uid�ɷ�����
        ChatStatus chatstatus = (ChatStatus)data[0];

        if (chatstatus == ChatStatus.ENTIRE)
        {
            byte[] uidandText = data.Skip(1).ToArray();
            await EntireChat(uidandText);
        }
        else if (chatstatus == ChatStatus.WHISPER) //�α��� �ý��� �ʿ� ���� ���ϰ� IP�� �����ϴ� ������ ���ϰ� �ش� Ŭ���̾�Ʈ���� ������ ������ ����UID�� �ٲپ�ߵ�
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

            // ��� Ŭ���̾�Ʈ���� �޽��� ��ε�ĳ��Ʈ
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

        // DB���� �߽����� �г����� �����ɴϴ�.
        var senderNickname = await _accountRepository.GetNickName(sendUseruidval);
        if (senderNickname == null)
        {
            // �߽��� ������ ������ ó�� (�α� ��� �Ǵ� ���� ó��)
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

        // �������� ���� ���� Ȯ�� �� �޽��� ����
        if (ClientManager.IsClientConnected(receivingUseruidval))
        {
            await ClientManager.SendToClient(receivingUseruidval, sendData);
        }
        else
        {
            // �����ڰ� ������ ���� ������ ó�� (�α� ��� �Ǵ� �ٸ� ó��)
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

        // DB���� �߽����� �г����� �����ɴϴ�.
        var senderNickname = await _accountRepository.GetNickName(sendUseruidval);
        if (senderNickname == null)
        {
            // �߽��� ������ ������ ó�� (�α� ��� �Ǵ� ���� ó��)
            Logger.SetLogger(LOGTYPE.INFO, "Sender user not found.");
            return;
        }

        string receivedText = Encoding.UTF8.GetString(uidandText, senduid.Length + guilduid.Length, uidandText.Length - senduid.Length - guilduid.Length);

        var (success, guildUsers) = await _accountRepository.GetAllGuildUsers(Guilduidval);
        if (!success)
        {
            // ��� ������ �������µ� ������ ��� ó��
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

        // ��� ���� �� �¶��� ������ �������� �޽��� ����
        if (guildUsers == null) return;
        var tasks = guildUsers
            .Where(userId => ClientManager.IsClientConnected(userId))  // ���� ���� Ȯ��
            .Select(userId => ClientManager.SendToClient(userId, sendData));

        await Task.WhenAll(tasks);
    }
}