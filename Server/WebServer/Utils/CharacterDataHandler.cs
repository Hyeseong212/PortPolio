using System;
using SharedCode.Model;

public class CharacterDataHandler : DataHandler<CharacterStore>
{
    public CharacterDataHandler(string csvPath, string characterId) : base(csvPath, characterId)
    {
    }

    public new CharacterStore GetItemById(int characterId)
    {
        return base.GetItemById(characterId);
    }
}