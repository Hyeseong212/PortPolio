using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
        //string rootPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;
        //string relativePath = @"WebServer\Resources\PropertyId\Character.csv";
#elif RELEASE
        string rootPath = AppDomain.CurrentDomain.BaseDirectory;
        string relativePath = @"Resources\PropertyId\Character.csv";
#endif
        //string fullPath = Path.Combine(rootPath, relativePath);

        CharacterDataList = LoadCharacterData(fullPath);
    }


    public void UpdatePlayerTR(byte[] data)//업데이트 시켜주고
    {
        long userUID = BitConverter.ToInt64(data, 0);
        for (int i = 0; i < UsersCharacter.Count; i++)
        {
            if (UsersCharacter[i].Uid == userUID)
            {
                UsersCharacter[i].Position = new Vector3(BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12), BitConverter.ToSingle(data, 16));
                UsersCharacter[i].Quaternion = new Quaternion(BitConverter.ToSingle(data, 20), BitConverter.ToSingle(data, 24), BitConverter.ToSingle(data, 28), BitConverter.ToSingle(data, 32));

                // 위치 로그 출력
                //Console.WriteLine($"User {usersCharacter[i].uid} POS: X={usersCharacter[i].m_position.X}, Y={usersCharacter[i].m_position.Y}, Z={usersCharacter[i].m_position.Z}");
                //Console.WriteLine($"User {usersCharacter[i].uid} Rotation: X={usersCharacter[i].m_quaternion.X}, Y={usersCharacter[i].m_quaternion.Y}, Z={usersCharacter[i].m_quaternion.Z}, W={usersCharacter[i].m_quaternion.W}");
            }
        }
    }
    public void UpdatePlayerDataInfo(byte[] data)
    {
        if ((GameInfo)data[0] == GameInfo.IsHit)
        {
            var realdata = data.Skip(1).ToArray();
            ProcessDamaged(realdata);
        }
        else if ((GameInfo)data[0] == GameInfo.CharacterData)
        {
            var realdata = data.Skip(1).ToArray();
            ProcessCharacterData(realdata);
        }
        else if ((GameInfo)data[0] == GameInfo.CharacterAnimationStatus)
        {
            var realdata = data.Skip(1).ToArray();
            ProcessAnimationStatus(realdata);
        }
        else if ((GameInfo)data[0] == GameInfo.SetPlayerCharacterLevel)
        {
            var realdata = data.Skip(1).ToArray();
            ProcessPlayerCharacterLevel(realdata);
        }

    }
    private void ProcessCharacterData(byte[] data)
    {
        int playerNumber = BitConverter.ToInt32(data, 0);
        int selectedCharacterId = BitConverter.ToInt32(data, 4);

        // CSV 파일 읽기

        // selectedCharacterId와 일치하는 CharacterData 찾기
        var selectedCharacterData = CharacterDataList.FirstOrDefault(c => c.CharacterId == selectedCharacterId);

        if (selectedCharacterData != null)
        {
            // 기존 캐릭터를 찾아 업데이트
            var existingCharacter = UsersCharacter.FirstOrDefault(c => c.PlayerNum == playerNumber);

            if (existingCharacter != null)
            {
                // 기존 캐릭터 업데이트
                existingCharacter.CharacterId = selectedCharacterData.CharacterId;
                existingCharacter.TotalHP = selectedCharacterData.TotalHP;
                existingCharacter.CurrentHP = selectedCharacterData.TotalHP; // 처음에는 현재 HP를 총 HP로 설정
                existingCharacter.TotalMP = selectedCharacterData.TotalMP;
                existingCharacter.CurrentMP = selectedCharacterData.TotalMP; // 처음에는 현재 MP를 총 MP로 설정
                existingCharacter.CharacterData = selectedCharacterData;

                Console.WriteLine($"Character updated: {existingCharacter.CharacterData.CharacterName}");
            }
            else
            {
                // 새로운 Character 객체 생성
                Character newCharacter = new Character
                {
                    PlayerNum = playerNumber,
                    CharacterId = selectedCharacterData.CharacterId,
                    TotalHP = selectedCharacterData.TotalHP,
                    CurrentHP = selectedCharacterData.TotalHP, // 처음에는 현재 HP를 총 HP로 설정
                    TotalMP = selectedCharacterData.TotalMP,
                    CurrentMP = selectedCharacterData.TotalMP, // 처음에는 현재 MP를 총 MP로 설정
                    CharacterData = selectedCharacterData
                };

                UsersCharacter.Add(newCharacter);

                Console.WriteLine($"Character created: {newCharacter.CharacterData.CharacterName}");
            }

            // 필요한 추가 처리...
        }
        else
        {
            Console.WriteLine("Character data not found for the given CharacterId: " + selectedCharacterId);
        }

        Packet packet = new Packet();

        int length = 0x01 + Utils.GetLength(playerNumber) + Utils.GetLength(selectedCharacterId);

        packet.push((byte)InGameProtocol.GameInfo);
        packet.push(length);
        packet.push((byte)GameInfo.CharacterData);
        packet.push(playerNumber);
        packet.push(selectedCharacterId);

        InGameSession?.SendToAllClient(packet);
    }


    private List<CharacterData> LoadCharacterData(string filePath)
    {
        var characterDataList = new List<CharacterData>();
        var lines = File.ReadAllLines(filePath);
        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더이므로 건너뜀
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
            // 해당 플레이어의 캐릭터를 찾을 수 없는 경우 처리
            Console.WriteLine("Character not found for the given player number.");
            return;
        }

        // 데미지 계산
        float shooterDamage = shooterCharacter.CharacterData.Attack + (shooterCharacter.Level - 1) * shooterCharacter.CharacterData.AttackPerLv;
        float targetDefense = targetCharacter.CharacterData.Defense;

        // 방어력 공식 적용하여 실제 데미지 계산
        float damageReduction = targetDefense / (100 + targetDefense);
        float actualDamage = shooterDamage * (1 - damageReduction);

        // 맞은 사람의 현재 체력에서 데미지 만큼 감소
        targetCharacter.CurrentHP -= actualDamage;

        // 체력이 0보다 작아지지 않도록 처리
        if (targetCharacter.CurrentHP < 0)
        {
            targetCharacter.CurrentHP = 0;

            // 결과 처리 로직 호출
            Task.Run(()=> HandleGameEndAsync());
            return;
        }

        // 로그 출력 (서버에서 디버깅용으로)
        //Console.WriteLine($"Player {targetPlayerNumber} took {actualDamage} damage from player {shooterPlayerNumber}. Remaining HP: {targetCharacter.CurrentHP}");

        Packet packet = new Packet();

        int length = 0x01 + Utils.GetLength(targetPlayerNumber) + Utils.GetLength(targetCharacter.CurrentHP);

        packet.push((byte)InGameProtocol.GameInfo);
        packet.push(length);
        packet.push((byte)GameInfo.IsHit);
        packet.push(targetPlayerNumber);
        packet.push(targetCharacter.CurrentHP);

        InGameSession?.SendToAllClient(packet);
    }
    private void ProcessAnimationStatus(byte[] data)
    {
        CharacterStatus characterStatus = (CharacterStatus)data[0];
        int PlayerNumber = BitConverter.ToInt32(data, 1);

        Packet packet = new Packet();

        int length = 0;

        if (characterStatus == CharacterStatus.ATTACK)
        {
            length = 0x01 + 0x01 + Utils.GetLength(PlayerNumber) + 0x04;
        }
        else
        {
            length = 0x01 + 0x01 + Utils.GetLength(PlayerNumber);
        }


        packet.push((byte)InGameProtocol.GameInfo);
        packet.push(length);
        packet.push((byte)GameInfo.CharacterAnimationStatus);
        packet.push((byte)characterStatus);
        packet.push(PlayerNumber);
        if (characterStatus == CharacterStatus.ATTACK)
        {
            int targetPlayerNumber = BitConverter.ToInt32(data, 5);
            packet.push(targetPlayerNumber);
        }

        InGameSession?.SendToAllClient(packet);
    }
    private void ProcessPlayerCharacterLevel(byte[] data)
    {
        int playerNumber = BitConverter.ToInt32(data, 0);
        int newLevel = 30;
        // 플레이어의 캐릭터 정보를 가져옴
        Character userCharacter = UsersCharacter.FirstOrDefault(c => c.PlayerNum == playerNumber);

        if (userCharacter != null)
        {
            // 새로운 레벨 설정
            userCharacter.Level = newLevel;

            // 체력, 공속, MP, 공격력 등을 레벨에 맞게 세팅
            userCharacter.TotalHP = userCharacter.CharacterData.TotalHP + (newLevel - 1) * userCharacter.CharacterData.HPPerLv;
            userCharacter.CurrentHP = userCharacter.TotalHP; // 레벨업 시 체력을 최대치로 설정
            userCharacter.TotalMP = userCharacter.CharacterData.TotalMP + (newLevel - 1) * userCharacter.CharacterData.MPPerLv;
            userCharacter.CurrentMP = userCharacter.TotalMP; // 레벨업 시 MP를 최대치로 설정

            // 패킷 생성
            Packet packet = new Packet();
            int length = 0x01 + Utils.GetLength(playerNumber) + Utils.GetLength(newLevel);

            packet.push((byte)InGameProtocol.GameInfo);
            packet.push(length);
            packet.push((byte)GameInfo.SetPlayerCharacterLevel);
            packet.push(playerNumber);
            packet.push(newLevel);

            InGameSession?.SendToAllClient(packet);
        }
        else
        {
            Console.WriteLine("Character not found for the given player number: " + playerNumber);
        }
    }
    private async Task HandleGameEndAsync()
    {
        // 게임 종료 패킷 전송
        Packet gameEndPacket = new Packet();
        int gameEndLength = 0x01;
        gameEndPacket.push((byte)InGameProtocol.SessionInfo);
        gameEndPacket.push(gameEndLength);
        gameEndPacket.push((byte)SessionInfo.GameEnd);
        InGameSession?.SendToAllClient(gameEndPacket);

        // 2분 기다리기
        await Task.Delay(TimeSpan.FromMinutes(2));

        // 2분 후 BanPacket 전송
        Packet gameBanPacket = new Packet();
        int gameBanLength = 0x01;
        gameBanPacket.push((byte)InGameProtocol.SessionInfo);
        gameBanPacket.push(gameBanLength);
        gameBanPacket.push((byte)SessionInfo.Ban);
        InGameSession?.SendToAllClient(gameBanPacket);

        // 1분 기다리기
        await Task.Delay(TimeSpan.FromMinutes(1));

        // 1분 후 인게임 세션 정리
        InGameSession?.EndGameCleanupAsync();
    }

    private bool disposed = false;

    // 기타 리소스들...

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
            // 관리되는 리소스 해제
            // 예: 데이터 리스트 클리어, 이벤트 핸들러 제거 등
            InGameSession = null;
            UsersCharacter.Clear();
            CharacterDataList.Clear();
        }

        // 관리되지 않는 리소스 해제
        // 필요한 경우 추가

        disposed = true;
    }

    ~InGameWorld()
    {
        Dispose(false);
    }
}