using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameMainView : MonoBehaviour
{
    [SerializeField] Image HPGauge;
    [SerializeField] Image MPGauge;
    [SerializeField] Text HPText;
    [SerializeField] Text MPText;
    [SerializeField] Image QSkill;
    [SerializeField] Image WSkill;
    [SerializeField] Image ESkill;
    [SerializeField] Image RSkill;

    int updateI = 0;
    private void FixedUpdate()
    {
        SetThisPlayerHPMP();
    }
    private void SetThisPlayerHPMP()
    {
        for (updateI = 0; updateI < InGameManager.Instance.characters.Count; updateI++)
        {
            if (InGameManager.Instance.characters[updateI].PlayerNum == InGameSessionController.Instance.thisPlayerInfo.playerNum)
            {
                HPGauge.fillAmount = InGameManager.Instance.characters[updateI].CurrentHP / InGameManager.Instance.characters[updateI].TotalHP;
                MPGauge.fillAmount = InGameManager.Instance.characters[updateI].CurrentMP / InGameManager.Instance.characters[updateI].TotalMP;
                HPText.text = InGameManager.Instance.characters[updateI].CurrentHP.ToString() + " / " + InGameManager.Instance.characters[updateI].TotalHP.ToString();
                MPText.text = InGameManager.Instance.characters[updateI].CurrentMP.ToString() + " / " + InGameManager.Instance.characters[updateI].TotalMP.ToString();
            }
        }
    }
}
