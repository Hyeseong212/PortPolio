using System;
using UnityEngine;

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
    public GameObject Bullet { get; set; }
}

