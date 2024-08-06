using SharedCode.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class SessionManager
{
    public SessionManager()
    {
        Logger.SetLogger(LOGTYPE.INFO, $"{this.ToString()} init Complete");
    }

    private ConcurrentDictionary<long, InGameSession> createdSessions = new ConcurrentDictionary<long, InGameSession>();


    public async Task<InGameSession> InGameSessionCreate(List<PlayerInfo> users, GameType gameType)
    {
        long sessionId = DateTime.Now.Ticks;
        InGameSession newSession = new InGameSession(sessionId, gameType, this);
        createdSessions.TryAdd(sessionId, newSession);

        // 새로운 세션 시작
        await newSession.StartSessionAsync();

        var addPlayerTasks = users.Select(player => newSession.AddPlayerAsync(player));
        await Task.WhenAll(addPlayerTasks);

        return newSession;
    }

    public async Task<InGameSession> GetSession(long sessionId)
    {
        if (createdSessions.TryGetValue(sessionId, out InGameSession session))
        {
            // 가정: 세션을 비동기적으로 가져오는 작업
            await Task.Delay(10); // 실제 비동기 작업으로 대체
            return session;
        }
        return null;
    }

    public async Task<bool> RemoveSession(long sessionId)
    {
        if (createdSessions.TryRemove(sessionId, out InGameSession session))
        {
            await session.EndGameCleanupAsync(); // 세션의 자원 정리 메서드 호출
            session = null; // 객체 참조 해제
            Logger.SetLogger(LOGTYPE.INFO, $"Session with ID {sessionId} has been removed.");
            return true;
        }
        Logger.SetLogger(LOGTYPE.INFO, $"Session with ID {sessionId} not found.");
        return false;
    }
}
