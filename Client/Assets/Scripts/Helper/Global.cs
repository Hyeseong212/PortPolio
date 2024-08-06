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
    public StandbyInfo standbyInfo;
    public List<MessageInfo> messageInfos;
    public string apiRequestUri = "http://127.0.0.1:5000/";
    //public string serverIP = "192.168.123.1";
    public string serverIP = "127.0.0.1";
    //public string serverIP = "192.168.219.100";
    public void Init()
    {
        standbyInfo = new StandbyInfo();
        Debug.Log("Global Init Complete");
        messageInfos = new List<MessageInfo>()
        {
            new MessageInfo() {  idx = 0, message = "중복된 ID입니다 다른아이디를 입력해주세요." },
            new MessageInfo() {  idx = 1, message = "회원가입 성공" },
            new MessageInfo() {  idx = 2, message = "가입 신청을 넣으시겠습니까?" },
            new MessageInfo() {  idx = 3, message = "이미 가입한 길드가 있어 \n 가입신청을 넣을수 없습니다" },
            new MessageInfo() {  idx = 4, message = "요청을 수락하시겠습니까?" },
            new MessageInfo() {  idx = 5, message = "ID를 입력해주세요" },
            new MessageInfo() {  idx = 6, message = "게임이 매칭 되었습니다!" }
        };
    }
    public void StaticLog(object obj)
    {
        Debug.Log(obj);
    }
}
