using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingView : MonoBehaviour
{
    [SerializeField] Image loadingImg;

    private void Start()
    {
        StartLoading();
    }

    IEnumerator Loadingbar()
    {
        float duration = 2f; // 2 seconds
        float elapsedTime = 0f;
        float targetFill = 0.3f; // 30%

        // First phase: Fill to 30% over 2 seconds
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            loadingImg.fillAmount = Mathf.Clamp01((elapsedTime / duration) * targetFill);
            yield return null;
        }

        // Wait until isSyncOK is true
        while (!InGameSessionController.Instance.thisPlayerInfo.IsSyncOK)
        {
            yield return null; // Wait for the next frame
        }

        // Second phase: Fill from 30% to 60% over 1 second
        float syncDuration = 1f;
        float syncElapsedTime = 0f;
        float startFill = 0.3f;
        float endFill = 0.6f;

        while (syncElapsedTime < syncDuration)
        {
            syncElapsedTime += Time.deltaTime;
            loadingImg.fillAmount = Mathf.Lerp(startFill, endFill, syncElapsedTime / syncDuration);
            yield return null;
        }

        // Wait until isPlayerInfoOK is true
        while (!InGameSessionController.Instance.thisPlayerInfo.IsPlayerInfoOK)
        {
            yield return null; // Wait for the next frame
        }

        // Third phase: Fill from 60% to 90% over 1 second
        float playerInfoDuration = 1f;
        float playerInfoElapsedTime = 0f;
        startFill = 0.6f;
        endFill = 0.9f;

        while (playerInfoElapsedTime < playerInfoDuration)
        {
            playerInfoElapsedTime += Time.deltaTime;
            loadingImg.fillAmount = Mathf.Lerp(startFill, endFill, playerInfoElapsedTime / playerInfoDuration);
            yield return null;
        }

        // Wait until isAllPlayerLoadingOK is true
        while (!InGameSessionController.Instance.thisPlayerInfo.IsAllPlayerLoadingOK)
        {
            yield return null; // Wait for the next frame
        }

        // Final phase: Fill from 90% to 100% over 0.5 seconds
        float finalDuration = 0.5f;
        float finalElapsedTime = 0f;
        startFill = 0.9f;
        endFill = 1f;

        while (finalElapsedTime < finalDuration)
        {
            finalElapsedTime += Time.deltaTime;
            loadingImg.fillAmount = Mathf.Lerp(startFill, endFill, finalElapsedTime / finalDuration);
            yield return null;
        }

        // Instantly fill to 100%
        loadingImg.fillAmount = 1f;
        InGameSessionController.Instance.thisPlayerInfo.IsLoadingOK = true;
        // Disable this GameObject
        InGameManager.Instance.GameStart();
        gameObject.SetActive(false);
    }

    // Call this function to start the loading bar
    public void StartLoading()
    {
        StartCoroutine(Loadingbar());
    }
}
