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
public class PiaPluginReckoning
{
#if UNITY_EDITOR
    public static void InitializeHooks(IntPtr? plugin_dll)
    {
        Value3d.InitializeHooks(plugin_dll);
        Value1d.InitializeHooks(plugin_dll);
    }
#endif
    public class Value3d
    {

#if UNITY_EDITOR
        //SetReckoningTimeout
        static Reckoning_SetValue3d SetValue3dNative;
        static Reckoning_SetValueToAll3d SetValueToAll3dNative;
        static Reckoning_GetValue3d GetValue3dNative;
        static Reckoning_SetSamplingDistance3d SetSamplingDistance3dNative;
        static Reckoning_GetSamplingDistance3d GetSamplingDistance3dNative;
        static Reckoning_IsInCommunication3d IsInCommunication3dNative;
        static Reckoning_Reset3d Reset3dNative;
        static Reckoning_SetClock3d SetClock3dNative;
        static Reckoning_SetReckoningTimeout3d SetReckoningTimeout3dNative;

        public static void InitializeHooks(IntPtr? plugin_dll)
        {
            IntPtr pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_SetValue3d");
            SetValue3dNative = (Reckoning_SetValue3d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_SetValue3d));
            PiaPluginUtil.UnityLog("InitializeHooks " + SetValue3dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_SetValueToAll3d");
            SetValueToAll3dNative = (Reckoning_SetValueToAll3d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_SetValueToAll3d));
            PiaPluginUtil.UnityLog("InitializeHooks " + SetValueToAll3dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_GetValue3d");
            GetValue3dNative = (Reckoning_GetValue3d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_GetValue3d));
            PiaPluginUtil.UnityLog("InitializeHooks " + GetValue3dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_SetSamplingDistance3d");
            SetSamplingDistance3dNative = (Reckoning_SetSamplingDistance3d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_SetSamplingDistance3d));
            PiaPluginUtil.UnityLog("InitializeHooks " + SetSamplingDistance3dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_GetSamplingDistance3d");
            GetSamplingDistance3dNative = (Reckoning_GetSamplingDistance3d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_GetSamplingDistance3d));
            PiaPluginUtil.UnityLog("InitializeHooks " + GetSamplingDistance3dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_IsInCommunication3d");
            IsInCommunication3dNative = (Reckoning_IsInCommunication3d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_IsInCommunication3d));
            PiaPluginUtil.UnityLog("InitializeHooks " + IsInCommunication3dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_Reset3d");
            Reset3dNative = (Reckoning_Reset3d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_Reset3d));
            PiaPluginUtil.UnityLog("InitializeHooks " + Reset3dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_SetClock3d");
            SetClock3dNative = (Reckoning_SetClock3d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_SetClock3d));
            PiaPluginUtil.UnityLog("InitializeHooks " + SetClock3dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_SetReckoningTimeout3d");
            SetReckoningTimeout3dNative = (Reckoning_SetReckoningTimeout3d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_SetReckoningTimeout3d));
            PiaPluginUtil.UnityLog("InitializeHooks " + SetReckoningTimeout3dNative);
        }
#endif

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Reckoning_SetValue3d(UInt16 port, UInt64 destConstantId, float posX, float posY, float posZ, bool isStop);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_SetValue3d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_SetValue3d")]
#endif
    private static extern PiaPlugin.Result SetValue3dNative(UInt16 port, UInt64 destConstantId, float posX, float posY, float posZ, bool isStop);
#endif
/*!
        @brief  Uses <tt>Reckoning3dProtocol</tt> to send data to all stations.

        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] destConstantId  The <tt>ConstantId</tt> of the recipient.
        @param[in] posX  The x-coordinate of the three-dimensional vector that you want to send.
        @param[in] posY  The y-coordinate of the three-dimensional vector that you want to send.
        @param[in] posZ  The z-coordinate of the three-dimensional vector that you want to send.
        @param[in] isStop  The flag that indicates whether the sent value can be used as is by the receiver. Use this function when you don't want the value to swing.
        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidArgument  One or more arguments are invalid. This error is also returned when the size of the data to send is too large. [:progErr]
        @retval ResultInvalidState  Pia initialization may not have been performed. [:progErr]
        @retval ResultNotInCommunication  Communication is not possible. [:handling]
*/

        public static PiaPlugin.Result SetValue(UInt16 port, UInt64 destConstantId, float posX, float posY, float posZ, bool isStop)
        {
            PiaPlugin.Result result = SetValue3dNative(port, destConstantId, posX, posY, posZ, isStop);
            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Reckoning_SetValueToAll3d(UInt16 port, float posX, float posY, float posZ, bool isStop);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_SetValueToAll3d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_SetValueToAll3d")]
#endif
    private static extern PiaPlugin.Result SetValueToAll3dNative(UInt16 port, float posX, float posY, float posZ, bool isStop);
#endif
/*!
        @brief  Uses <tt>Reckoning3dProtocol</tt> to send data to all stations.

        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] posX  The x-coordinate of the three-dimensional vector that you want to send.
        @param[in] posY  The y-coordinate of the three-dimensional vector that you want to send.
        @param[in] posZ  The z-coordinate of the three-dimensional vector that you want to send.
        @param[in] isStop  The flag that indicates whether the sent value can be used as is by the receiver. Use this function when you don't want the value to swing.
        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidArgument  One or more arguments are invalid. This error is also returned when the size of the data to send is too large. [:progErr]
        @retval ResultInvalidState  Pia initialization may not have been performed. [:progErr]
        @retval ResultNotInCommunication  Communication is not possible. [:handling]
*/

        public static PiaPlugin.Result SetValueToAll(UInt16 port, float posX, float posY, float posZ, bool isStop)
        {
            PiaPlugin.Result result = SetValueToAll3dNative(port, posX, posY, posZ, isStop);
            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Reckoning_GetValue3d(UInt16 port, [Out] out UInt64 destConstantId, [Out] out float pPosX, [Out] out float pPosY, [Out] out float pPosZ);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_GetValue3d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_GetValue3d")]
#endif
    private static extern PiaPlugin.Result GetValue3dNative(UInt16 port, [Out] out UInt64 destConstantId, [Out] out  float posX, [Out] out  float posY, [Out] out  float posZ);
#endif

/*!
        @brief  Uses <tt>Reckoning3dProtocol</tt> to load the received data into a buffer.

        @param[in] port  The port number that specifies the ID of the <tt>Reckoning3dProtocol</tt> that is created. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[out] destConstantId  The station ID of the station that sent the received data.
        @param[out] pPosX  The value of the x-coordinate of the three-dimensional vector that was received.
        @param[out] pPosY  The value of the y-coordinate of the three-dimensional vector that was received.
        @param[out] pPosZ  The value of the z-coordinate of the three-dimensional vector that was received.
        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidState  Pia initialization may not have been performed. [:progErr]
        @retval ResultInvalidArgument  One or more arguments are invalid. [:progErr]
        @retval ResultNotInCommunication  Communication is not possible. [:handling]
        @retval ResultNoData  No data was received. [:handling]
        @retval ResultBufferShortage  The received data is too large to fit into the receive buffer.
        This result might occur due to the reception of invalid data, for example due to cheating by the sender, so it must be handled at run time. [:handling]
        @retval ResultBufferIsFull  There is not enough space in the send buffer. If this occurs frequently, consider increasing the buffer count for send packets, specified by <tt>PiaPlugin.InitializeTransportSetting()</tt>. [:handling]
*/

        public static PiaPlugin.Result GetValue(UInt16 port, out UInt64 destConstantId, out float pPosX, out float pPosY, out float pPosZ)
        {
            PiaPlugin.Result result = GetValue3dNative(port, out destConstantId, out pPosX, out pPosY, out pPosZ);
            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Reckoning_SetSamplingDistance3d(UInt16 port, float distance);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_SetSamplingDistance3d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_SetSamplingDistance3d")]
#endif
    private static extern void SetSamplingDistance3dNative(UInt16 port,float distance);
#endif
/*!
        @brief  Sets the value to use when determining whether to use the value passed by the <tt>SendValue</tt> function as the sampling value.
        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] value  The value to use for comparison.
*/
        public static void SetSamplingDistance(UInt16 port, float distance)
        {
            SetSamplingDistance3dNative(port, distance);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate float Reckoning_GetSamplingDistance3d(UInt16 port);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_GetSamplingDistance3d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_GetSamplingDistance3d")]
#endif
    private static extern float GetSamplingDistance3dNative(UInt16 port);
#endif
/*!
        @brief  Gets the value to use when determining whether to use the value passed by the <tt>SendValue</tt> function as the sampling value.
        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @return  If successful, the function returns the value to use for the comparison. Returns <tt>–1</tt> when failed.
*/
        public static float GetSamplingDistance(UInt16 port)
        {
            return GetSamplingDistance3dNative(port);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool Reckoning_IsInCommunication3d(UInt16 port);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_IsInCommunication3d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_IsInCommunication3d")]
#endif
    private static extern bool IsInCommunication3dNative(UInt16 port);
#endif
/*!
        @brief  Determines whether communication is possible.
        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @return  Returns <tt>true</tt> if communication is possible, or <tt>false</tt> otherwise.
*/
        public static bool IsInCommunication(UInt16 port)
        {
            return IsInCommunication3dNative(port);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Reckoning_Reset3d(UInt16 port);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_Reset3d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_Reset3d")]
#endif
    private static extern bool Reset3dNative(UInt16 port);
#endif
/*!
        @brief  Initializes all data, such as the sample buffers used for estimation and the last received data.
        @details  Called when you want to redo the estimation, such as when resuming synchronization.
        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
*/
        public static void Reset(UInt16 port)
        {
            Reset3dNative(port);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Reckoning_SetClock3d(UInt16 port, UInt64 clock);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_SetClock3d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_SetClock3d")]
#endif
    private static extern void SetClock3dNative(UInt16 port, UInt64 clock);
#endif
/*!
        @brief  Sets the current clock value.
        @details  Because the estimate is based on the current clock value, it must be set every frame.
        @param[in] port  The port number that specifies the ID of the <tt>Reckoning3dProtocol</tt> that is created. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] clock  The current clock value.
*/
        public static void SetClock(UInt16 port, UInt64 clock)
        {
            SetClock3dNative(port, clock);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Reckoning_SetReckoningTimeout3d(UInt16 port, UInt64 clock);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_SetReckoningTimeout3d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_SetReckoningTimeout3d")]
#endif
    private static extern void SetReckoningTimeout3dNative(UInt16 port, UInt64 clock);
#endif
/*!
        @brief  Sets the current clock value.
        @details  Because the estimate is based on the current clock value, it must be set every frame.
        @param[in] port  The port number that specifies the ID of the <tt>Reckoning3dProtocol</tt> that is created. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] clock  The current clock value.
*/
        public static void SetReckoningTimeout(UInt16 port, UInt64 clock)
        {
            SetReckoningTimeout3dNative(port, clock);
        }
    }


    public class Value1d
    {

#if UNITY_EDITOR
        static Reckoning_SetValue1d SetValue1dNative;
        static Reckoning_SetValueToAll1d SetValueToAll1dNative;
        static Reckoning_GetValue1d GetValue1dNative;
        static Reckoning_SetSamplingDistance1d SetSamplingDistance1dNative;
        static Reckoning_GetSamplingDistance1d GetSamplingDistance1dNative;
        static Reckoning_IsInCommunication1d IsInCommunication1dNative;
        static Reckoning_Reset1d Reset1dNative;
        static Reckoning_SetClock1d SetClock1dNative;
        static Reckoning_SetReckoningTimeout1d SetReckoningTimeout1dNative;

    public static void InitializeHooks(IntPtr? plugin_dll)
        {
            IntPtr pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_SetValue1d");
            SetValue1dNative = (Reckoning_SetValue1d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_SetValue1d));
            PiaPluginUtil.UnityLog("InitializeHooks " + SetValue1dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_SetValueToAll1d");
            SetValueToAll1dNative = (Reckoning_SetValueToAll1d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_SetValueToAll1d));
            PiaPluginUtil.UnityLog("InitializeHooks " + SetValueToAll1dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_GetValue1d");
            GetValue1dNative = (Reckoning_GetValue1d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_GetValue1d));
            PiaPluginUtil.UnityLog("InitializeHooks " + GetValue1dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_SetSamplingDistance1d");
            SetSamplingDistance1dNative = (Reckoning_SetSamplingDistance1d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_SetSamplingDistance1d));
            PiaPluginUtil.UnityLog("InitializeHooks " + SetSamplingDistance1dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_GetSamplingDistance1d");
            GetSamplingDistance1dNative = (Reckoning_GetSamplingDistance1d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_GetSamplingDistance1d));
            PiaPluginUtil.UnityLog("InitializeHooks " + GetSamplingDistance1dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_IsInCommunication1d");
            IsInCommunication1dNative = (Reckoning_IsInCommunication1d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_IsInCommunication1d));
            PiaPluginUtil.UnityLog("InitializeHooks " + IsInCommunication1dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_Reset1d");
            Reset1dNative = (Reckoning_Reset1d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_Reset1d));
            PiaPluginUtil.UnityLog("InitializeHooks " + Reset1dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_SetClock1d");
            SetClock1dNative = (Reckoning_SetClock1d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_SetClock1d));
            PiaPluginUtil.UnityLog("InitializeHooks " + SetClock1dNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Reckoning_SetReckoningTimeout1d");
            SetReckoningTimeout1dNative = (Reckoning_SetReckoningTimeout1d)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Reckoning_SetReckoningTimeout1d));
            PiaPluginUtil.UnityLog("InitializeHooks " + SetReckoningTimeout1dNative);
        }
#endif

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Reckoning_SetValue1d(UInt16 port, UInt64 destConstantId, float value, bool isStop);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_SetValue1d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_SetValue1d")]
#endif
    private static extern PiaPlugin.Result SetValue1dNative(UInt16 port, UInt64 destConstantId, float value, bool isStop);
#endif
/*!
        @brief  Uses <tt>Reckoning1dProtocol</tt> to send data to all stations.

        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] destConstantId  The <tt>ConstantId</tt> of the recipient.
        @param[in] value  The floating point number that you want to send.
        @param[in] isStop  The flag that indicates whether the sent value can be used as is by the receiver. Use this function when you don't want the value to swing.
        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidArgument  One or more arguments are invalid. This error is also returned when the size of the data to send is too large. [:progErr]
        @retval ResultInvalidState  Pia initialization may not have been performed. [:progErr]
        @retval ResultNotInCommunication  Communication is not possible. [:handling]
        @retval ResultTemporaryUnavailable  The function is temporarily unavailable because the session is migrating. (Only returned when the joint session feature is being used.) [:handling]
*/

        public static PiaPlugin.Result SetValue(UInt16 port, UInt64 destConstantId, float value, bool isStop)
        {
            PiaPlugin.Result result = SetValue1dNative(port, destConstantId, value, isStop);
            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Reckoning_SetValueToAll1d(UInt16 port, float value, bool isStop);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_SetValueToAll1d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_SetValueToAll1d")]
#endif
    private static extern PiaPlugin.Result SetValueToAll1dNative(UInt16 port, float value, bool isStop);
#endif
/*!
        @brief  Uses <tt>Reckoning1dProtocol</tt> to send data to all stations.

        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] value  The floating point number that you want to send.
        @param[in] isStop  The flag that indicates whether the sent value can be used as is by the receiver. Use this function when you don't want the value to swing.
        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidState  Pia initialization may not have been performed. [:progErr]
        @retval ResultInvalidArgument  One or more arguments are invalid. This error is also returned when the size of the data to send is too large. [:progErr]
        @retval ResultNotInCommunication  Communication is not possible. [:handling]
        @retval ResultTemporaryUnavailable  The function is temporarily unavailable because the session is migrating. (Only returned when the joint session feature is being used.) [:handling]
*/

        public static PiaPlugin.Result SetValueToAll(UInt16 port, float value, bool isStop)
        {
            PiaPlugin.Result result = SetValueToAll1dNative(port, value, isStop);
            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Reckoning_GetValue1d(UInt16 port, [Out] out UInt64 destConstantId, [Out] out float pValue);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_GetValue1d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_GetValue1d")]
#endif
    private static extern PiaPlugin.Result GetValue1dNative(UInt16 port, [Out] out UInt64 destConstantId, [Out] out  float pValue);
#endif

/*!
        @brief  Uses <tt>Reckoning1dProtocol</tt> to load the received data into the buffer.

        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[out] destConstantId  The station ID of the station that sent the received data.
        @param[out] pValue  The received floating point number.
        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidState  Pia initialization may not have been performed. [:progErr]
        @retval ResultInvalidArgument  One or more arguments are invalid. [:progErr]
        @retval ResultNotInCommunication  Communication is not possible. [:handling]
        @retval ResultNoData  No data was received. [:handling]
        @retval ResultBufferShortage  The received data is too large to fit into the receive buffer.
        This result might occur due to the reception of invalid data, for example due to cheating by the sender, so it must be handled at run time. [:handling]
        @retval ResultBufferIsFull  There is not enough space in the send buffer. If this occurs frequently, consider increasing the buffer count for send packets, specified by <tt>PiaPlugin.InitializeTransportSetting()</tt>. [:handling]
*/

        public static PiaPlugin.Result GetValue(UInt16 port, out UInt64 destConstantId, out float pValue)
        {
            PiaPlugin.Result result = GetValue1dNative(port, out destConstantId, out pValue);
            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Reckoning_SetSamplingDistance1d(UInt16 port, float value);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_SetSamplingDistance1d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_SetSamplingDistance1d")]
#endif
    private static extern void SetSamplingDistance1dNative(UInt16 port,float value);
#endif
/*!
        @brief  Sets the value to use when determining whether to use the value passed by the <tt>SendValue</tt> function as the sampling value.
        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] value  The value to use for comparison.
*/
        public static void SetSamplingDistance(UInt16 port, float value)
        {
            SetSamplingDistance1dNative(port, value);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate float Reckoning_GetSamplingDistance1d(UInt16 port);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_GetSamplingDistance1d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_GetSamplingDistance1d")]
#endif
    private static extern float GetSamplingDistance1dNative(UInt16 port);
#endif
/*!
        @brief  Gets the value to use when determining whether to use the value passed by the <tt>SendValue</tt> function as the sampling value.
        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @return  If successful, the function returns the value to use for the comparison. Returns <tt>–1</tt> when failed.
*/
        public static float GetSamplingDistance(UInt16 port)
        {
            return GetSamplingDistance1dNative(port);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool Reckoning_IsInCommunication1d(UInt16 port);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_IsInCommunication1d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_IsInCommunication1d")]
#endif
    private static extern bool IsInCommunication1dNative(UInt16 port);
#endif
/*!
        @brief  Determines whether communication is possible.
        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @return  Returns <tt>true</tt> if communication is possible, or <tt>false</tt> otherwise.
*/
        public static bool IsInCommunication(UInt16 port)
        {
            return IsInCommunication1dNative(port);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Reckoning_Reset1d(UInt16 port);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_Reset1d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_Reset1d")]
#endif
    private static extern bool Reset1dNative(UInt16 port);
#endif
/*!
        @brief  Initializes all data, such as the sample buffers used for estimation and the last received data.
        @details  Called when you want to redo the estimation, such as when resuming synchronization.
        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
*/
        public static void Reset(UInt16 port)
        {
            Reset1dNative(port);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Reckoning_SetClock1d(UInt16 port, UInt64 clock);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_SetClock1d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_SetClock1d")]
#endif
    private static extern void SetClock1dNative(UInt16 port, UInt64 clock);
#endif
/*!
        @brief  Sets the current clock value.
        @details  Because the estimate is based on the current clock value, it must be set every frame.
        @param[in] port  The port number that specifies the ID of the <tt>Reckoning3dProtocol</tt> that is created. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] clock  The current clock value.
*/
        public static void SetClock(UInt16 port, UInt64 clock)
        {
            SetClock1dNative(port, clock);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Reckoning_SetReckoningTimeout1d(UInt16 port, UInt64 clock);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Reckoning_SetReckoningTimeout1d")]
#else
    [DllImport("__Internal", EntryPoint = "Reckoning_SetReckoningTimeout1d")]
#endif
    private static extern void SetReckoningTimeout1dNative(UInt16 port, UInt64 clock);
#endif
/*!
        @brief  Sets the current clock value.
        @details  Because the estimate is based on the current clock value, it must be set every frame.
        @param[in] port  The port number specifying the ID of the created <tt>Reckoning1dProtocol</tt>. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] clock  The current clock value.
*/
        public static void SetReckoningTimeout(UInt16 port, UInt64 clock)
        {
            SetReckoningTimeout1dNative(port, clock);
        }
    }
}
