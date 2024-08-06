namespace WebServer.Model.HttpCommand
{
    public class GuildCreateRequest
    {
        public long Creator { get; set; }
        public string GuildName { get; set; }

    }
    public class GuildCreateResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

    }
    public class FindGuildRequest
    {
        public string GuildName { get; set; }
    }
    public class FindGuildResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string GuildInfo { get; set; }

    }
    public class RequestingJoinGuildRequest
    {
        public long GuildId { get; set; }
        public long AccountId { get; set; }
    }
    public class RequestingJoinGuildResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
    public class GetGuildInfoRequest
    {
        public long GuildId { get; set; }
    }
    public class GetGuildInfoResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string MyGuildInfo { get; set; }
    }
    public class GuildJoinOkRequest
    {
        public long AccountId { get; set; }
        public long GuildId { get; set; }
    }
    public class GuildJoinOkResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
    public class GetGuildJoinRequest
    {
        public long GuildId { get; set; }
    }
    public class GetGuildJoinResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string GuildRequestInfo { get; set; }
    }
    public class GuildResignRequest
    {
        public long AccountId { get; set; }
    }
    public class GuildResignResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
