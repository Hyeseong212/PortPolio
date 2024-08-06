using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using SharedCode.Model.HttpCommand;

public class LOGINVIEW : MonoBehaviour
{
    [SerializeField] Button loginBtn;
    [SerializeField] InputField IDInputField;
    [SerializeField] InputField PasswordInpuField;
    [SerializeField] Button signUpBtn;

    void Start()
    {
        loginBtn.onClick.AddListener(delegate
        {
            Login();
        });
        signUpBtn.onClick.AddListener(delegate
        {
            SignUp();
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            IDInputField.text = "netrogold";
            PasswordInpuField.text = "Sjh011009!";
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            IDInputField.text = "Rbiotech";
            PasswordInpuField.text = "Sjh011009!";
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            IDInputField.text = "netrohong";
            PasswordInpuField.text = "Sjh011009!";
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            IDInputField.text = "netrosjh";
            PasswordInpuField.text = "Sjh011009!";
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            IDInputField.text = "new";
            PasswordInpuField.text = "new!";
        }
    }

    private void Login()
    {
        if (string.IsNullOrEmpty(IDInputField.text) || string.IsNullOrEmpty(PasswordInpuField.text))
        {
            PopupController.Instance.SetActivePopupWithMessage(POPUPTYPE.MESSAGE, true, "ID를 입력해주세요", null, null);
            return;
        }

        AccountLoginRequest loginInfo = new AccountLoginRequest
        {
            Id = IDInputField.text,
            Password = PasswordInpuField.text
        };

        StartCoroutine(WebAPIController.Instance.PostRequest<AccountLoginRequest>(Global.Instance.apiRequestUri + "Account/Login", loginInfo, WebLoginController.Instance.OnLoginResponse));
    }

    private void SignUp()
    {
        this.gameObject.SetActive(false);
        ViewController.Instance.SetActiveView(VIEWTYPE.SIGNUP, true);
    }
}
