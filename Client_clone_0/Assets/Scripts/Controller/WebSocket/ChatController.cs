using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

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
    public void ProcessChatPacket(byte[] realData, int length)
    {
        string message = Encoding.UTF8.GetString(realData, 1, length - 1);
        ChatStatus status = (ChatStatus)realData[0];

        if (status == ChatStatus.ENTIRE)
        {
            ReceiveMessage(SetChatMessage("#FFFFFF", message)); // ��ü ä���� ���
        }
        else if (status == ChatStatus.WHISPER)
        {
            ReceiveMessage(SetChatMessage("#FFB6C1", message)); // �ӼӸ��� ����ũ
        }
        else if (status == ChatStatus.GUILD)
        {
            ReceiveMessage(SetChatMessage("#C0FF50", message)); // ��� ä���� ����
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
            // `MainView` �̱����� �ִ� ���
            MainView mainView = FindObjectOfType<MainView>();
            if (mainView != null)
            {
                GameObject chat = Instantiate(mainView.ChatObject, mainView.ChatParentObject.transform);
                chat.SetActive(true);
                // ��� �ڽ� Text ������Ʈ ��������
                List<Text> textObjects = mainView.ChatParentObject.GetComponentsInChildren<Text>().ToList();

                // RectTransform ������Ʈ ��������
                RectTransform chatParentRectTransform = mainView.ChatParentObject.GetComponent<RectTransform>();

                // �ڽ� Text ������Ʈ�� ������ �� Text�� ���̷� �������� ���̸� ���
                float requiredHeight = textObjects.Count * 20;

                // ���� ������ ���̿� ���Ͽ� �ʿ��ϸ� �ø���
                if (requiredHeight > chatParentRectTransform.sizeDelta.y)
                {
                    chatParentRectTransform.sizeDelta = new Vector2(chatParentRectTransform.sizeDelta.x, requiredHeight);
                }
                chat.GetComponent<Text>().text = message;
            }
            // ���̾ƿ� ���� ������Ʈ
            // ���̾ƿ� ���� ������Ʈ

            // ��ũ�Ѻ並 �� ���� ����
            mainView.chatScrollbar.verticalNormalizedPosition = 0;

        });
    }
}
