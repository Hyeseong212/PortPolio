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
            new MessageInfo() {  Idx = 0, Message = "�ߺ��� ID�Դϴ� �ٸ����̵� �Է����ּ���." },
            new MessageInfo() {  Idx = 1, Message = "ȸ������ ����" },
            new MessageInfo() {  Idx = 2, Message = "���� ��û�� �����ðڽ��ϱ�?" },
            new MessageInfo() {  Idx = 3, Message = "�̹� ������ ��尡 �־� \n ���Խ�û�� ������ �����ϴ�" },
            new MessageInfo() {  Idx = 4, Message = "��û�� �����Ͻðڽ��ϱ�?" },
            new MessageInfo() {  Idx = 5, Message = "ID�� �Է����ּ���" },
            new MessageInfo() {  Idx = 6, Message = "������ ��Ī �Ǿ����ϴ�!" }
        };
    }
    public void StaticLog(object obj)
    {
        Debug.Log(obj);
    }
}
