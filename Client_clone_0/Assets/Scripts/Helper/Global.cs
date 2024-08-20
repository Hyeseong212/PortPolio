using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour
{

    private static Global instance;
    public static Global Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Global>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("GlobalSingleton");
                    instance = singletonObject.AddComponent<Global>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }
    public StandbyInfo StandbyInfo;
    public List<MessageInfo> MessageInfos;
    public string ApiRequestUri = "http://127.0.0.1:5000/";
    //public string serverIP = "192.168.123.1";
    public string ServerIP = "127.0.0.1";
    //public string serverIP = "192.168.219.100";
    public void Init()
    {
        StandbyInfo = new StandbyInfo();
        Debug.Log("Global Init Complete");
        MessageInfos = new List<MessageInfo>()
        {
            new MessageInfo() {  Idx = 0, Message = "중복된 ID입니다 다른아이디를 입력해주세요." },
            new MessageInfo() {  Idx = 1, Message = "회원가입 성공" },
            new MessageInfo() {  Idx = 2, Message = "가입 신청을 넣으시겠습니까?" },
            new MessageInfo() {  Idx = 3, Message = "이미 가입한 길드가 있어 \n 가입신청을 넣을수 없습니다" },
            new MessageInfo() {  Idx = 4, Message = "요청을 수락하시겠습니까?" },
            new MessageInfo() {  Idx = 5, Message = "ID를 입력해주세요" },
            new MessageInfo() {  Idx = 6, Message = "게임이 매칭 되었습니다!" }
        };
    }
    public void StaticLog(object obj)
    {
        Debug.Log(obj);
    }
}
