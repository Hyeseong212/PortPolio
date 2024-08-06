using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterTrController : MonoBehaviour
{
    private static CharacterTrController instance;
    public static CharacterTrController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CharacterTrController>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("CharacterTrControllerSingleton");
                    instance = singletonObject.AddComponent<CharacterTrController>();
                    var singletonParent = FindObjectOfType<InGameSingleton>();
                    Instantiate(singletonObject, singletonParent.transform);
                }
            }
            return instance;
        }
    }

    [SerializeField] List<GameObject> Characters = new List<GameObject>();
    public Dictionary<long, GameObject> UserTr = new Dictionary<long, GameObject>();
    private Dictionary<long, Vector3> targetPositions = new Dictionary<long, Vector3>();
    private Dictionary<long, Quaternion> targetRotations = new Dictionary<long, Quaternion>();
    GameObject ThisUserCharacter;
    public float lerpSpeed = 20f;

    public void Init()
    {
        Characters.Clear();
        // 모든 게임 오브젝트를 포함하여 찾기
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            // 태그가 "Player"인 오브젝트만 추가
            if (obj.CompareTag("Player"))
            {
                Characters.Add(obj);
            }
        }
        Characters.Reverse();
        UserTr.Clear();
        if (InGameSessionController.Instance.thisPlayerInfo.playerNum == -1)
        {
            return;
        }
        ThisUserCharacter = Characters[InGameSessionController.Instance.thisPlayerInfo.playerNum - 1];
        UserTr.Add(Global.Instance.standbyInfo.userEntity.UserUID, ThisUserCharacter);
        InGameManager.Instance.Init();
    }
    public void TestInit()
    {
        Characters.Clear();
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            // 태그가 "Player"인 오브젝트만 추가
            if (obj.CompareTag("Player"))
            {
                Characters.Add(obj);
            }
        }
        Characters.Reverse();

        UserTr.Clear();
        if (InGameSessionController.Instance.thisPlayerInfo.playerNum == -1)
        {
            return;
        }
        ThisUserCharacter = Characters[InGameSessionController.Instance.thisPlayerInfo.playerNum - 1];
        UserTr.Add(0, ThisUserCharacter);
        InGameManager.Instance.Init();
    }
    public void FixedUpdate()
    {
        if (InGameSessionController.Instance.thisPlayerInfo.isPlayerInfoOK && ThisUserCharacter != null)
        {
            LerpingOpponentMove();
            SendClientCharacterTr(ThisUserCharacter.transform.localPosition, ThisUserCharacter.transform.localRotation);
        }
    }

    public void UpdatePlayerTR(byte[] data)
    {
        long userUID = BitConverter.ToInt64(data, 0);
        int playerNum = BitConverter.ToInt32(data, 8);
        if (!UserTr.ContainsKey(userUID))
        {
            UserTr.Add(userUID, Characters[playerNum - 1]);
        }
        else
        {
            if (userUID != Global.Instance.standbyInfo.userEntity.UserUID)
            {
                float posX = BitConverter.ToSingle(data, 12);
                float posY = BitConverter.ToSingle(data, 16);
                float posZ = BitConverter.ToSingle(data, 20);

                float rotX = BitConverter.ToSingle(data, 24);
                float rotY = BitConverter.ToSingle(data, 28);
                float rotZ = BitConverter.ToSingle(data, 32);
                float rotW = BitConverter.ToSingle(data, 36);

                Vector3 newPosition = new Vector3(posX, posY, posZ);
                Quaternion newRotation = new Quaternion(rotX, rotY, rotZ, rotW);

                targetPositions[userUID] = newPosition;
                targetRotations[userUID] = newRotation;
            }
        }
    }
    private void LerpingOpponentMove()
    {
        foreach (var kvp in UserTr)
        {
            long userId = kvp.Key;
            GameObject character = kvp.Value;

            if (character != ThisUserCharacter)
            {
                if (targetPositions.TryGetValue(userId, out Vector3 targetPosition) &&
                    targetRotations.TryGetValue(userId, out Quaternion targetRotation))
                {
                    character.transform.localPosition = Vector3.Lerp(
                        character.transform.localPosition,
                        targetPosition,
                        lerpSpeed * Time.deltaTime
                    );

                    character.transform.localRotation = Quaternion.Slerp(
                        character.transform.localRotation,
                        targetRotation,
                        lerpSpeed * Time.deltaTime
                    );
                }
            }
        }
    }
    private void SendClientCharacterTr(Vector3 clientChTr, Quaternion quaternion)
    {
        Packet packet = new Packet();
        int length = 0x01 + 0x01 + Utils.GetLength(Global.Instance.standbyInfo.userEntity.UserUID) + Utils.GetLength(clientChTr.x) + Utils.GetLength(clientChTr.y) + Utils.GetLength(clientChTr.z)
            + Utils.GetLength(quaternion.x) + Utils.GetLength(quaternion.y) + Utils.GetLength(quaternion.z) + Utils.GetLength(quaternion.w);

        packet.push((byte)InGameProtocol.CharacterTr);
        packet.push(length);
        packet.push(Global.Instance.standbyInfo.userEntity.UserUID);
        packet.push(clientChTr.x);
        packet.push(clientChTr.y);
        packet.push(clientChTr.z);
        packet.push(quaternion.x);
        packet.push(quaternion.y);
        packet.push(quaternion.z);
        packet.push(quaternion.w);

        InGameTCPController.Instance.SendToInGameServer(packet);
    }

    public void ProcessUpdatePlayerPacket(byte[] data)
    {
        if ((SessionInfo)data[0] == SessionInfo.TransformInfo)
        {
            byte[] trData = data.Skip(1).ToArray();
            UpdatePlayerTR(trData);
        }
    }
}
