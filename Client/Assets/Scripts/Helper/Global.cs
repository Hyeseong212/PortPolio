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
            new MessageInfo() {  idx = 0, message = "�ߺ��� ID�Դϴ� �ٸ����̵� �Է����ּ���." },
            new MessageInfo() {  idx = 1, message = "ȸ������ ����" },
            new MessageInfo() {  idx = 2, message = "���� ��û�� �����ðڽ��ϱ�?" },
            new MessageInfo() {  idx = 3, message = "�̹� ������ ��尡 �־� \n ���Խ�û�� ������ �����ϴ�" },
            new MessageInfo() {  idx = 4, message = "��û�� �����Ͻðڽ��ϱ�?" },
            new MessageInfo() {  idx = 5, message = "ID�� �Է����ּ���" },
            new MessageInfo() {  idx = 6, message = "������ ��Ī �Ǿ����ϴ�!" }
        };
    }
    public void StaticLog(object obj)
    {
        Debug.Log(obj);
    }
}
