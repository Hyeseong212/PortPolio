using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SharedCode.MessagePack;
using MessagePack;

public class ChatController : MonoBehaviour
{
    private static ChatController instance;
    public static ChatController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ChatController>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("ChatControllerSingleton");
                    instance = singletonObject.AddComponent<ChatController>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }

    public void Init()
    {
        Debug.Log("ChatController Init Complete");
    }

    public void ProcessChatPacket(byte[] realData)
    {
        var chatPacket = MessagePackSerializer.Deserialize<ChatPacket>(realData);

        string message = chatPacket.Message;
        ChatStatus status = chatPacket.Status;

        if (status == ChatStatus.ENTIRE)
        {
            ReceiveMessage(SetChatMessage("#FFFFFF", message)); // 전체 채팅은 흰색
        }
        else if (status == ChatStatus.WHISPER)
        {
            ReceiveMessage(SetChatMessage("#FFB6C1", message)); // 귓속말은 연핑크
        }
        else if (status == ChatStatus.GUILD)
        {
            ReceiveMessage(SetChatMessage("#C0FF50", message)); // 길드 채팅은 연두
        }
    }

    private string SetChatMessage(string colorCode, string message)
    {
        return $"<color={colorCode}>{message}</color>";
    }

    public void ReceiveMessage(string message)
    {
        WebSocketController.Instance.EnqueueDispatcher(() =>
        {
            MainView mainView = FindObjectOfType<MainView>();
            if (mainView != null)
            {
                GameObject chat = Instantiate(mainView.ChatObject, mainView.ChatParentObject.transform);
                chat.SetActive(true);
                List<Text> textObjects = mainView.ChatParentObject.GetComponentsInChildren<Text>().ToList();
                RectTransform chatParentRectTransform = mainView.ChatParentObject.GetComponent<RectTransform>();
                float requiredHeight = textObjects.Count * 20;
                if (requiredHeight > chatParentRectTransform.sizeDelta.y)
                {
                    chatParentRectTransform.sizeDelta = new Vector2(chatParentRectTransform.sizeDelta.x, requiredHeight);
                }
                chat.GetComponent<Text>().text = message;
            }
            if (mainView != null)
            {
                mainView.chatScrollbar.verticalNormalizedPosition = 0;
            }
        });
    }
}
