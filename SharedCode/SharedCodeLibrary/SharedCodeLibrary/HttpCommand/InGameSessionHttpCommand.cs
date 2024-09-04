using System;
using System.Collections.Generic;
using System.Text;

namespace SharedCodeLibrary.HttpCommand
{
    public class NewInGameSessionRequest
    {
        public string SessionId { get; set; }
        public string IPandPort { get; set; }
    }
    public class NewInGameSessionResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
