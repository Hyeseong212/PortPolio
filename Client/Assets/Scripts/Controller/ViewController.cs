using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewController : MonoBehaviour
{
    private static ViewController instance;
    public static ViewController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ViewController>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("ViewControllerSingletone");
                    instance = singletonObject.AddComponent<ViewController>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }

    [SerializeField] LOGINVIEW loginpopup;
    [SerializeField] SIGNUPVIEW signpopup;
    [SerializeField] GUILDVIEW guildView;
    [SerializeField] GameStartView gameStartView;
    [SerializeField] GameObject loadingView;

    public void Init()
    {
        Debug.Log("ViewController Init Complete");
    }
    public void SetActiveView(VIEWTYPE type, bool isActive)
    {
        switch (type)
        {
            case VIEWTYPE.LOGIN:
                loginpopup.gameObject.SetActive(isActive);
                break;
            case VIEWTYPE.SIGNUP:
                signpopup.gameObject.SetActive(isActive);
                break;
            case VIEWTYPE.GUILD:
                guildView.gameObject.SetActive(isActive);
                break;
            case VIEWTYPE.GAMESTART:
                gameStartView.gameObject.SetActive(isActive);
                break;
            case VIEWTYPE.LOADING:
                loadingView.gameObject.SetActive(isActive);
                break;
            default:
                Debug.Log("This Popup Type is not Exist");
                break;
        }
    }
}
