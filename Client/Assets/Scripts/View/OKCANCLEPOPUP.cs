using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OKCANCLEPOPUP : MonoBehaviour
{
    [SerializeField] Text messageTxt;
    [SerializeField] Button OKbtn;
    [SerializeField] Button Cancelbtn;

    Action thisOKAction;
    Action thisCancelAction;

    public void Start()
    {
        OKbtn.onClick.AddListener(delegate
        {
            if (thisOKAction != null)
            {
                thisOKAction.Invoke();
            }
            gameObject.SetActive(false);
        });
        Cancelbtn.onClick.AddListener(delegate
        {
            if (thisCancelAction != null)
            {
                thisCancelAction.Invoke();
            }
            gameObject.SetActive(false);
        });
    }
    public void Init(string message, Action OkAction, Action cancelAction)
    {
        thisOKAction = OkAction;
        thisCancelAction = cancelAction;
        messageTxt.text = message;
    }
}
