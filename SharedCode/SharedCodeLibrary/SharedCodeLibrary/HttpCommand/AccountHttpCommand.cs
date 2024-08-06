
namespace SharedCode.Model.HttpCommand
{
    public class Hello
    {
        public string Name { get; set; }
    }
    public class AccountCreateRequest : BaseRequest
    {
        public string Id { get; set; }
        public string Password { get; set; }
        public string NickName { get; set; }
    }

    public class AccountCreateResponse : BaseResponse
    {
        public new bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class AccountLoginRequest : BaseRequest
    {
        public string Id { get; set; }
        public string Password { get; set; }
    }

    public class AccountLoginResponse : BaseResponse
    {
        public new bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string Userentity { get; set; }

    }
    public class AccountLogoutRequest : BaseRequest
    {
        public long AccountId { get; set; }
    }

    public class AccountLogoutResponse : BaseResponse
    {
        public new bool IsSuccess { get; set; }
        public string Message { get; set; }

    }
    public class AccountNickNameRequest : BaseRequest
    {
        public long AccountId { get; set; }
        public string NickName { get; set; }
    }

    public class AccountNickNameResponse : BaseResponse
    {
        public new bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
