using System.Collections.Generic;

namespace WebServer.Model.HttpCommand
{
    public class GetRankRequest
    {

    }
    public class GetRankResponse
    {
        public bool IsSuccess { get; set; }
        public List<RankRating> RankList { get; set; }
    }
    public class GetMyRankRequest
    {
        public long AccountId { get; set; }
    }
    public class GetMyRankResponse
    {
        public bool IsSuccess { get; set; }
        public int Rating { get; set; }
    }
    public class SetMyRankRequest
    {
        public long AccountId { get; set; }
    }
    public class SetMyRankResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
