using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;    // 카메라가 따라갈 대상
    public Vector3 offset;      // 카메라와 대상 사이의 오프셋

    void Start()
    {
        // 현재 위치를 기반으로 초기 오프셋 설정
        offset = new Vector3(-2.9f, 8.4f, -2.9f); //transform.position - target.position;
    }
    public void Init()
    {
    }

    void LateUpdate()
    {
        // 대상의 위치와 오프셋을 기반으로 카메라 위치 업데이트
        transform.position = target.position + offset;
    }
}
