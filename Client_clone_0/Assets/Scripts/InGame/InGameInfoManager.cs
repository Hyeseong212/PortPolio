using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InGameInfoManager : MonoBehaviour
{
    private static InGameInfoManager instance;
    public static InGameInfoManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InGameInfoManager>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("InGameInfoManagerSingleton");
                    instance = singletonObject.AddComponent<InGameInfoManager>();
                    var singletonParent = FindObjectOfType<InGameSingleton>();
                    Instantiate(singletonObject, singletonParent.transform);
                }
            }
            return instance;
        }
    }
    public List<Character> characters = new List<Character>();

    public void Init()
    {

    }

    public void ProcessInGameInfoPacket(byte[] realData)
    {
        if ((GameInfo)realData[0] == GameInfo.IsHit)
        {
            //���������� ������ ó���� ���� �÷��̾�� ü�³ѱ��
            //���⼱ ü�� ó��
            var data = realData.Skip(1).ToArray();
            ProcessPlayerHit(data);
        }
        else if ((GameInfo)realData[0] == GameInfo.CharacterData)
        {
            //���⼱ �������𸣴� �Ѹ� ��Ƴ���
            var data = realData.Skip(1).ToArray();
            ProcessCharacterData(data);
        }
        else if ((GameInfo)realData[0] == GameInfo.CharacterAnimationStatus)
        {
            //�÷��̾� �ѹ��� �������ͽ� �״�� �޾Ƽ� ����
            var data = realData.Skip(1).ToArray();
            ProcessCharacterAnimation(data);
        }
        else if ((GameInfo)realData[0] == GameInfo.SetPlayerCharacterLevel)
        {
            var data = realData.Skip(1).ToArray();
            ProcessPlayerCharacterLevel(data);
        }
    }
    private void ProcessPlayerCharacterLevel(byte[] data)
    {
        int playerNumber = BitConverter.ToInt32(data, 0);
        int level = BitConverter.ToInt32(data, 4);

        var character = characters.FirstOrDefault(character => character.PlayerNum == playerNumber);
        InGameTCPController.Instance.EnqueueDispatcher(() =>
        {
            character.SetLevel(level);
        });
    }
    private void ProcessPlayerHit(byte[] data)
    {
        int playerNum = BitConverter.ToInt32(data, 0);
        float playerHP = BitConverter.ToSingle(data, 4);

        var character = characters.FirstOrDefault(character => character.PlayerNum == playerNum);
        character.SetCurrentHP(playerHP);
    }
    private void ProcessCharacterData(byte[] data)
    {
        //���⼭ �÷��̾� �ѹ��� ĳ���� �޾Ƽ� ����
        var playerNumber = BitConverter.ToInt32(data, 0);
        var characterId = BitConverter.ToInt32(data, 4);

        for (int i = 0; i < InGameManager.Instance.characters.Count; i++)
        {
            if (InGameManager.Instance.characters[i].PlayerNum == playerNumber)
            {
                InGameManager.Instance.characters[i].gameObject.SetActive(true);
                var character = CharacterDataManager.Instance.GetChracterData(characterId);
                InGameManager.Instance.characters[i].SetCharacterModel(characterId);
                InGameManager.Instance.characters[i].SetCharacterData(character);
                InGameManager.Instance.characters[i].gameObject.GetComponent<UnitMovement>().m_animatorController = InGameManager.Instance.characters[i].gameObject.GetComponentInChildren<AnimatorController>();
            }
        }
        Packet packet = new Packet();

        int length = 0x01 + Utils.GetLength(InGameSessionController.Instance.thisPlayerInfo.PlayerNum);

        packet.push((byte)InGameProtocol.SessionInfo);
        packet.push(length);
        packet.push((byte)SessionInfo.LoadingOK);
        packet.push(InGameSessionController.Instance.thisPlayerInfo.PlayerNum);

        InGameTCPController.Instance.SendToInGameServer(packet);
    }
    private void ProcessCharacterAnimation(byte[] data)
    {
        CharacterStatus characterStatus = (CharacterStatus)data[0];
        int playerNum = data[1];

        if (playerNum == InGameManager.Instance.selectedCharacter.PlayerNum)
        {
            return;
        }

        var Animecharacter = characters.FirstOrDefault(character => character.PlayerNum == playerNum);
        if (characterStatus == CharacterStatus.ATTACK)
        {
            int targetNum = data[2];
            var Targetcharacter = characters.FirstOrDefault(character => character.PlayerNum == targetNum);
            Animecharacter.Target = Targetcharacter.gameObject;
        }
        Animecharacter.GetComponentInChildren<AnimatorController>().CurrentStatus = characterStatus;
    }
}
