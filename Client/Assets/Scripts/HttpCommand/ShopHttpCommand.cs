namespace WebServer.Model.HttpCommand
{
    public class ShopBuyItemRequest : BaseRequest
    {
        //어카운트 ID
        public long AccountId { get; set; }
        //상품 ID
        public int ItemId { get; set; }
        public int Count { get; set; }
    }
    public class ShopBuyItemResponse : BaseResponse
    {

        public new bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
    public class ShopSellItemRequest : BaseRequest
    {
        //어카운트 ID
        public long AccountId { get; set; }
        //상품 ID
        public int ItemId { get; set; }
        public int Count { get; set; }
    }
    public class ShopSellItemResponse : BaseResponse
    {
        public new bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
    public class ShopBuyCharacterRequest : BaseRequest
    {
        //어카운트 ID
        public long AccountId { get; set; }
        //상품 ID
        public int ItemId { get; set; }
        public int Count { get; set; }
    }
    public class ShopBuyCharacterResponse : BaseResponse
    {
        public new bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

}
