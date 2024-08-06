using System;
using SharedCode.Model;

public class ItemHandler : DataHandler<Item>
{
    public ItemHandler(string csvPath, string idType) : base(csvPath, idType)
    {
    }

    public new Item GetItemById(int itemId)
    {
        return base.GetItemById(itemId);
    }
}
