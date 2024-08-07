using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Model;
using WebServer.Repository.Interface;
using SharedCode.MessagePack;
using MessagePack;
using System.Net.WebSockets;

public class WebSocketChatService
{
    private readonly IAccountRepository _accountRepository;

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

        var chatPacket = MessagePackSerializer.Deserialize<ChatPacket>(data);

        switch (chatPacket.Status)
        {
            case ChatStatus.ENTIRE:
                await EntireChat(chatPacket);
                break;
            case ChatStatus.WHISPER:
                await WhisperChat(chatPacket);
                break;
            case ChatStatus.GUILD:
                await GuildChat(chatPacket);
                break;
            default:
                break;
        }
    }

    private async Task EntireChat(ChatPacket chatPacket)
    {
        var user = await _accountRepository.GetNickName(chatPacket.SenderId);

        if (user != null && user.AccountNickName != null)
        {
            var sendData = new ChatPacket
            {
                Status = ChatStatus.ENTIRE,
                SenderId = chatPacket.SenderId,
                Message = $"{user.AccountNickName} : {chatPacket.Message}"
            };

            var serializedData = MessagePackSerializer.Serialize(sendData);
            await ClientManager.Broadcast(serializedData);
        }
    }

    private async Task WhisperChat(ChatPacket chatPacket)
    {
        var senderNickname = await _accountRepository.GetNickName(chatPacket.SenderId);
        if (senderNickname == null || senderNickname.AccountNickName == null)
        {
            Logger.SetLogger(LOGTYPE.INFO, "Sender user not found.");
            return;
        }

        var sendData = new ChatPacket
        {
            Status = ChatStatus.WHISPER,
            SenderId = chatPacket.SenderId,
            ReceiverId = chatPacket.ReceiverId,
            Message = $"{senderNickname.AccountNickName} : {chatPacket.Message}"
        };

        var serializedData = MessagePackSerializer.Serialize(sendData);
        if (chatPacket.ReceiverId.HasValue && ClientManager.IsClientConnected(chatPacket.ReceiverId.Value))
        {
            await ClientManager.SendToClient(chatPacket.ReceiverId.Value, serializedData);
        }
        else
        {
            Logger.SetLogger(LOGTYPE.INFO, "Receiver user is not connected.");
        }
    }

    private async Task GuildChat(ChatPacket chatPacket)
    {
        var senderNickname = await _accountRepository.GetNickName(chatPacket.SenderId);
        if (senderNickname == null || senderNickname.AccountNickName == null)
        {
            Logger.SetLogger(LOGTYPE.INFO, "Sender user not found.");
            return;
        }

        var (success, guildUsers) = await _accountRepository.GetAllGuildUsers(chatPacket.GuildId.Value);
        if (!success || guildUsers == null)
        {
            Logger.SetLogger(LOGTYPE.INFO, "Failed to get guild users.");
            return;
        }

        var sendData = new ChatPacket
        {
            Status = ChatStatus.GUILD,
            SenderId = chatPacket.SenderId,
            GuildId = chatPacket.GuildId,
            Message = $"{senderNickname.AccountNickName} : {chatPacket.Message}"
        };

        var serializedData = MessagePackSerializer.Serialize(sendData);

        var tasks = guildUsers
            .Where(userId => ClientManager.IsClientConnected(userId))
            .Select(userId => ClientManager.SendToClient(userId, serializedData));

        await Task.WhenAll(tasks);
    }
}
