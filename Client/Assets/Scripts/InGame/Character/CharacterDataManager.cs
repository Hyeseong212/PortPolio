using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class CharacterDataManager : MonoBehaviour
{
    private static CharacterDataManager instance;
    public static CharacterDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CharacterDataManager>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("CharacterDataManagerSingleton");
                    instance = singletonObject.AddComponent<CharacterDataManager>();
                    var singletonParent = FindObjectOfType<InGameSingleton>();
                    Instantiate(singletonObject, singletonParent.transform);
                }
            }
            return instance;
        }
    }
    private List<CharacterData> characters = new List<CharacterData>();
    private string characterDataPath = "Assets/Resources/PropertyId/Character.csv";

    public void Awake()
    {
        GetCharacterFromCSV();
    }

    public CharacterData GetChracterData(int chracterID)
    {
        // ���ϴ� ĳ���� ID�� �Ӽ��� �����ͼ� ��ü�� ����
        CharacterData selectedCharacter = characters.Find(c => c.CharacterId == chracterID);
        if (selectedCharacter != null)
        {
            return selectedCharacter;
        }
        else
        {
            Debug.LogError("Character ID not found.");
            return null;
        }
    }
    private void GetCharacterFromCSV()
    {
        string[] dataLines = File.ReadAllLines(characterDataPath);
        foreach (string line in dataLines.Skip(1)) // ù ��° ���� �����ϱ� ���� Skip(1)�� ����մϴ�.
        {
            string[] data = line.Split(',');
            CharacterData character = new CharacterData
            {
                CharacterId = int.Parse(data[0]),
                CharacterName = data[1],
                AttackRange = int.Parse(data[2]),
                AttackRatePerLv = float.Parse(data[3]),
                AttackRate = float.Parse(data[4]),
                AttackPerLv = float.Parse(data[5]), // �� �κ��� float.Parse�� �����մϴ�.
                Attack = float.Parse(data[6]),
                Defense = float.Parse(data[7]),
                BulletId = int.Parse(data[8]),
                TotalHP = int.Parse(data[9]),
                HPPerLv = int.Parse(data[10]),
                TotalMP = int.Parse(data[11]),
                MPPerLv = int.Parse(data[12])
            };
            characters.Add(character);
        }
    }
    public GameObject GetBullet(int bulletId)
    {
        string bulletPath = $"Bullet/Bullet_{bulletId}";
        var bullet = Resources.Load<GameObject>(bulletPath);
        if (bullet != null)
        {
            return bullet;
        }
        else
        {
            return null;
            Debug.LogError($"Bullet prefab with ID {bulletId} not found at path: {bulletPath}");
        }
    }
}
