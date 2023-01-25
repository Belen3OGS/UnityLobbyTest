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


//#undef UNITY_EDITOR_OR_STANDALONE
//#define UNITY_ONLY_SWITCH

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

// -----------------------------------------------------------------------
//! @brief  Class that combines the PiaClone modules.
// -----------------------------------------------------------------------
public class PiaPluginClone
{

#if UNITY_EDITOR
    public static void InitializeHooks(IntPtr? plugin_dll)
    {
        BroadcastEvent.InitializeHooks(plugin_dll);
        Atomic.InitializeHooks(plugin_dll);
        Clock.InitializeHooks(plugin_dll);
    }
#endif

    // -----------------------------------------------------------------------
    //! @brief  Class that is a compilation of the features that use <tt>BroadcastEventProtocol</tt>.
    // -----------------------------------------------------------------------
    public class BroadcastEvent
    {
/*!
          @brief  Represents the synchronization state.
*/
        public enum State : byte
        {
            NotSynchronized = 0,  //!<  The state where synchronous communication is not taking place.
            Synchronizing,        //!<  The state where synchronous communication is taking place. The state transitions to this state when notification to all stations participating in the session has completed. In this state, you can call the @ref BroadcastEvent.Send() function.
        }
#if UNITY_EDITOR
        static CloneBroadcastEvent_GetPayloadSizeMax GetPayloadSizeMaxNative;
        static CloneBroadcastEvent_GetEventStateStation GetEventStateStationNative;
        static CloneBroadcastEvent_IsInCommunication IsInCommunicationNative;
        static CloneBroadcastEvent_IsSynchronizing IsSynchronizingNative;
        static CloneBroadcastEvent_Receive ReceiveNative;
        static CloneBroadcastEvent_Send SendNative;
        static CloneBroadcastEvent_ReadySend ReadySendNative;
        static CloneBroadcastEvent_ReadyReceive ReadyReceiveNative;
        static CloneBroadcastEvent_SetThroughputLimit SetThroughputLimitNative;

        public static void InitializeHooks(IntPtr? plugin_dll)
        {
            IntPtr pAddressOfFunctionToCall;

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneBroadcastEvent_GetPayloadSizeMax");
            GetPayloadSizeMaxNative = (CloneBroadcastEvent_GetPayloadSizeMax)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneBroadcastEvent_GetPayloadSizeMax));
            PiaPluginUtil.UnityLog("InitializeHooks " + GetPayloadSizeMaxNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneBroadcastEvent_GetEventStateStation");
            GetEventStateStationNative = (CloneBroadcastEvent_GetEventStateStation)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneBroadcastEvent_GetEventStateStation));
            PiaPluginUtil.UnityLog("InitializeHooks " + GetEventStateStationNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneBroadcastEvent_IsInCommunication");
            IsInCommunicationNative = (CloneBroadcastEvent_IsInCommunication)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneBroadcastEvent_IsInCommunication));
            PiaPluginUtil.UnityLog("InitializeHooks " + IsInCommunicationNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneBroadcastEvent_IsSynchronizing");
            IsSynchronizingNative = (CloneBroadcastEvent_IsSynchronizing)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneBroadcastEvent_IsSynchronizing));
            PiaPluginUtil.UnityLog("InitializeHooks " + IsSynchronizingNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneBroadcastEvent_Receive");
            ReceiveNative = (CloneBroadcastEvent_Receive)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneBroadcastEvent_Receive));
            PiaPluginUtil.UnityLog("InitializeHooks " + ReceiveNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneBroadcastEvent_Send");
            SendNative = (CloneBroadcastEvent_Send)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneBroadcastEvent_Send));
            PiaPluginUtil.UnityLog("InitializeHooks " + SendNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneBroadcastEvent_ReadyReceive");
            ReadyReceiveNative = (CloneBroadcastEvent_ReadyReceive)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneBroadcastEvent_ReadyReceive));
            PiaPluginUtil.UnityLog("InitializeHooks " + ReadyReceiveNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneBroadcastEvent_ReadySend");
            ReadySendNative = (CloneBroadcastEvent_ReadySend)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneBroadcastEvent_ReadySend));
            PiaPluginUtil.UnityLog("InitializeHooks " + ReadySendNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneBroadcastEvent_SetThroughputLimit");
            SetThroughputLimitNative = (CloneBroadcastEvent_SetThroughputLimit)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneBroadcastEvent_SetThroughputLimit));
            PiaPluginUtil.UnityLog("InitializeHooks " + SetThroughputLimitNative);
        }
#endif

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt32 CloneBroadcastEvent_GetPayloadSizeMax(UInt16 port);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneBroadcastEvent_GetPayloadSizeMax")]
#else
    [DllImport("__Internal", EntryPoint = "CloneBroadcastEvent_GetPayloadSizeMax")]
#endif
        private static extern UInt32 GetPayloadSizeMaxNative(UInt16 port);
#endif

/*!
        @brief  Gets the maximum size that can be specified with Send().
        @param[in] port  The port number specifying the ID of the created <tt>BroadcastEventProtocol</tt>. Specify a value in the range from <tt>0</tt> to (number created - 1).
        @return  Returns the maximum size that can be specified with <tt>Send()</tt>. Returns <tt>0</tt> when in a state where this function cannot be called.
*/
        public static UInt32 GetPayloadSizeMax(UInt16 port)
        {
            return GetPayloadSizeMaxNative(port);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate State CloneBroadcastEvent_GetEventStateStation(UInt16 port, UInt64 constantId);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneBroadcastEvent_GetEventStateStation")]
#else
    [DllImport("__Internal", EntryPoint = "CloneBroadcastEvent_GetEventStateStation")]
#endif
        private static extern State GetEventStateStationNative(UInt16 port, UInt64 constantId);
#endif

/*!
       @brief  Gets the current state of synchronization.
       @param[in] port  The port number specifying the ID of the created <tt>BroadcastEventProtocol</tt>. Specify a value in the range from <tt>0</tt> to (number created - 1).
       @param[in] constantId  The <tt>ConstantId</tt> of the communication peer.
       @return  Returns the current state of synchronization. Returns <tt>State.NotSynchronized</tt> when this function is not in a callable state.
*/
        public static State GetEventState(UInt16 port, UInt64 constantId)
        {
            return GetEventStateStationNative(port, constantId);
        }


#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool CloneBroadcastEvent_IsInCommunication(UInt16 port, UInt64 constantId);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneBroadcastEvent_IsInCommunication")]
#else
    [DllImport("__Internal", EntryPoint = "CloneBroadcastEvent_IsInCommunication")]
#endif
        private static extern bool IsInCommunicationNative(UInt16 port, UInt64 constantId);
#endif

/*!
        @brief  Determines whether the specified node is communicating.
        @param[in] port  The port number specifying the ID of the created <tt>BroadcastEventProtocol</tt>. Specify a value in the range from <tt>0</tt> to (number created - 1).
        @param[in] constantId  The <tt>ConstantId</tt> of the communication peer.
        @return  Returns <tt>true</tt> if the node is communicating.
*/
        public static bool IsInCommunication(UInt16 port, UInt64 constantId)
        {
            return IsInCommunicationNative(port, constantId);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool CloneBroadcastEvent_IsSynchronizing(UInt16 port);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneBroadcastEvent_IsSynchronizing")]
#else
    [DllImport("__Internal", EntryPoint = "CloneBroadcastEvent_IsSynchronizing")]
#endif
        private static extern bool IsSynchronizingNative(UInt16 port);
#endif

/*!
        @brief  Determines whether the synchronization process is underway when leaving the session.
        @param[in] port  The port number specifying the ID of the created <tt>BroadcastEventProtocol</tt>. Specify a value in the range from <tt>0</tt> to (number created - 1).
        @return  Returns <tt>true</tt> if in the process of synchronizing.
*/
        public static bool IsSynchronizing(UInt16 port)
        {
            return IsSynchronizingNative(port);
        }


#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result CloneBroadcastEvent_Receive(UInt16 port, [Out] out UInt64 srcConstantId, [Out] out UInt32 srcVariableId, IntPtr pRecvBuf, [Out] out UInt32 recvDataSize, UInt32 recvBufSize);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneBroadcastEvent_Receive")]
#else
    [DllImport("__Internal", EntryPoint = "CloneBroadcastEvent_Receive")]
#endif
        private static extern PiaPlugin.Result ReceiveNative(UInt16 port, [Out] out UInt64 srcConstantId, [Out] out UInt32 srcVariableId, IntPtr pRecvBuf, [Out] out UInt32 recvDataSize, UInt32 recvBufSize);
#endif

/*!
        @brief  Use <tt>BroadcastEventProtocol</tt> to load the received data into the buffer.
        @param[in] port  The port number specifying the ID of the created <tt>BroadcastEventProtocol</tt>. Specify a value in the range from <tt>0</tt> to (number created - 1).
        @param[out] srcConstantId  The station ID of the station that sent the received data.
        @param[out] srcVariableId  The buffer for writing the <tt>VariableId</tt> of the sender of the received data.
        @param[out] recvBuf  Buffer that stores the received data.
        @param[out] recvDataSize  The size of the received data. The value is in bytes.
        @return  If successful, returns a result indicating success.
        @retval ResultInvalidArgument  An argument is not valid. [:progErr]
        @retval ResultInvalidState  This function is not in a callable state. Returned in cases such as when PiaUnity is not initialized or when not communicating. [:progErr]
        @retval ResultNoData  No data was received. [:handling]
        @retval ResultBufferShortage  The received data is too large to fit into the receive buffer. This result must be handled appropriately because it can occur when invalid data is sent,
                                        such as when the sender is cheating. [:progErr]
*/
        public static PiaPlugin.Result Receive(UInt16 port, out UInt64 srcConstantId, out UInt32 srcVariableId, byte[] recvBuf, out UInt32 recvDataSize)
        {
            int bufferSize = recvBuf.Length;
            GCHandle handle = GCHandle.Alloc(recvBuf, GCHandleType.Pinned);
            PiaPlugin.Result result = ReceiveNative(port, out srcConstantId, out srcVariableId, handle.AddrOfPinnedObject(), out recvDataSize, (UInt32)bufferSize);
            handle.Free();
            return result;
        }


#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result CloneBroadcastEvent_Send(UInt16 port, IntPtr pData, UInt32 dataSize);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneBroadcastEvent_Send")]
#else
    [DllImport("__Internal", EntryPoint = "CloneBroadcastEvent_Send")]
#endif
        private static extern PiaPlugin.Result SendNative(UInt16 port, IntPtr pData, UInt32 dataSize);
#endif

/*!
        @brief  Use <tt>BroadcastEventProtocol</tt> to send data to all stations.
        @param[in] port  The port number specifying the ID of the created <tt>BroadcastEventProtocol</tt>. Specify a value in the range from <tt>0</tt> to (number created - 1).
        @param[in] data  Byte array data to send.
        @param[in] dataSize  Size of the data to send. The value is in bytes.
        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidArgument  An argument is not valid. [:progErr]
        @retval ResultInvalidState  It might also be that @ref BroadcastEvent.State is a state other than <tt>Synchronizing</tt>, or that <tt>BroadcastEventProtocol</tt> is not initialized, or that the setup of the session has not completed. [:progErr]
        @retval ResultBufferIsFull  There is not enough space in the send buffer. If this result occurs frequently, consider increasing the data count in the send buffer specified by the @ref PiaPlugin.InitializeCloneSetting class. [:handling]
        @retval ResultTemporaryUnavailable  <tt>BroadcastEventProtocol</tt> is processing a new joining station, so data temporarily cannot be sent. [:handling]

*/
        public static PiaPlugin.Result Send(UInt16 port, byte[] data, UInt32 dataSize)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            PiaPlugin.Result result = SendNative(port, handle.AddrOfPinnedObject(), dataSize);
            handle.Free();

            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result CloneBroadcastEvent_ReadySend(UInt16 port, UInt32 dataSize);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneBroadcastEvent_ReadySend")]
#else
    [DllImport("__Internal", EntryPoint = "CloneBroadcastEvent_ReadySend")]
#endif
        private static extern PiaPlugin.Result ReadySendNative(UInt16 port, UInt32 dataSize);
#endif

/*!
        @brief  Determines whether this state is a ready state for sending.
        @param[in] port  The port number specifying the ID of the created <tt>BroadcastEventProtocol</tt>. Specify a value in the range from <tt>0</tt> to (number created - 1).
        @param[in] dataSize  The amount of data planned to be sent.
        @return  If sending is possible in this state, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidState  The setup for <tt>session</tt> might not have been completed. [:progErr]
        @retval ResultInvalidArgument  An argument is not valid. [:progErr]
        @retval ResultBufferIsFull  There is not enough space in the send buffer. If this happens frequently, consider increasing the send buffer size specified in the <tt>Initialize()</tt> function. [:handling]
        @retval ResultNotInCommunication  Not @ref BroadcastEvent.State.Synchronizing. Or, it indicates that stations are not communicating. [:handling]
        @retval ResultTemporaryUnavailable  <tt>BroadcastEventProtocol</tt> is processing a new joining station, so data temporarily cannot be sent.
*/
        public static PiaPlugin.Result ReadySend(UInt16 port, UInt32 dataSize)
        {
            PiaPlugin.Result result = ReadySendNative(port, dataSize);
            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result CloneBroadcastEvent_ReadyReceive(UInt16 port, UInt32 dataSize);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneBroadcastEvent_ReadyReceive")]
#else
    [DllImport("__Internal", EntryPoint = "CloneBroadcastEvent_ReadyReceive")]
#endif
        private static extern PiaPlugin.Result ReadyReceiveNative(UInt16 port, UInt32 dataSize);
#endif

/*!
        @brief  Determines whether this state is a ready state for receiving.
        @param[in] port  The port number specifying the ID of the created <tt>BroadcastEventProtocol</tt>. Specify a value in the range from <tt>0</tt> to (number created - 1).
        @param[in] dataSize  The size, in bytes, of the buffer for writing the received data.
        @return  If receiving is possible in this state, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultNoData  No data was received. [:handling]
        @retval ResultBufferShortage  The received data is too large to fit into the receive buffer.
                                         This result must be handled appropriately because it can occur when invalid data is sent,
                                         such as when the sender is cheating. [:progErr]
*/
        public static PiaPlugin.Result ReadyReceive(UInt16 port, UInt32 dataSize)
        {
            PiaPlugin.Result result = ReadyReceiveNative(port, dataSize);
            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CloneBroadcastEvent_SetThroughputLimit(UInt16 port, UInt32 throughputLimit);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneBroadcastEvent_SetThroughputLimit")]
#else
    [DllImport("__Internal", EntryPoint = "CloneBroadcastEvent_SetThroughputLimit")]
#endif
        private static extern void SetThroughputLimitNative(UInt16 port, UInt32 throughputLimit);
#endif

/*!
        @brief  Set the maximum amount of data that this protocol sends per call to the <tt>PiaPlugin.Dispatch</tt> function.
        @param[in] port  The port number specifying the ID of the created <tt>BroadcastEventProtocol</tt>. Specify a value in the range from <tt>0</tt> to (number created - 1).
        @param[in] throughputLimit  The maximum amount of send data to configure (in bytes).
*/
        public static void SetThroughputLimit(UInt16 port, UInt32 throughputLimit)
        {
            SetThroughputLimitNative(port, throughputLimit);
        }

    }

    // -----------------------------------------------------------------------
    //! @brief  Class that is a compilations of the features that use <tt>AtomicProtocol</tt>.
    // -----------------------------------------------------------------------

    public class Atomic
    {
/*!
      @brief  Represents the locked state.
*/
        public enum LockStatus : byte
        {
            LockStatus_Unlocked,  //!<  The station does not have the lock.
            LockStatus_TryLock,   //!<  The station is trying to get the lock.
            LockStatus_Locked     //!<  The station has the lock.
        }

#if UNITY_EDITOR
        static CloneAtomic_TryLock TryLockNative;
        static CloneAtomic_Unlock UnlockNative;
        static CloneAtomic_GetLockStatus GetLockStatusNative;

        public static void InitializeHooks(IntPtr? plugin_dll)
        {
            IntPtr pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneAtomic_TryLock");
            TryLockNative = (CloneAtomic_TryLock)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneAtomic_TryLock));
            PiaPluginUtil.UnityLog("InitializeHooks " + TryLockNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneAtomic_Unlock");
            UnlockNative = (CloneAtomic_Unlock)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneAtomic_Unlock));
            PiaPluginUtil.UnityLog("InitializeHooks " + UnlockNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneAtomic_GetLockStatus");
            GetLockStatusNative = (CloneAtomic_GetLockStatus)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneAtomic_GetLockStatus));
            PiaPluginUtil.UnityLog("InitializeHooks " + GetLockStatusNative);
        }
#endif

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CloneAtomic_TryLock(UInt32 id, UInt32 priority);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneAtomic_TryLock")]
#else
    [DllImport("__Internal", EntryPoint = "CloneAtomic_TryLock")]
#endif
        private static extern void TryLockNative(UInt32 id, UInt32 priority);
#endif

/*!
    @brief  Tries to get the lock.
        @param[in] id  The ID of the lock to try to get. Specify a value in the range from 0 to 255.
        @param[in] priority  The priority level to use when stations call <tt>TryLock()</tt> at the same time. Lower values represent higher priority.
*/
        public static void TryLock(UInt32 id, UInt32 priority)
        {
            TryLockNative(id, priority);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CloneAtomic_Unlock(UInt32 id);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneAtomic_Unlock")]
#else
    [DllImport("__Internal", EntryPoint = "CloneAtomic_Unlock")]
#endif
        private static extern void UnlockNative(UInt32 id);
#endif

/*!
        @brief  Releases the lock.
        @param[in] port  The port number that specifies the ID of the <tt>AtomicProtocol</tt> that is created. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] id  The ID of the lock to release. Specify a value in the range from 0 to 255.
*/
        public static void Unlock(UInt32 id)
        {
            UnlockNative(id);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate LockStatus CloneAtomic_GetLockStatus(UInt32 id);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneAtomic_GetLockStatus")]
#else
    [DllImport("__Internal", EntryPoint = "CloneAtomic_GetLockStatus")]
#endif
        private static extern LockStatus GetLockStatusNative(UInt32 id);
#endif

/*!
        @brief  Gets the lock possession status.
        @param[in] port  The port number that specifies the ID of the <tt>AtomicProtocol</tt> that is created. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] id  The ID for getting the lock possession status. Specify a value in the range from 0 to 255.
       @return  The lock possession status. Returns <tt>LockStatus_Unlocked</tt> when in a state where this function cannot be called.
*/
        public static LockStatus GetLockStatus(UInt32 id)
        {
            return GetLockStatusNative(id);
        }

    }
    // -----------------------------------------------------------------------
    //! @brief  Class that is a compilations of the features that use <tt>ClockProtocol</tt>.
    // -----------------------------------------------------------------------

    public class Clock
    {

        public const UInt64 InvalidClock = 0xffffffffffffffff; //!<  An invalid clock value.

#if UNITY_EDITOR
        static CloneClock_GetClock GetClockNative;
        static CloneClock_IsSynchronizingClock IsSynchronizingClockNative;
        static CloneClock_SynchronizeClock SynchronizeClockNative;
        static CloneClock_UpdateClock UpdateClockNative;

        public static void InitializeHooks(IntPtr? plugin_dll)
        {
            IntPtr pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneClock_GetClock");
            GetClockNative = (CloneClock_GetClock)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneClock_GetClock));
            PiaPluginUtil.UnityLog("InitializeHooks " + GetClockNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneClock_IsSynchronizingClock");
            IsSynchronizingClockNative = (CloneClock_IsSynchronizingClock)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneClock_IsSynchronizingClock));
            PiaPluginUtil.UnityLog("InitializeHooks " + IsSynchronizingClockNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneClock_SynchronizeClock");
            SynchronizeClockNative = (CloneClock_SynchronizeClock)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneClock_SynchronizeClock));
            PiaPluginUtil.UnityLog("InitializeHooks " + SynchronizeClockNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "CloneClock_UpdateClock");
            UpdateClockNative = (CloneClock_UpdateClock)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(CloneClock_UpdateClock));
            PiaPluginUtil.UnityLog("InitializeHooks " + UpdateClockNative);
        }
#endif

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result CloneClock_GetClock(ref UInt64 pClock);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneClock_GetClock")]
#else
        [DllImport("__Internal", EntryPoint = "CloneClock_GetClock")]
#endif
        private static extern PiaPlugin.Result GetClockNative(ref UInt64 pClock);
#endif

/*!
       @brief  Gets the current clock.
       @return  Returns the current clock. Returns @ref InvalidClock if a valid clock does not exist because, for example, the object is not active.
*/
        public static PiaPlugin.Result GetClock(ref UInt64 pClock)
        {
            return GetClockNative(ref pClock);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool CloneClock_IsSynchronizingClock();
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneClock_IsSynchronizingClock")]
#else
    [DllImport("__Internal", EntryPoint = "CloneClock_IsSynchronizingClock")]
#endif
        private static extern bool IsSynchronizingClockNative();
#endif

/*!
        @brief  Determines whether the clock is being adjusted.
        @return  Returns <tt>true</tt> if the clock is being adjusted.
*/
        public static bool IsSynchronizingClock()
        {
            return IsSynchronizingClockNative();
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result CloneClock_SynchronizeClock();
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneClock_SynchronizeClock")]
#else
    [DllImport("__Internal", EntryPoint = "CloneClock_SynchronizeClock")]
#endif
        private static extern PiaPlugin.Result SynchronizeClockNative();
#endif

/*!
        @brief  Starts readjusting the clock.
        @details  The clock managed by <tt>ClockProtocol</tt> for this station is readjusted to synchronize with the clock of the host. Time discrepancy between stations can increase when synchronous communication continues for a long time,
                    becoming as large as several seconds over the course of 10 hours. Consider making periodic calls to this function if your application involves prolonged use of synchronous communication.
        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>. You must make sure that the implementation of this function in your application does not return any errors.
        @retval ResultInvalidState  The clock has overflowed or is already being readjusted. [:progErr]
*/
        public static PiaPlugin.Result SynchronizeClock()
        {
            return SynchronizeClockNative();
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CloneClock_UpdateClock(UInt64 elapsedTime, UInt64 increaseTimeMin, UInt64 increaseTimeMax);
#else
#if UNITY_STANDALONE

        [DllImport("nn_piaPlugin", EntryPoint = "CloneClock_UpdateClock")]
#else
    [DllImport("__Internal", EntryPoint = "CloneClock_UpdateClock")]
#endif
        private static extern void UpdateClockNative(UInt64 elapsedTime, UInt64 increaseTimeMin, UInt64 increaseTimeMax);
#endif

/*!
        @brief  Advances the clock time.
        @details  The clock managed by <tt>ClockProtocol</tt> is advanced by the time passed as the argument of this function.
               This function must be called periodically (once per game frame).
        @param[in] elapsedTime  The amount to increase the clock managed by <tt>ClockProtocol</tt>.
        @param[in] increaseTimeMin  This parameter is used when @ref Clock.IsSynchronizingClock is enabled. It represents the minimum amount by which to increment the clock when synchronizing the time with the host clock.
        @param[in] increaseTimeMax  This parameter is used when @ref Clock.IsSynchronizingClock is enabled. It represents the maximum amount by which to increment the clock when synchronizing the time with the host clock.
*/
        public static void UpdateClock(UInt64 elapsedTime, UInt64 increaseTimeMin, UInt64 increaseTimeMax)
        {
            UpdateClockNative(elapsedTime, increaseTimeMin, increaseTimeMax);
        }
    }
}
