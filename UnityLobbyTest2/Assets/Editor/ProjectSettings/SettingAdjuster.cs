using UnityEditor;

[InitializeOnLoad]
public static class SettingAdjuster
{
    private const string PreviousUnityVersionKey = "UNITY_VERSION";
    private const string PiaUnitySampleApplicationId = "0x0100289000012000";

    // The constructor that is called when the editor starts.
    static SettingAdjuster()
    {
        string currentUnityVersion = UnityEngine.Application.unityVersion;
        string previousUnityVersion = EditorUserSettings.GetConfigValue(PreviousUnityVersionKey);

        // When starting for the first time, or when upgrading or downgrading.
        if (string.IsNullOrEmpty(previousUnityVersion) || currentUnityVersion != previousUnityVersion)
        {
            EditorUserSettings.SetConfigValue(PreviousUnityVersionKey, currentUnityVersion);
            AdjustSetting();
            return;
        }
    }

/*!
    @brief  Changes some of the PlayerSettings values to the required values.
*/
    private static void AdjustSetting()
    {
        PlayerSettings.runInBackground = true;
        PlayerSettings.Switch.applicationID = PiaUnitySampleApplicationId;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Switch, "NN_ACCOUNT_OPENUSER_ENABLE");
    }

}
