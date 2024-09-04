using Newtonsoft.Json;
using SharedCode.Model;

namespace InGameServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 전달된 인자가 있는지 확인
            if (args.Length > 0)
            {
                // 첫 번째 인자 (JSON 문자열) 받기
                string sessionInfoJson = args[0];

                try
                {
                    // JSON 문자열을 SessionInfoClass 객체로 역직렬화
                    InGameSessionProcessArgs sessionInfo = JsonConvert.DeserializeObject<InGameSessionProcessArgs>(sessionInfoJson);

                    // 역직렬화된 객체 사용
                    Console.WriteLine($"Session ID: {sessionInfo.SessionId}");
                    Console.WriteLine($"Users: {sessionInfo.Users}");

                    // InGameSession 객체 생성 및 세션 시작
                    InGameSession inGameSession = new InGameSession(long.Parse(sessionInfo.SessionId), sessionInfo.Users);
                    inGameSession.StartSession();  // 세션 시작

                    // 프로그램이 종료되지 않고 계속 실행됨 (리스닝 중)
                    // 이 시점에서 세션이 종료될 때까지 계속 대기함
                    // 종료는 다른 조건에 의해 발생해야 함
                }
                catch (Exception ex)
                {
                    // JSON 역직렬화 실패 시 예외 처리
                    Console.WriteLine($"Failed to deserialize session info: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No arguments received.");
            }
        }
    }
}
