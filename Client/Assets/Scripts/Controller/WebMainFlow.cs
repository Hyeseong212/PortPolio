using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebMainFlow : MonoBehaviour
{
    private void Awake()
    {
        WebSocketController.Instance.Init();
    }
}
