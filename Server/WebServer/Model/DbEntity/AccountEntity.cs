using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebServer.Model.DbEntity
{
    [Table("account")]
    public class AccountEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("account_id")]
        public long AccountId { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("user_password")]
        public string UserPassword { get; set; }
    }

    [Table("account_character")]
    public class AccountCharacterEntity
    {
        [Key]
        [Column("account_id")]
        public long AccountId { get; set; }
        [Key]
        [Column("account_character")]
        public int AccountCharacter { get; set; }
    }

    [Table("account_currency")]
    public class AccountCurrencyEntity
    {
        [Key]
        [Column("account_id")]
        public long AccountId { get; set; }
        [Column("gold")]
        public long Gold { get; set; }
    }

    [Table("account_nickname")]
    public class AccountNickNameEntity
    {
        [Key]
        [Column("account_id")]
        public long AccountId { get; set; }
        [Column("account_nickName")]
        public string AccountNickName { get; set; }
    }
    [Table("inventory")]
    public class InventoryEntity
    {
        [Key]
        [Column("ItemUID")]
        public string ItemUID { get; set; }
        [Column("account_id")]
        public long AccountId { get; set; }
        [Column("item_id")]
        public int ItemId { get; set; }
        [Column("count")]
        public long Count { get; set; }

        public InventoryEntity()
        {
            ItemUID = Guid.NewGuid().ToString();
        }
    }
    [Table("account_rating")]
    public class AccountRatingEntity
    {
        [Key]
        [Column("account_id")]
        public long AccountId { get; set; }
        [Column("normal_rating")]
        public int NormalRating { get; set; }
        [Column("rank_rating")]
        public int RankRating { get; set; }
    }
    [Table("account_Guild")]
    public class AccountGuildEntity
    {
        [Key]
        [Column("account_id")]
        public long AccountId { get; set; }
        [Column("guild_id")]
        public long GuildId { get; set; }
        [Column("user_class")]
        public int UserClass { get; set; }
    }
    [Table("guild")]
    public class GuildEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("guild_id")]
        public long GuildId { get; set; }
        [Column("guild_name")]
        public string GuildName { get; set; }
    }
    [Table("guild_request_user")]
    public class GuildRequestUserEntity
    {
        [Key]
        [Column("request_User")]
        public long UserId { get; set; }
        [Key]
        [Column("guild_Id")]
        public long GuildId { get; set; }
    }
}
