using System.Numerics;

public class Character
{
    public long Uid; // 사용하는 유저
    public int PlayerNum;
    public int CharacterId; // 어떤 캐릭터인지
    public float TotalHP; // 캐릭터 HP
    public float CurrentHP; // 캐릭터 HP
    public float TotalMP; // 캐릭터 MP
    public float CurrentMP; // 캐릭터 MP
    public bool IsHit; // 캐릭터가 맞았는지
    public int Level; // 캐릭터 레벨
    public Vector3 Position;
    public Quaternion Quaternion;
    public CharacterData CharacterData;

    public Character()
    {
        Uid = 0;
        PlayerNum = -1;
        CharacterId = -1;
        CurrentHP = 0;
        TotalHP = 0;
        CurrentMP = 0;
        TotalMP = 0;
        IsHit = false;
        Level = 1;
        CharacterData = new CharacterData();
    }
}

[Serializable]
public class CharacterData
{
    public int CharacterId { get; set; }
    public string CharacterName { get; set; }
    public int AttackRange { get; set; }
    public float AttackRatePerLv { get; set; }
    public float AttackRate { get; set; }
    public float AttackPerLv { get; set; }
    public float Attack { get; set; }
    public float Defense { get; set; }
    public int BulletId { get; set; }
    public float TotalHP { get; set; }
    public float HPPerLv { get; set; }
    public float TotalMP { get; set; }
    public float MPPerLv { get; set; }
}
public enum CharacterStatus
{
    NONE,
    IDLE,
    MOVE,
    ATTACK,
    DAMAGED,
    DIE
}
