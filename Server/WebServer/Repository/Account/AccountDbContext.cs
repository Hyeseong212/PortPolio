using Microsoft.EntityFrameworkCore;
using WebServer.Model;
using WebServer.Model.DbEntity;

namespace WebServer.Repository
{
    public class AccountDbContext : DbContext
    {
        public DbSet<AccountEntity> Account { get; set; }
        public DbSet<AccountCharacterEntity> AccountCharacter { get; set; }
        public DbSet<AccountCurrencyEntity> AccountCurrency { get; set; }   
        public DbSet<AccountNickNameEntity> AccountNickName { get; set; }
        public DbSet<InventoryEntity> Inventory { get; set; } 
        public DbSet<AccountRatingEntity> AccountRating { get; set; }
        public DbSet<AccountGuildEntity> AccountGuild { get; set; }
        public DbSet<GuildEntity> Guild { get; set; }
        public DbSet<GuildRequestUserEntity> GuildRequestUser { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 현재 작업 디렉터리를 가져옴
            string currentDirectory = Directory.GetCurrentDirectory();
            string configFilePath = Path.Combine(currentDirectory, "Configuration", "DBconfig.txt");

            var ipAndPort = Utils.GetMySQLDbConfig(configFilePath);

            string connectionString = $"server={ipAndPort.Item1}; port={ipAndPort.Item2}; database=WebServerDB; user=root; password=1234";
            //string connectionString = "server=192.168.123.1; port=3306; database=WebServerDB; user=root; password=1234";
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            //optionsBuilder.LogTo(Console.WriteLine);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountEntity>().HasKey(x => x.AccountId);
            modelBuilder.Entity<AccountCharacterEntity>().HasKey(x => new { x.AccountId, x.AccountCharacter });
            modelBuilder.Entity<AccountCurrencyEntity>().HasKey(x => new { x.AccountId });
            modelBuilder.Entity<AccountNickNameEntity>().HasKey(x => new { x.AccountId });
            modelBuilder.Entity<InventoryEntity>().HasKey(x => new { x.ItemUID });  
            modelBuilder.Entity<AccountRatingEntity>().HasKey(x => new { x.AccountId });
            modelBuilder.Entity<AccountGuildEntity>().HasKey(x => new { x.AccountId });
            modelBuilder.Entity<GuildEntity>().HasKey(x => new { x.GuildId });
            modelBuilder.Entity<GuildRequestUserEntity>().HasKey(x => new { x.GuildId , x.UserId });
        }
    }
}
