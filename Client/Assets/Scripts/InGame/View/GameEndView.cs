using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEndView : MonoBehaviour
{
    [SerializeField] Button GotolobbyBtn;
    void Start()
    {
        GotolobbyBtn.onClick.AddListener(()=> 
        {
            GotoLobby();
        });
    }
    private void GotoLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
}
