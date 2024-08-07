using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.IO;
using System.Threading.Tasks;
using MessagePack;
using SharedCode.MessagePack;
using SharedCode.Model;

internal class InGameWorld : IDisposable
{
    public InGameSession InGameSession;
    public List<Character> UsersCharacter;
    public List<CharacterData> CharacterDataList = new List<CharacterData>();

    public InGameWorld(InGameSession inGameSession)
    {
        InGameSession = inGameSession;
        UsersCharacter = new List<Character>();

#if DEBUG
        string currentDirectory = Directory.GetCurrentDirectory();
        string fullPath = Path.Combine(currentDirectory, "\\Resources\\PropertyId", "Character.csv");
#elif RELEASE
        string rootPath = AppDomain.CurrentDomain.BaseDirectory;
        string relativePath = @"Resources\PropertyId\Character.csv";
        string fullPath = Path.Combine(rootPath, relativePath);
#endif

        CharacterDataList = LoadCharacterData(fullPath);
    }

    public void UpdatePlayerTR(byte[] data)
    {
        long userUID = BitConverter.ToInt64(data, 0);
        for (int i = 0; i < UsersCharacter.Count; i++)
        {
            if (UsersCharacter[i].Uid == userUID)
            {
                UsersCharacter[i].Position = new Vector3(BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12), BitConverter.ToSingle(data, 16));
                UsersCharacter[i].Quaternion = new Quaternion(BitConverter.ToSingle(data, 20), BitConverter.ToSingle(data, 24), BitConverter.ToSingle(data, 28), BitConverter.ToSingle(data, 32));
            }
        }
    }

    public void UpdatePlayerDataInfo(byte[] data)
    {
        var packet = MessagePackSerializer.Deserialize<Packet>(data);

        if ((GameInfo)packet.Data[0] == GameInfo.IsHit)
        {
            var realData = packet.Data.Skip(1).ToArray();
            ProcessDamaged(realData);
        }
        else if ((GameInfo)packet.Data[0] == GameInfo.CharacterData)
        {
            var realData = packet.Data.Skip(1).ToArray();
            ProcessCharacterData(realData);
        }
        else if ((GameInfo)packet.Data[0] == GameInfo.CharacterAnimationStatus)
        {
            var realData = packet.Data.Skip(1).ToArray();
            ProcessAnimationStatus(realData);
        }
        else if ((GameInfo)packet.Data[0] == GameInfo.SetPlayerCharacterLevel)
        {
            var realData = packet.Data.Skip(1).ToArray();
            ProcessPlayerCharacterLevel(realData);
        }
    }

    private void ProcessCharacterData(byte[] data)
    {
        int playerNumber = BitConverter.ToInt32(data, 0);
        int selectedCharacterId = BitConverter.ToInt32(data, 4);

        var selectedCharacterData = CharacterDataList.FirstOrDefault(c => c.CharacterId == selectedCharacterId);

        if (selectedCharacterData != null)
        {
            var existingCharacter = UsersCharacter.FirstOrDefault(c => c.PlayerNum == playerNumber);

            if (existingCharacter != null)
            {
                existingCharacter.CharacterId = selectedCharacterData.CharacterId;
                existingCharacter.TotalHP = selectedCharacterData.TotalHP;
                existingCharacter.CurrentHP = selectedCharacterData.TotalHP;
                existingCharacter.TotalMP = selectedCharacterData.TotalMP;
                existingCharacter.CurrentMP = selectedCharacterData.TotalMP;
                existingCharacter.CharacterData = selectedCharacterData;

                Logger.SetLogger(LOGTYPE.INFO, $"Character updated: {existingCharacter.CharacterData.CharacterName}");
            }
            else
            {
                Character newCharacter = new Character
                {
                    PlayerNum = playerNumber,
                    CharacterId = selectedCharacterData.CharacterId,
                    TotalHP = selectedCharacterData.TotalHP,
                    CurrentHP = selectedCharacterData.TotalHP,
                    TotalMP = selectedCharacterData.TotalMP,
                    CurrentMP = selectedCharacterData.TotalMP,
                    CharacterData = selectedCharacterData
                };

                UsersCharacter.Add(newCharacter);

                Logger.SetLogger(LOGTYPE.INFO, $"Character created: {newCharacter.CharacterData.CharacterName}");
            }

            var responsePacket = new Packet
            {
                Protocol = (byte)InGameProtocol.GameInfo,
                Data = MessagePackSerializer.Serialize(new CharacterDataPacket
                {
                    PlayerNumber = playerNumber,
                    CharacterId = selectedCharacterId
                })
            };

            var serializedData = MessagePackSerializer.Serialize(responsePacket);
            InGameSession?.SendToAllClient(serializedData);
        }
        else
        {
            Logger.SetLogger(LOGTYPE.INFO, "Character data not found for the given CharacterId: " + selectedCharacterId);
        }
    }

    private List<CharacterData> LoadCharacterData(string filePath)
    {
        var characterDataList = new List<CharacterData>();
        var lines = File.ReadAllLines(filePath);
        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            var characterData = new CharacterData
            {
                CharacterId = int.Parse(values[0]),
                CharacterName = values[1],
                AttackRange = int.Parse(values[2]),
                AttackRatePerLv = float.Parse(values[3]),
                AttackRate = float.Parse(values[4]),
                AttackPerLv = float.Parse(values[5]),
                Attack = float.Parse(values[6]),
                Defense = float.Parse(values[7]),
                BulletId = int.Parse(values[8]),
                TotalHP = int.Parse(values[9]),
                HPPerLv = int.Parse(values[10]),
                TotalMP = int.Parse(values[11]),
                MPPerLv = int.Parse(values[12])
            };
            characterDataList.Add(characterData);
        }
        return characterDataList;
    }

    private void ProcessDamaged(byte[] data)
    {
        int shooterPlayerNumber = BitConverter.ToInt32(data, 0);
        int targetPlayerNumber = BitConverter.ToInt32(data, 4);

        Character shooterCharacter = UsersCharacter.FirstOrDefault(c => c.PlayerNum == shooterPlayerNumber);
        Character targetCharacter = UsersCharacter.FirstOrDefault(c => c.PlayerNum == targetPlayerNumber);

        if (shooterCharacter == null || targetCharacter == null)
        {
            Logger.SetLogger(LOGTYPE.INFO, "Character not found for the given player number.");
            return;
        }

        float shooterDamage = shooterCharacter.CharacterData.Attack + (shooterCharacter.Level - 1) * shooterCharacter.CharacterData.AttackPerLv;
        float targetDefense = targetCharacter.CharacterData.Defense;

        float damageReduction = targetDefense / (100 + targetDefense);
        float actualDamage = shooterDamage * (1 - damageReduction);

        targetCharacter.CurrentHP -= actualDamage;

        if (targetCharacter.CurrentHP < 0)
        {
            targetCharacter.CurrentHP = 0;
            Task.Run(() => HandleGameEndAsync());
            return;
        }

        var responsePacket = new Packet
        {
            Protocol = (byte)InGameProtocol.GameInfo,
            Data = MessagePackSerializer.Serialize(new DamagePacket
            {
                PlayerNumber = targetPlayerNumber,
                CurrentHP = targetCharacter.CurrentHP
            })
        };

        var serializedData = MessagePackSerializer.Serialize(responsePacket);
        InGameSession?.SendToAllClient(serializedData);
    }

    private void ProcessAnimationStatus(byte[] data)
    {
        CharacterStatus characterStatus = (CharacterStatus)data[0];
        int playerNumber = BitConverter.ToInt32(data, 1);

        var responsePacket = new Packet
        {
            Protocol = (byte)InGameProtocol.GameInfo,
            Data = MessagePackSerializer.Serialize(new AnimationStatusPacket
            {
                CharacterStatus = characterStatus,
                PlayerNumber = playerNumber,
                TargetPlayerNumber = characterStatus == CharacterStatus.ATTACK ? BitConverter.ToInt32(data, 5) : (int?)null
            })
        };

        var serializedData = MessagePackSerializer.Serialize(responsePacket);
        InGameSession?.SendToAllClient(serializedData);
    }

    private void ProcessPlayerCharacterLevel(byte[] data)
    {
        int playerNumber = BitConverter.ToInt32(data, 0);
        int newLevel = 30;

        Character userCharacter = UsersCharacter.FirstOrDefault(c => c.PlayerNum == playerNumber);

        if (userCharacter != null)
        {
            userCharacter.Level = newLevel;
            userCharacter.TotalHP = userCharacter.CharacterData.TotalHP + (newLevel - 1) * userCharacter.CharacterData.HPPerLv;
            userCharacter.CurrentHP = userCharacter.TotalHP;
            userCharacter.TotalMP = userCharacter.CharacterData.TotalMP + (newLevel - 1) * userCharacter.CharacterData.MPPerLv;
            userCharacter.CurrentMP = userCharacter.TotalMP;

            var responsePacket = new Packet
            {
                Protocol = (byte)InGameProtocol.GameInfo,
                Data = MessagePackSerializer.Serialize(new PlayerLevelPacket
                {
                    PlayerNumber = playerNumber,
                    NewLevel = newLevel
                })
            };

            var serializedData = MessagePackSerializer.Serialize(responsePacket);
            InGameSession?.SendToAllClient(serializedData);
        }
        else
        {
            Logger.SetLogger(LOGTYPE.INFO, "Character not found for the given player number: " + playerNumber);
        }
    }

    private async Task HandleGameEndAsync()
    {
        var gameEndPacket = new Packet
        {
            Protocol = (byte)InGameProtocol.SessionInfo,
            Data = new byte[] { (byte)SessionInfo.GameEnd }
        };

        var serializedData = MessagePackSerializer.Serialize(gameEndPacket);
        InGameSession?.SendToAllClient(serializedData);

        await Task.Delay(TimeSpan.FromMinutes(2));

        var gameBanPacket = new Packet
        {
            Protocol = (byte)InGameProtocol.SessionInfo,
            Data = new byte[] { (byte)SessionInfo.Ban }
        };

        serializedData = MessagePackSerializer.Serialize(gameBanPacket);
        InGameSession?.SendToAllClient(serializedData);

        await Task.Delay(TimeSpan.FromMinutes(1));

        await InGameSession?.EndGameCleanupAsync();
    }

    private bool disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            InGameSession = null;
            UsersCharacter.Clear();
            CharacterDataList.Clear();
        }

        disposed = true;
    }

    ~InGameWorld()
    {
        Dispose(false);
    }
}
