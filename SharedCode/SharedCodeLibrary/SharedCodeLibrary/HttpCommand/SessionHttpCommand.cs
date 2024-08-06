namespace SharedCode.Model.HttpCommand
{
    public class RenewSessionRequest
    {
        public long AccounId { get; set; }
        public string SessionId { get; set; }
    }
    public class RenewSessionResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
