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

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

// -----------------------------------------------------------------------
//! @brief  Class that is a collection of features used by PiaSync.
// -----------------------------------------------------------------------
public class PiaPluginSync
{
/*!
    @brief  Represents the stage of progress of synchronous communication.
*/
    public enum State
    {
        NotSynchronized = 0,  //!<  Synchronous communication is not taking place.
        Starting,             //!<  Synchronous communication is starting. Although the synchronization data to send must be set, it cannot yet be retrieved.
        Synchronizing,         //!<  Synchronous communication is taking place. The synchronization data to send must be set, and it can be retrieved.
        Ending,               //!<  Synchronous communication is ending.
    };

    public const UInt32 FrameDelayMax = 32;    //!<  Maximum value of input delay, as a number of frames.

/*!
    @brief  Compilation of settings needed to start synchronous communication.
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class SyncStartArgument
    {
/*!
        @brief  Specifies a portion of synchronization data, in the <tt>Setting</tt> structure that sets a positive value size, as the local target of transmission.
*/
        public int usingDataIdBitmap;

/*!
        @brief  Input delay.
        @details  Specify a value in the range from <tt>1</tt> to <tt>FrameDelayMax</tt>.
*/
        public byte delay;

/*!
        @brief  Specifies a portion of synchronization data, in the <tt>Setting</tt> structure that sets a positive value size, as the local target of transmission.
*/
        public byte sendPeriod;
    }

/*!
    @brief  Sets synchronization data to send during synchronous communication.
*/
    [StructLayout(LayoutKind.Sequential)]
    public struct SetDataArgument
    {
        public byte dataId;         //!<  Data ID for the synchronization data to set.
        public UInt32 dataSize;       //!<  Size of the synchronization data to set.
        public IntPtr pData;          //!<  Pointer to the synchronization data to set. This synchronization data is accessed from the native code. Set a pointer to data that will not be subjected to memory compaction.
    }

/*!
    @brief  Sets the information for getting the synchronization data received during synchronous communication.
*/
    [StructLayout(LayoutKind.Sequential)]
    public struct GetDataArgument
    {
        public UInt64 constantId;  //!<  The ID of the station that is sending the synchronization data to be retrieved.
        public byte dataId;     //!<  The data ID of the synchronization data to get.
        public UInt32 dataSize;   //!<  The size of the synchronization data to get.
        public IntPtr pData;      //!<  Pointer to the synchronization data. This synchronization data is accessed from the native code. Set a pointer to data that will not be subjected to memory compaction.
    }


#if UNITY_EDITOR
    static Sync_Step StepNative;
    static Sync_Start StartNative;
    static Sync_End EndNative;
    static Sync_EndAll EndAllNative;
    static Sync_SetData SetDataNative;
    static Sync_GetData GetDataNative;
    static Sync_ReadySetData ReadySetDataNative;
    static Sync_ReadySetData2 ReadySetDataNative2;
    static Sync_ReadyGetData ReadyGetDataNative;
    static Sync_GetSyncState GetSyncStateNative;
    static Sync_CheckEntry CheckEntryNative;
    static Sync_GetFrameNo GetFrameNoNative;
    static Sync_GetDelay GetDelayNative;
    static Sync_GetDelayMax GetDelayMaxNative;
    static Sync_RequestToChangeDelay RequestToChangeDelayNative;
    public static void InitializeHooks(IntPtr? plugin_dll)
    {
        IntPtr pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_Step");
        StepNative = (Sync_Step)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_Step));
        PiaPluginUtil.UnityLog("InitializeHooks " + StepNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_Start");
        StartNative = (Sync_Start)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_Start));
        PiaPluginUtil.UnityLog("InitializeHooks " + StartNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_End");
        EndNative = (Sync_End)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_End));
        PiaPluginUtil.UnityLog("InitializeHooks " + EndNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_EndAll");
        EndAllNative = (Sync_EndAll)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_EndAll));
        PiaPluginUtil.UnityLog("InitializeHooks " + EndAllNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_SetData");
        SetDataNative = (Sync_SetData)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_SetData));
        PiaPluginUtil.UnityLog("InitializeHooks " + SetDataNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_GetData");
        GetDataNative = (Sync_GetData)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_GetData));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetDataNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_ReadySetData");
        ReadySetDataNative = (Sync_ReadySetData)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_ReadySetData));
        PiaPluginUtil.UnityLog("InitializeHooks " + ReadySetDataNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_ReadySetData2");
        ReadySetDataNative2 = (Sync_ReadySetData2)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_ReadySetData2));
        PiaPluginUtil.UnityLog("InitializeHooks " + ReadySetDataNative2);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_ReadyGetData");
        ReadyGetDataNative = (Sync_ReadyGetData)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_ReadyGetData));
        PiaPluginUtil.UnityLog("InitializeHooks " + ReadyGetDataNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_GetSyncState");
        GetSyncStateNative = (Sync_GetSyncState)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_GetSyncState));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetSyncStateNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_CheckEntry");
        CheckEntryNative = (Sync_CheckEntry)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_CheckEntry));
        PiaPluginUtil.UnityLog("InitializeHooks " + CheckEntryNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_GetFrameNo");
        GetFrameNoNative = (Sync_GetFrameNo)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_GetFrameNo));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetFrameNoNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_GetDelay");
        GetDelayNative = (Sync_GetDelay)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_GetDelay));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetDelayNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_GetDelayMax");
        GetDelayMaxNative = (Sync_GetDelayMax)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_GetDelayMax));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetDelayMaxNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Sync_RequestToChangeDelay");
        RequestToChangeDelayNative = (Sync_RequestToChangeDelay)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Sync_RequestToChangeDelay));
        PiaPluginUtil.UnityLog("InitializeHooks " + RequestToChangeDelayNative);
    }
#endif

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result Sync_Step();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Sync_Step")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_Step")]
#endif
    private static extern PiaPlugin.Result StepNative();
#endif

/*!
    @brief  Advances synchronization by one frame.
    @details  While not <tt>State.NotSynchronized</tt>, call this function for each frame. Call this function at the beginning of each frame.
                This function can also be called when the state is <tt>State.NotSynchronized</tt>.
                When this function is called, the values that can be obtained with <tt>PiaPluginSync.GetSyncState()</tt> are updated.
    @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
    @retval ResultInvalidState  Stations are not communicating. [:progErr]
    @retval ResultDataIsNotArrivedYet  The frame could not be advanced because the synchronization data has not arrived from other stations.
    @retval ResultDataIsNotSet  The local station has not yet set the synchronization data to be sent. [:progErr]
    @retval ResultTemporaryUnavailable  An individual synchronization finalization process is in progress.
*/
    public static PiaPlugin.Result Step()
    {
        PiaPlugin.Result result = StepNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result Sync_Start([In, MarshalAs(UnmanagedType.LPStruct)] SyncStartArgument syncStartArg);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Sync_Start")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_Start")]
#endif
    private static extern PiaPlugin.Result StartNative([In, MarshalAs(UnmanagedType.LPStruct)] SyncStartArgument syncStartArg);
#endif

/*!
    @brief  Starts synchronization.
    @param[in] syncStartArg  Settings needed to start synchronous communication.
    @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
    @retval ResultInvalidState  This function cannot be called in the current <tt>SyncProtocol::State</tt>. Or, it indicates that stations are not communicating. [:progErr]
    @retval ResultInvalidArgument  An invalid argument was specified. [:progErr]
*/
    public static PiaPlugin.Result Start(SyncStartArgument syncStartArg)
    {
        PiaPlugin.Result result = StartNative(syncStartArg);
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result Sync_End();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Sync_End")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_End")]
#endif
    private static extern PiaPlugin.Result EndNative();
#endif

/*!
    @brief  Ends synchronous communication.
    @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
*/
    public static PiaPlugin.Result End()
    {
        PiaPlugin.Result result = EndNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result Sync_EndAll();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Sync_EndAll")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_EndAll")]
#endif
    private static extern PiaPlugin.Result EndAllNative();
#endif

/*!
    @brief  Ends synchronous communication for all stations.
    @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
*/
    public static PiaPlugin.Result EndAll()
    {
        PiaPlugin.Result result = EndAllNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result Sync_SetData([In, MarshalAs(UnmanagedType.LPStruct)] SetDataArgument arg);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Sync_SetData")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_SetData")]
#endif
    private static extern PiaPlugin.Result SetDataNative([In, MarshalAs(UnmanagedType.LPStruct)] SetDataArgument arg);
#endif

/*!
    @brief  Sets synchronization data to send during synchronous communication.
    @details  The data set to <tt>SetDataArgument</tt> is accessed from the native code. Set a pointer to data that will not be subjected to memory compaction.
    @param[in] arg  Synchronization data to be sent.
    @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>. You must make sure that the implementation of this function in your application does not return any errors.
    @retval ResultInvalidState  This function cannot be called in the current <tt>SyncProtocol::State</tt>. Or, it indicates that stations are not communicating. [:progErr]
    @retval ResultInvalidArgument  An invalid argument was specified. [:progErr]
    @retval ResultAlreadyExists  Synchronization data has already been set. [:progErr]
    @retval ResultNoData  There is no need to set data, for example because the state of synchronous communication has not progressed to <tt>State.Synchronizing</tt> or <tt>State.Starting</tt>.
*/
    public static PiaPlugin.Result SetData(SetDataArgument arg)
    {
        PiaPlugin.Result result = SetDataNative(arg);
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result Sync_GetData([In, MarshalAs(UnmanagedType.LPStruct)] GetDataArgument arg);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Sync_GetData")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_GetData")]
#endif
    [return: MarshalAs(UnmanagedType.U1)]
    private static extern PiaPlugin.Result GetDataNative([In, MarshalAs(UnmanagedType.LPStruct)] GetDataArgument arg);
#endif

/*!
    @brief  Gets the synchronization data received during synchronous communication.
    @details  Synchronization data for data IDs that have bits set can be obtained using <tt>SyncStartArg.usingDataIdBitmap</tt>. The function succeeds when the state of synchronous communication has progressed to <tt>State.Synchronizing</tt>.
                The data set to <tt>GetDataArgument</tt> is accessed from the native code. Set a pointer to data that will not be subjected to memory compaction.
    @param[in] arg  Synchronization data received during synchronous communication.
    @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>. You must make sure that the implementation of this function in your application does not return any errors.
    @retval ResultInvalidState  This <tt>PiaPluginSync.State</tt> cannot be called with this function. Alternatively, it could indicate that synchronization data cannot be retrieved in this frame. Or, it indicates that stations are not communicating. [:progErr]
    @retval ResultInvalidArgument  An invalid argument was specified. [:progErr]
    @retval ResultNoData  This state cannot get synchronization data. The state of synchronous communication is not <tt>State.Synchronizing</tt>, or the data ID is not valid.
*/
    public static PiaPlugin.Result GetData(GetDataArgument arg)
    {
        PiaPlugin.Result result = GetDataNative(arg);
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result Sync_ReadySetData();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Sync_ReadySetData")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_ReadySetData")]
#endif
    private static extern PiaPlugin.Result ReadySetDataNative();
#endif

/*!
    @brief  Checks whether it is possible in this state to set the synchronization data to send.
    @return  If in a state where the synchronization data to be sent can be configured, returns a result value for which <tt>IsSuccess()</tt> returns <tt>true</tt>.
    @retval ResultNotInCommunication  Not communicating, or the progress of synchronous communication is a state other than @ref State.Starting, or @ref State.Synchronizing.
                                        This result is also returned if there are stations that have not yet started synchronous communication. [:handling]
    @retval ResultTemporaryUnavailable  Synchronization data is already configured, or the frame does not require this configuration. [:handling]
*/
    public static PiaPlugin.Result ReadySetData()
    {
        return ReadySetDataNative();
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result Sync_ReadySetData2(byte dataId);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Sync_ReadySetData2")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_ReadySetData2")]
#endif
    private static extern PiaPlugin.Result ReadySetDataNative2(byte dataId);
#endif

/*!
    @brief  Checks whether it is possible in this state to set the synchronization data to send.
    @param[in] dataId  Data ID for the synchronization data being set.
    @return  If in a state where the synchronization data to be sent can be configured, returns a result value for which <tt>IsSuccess()</tt> returns <tt>true</tt>.
    @retval ResultNotInCommunication  Not communicating, or the progress of synchronous communication is a state other than @ref State.Starting, or @ref State.Synchronizing.
                                        This result is also returned if there are stations that have not yet started synchronous communication. [:handling]
    @retval ResultTemporaryUnavailable  Synchronization data is already configured, or the frame does not require this configuration. [:handling]
    @retval ResultInvalidArgument  An invalid argument was specified. [:progErr]
*/
    public static PiaPlugin.Result ReadySetData(byte dataId)
    {
        return ReadySetDataNative2(dataId);
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result Sync_ReadyGetData(UInt64 constantId);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Sync_ReadyGetData")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_ReadyGetData")]
#endif
    private static extern PiaPlugin.Result ReadyGetDataNative(UInt64 constantId);
#endif

/*!
    @brief  Checks whether it is possible in this state to get the received synchronization data.
    @param[in] constantId  The ID of the station to receive from.
    @return  If the received synchronization data can be obtained, returns a result value for which <tt>IsSuccess()</tt> returns <tt>true</tt>.
    @retval ResultNotInCommunication  Not communicating, or the progress of synchronous communication is a state other than @ref State.Synchronizing.
                                        It might also be the case that @ref CheckEntry is <tt>false</tt> for some station. [:handling]
    @retval ResultTemporaryUnavailable  This frame cannot get synchronization data. [:handling]
*/
    public static PiaPlugin.Result ReadyGetData(UInt64 constantId)
    {
        return ReadyGetDataNative(constantId);
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate State Sync_GetSyncState();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Sync_GetSyncState")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_GetSyncState")]
#endif
    private static extern State GetSyncStateNative();
#endif

/*!
    @brief  Gets the current state of synchronization.
    @return  Returns the current state of synchronization.
*/
    public static State GetSyncState()
    {
        return GetSyncStateNative();
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool Sync_CheckEntry(UInt64 constantId);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "Sync_CheckEntry")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_CheckEntry")]
#endif
        private static extern bool CheckEntryNative(UInt64 constantId);
#endif

/*!
    @brief  Determines whether the specified station has joined synchronization.
    @param[in] constantId  The <tt>ConstantId</tt> of the station you want to check.
    @return  Returns <tt>true</tt> if the specified station has joined synchronization.
*/
    public static bool CheckEntry(UInt64 constantId)
    {
        return CheckEntryNative(constantId);
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt32 Sync_GetFrameNo();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Sync_GetFrameNo")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_GetFrameNo")]
#endif
    private static extern UInt32 GetFrameNoNative();
#endif

/*!
    @brief  Gets the number of frames since entering the <tt>State.Synchronizing</tt> state.
    @details  The frame count advances by 1 for every successful call to <tt>SyncProtocolStep</tt> while the state is <tt>State.Synchronizing</tt>.
    @return  Returns the frame count since entering the <tt>State.Synchronizing</tt> state.
                Note that this function returns <tt>0</tt> if the state is not <tt>State.Synchronizing</tt>, but it also returns <tt>0</tt> for the first frame in the <tt>State.Synchronizing</tt> state.
*/
    public static UInt32 GetFrameNo()
    {
        return GetFrameNoNative();
    }


#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate byte Sync_GetDelay();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Sync_GetDelay")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_GetDelay")]
#endif
    private static extern byte GetDelayNative();
#endif

/*!
    @brief  Gets the value actually set for the input delay.
    @return  Returns the number of frames to delay input.
*/
    public static byte GetDelay()
    {
        return GetDelayNative();
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate byte Sync_GetDelayMax();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Sync_GetDelayMax")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_GetDelayMax")]
#endif
    private static extern byte GetDelayMaxNative();
#endif

/*!
    @brief  Gets the maximum value for the input delay.
    @return  Returns the maximum number of frames to delay input.
*/
    public static byte GetDelayMax()
    {
        return GetDelayMaxNative();
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result Sync_RequestToChangeDelay(byte newDelay);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Sync_RequestToChangeDelay")]
#else
    [DllImport("__Internal", EntryPoint = "Sync_RequestToChangeDelay")]
#endif
    private static extern PiaPlugin.Result RequestToChangeDelayNative(byte newDelay);
#endif

/*!
    @brief  Sets an input delay change request as synchronization data.
    @details  The requested input delay must be no larger than @ref FrameDelayMax at the time of initialization.
                                  If you request an input delay of zero, the library assumes that you have not requested a change.
                                  Even if this function succeeds, the input delay might not change to the requested value.
                                  If more than one request is received in the same frame, the largest requested input delay value gets precedence.
                                  If the input delay is already being changed in response to a request when another input delay change request arrives as synchronization data, the newly arrived request is ignored.
    @param[in] newDelay  Requested input delay value.
    @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
    @retval ResultInvalidState  The function was called in an invalid state. Or, it indicates that stations are not communicating. [:progErr]
    @retval ResultInvalidArgument  An invalid argument was specified. [:progErr]
    @retval ResultAlreadyExists  Already set. [:progErr]
*/
    public static PiaPlugin.Result RequestToChangeDelay(byte newDelay)
    {
        return RequestToChangeDelayNative(newDelay);
    }
}
