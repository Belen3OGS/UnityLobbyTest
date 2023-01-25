/*--------------------------------------------------------------------------------*
  Copyright Nintendo.  All rights reserved.

  These coded instructions, statements, and computer programs contain proprietary
  information of Nintendo and/or its licensed developers and are protected by
  national and international copyright laws. They may not be disclosed to third
  parties or copied or duplicated in any form, in whole or in part, without the
  prior written consent of Nintendo.

  The content herein is highly confidential and should be handled accordingly.
 *--------------------------------------------------------------------------------*/

// -----------------------------------------------------------------------
// Macros for platform-dependent code.
// These macros are used to enable code to run only on specific platforms or builds.
// -----------------------------------------------------------------------
#if (UNITY_EDITOR || UNITY_STANDALONE)
// Only when running in Unity Editor or standalone builds.
// The following macro is enabled when running in Unity Editor, regardless of the Switch Platform setting.
#define UNITY_EDITOR_OR_STANDALONE
#endif

#if (UNITY_SWITCH && !UNITY_EDITOR_OR_STANDALONE)
// Only on Switch builds.
#define UNITY_ONLY_SWITCH
#endif


public static class NNAccountSetup
{
#if UNITY_ONLY_SWITCH
    private static nn.account.Uid s_Uid;
    public static nn.account.Uid Uid
    {
        get
        {
            return s_Uid;
        }
    }

    private static nn.account.UserHandle s_UserHandle;
    public static nn.account.UserHandle UserHandle
    {
        get
        {
            return s_UserHandle;
        }
    }

    private static nn.account.NetworkServiceAccountId s_NsaId;
    public static nn.account.NetworkServiceAccountId NsaId
    {
        get
        {
            return s_NsaId;
        }
    }

    public static void OpenUserAuto()
    {
        nn.Result result;
        nn.account.Account.Initialize();

        // Get the Uid list.
        nn.account.Uid[] uidList = new nn.account.Uid[nn.account.Account.UserCountMax];
        int userAccountNum = 0;
        result = nn.account.Account.ListQualifiedUsers(ref userAccountNum, uidList, nn.account.Account.UserCountMax);
        if(!result.IsSuccess())
        {
            PiaPluginUtil.UnityLog("ListQualifiedUsers is failed. ErrorCode : " + result.innerValue);
            PiaPlugin.Assert(false);
        }

        // Change the account to an open state.
        for (int i = 0; i < userAccountNum; ++i)
        {
            result = nn.account.Account.OpenUser(ref s_UserHandle, uidList[i]);
            if (!result.IsSuccess())
            {
                PiaPluginUtil.UnityLog("OpenUser is failed. ErrorCode : " + result.innerValue);
                continue;
            }

            // Adopt the first valid Uid that is found.
            s_Uid = uidList[i];
            break;
        }

        // Get NetworkServiceAccountId.
        result = nn.account.NetworkServiceAccount.GetId(ref s_NsaId, s_UserHandle);
        if (!result.IsSuccess())
        {
            PiaPluginUtil.UnityLog("GetId is failed. ErrorCode : " + result.innerValue);
            PiaPlugin.Assert(false);
            return;
        }
    }

    public static void OpenUserWithShowUserSelector()
    {
        nn.Result result;
        nn.account.Account.Initialize();

        // Open the screen for selecting a user.
        result = nn.account.Account.ShowUserSelector(ref s_Uid);
        if (!result.IsSuccess())
        {
            PiaPluginUtil.UnityLog("ShowUserSelector is failed. ErrorCode : " + result.innerValue);
            // In this sample, the error code screen is displayed regardless of which error occurs (when a user has not been selected in B).
            // Note that, with the retail product, the appropriate error processing differs depending on the error.
            nn.err.Error.Show(result);
            return;
        }

        // Change the account to an open state.
        result = nn.account.Account.OpenUser(ref s_UserHandle, s_Uid);
        if (!result.IsSuccess())
        {
            PiaPluginUtil.UnityLog("OpenUser is failed. ErrorCode : " + result.innerValue);
            PiaPlugin.Assert(false);
            return;
        }

        // Get NetworkServiceAccountId.
        nn.account.NetworkServiceAccount.GetId(ref s_NsaId, s_UserHandle);
        if (!result.IsSuccess())
        {
            PiaPluginUtil.UnityLog("GetId is failed. ErrorCode : " + result.innerValue);
            PiaPlugin.Assert(false);
            return;
        }
    }

    public static void CloseUser()
    {
        nn.account.Account.CloseUser(s_UserHandle);
    }
#endif
}
