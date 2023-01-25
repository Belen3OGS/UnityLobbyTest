/*--------------------------------------------------------------------------------*
  Copyright Nintendo.  All rights reserved.

  These coded instructions, statements, and computer programs contain proprietary
  information of Nintendo and/or its licensed developers and are protected by
  national and international copyright laws. They may not be disclosed to third
  parties or copied or duplicated in any form, in whole or in part, without the
  prior written consent of Nintendo.

  The content herein is highly confidential and should be handled accordingly.
 *--------------------------------------------------------------------------------*/

using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class PlayModeStateChangedExample
{

    static PlayModeStateChangedExample()
    {
        EditorApplication.playModeStateChanged += OnChangedPlayMode;
    }

    //Play back and stop event monitoring.
    private static void OnChangedPlayMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            UnityEngine.Debug.Log("PiaPluginRuntimeChecker : Begin editor play");
        }
        else if (state == PlayModeStateChange.EnteredPlayMode)
        {
            UnityEngine.Debug.Log("PiaPluginRuntimeChecker : Editor play started");
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            UnityEngine.Debug.Log("PiaPluginRuntimeChecker : Begin editor stop");
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            UnityEngine.Debug.Log("PiaPluginRuntimeChecker : Editor play Stopped");
            PiaPlugin.ClosePluginDll();
        }
    }
}
