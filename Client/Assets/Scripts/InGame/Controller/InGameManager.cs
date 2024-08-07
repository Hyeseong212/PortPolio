using Rito.FogOfWar;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MessagePack;
using SharedCode.MessagePack;

public class InGameManager : MonoBehaviour
{
    private static InGameManager instance;
    public static InGameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InGameManager>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("PlayerControllerSingleton");
                    instance = singletonObject.AddComponent<InGameManager>();
                    var singletonParent = FindObjectOfType<InGameSingleton>();
                    Instantiate(singletonObject, singletonParent.transform);
                }
            }
            return instance;
        }
    }

    [SerializeField] GameObject[] players;
    [SerializeField] GameObject Marker;

    public List<Character> characters = new List<Character>();
    public List<Image> PlayerStatusBar = new List<Image>();
    public List<Image> PlayerHPBar = new List<Image>();
    public List<Image> PlayerMPBar = new List<Image>();
    public Character selectedCharacter;
    public Character targetedCharacter;
    private Camera characterCam;

    public void GameStart()
    {
        Invoke("PrivateGameStart", 1f);
    }

    private void PrivateGameStart()
    {
        var playerNum = InGameSessionController.Instance.thisPlayerInfo.playerNum;

        var levelPacket = new SetPlayerCharacterLevelPacket
        {
            PlayerNum = playerNum
        };

        var packet = new Packet
        {
            Protocol = (byte)InGameProtocol.GameInfo,
            Data = MessagePackSerializer.Serialize(levelPacket)
        };

        InGameTCPController.Instance.SendToInGameServer(packet);
    }

    public void Init()
    {
        var characterCam = FindObjectOfType<CameraFollow>();
        this.characterCam = characterCam.GetComponent<Camera>();
        Global.Instance.StaticLog($"this PlayerNumber is {InGameSessionController.Instance.thisPlayerInfo.playerNum}");
        selectedCharacter = characters[InGameSessionController.Instance.thisPlayerInfo.playerNum - 1];

        characterCam.target = players[InGameSessionController.Instance.thisPlayerInfo.playerNum - 1].transform;
        for (int i = 0; i < players.Length; i++)
        {
            if (InGameSessionController.Instance.thisPlayerInfo.playerNum != i + 1)
            {
                players[i].GetComponent<UnitMovement>().enabled = false;
                players[i].GetComponent<AStarPathfinding>().enabled = false;
                players[i].GetComponent<FowUnit>().enabled = false;
            }
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            Vector3 screenPos = characterCam.WorldToScreenPoint(characters[i].transform.position + Vector3.up * 2.5f);
            PlayerStatusBar[i].transform.position = screenPos;

            PlayerHPBar[i].fillAmount = characters[i].CurrentHP / characters[i].TotalHP;
            PlayerMPBar[i].fillAmount = characters[i].CurrentMP / characters[i].TotalMP;
        }
    }

    void Update()
    {
        if (characterCam == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = characterCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Character clickedCharacter = hit.collider.GetComponent<Character>();
                if (clickedCharacter != null && FowManager.Instance.IsCharacterVisible(clickedCharacter.gameObject))
                {
                    if (targetedCharacter != null)
                    {
                        targetedCharacter.RemoveOutlineShader();
                    }
                    if (clickedCharacter == selectedCharacter)
                        return;
                    targetedCharacter = clickedCharacter;
                    targetedCharacter.ApplyOutlineShader(targetedCharacter.outlineClicked);

                    selectedCharacter.Target = targetedCharacter.gameObject;
                    selectedCharacter.Attack();
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = characterCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    Marker.SetActive(false);
                    Marker.transform.position = hit.point;
                    Marker.SetActive(true);
                }
            }
        }
    }
}
