using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCode : MonoBehaviour
{
    public int PlayerNumber = 0;
    public int CharacterID = 0;
    public int OpponentCharacterID = 0;
    void Start()
    {
        InGameSessionController.Instance.thisPlayerInfo.PlayerNum = PlayerNumber;
        Global.Instance.StaticLog($"PlayerNum : {InGameSessionController.Instance.thisPlayerInfo.PlayerNum }");
        //CharacterTrController.Instance.TestInit();
        //for (int i = 0; i < 4; i++)
        //{
        //    if (InGameManager.Instance.characters[i].PlayerNum == InGameSessionController.Instance.thisPlayerInfo.PlayerNum)
        //    {
        //        var character = CharacterDataManager.Instance.GetChracterData(CharacterID);
        //        InGameManager.Instance.characters[i].SetCharacterModel(CharacterID);
        //        InGameManager.Instance.characters[i].SetCharacterData(character);
        //        InGameManager.Instance.characters[i].gameObject.GetComponent<UnitMovement>().m_animatorController = InGameManager.Instance.characters[i].gameObject.GetComponentInChildren<AnimatorController>();
        //        InGameManager.Instance.characters[i].gameObject.SetActive(true);
        //    }
        //    else
        //    {
        //        var character = CharacterDataManager.Instance.GetChracterData(i);
        //        InGameManager.Instance.characters[i].SetCharacterModel(i);
        //        InGameManager.Instance.characters[i].SetCharacterData(character);
        //        InGameManager.Instance.characters[i].gameObject.GetComponent<UnitMovement>().m_animatorController = InGameManager.Instance.characters[i].gameObject.GetComponentInChildren<AnimatorController>();
        //        InGameManager.Instance.characters[i].gameObject.SetActive(true);
        //    }
        //}

    }

}
