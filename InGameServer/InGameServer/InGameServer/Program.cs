using Newtonsoft.Json;
using SharedCode.Model;

using Newtonsoft.Json;
using System;
using System.Text;

namespace InGameServer
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // InGameSession 객체 생성 및 세션 시작
            InGameSession inGameSession = new InGameSession();
            inGameSession.StartSession(); 
        }
    }
}

