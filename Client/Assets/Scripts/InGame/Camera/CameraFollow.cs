using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;    // ī�޶� ���� ���
    public Vector3 offset;      // ī�޶�� ��� ������ ������

    void Start()
    {
        // ���� ��ġ�� ������� �ʱ� ������ ����
        offset = new Vector3(-2.9f, 8.4f, -2.9f); //transform.position - target.position;
    }
    public void Init()
    {
    }

    void LateUpdate()
    {
        // ����� ��ġ�� �������� ������� ī�޶� ��ġ ������Ʈ
        transform.position = target.position + offset;
    }
}
