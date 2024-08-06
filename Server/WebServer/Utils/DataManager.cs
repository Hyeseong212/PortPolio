using WebServer.Model;
using WebServer.Repository.Interface;
using WebServer.Service;

public class DataManager
{
    public CharacterDataHandler CharacterDataHandler = new CharacterDataHandler("Resources/CharacterInfo.csv", "CharacterId");
    public ItemHandler ItemHandler = new ItemHandler("Resources/ShopItem.csv", "ItemId");
    public MessageHandler MessageHandler = new MessageHandler("Resources/MessagesInfo.csv");

    public DataManager()
    {
    }
}

