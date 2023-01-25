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


using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;

// -----------------------------------------------------------------------
//! @brief  Class that brings together valuable functionality.
// -----------------------------------------------------------------------
public class PiaPluginUtil : MonoBehaviour
{
#if UNITY_EDITOR
    static Pia_GetNativeVersionInfoNative GetNativeVersionInfoNative;

    public static void InitializeHooks(IntPtr? plugin_dll)
    {
        IntPtr pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Pia_SetDebugLogFunc");
        PiaLog.Pia_SetDebugLogFuncNative = (PiaLog.Pia_SetDebugLogFunc)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(PiaLog.Pia_SetDebugLogFunc));
        PiaPluginUtil.UnityLog("InitializeHooks " + PiaLog.Pia_SetDebugLogFuncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Pia_SetAssertFunc");
        PiaAssert.Pia_SetAssertFuncNative = (PiaAssert.Pia_SetAssertFunc)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(PiaAssert.Pia_SetAssertFunc));
        PiaPluginUtil.UnityLog("InitializeHooks " + PiaAssert.Pia_SetAssertFuncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Pia_GetNativeVersionInfo");
        GetNativeVersionInfoNative = (Pia_GetNativeVersionInfoNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Pia_GetNativeVersionInfoNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetNativeVersionInfoNative);
    }
#endif

/*!
    @cond  PRIVATE
*/
    [StructLayout(LayoutKind.Sequential)]
    internal class VersionInfoNative : IDisposable
    {
        public IntPtr date { get; private set; }
        public UInt16 dateLength { get; private set; }
        public IntPtr rootName { get; private set; }
        public UInt16 rootNameLength { get; private set; }
        public UInt32 versionMajor { get; private set; }
        public UInt32 versionMinor { get; private set; }
        public UInt32 versionMicro { get; private set; }
        public UInt32 revision { get; private set; }

        internal VersionInfoNative()
        {
            Reset();
        }
        internal VersionInfoNative(VersionInfo versionInfo)
        {
            Reset();
            int bufferSize = 0;
            date = UnmanagedMemoryManager.WriteUtf8(versionInfo.date, ref bufferSize);
            rootName = UnmanagedMemoryManager.WriteUtf8(versionInfo.rootName, ref bufferSize);
        }

        public void Reset()
        {
            date = IntPtr.Zero;
            rootName = IntPtr.Zero;
            versionMajor = 0;
            versionMinor = 0;
            versionMicro = 0;
            revision = 0;
        }

        public void Dispose() { }
    }
    //! @endcond

/*!
     @brief  Class used for getting Pia and Pia plug-in version information.
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class VersionInfo
    {
        public string date = "";                    //!<  The date.
        public string rootName = "";                //!<  The root name.
        public UInt32 versionMajor;                 //!<  The value of the major version.
        public UInt32 versionMinor;                 //!<  The value of the minor version.
        public UInt32 versionMicro;                 //!<  The value of the micro version.
        public UInt32 revision;                     //!<  The value of the revision.

        public VersionInfo()
        {
            date = "";
            rootName = "";
            versionMajor = 0;
            versionMinor = 0;
            versionMicro = 0;
            revision = 0;
        }
        internal VersionInfo(VersionInfoNative versionInfoNative)
        {
            date = UnmanagedMemoryManager.ReadUtf8(versionInfoNative.date, versionInfoNative.dateLength);
            rootName = UnmanagedMemoryManager.ReadUtf8(versionInfoNative.rootName, versionInfoNative.rootNameLength);
            versionMajor = versionInfoNative.versionMajor;
            versionMinor = versionInfoNative.versionMinor;
            versionMicro = versionInfoNative.versionMicro;
            revision = versionInfoNative.revision;
        }
    }

/*!
    @brief  Outputs the log.
    @param[in] msg  String exported as a log.
*/
#if !UNITY_EDITOR
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
#endif
    public static void UnityLog(string msg)
    {
#if UNITY_EDITOR
        Debug.Log(msg);
#elif UNITY_ONLY_SWITCH
        UnityEngine.Switch.Utility.PrintLine(msg);
#else
        Debug.Log(msg);
#endif
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate PiaPlugin.Result Pia_GetNativeVersionInfoNative([Out, MarshalAs(UnmanagedType.LPStruct)] VersionInfoNative piaVersionInfoNative,
                                                                        [Out, MarshalAs(UnmanagedType.LPStruct)] VersionInfoNative piaPluginNativeVersionInfoNative);
#else
#if UNITY_ANDROID || UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_GetNativeVersionInfo")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_GetNativeVersionInfo")]
#endif
    private static extern PiaPlugin.Result GetNativeVersionInfoNative([Out, MarshalAs(UnmanagedType.LPStruct)] VersionInfoNative piaVersionInfoNative,
                                                                        [Out, MarshalAs(UnmanagedType.LPStruct)] VersionInfoNative piaPluginNativeVersionInfoNative);
#endif

/*!
    @brief  Gets information about the version of Pia used in the build of the package and the version of the Pia plug-in itself.
    @param[out] piaVersionInfo  The version of Pia used in the build of the PiaUnity package.
    @param[out] piaPluginNativeVersionInfo  The version of the Pia plug-in.
*/
    public static PiaPlugin.Result GetNativeVersionInfo(ref VersionInfo piaVersionInfo, ref VersionInfo piaPluginNativeVersionInfo)
    {
        using (VersionInfoNative piaVersionInfoNative = new VersionInfoNative(piaVersionInfo))
        using (VersionInfoNative piaPluginNativeVersionInfoNative = new VersionInfoNative(piaPluginNativeVersionInfo))
        {
            PiaPlugin.Result result = GetNativeVersionInfoNative(piaVersionInfoNative, piaPluginNativeVersionInfoNative);
            if (result.IsSuccess())
            {
                piaVersionInfo = new VersionInfo(piaVersionInfoNative);
                piaPluginNativeVersionInfo = new VersionInfo(piaPluginNativeVersionInfoNative);
            }
            return result;
        }
    }


#if UNITY_EDITOR_OR_STANDALONE
    static PiaLog s_PiaLog = null;
    static bool s_RegistFlag = false;
    static bool s_RegistTrigger = false;

//    @cond  PRIVATE
    //Adds to the Pia output log.
    public class PiaLog
    {
#if UNITY_EDITOR
        public static Pia_SetDebugLogFunc Pia_SetDebugLogFuncNative;
#endif

        public delegate void DebugLogDelegate(string str);
#if UNITY_EDITOR
        // Output the preview in the editor to the log and to the editor console.
        DebugLogDelegate debugLogFunc = msg => Debug.Log(msg);
#elif UNITY_STANDALONE
        // Add the standalone execution to the log without a stack trace.
        DebugLogDelegate debugLogFunc = msg => Console.Out.WriteLine(msg);
#endif

        ~PiaLog()
        {
//            PiaPluginUtil.UnityLog("~PiaLog called.");
//            s_PiaLog.Unregist();
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Pia_SetDebugLogFunc(DebugLogDelegate func);
#elif UNITY_STANDALONE
        [DllImport("nn_piaPlugin", EntryPoint = "Pia_SetDebugLogFunc")]
        public static extern void Pia_SetDebugLogFuncNative(DebugLogDelegate func);
#endif
        // Add the C# log print function called by C++.
        public void Register()
        {
#if UNITY_EDITOR_OR_STANDALONE
            Pia_SetDebugLogFuncNative(debugLogFunc);
#endif
        }
        // Clear the C# log print function called by C++.
        public void Unregister()
        {
#if UNITY_EDITOR
            if (PiaPlugin.IsHookPluginDll())
            {
                debugLogFunc = null;
                Pia_SetDebugLogFuncNative(null);
            }
#endif
        }
    }



/*!
    @brief  Updates the Pia log export settings in the Window version or in UnityEditor.
*/
    public static void UpdatePiaLog()
    {
        if (s_RegistTrigger)
        {
            if (s_RegistFlag)
            {
                PiaPluginUtil.UnityLog("UpdatePiaLog : EnablePiaLog");
                s_PiaLog.Register();
            }
            else
            {
                PiaPluginUtil.UnityLog("UpdatePiaLog : DisablePiaLogNative");
                s_PiaLog.Unregister();
            }
            s_RegistTrigger = false;
        }
    }
    //! @endcond

/*!
    @brief  Enables Pia output logs in the Windows version or in UnityEditor.
    @details  These functions must be enabled every time, when enabling in UnityEditor, before calling <tt>PiaPlugin.InitializeFramework()</tt>.
*/
    public static void EnablePiaLog()
    {
        PiaPluginUtil.UnityLog("EnablePiaLog");
        if (s_PiaLog == null)
        {
            s_PiaLog = new PiaLog();
        }
        s_RegistFlag = true;
        s_RegistTrigger = true;
    }

/*!
    @brief  Disables Pia output logs in the Windows version or in UnityEditor.
*/
    public static void DisablePiaLog()
    {
        PiaPluginUtil.UnityLog("DisablePiaLog");
#if UNITY_EDITOR
        if (s_PiaLog != null)
        {
            s_RegistFlag = false;
            s_RegistTrigger = true;
            s_PiaLog.Unregister();
        }
#endif
    }

//    @cond  PRIVATE

    public class PiaAssert
    {
#if UNITY_EDITOR
        public static Pia_SetAssertFunc Pia_SetAssertFuncNative;
#endif

        public delegate void AssertDelegate();
        AssertDelegate assertDelegate = () => UintyAssert(); //Debug.Assert(false);

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Pia_SetAssertFunc(AssertDelegate func);
#elif UNITY_STANDALONE
        [DllImport("nn_piaPlugin", EntryPoint = "Pia_SetAssertFunc")]
        public static extern void Pia_SetAssertFuncNative(AssertDelegate func);
#endif
        public void Enable()
        {
            PiaPluginUtil.UnityLog("Regist Unity Assert Funciton for PIA_ASSERT");
            Pia_SetAssertFuncNative(assertDelegate);
        }

        public static void UintyAssert()
        {
            Debug.Assert(false);
        }

    }
    //! @endcond
#endif // UNITY_EDITOR_OR_STANDALONE
}
