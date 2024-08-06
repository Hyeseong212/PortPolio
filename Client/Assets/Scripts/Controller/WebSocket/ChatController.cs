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
            // `MainView` 싱글톤이 있는 경우
            MainView mainView = FindObjectOfType<MainView>();
            if (mainView != null)
            {
                GameObject chat = Instantiate(mainView.ChatObject, mainView.ChatParentObject.transform);
                chat.SetActive(true);
                // 모든 자식 Text 컴포넌트 가져오기
                List<Text> textObjects = mainView.ChatParentObject.GetComponentsInChildren<Text>().ToList();

                // RectTransform 컴포넌트 가져오기
                RectTransform chatParentRectTransform = mainView.ChatParentObject.GetComponent<RectTransform>();

                // 자식 Text 컴포넌트의 개수와 각 Text의 높이로 컨텐츠의 높이를 계산
                float requiredHeight = textObjects.Count * 20;

                // 현재 컨텐츠 높이와 비교하여 필요하면 늘리기
                if (requiredHeight > chatParentRectTransform.sizeDelta.y)
                {
                    chatParentRectTransform.sizeDelta = new Vector2(chatParentRectTransform.sizeDelta.x, requiredHeight);
                }
                chat.GetComponent<Text>().text = message;
            }
            // 레이아웃 강제 업데이트
            // 레이아웃 강제 업데이트

            // 스크롤뷰를 맨 위로 설정
            mainView.chatScrollbar.verticalNormalizedPosition = 0;

        });
    }
}
