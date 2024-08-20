using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameSingleton : MonoBehaviour
{
    public void Awake()
    {
        InGameSessionController.Instance.Init();
    }
}
