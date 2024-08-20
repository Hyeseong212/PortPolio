using UnityEngine;
using Newtonsoft.Json;

using UnityEngine.UI;
using SharedCode.Model.HttpCommand;

public class SIGNUPVIEW : MonoBehaviour
{
    [SerializeField] InputField nameInputfield;
    [SerializeField] InputField idInputfield;
    [SerializeField] InputField pwInputfield;
    [SerializeField] Button signUpBtn;
    [SerializeField] Button BackBtn;
    private void Start()
    {
        signUpBtn.onClick.AddListener(delegate 
        {
            SignUp();
        });
        BackBtn.onClick.AddListener(delegate
        {
            gameObject.SetActive(false);
            ViewController.Instance.SetActiveView(VIEWTYPE.LOGIN, true);
        });
    }
    private void SignUp()
    {
        AccountCreateRequest CreateInfo = new AccountCreateRequest
        {
            Id = idInputfield.text,
            Password = pwInputfield.text,
            NickName = nameInputfield.text
        };

        StartCoroutine(WebAPIController.Instance.PostRequest<AccountCreateRequest>(Global.Instance.ApiRequestUri + "Account/Create", CreateInfo, WebLoginController.Instance.OnAccountCreateResponse));


        //////////소켓 통신 (legacy)/////////
        //string signupInfoJSON = JsonConvert.SerializeObject(signUpInfo);
        //int Length = 0x01 + Utils.GetLength(signupInfoJSON);

        //Packet packet = new Packet();
        //packet.push((byte)Protocol.Login);
        //packet.push(Length);
        //packet.push((byte)LoginRequestType.SignupRequest);
        //packet.push(signupInfoJSON);
        //TCPController.Instance.SendToServer(packet);
    }
}
