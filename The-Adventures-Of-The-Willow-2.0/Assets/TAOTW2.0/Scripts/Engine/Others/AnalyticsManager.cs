using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Analytics;

public class AnalyticsManager : MonoBehaviour
{
    void Start()
    {
        GameOpened();
    }
    public void EditorOpened()
    {
        AnalyticsResult analyticsResult = Analytics.CustomEvent("EditorOpened");
        Debug.Log("analyticsResult: " + analyticsResult);
    }

    public void GameOpened()
    {
        AnalyticsResult analyticsResult = Analytics.CustomEvent("GameOpened");
        Debug.Log("analyticsResult: " + analyticsResult);
    }
    public void GameLevelsOpened()
    {
        AnalyticsResult analyticsResult = Analytics.CustomEvent("GameLevelsOpened");
        Debug.Log("analyticsResult: " + analyticsResult);
    }
    public void GameCommunityLevelsOpened()
    {
        AnalyticsResult analyticsResult = Analytics.CustomEvent("GameCommunityLevelsOpened");
        Debug.Log("analyticsResult: " + analyticsResult);
    }
    public void GameExtraLevelsOpened()
    {
        AnalyticsResult analyticsResult = Analytics.CustomEvent("GameExtraLevelsOpened");
        Debug.Log("analyticsResult: " + analyticsResult);
    }

}
