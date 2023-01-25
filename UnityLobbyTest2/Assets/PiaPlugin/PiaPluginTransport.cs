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
//! @brief  Class that is a compilation of the features used by <tt>PiaTransport</tt>.
// -----------------------------------------------------------------------
public class PiaPluginTransport
{
    public const int PacketAnalysisDataEntriesMax = 128;      //!<  The maximum number of entries for <tt>PacketAnalysisData</tt>.
    public const int PacketAnalysisDataNameLengthMax = 32;      //!<  The maximum length for names attached to <tt>PacketAnalysisData</tt> instances.
    public const int ConnectionAnalysisDataEntriesMax = 32;  //!<  The maximum number of entries for <tt>PacketAnalysisData</tt>.

    public const UInt32 ThroughputLimitMax = 0x0FFFFFFF; //!<  The maximum value that can be set by <tt>SetReliableThroughputLimit()</tt>.

/*!
      @brief  Class with the data required for latency simulation and packet loss simulation.
*/
    [StructLayout(LayoutKind.Sequential)]
    public class EmulationSetting
    {
        public int sendThreadPacketLossRatio;           //!<  The packet loss rate emulated on the send thread.
        public int receiveThreadPacketLossRatio;        //!<  The packet loss rate emulated on the receive thread.
        public int sendThreadLatencyEmulationMin;       //!<  The minimum value for the send latency emulation time.
        public int sendThreadLatencyEmulationMax;       //!<  The maximum value for the send latency emulation time.
        public int receiveThreadLatencyEmulationMin;    //!<  The minimum value for the receive latency emulation time.
        public int receiveThreadLatencyEmulationMax;    //!<  The maximum value for the receive latency emulation time.

/*!
          @brief  Constructor.
*/
        public EmulationSetting()
        {
            sendThreadPacketLossRatio = -1;
            receiveThreadPacketLossRatio = -1;
            sendThreadLatencyEmulationMin = -1;
            sendThreadLatencyEmulationMax = -1;
            receiveThreadLatencyEmulationMin = -1;
            receiveThreadLatencyEmulationMax = -1;
        }
    }

#if UNITY_EDITOR
    static TransportAnalysisData.PrintTransportAnalysisData PrintNative;
    static ConnectionAnalysisData.PrintConnectionAnalysisData PrintNative2;
    static PacketAnalysisData.PrintPacketAnalysisData PrintNative3;

    static Transport_SetEmulationSetting SetEmulationSettingNative;
    static Transport_UpdateTransportAnalyzer UpdateTransportAnalyzerNative;
    static Transport_GetTransportAnalysisData GetTransportAnalysisDataNative;
    static Transport_GetTransportAnalysisData2 GetTransportAnalysisDataNative2;

    public static void InitializeHooks(IntPtr? plugin_dll)
    {
        IntPtr pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Transport_PrintTransportAnalysisData");
        PrintNative = (TransportAnalysisData.PrintTransportAnalysisData)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportAnalysisData.PrintTransportAnalysisData));
        PiaPluginUtil.UnityLog("InitializeHooks " + PrintNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Transport_PrintConnectionAnalysisData");
        PrintNative2 = (ConnectionAnalysisData.PrintConnectionAnalysisData)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(ConnectionAnalysisData.PrintConnectionAnalysisData));
        PiaPluginUtil.UnityLog("InitializeHooks " + PrintNative2);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Transport_PrintPacketAnalysisData");
        PrintNative3 = (PacketAnalysisData.PrintPacketAnalysisData)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(PacketAnalysisData.PrintPacketAnalysisData));
        PiaPluginUtil.UnityLog("InitializeHooks " + PrintNative3);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Transport_SetEmulationSetting");
        SetEmulationSettingNative = (Transport_SetEmulationSetting)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Transport_SetEmulationSetting));
        PiaPluginUtil.UnityLog("InitializeHooks " + SetEmulationSettingNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Transport_UpdateTransportAnalyzer");
        UpdateTransportAnalyzerNative = (Transport_UpdateTransportAnalyzer)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Transport_UpdateTransportAnalyzer));
        PiaPluginUtil.UnityLog("InitializeHooks " + UpdateTransportAnalyzerNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Transport_GetTransportAnalysisData");
        GetTransportAnalysisDataNative = (Transport_GetTransportAnalysisData)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Transport_GetTransportAnalysisData));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetTransportAnalysisDataNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Transport_GetTransportAnalysisData2");
        GetTransportAnalysisDataNative2 = (Transport_GetTransportAnalysisData2)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Transport_GetTransportAnalysisData2));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetTransportAnalysisDataNative2);

        Unreliable.InitializeHooks(plugin_dll);
        Reliable.InitializeHooks(plugin_dll);
        BroadcastReliable.InitializeHooks(plugin_dll);
        StreamBroadcastReliable.InitializeHooks(plugin_dll);
    }
#endif


/*!
      @brief  <tt>TransportAnalysisData</tt> is a structure that combines analysis data for send and receive packets,
         and parameters indicating the quality of the connection (RTT and packet loss) for each station.
*/
    [StructLayout(LayoutKind.Sequential)]
    public struct TransportAnalysisData
    {
        public PacketAnalysisData sendPacketAnalysisData;           //!<  Sent packet analysis data.
        public PacketAnalysisData sendUnicastPacketAnalysisData;    //!<  Analysis data for the packet sent using unicast.
        public PacketAnalysisData sendBroadcastPacketAnalysisData;  //!<  Analysis data for the packet sent using broadcast.
        public PacketAnalysisData recvPacketAnalysisData;           //!<  Received packet analysis data.
        public ConnectionAnalysisData connectionAnalysisData;       //!<  Analysis data for connection quality.
        public System.UInt32 dispatchCount;                         //!<  The dispatch count.

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PrintTransportAnalysisData([In, MarshalAs(UnmanagedType.LPStruct)] IntPtr data, [MarshalAs(UnmanagedType.U1)] bool isDetail, [MarshalAs(UnmanagedType.U1)] bool isTotalOnly);
#else
#if UNITY_STANDALONE
        [DllImport("nn_piaPlugin", EntryPoint = "Transport_PrintTransportAnalysisData")]
#else
        [DllImport("__Internal", EntryPoint = "Transport_PrintTransportAnalysisData")]
#endif
        private static extern void PrintNative([In, MarshalAs(UnmanagedType.LPStruct)] IntPtr data, [MarshalAs(UnmanagedType.U1)] bool isDetail, [MarshalAs(UnmanagedType.U1)] bool isTotalOnly);
#endif

/*!
         @brief  Sends the data to the console.

         @param[in] isDetail  Set to <tt>true</tt> to also send the protocol used internally by Pia.
         @param[in] isTotalOnly  Set to <tt>false</tt> when exporting the analysis data for unicast and broadcast sends separately.
*/
        public void Print(bool isDetail, bool isTotalOnly)
        {
            int size = Marshal.SizeOf(this);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, ptr, false);
            PrintNative(ptr, isDetail, isTotalOnly);
            Marshal.FreeHGlobal(ptr);
        }
    }

/*!
      @brief  The <tt>ConnectionAnalysisData</tt> structure contains parameters related to the connection quality (such as the RTT and the packet loss rate).
*/
    [StructLayout(LayoutKind.Sequential)]
    public struct ConnectionAnalysisData
    {
/*!
          @brief  Contains information required for tabulating analysis results for each <tt>Station</tt>.
*/
        [StructLayout(LayoutKind.Sequential)]
        public struct Entry
        {
            public System.Int32 rtt;     //!<  RTT.
            public System.Int32 rttMin;  //!<  Minimum RTT.
            public System.Int32 rttMax;  //!<  Maximum RTT.

            public System.UInt32 prevTotalUnicastPacketNum;         //!<  The number of packets sent in the previous unicast.
            public System.UInt32 currTotalUnicastPacketNum;         //!<  The number of packets sent in this unicast.
            public System.UInt32 prevTotalUnicastPacketLossNum;     //!<  The number of lost packets sent in the previous unicast.
            public System.UInt32 currTotalUnicastPacketLossNum;     //!<  The number of lost packets sent in this unicast.

            public System.UInt32 prevTotalBroadcastPacketNum;           //!<  The number of packets sent in the previous broadcast.
            public System.UInt32 currTotalBroadcastPacketNum;           //!<  The number of packets sent in this broadcast.
            public System.UInt32 prevTotalBroadcastPacketLossNum;       //!<  The number of sent packets lost in the previous broadcast.
            public System.UInt32 currTotalBroadcastPacketLossNum;       //!<  The number of sent packets lost in this broadcast.

            [MarshalAs(UnmanagedType.U1)]
            public bool isValid;  //!<  <tt>true</tt> if this entry is valid.
        };

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ConnectionAnalysisDataEntriesMax)]
        public Entry[] entry;                         //!<  Entry.
        public System.UInt32 passedMilliSec;          //!<  Time elapsed (in milliseconds).

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PrintConnectionAnalysisData([In, MarshalAs(UnmanagedType.LPStruct)] IntPtr connectionAnalysisData);
#else
#if UNITY_STANDALONE
        [DllImport("nn_piaPlugin", EntryPoint = "Transport_PrintConnectionAnalysisData")]
#else
        [DllImport("__Internal", EntryPoint = "Transport_PrintConnectionAnalysisData")]
#endif
        private static extern void PrintNative2([In, MarshalAs(UnmanagedType.LPStruct)] IntPtr connectionAnalysisData);
#endif

/*!
         @brief  Sends the data to the console (except in the Release build).
*/
        public void Print()
        {
            int size = Marshal.SizeOf(this);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, ptr, false);
            PrintNative2(ptr);
            Marshal.FreeHGlobal(ptr);
        }
    }

/*!
     @cond  PRIVATE
     @brief  A structure representing a protocol ID.
*/
    [StructLayout(LayoutKind.Sequential)]
    public struct ProtocolId
    {
        public System.UInt32 value;
    };
    //! @endcond



/*!
     @brief  Stores information required for tabulating the results of an analysis of the data in sent and received packets.
*/
    [StructLayout(LayoutKind.Sequential)]
    public struct PacketAnalysisData
    {
/*!
         @brief  Contains information required for tabulating analysis results for each <tt>ProtocolId</tt>.
*/
        [StructLayout(LayoutKind.Sequential)]
        public struct Entry
        {
            public ProtocolId protocolId;                     //!<  ProtocolId
            public System.UInt32 totalNum;                    //!<  Specifies the number of protocol messages that correspond to <tt>ProtocolId</tt>.
            public System.UInt32 totalDataSize;               //!<  Specifies the total amount of data, in bytes, corresponding to <tt>ProtocolId</tt>.
            public System.UInt32 sumTotalNum;                 //!<  Specifies the number of protocol messages that correspond to <tt>ProtocolId</tt>. This value is not cleared by the <tt>ClearCounters()</tt> function.
            public System.UInt64 sumTotalDataSize;            //!<  Specifies the total amount of data, in bytes, corresponding to <tt>ProtocolId</tt>. This value is not cleared by the <tt>ClearCounters()</tt> function.
        };

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PacketAnalysisDataEntriesMax)]
        public Entry[] entry;                                                              //!<  Specifies an array of entries.
        public System.Int32 nowEntryNum;                                                   //!<  Specifies the current number of entries.
        public System.Int32 passedMilliSec;                                                //!<  Specifies the elapsed time, in milliseconds, since the execution of the <tt>PiaTransport</tt> startup process.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PacketAnalysisDataNameLengthMax)]
        public string name;                                                                //!<  Specifies the name to attach to the instance. The string must be terminated with <tt>\0</tt>.
        public System.Int32 totalPacketNum;                                                //!<  Specifies the number of packets analyzed.
        public System.Int32 totalPacketSize;                                               //!<  Specifies the total size of the packets analyzed.

        public System.Int32 sumTotalPacketNum;                                             //!<  Specifies the number of packets counted since analysis started. This value is not cleared by the <tt>ClearCounters()</tt> function.
        public System.Int64 sumTotalPacketSize;                                            //!<  Specifies the total size of the packets counted since analysis started. This value is not cleared by the <tt>ClearCounters()</tt> function.

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PrintPacketAnalysisData([In, MarshalAs(UnmanagedType.LPStruct)] IntPtr packetAnalysisData, [MarshalAs(UnmanagedType.U1)] bool isDetail);
#else
#if UNITY_STANDALONE
        [DllImport("nn_piaPlugin", EntryPoint = "Transport_PrintPacketAnalysisData")]
#else
        [DllImport("__Internal", EntryPoint = "Transport_PrintPacketAnalysisData")]
#endif
        private static extern void PrintNative3([In, MarshalAs(UnmanagedType.LPStruct)] IntPtr packetAnalysisData, [MarshalAs(UnmanagedType.U1)] bool isDetail);
#endif

/*!
         @brief  Prints the data.

         @param[in] isDetail  <tt>true</tt> to get verbose output.
*/
        public void Print(bool isDetail)
        {
            int size = Marshal.SizeOf(this);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, ptr, false);
            PrintNative3(ptr, isDetail);
            Marshal.FreeHGlobal(ptr);
        }
    };


    // -----------------------------------------------------------------------
    //! @brief  This class is a collection of the features that use <tt>UnreliableProtocol</tt>.
    // -----------------------------------------------------------------------

    public class Unreliable
    {
#if UNITY_EDITOR
        static TransportUnreliable_GetSendDataSizeMax GetSendUnreliableDataSizeMaxNative;
        static TransportUnreliable_SendToAll SendToAllUnreliableNative;
        static TransportUnreliable_Send SendUnreliableNative;
        static TransportUnreliable_Receive ReceiveUnreliableNative;

        public static void InitializeHooks(IntPtr? plugin_dll)
        {
            IntPtr pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportUnreliable_GetSendDataSizeMax");
            GetSendUnreliableDataSizeMaxNative = (TransportUnreliable_GetSendDataSizeMax)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportUnreliable_GetSendDataSizeMax));
            PiaPluginUtil.UnityLog("InitializeHooks " + GetSendUnreliableDataSizeMaxNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportUnreliable_SendToAll");
            SendToAllUnreliableNative = (TransportUnreliable_SendToAll)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportUnreliable_SendToAll));
            PiaPluginUtil.UnityLog("InitializeHooks " + SendToAllUnreliableNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportUnreliable_Send");
            SendUnreliableNative = (TransportUnreliable_Send)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportUnreliable_Send));
            PiaPluginUtil.UnityLog("InitializeHooks " + SendUnreliableNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportUnreliable_Receive");
            ReceiveUnreliableNative = (TransportUnreliable_Receive)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportUnreliable_Receive));
            PiaPluginUtil.UnityLog("InitializeHooks " + ReceiveUnreliableNative);
        }
#endif

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt32 TransportUnreliable_GetSendDataSizeMax();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportUnreliable_GetSendDataSizeMax")]
#else
    [DllImport("__Internal", EntryPoint = "TransportUnreliable_GetSendDataSizeMax")]
#endif
    private static extern UInt32 GetSendUnreliableDataSizeMaxNative();
#endif

/*!
        @brief  Gets the maximum data size that can be sent at the same time using @ref SendToAll and @ref Send. The value is in bytes.
        @details  This function must be called after the station has joined the session. The correct value is not returned if this function is called sooner.

        @return  The maximum data size that can be sent at the same time is returned using @ref SendToAll and @ref Send. The value is in bytes.
*/
        public static UInt32 GetSendUnreliableDataSizeMax()
        {
            return GetSendUnreliableDataSizeMaxNative();
        }


#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportUnreliable_SendToAll(UInt16 port, IntPtr pData, UInt32 dataSize);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportUnreliable_SendToAll")]
#else
    [DllImport("__Internal", EntryPoint = "TransportUnreliable_SendToAll")]
#endif
    private static extern PiaPlugin.Result SendToAllUnreliableNative(UInt16 port, IntPtr pData, UInt32 dataSize);
#endif

/*!
        @brief  Uses <tt>UnreliableProtocol</tt> to send byte array data to all stations.

        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] data  Byte array data to send. This size must not be greater than the value returned by the <tt>GetSendUnreliableDataSizeMax()</tt> function.
        @param[in] dataSize  Size of the data to send. The value is in bytes.

        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidArgument  One or more arguments are invalid. This error is also returned when the size of the data to send is too large. [:progErr]
        @retval ResultInvalidState  Pia initialization may not have been performed. [:progErr]
        @retval ResultNotInCommunication  Communication is not possible. [:handling]
        @retval ResultBufferIsFull  There is not enough space in the send buffer. If this occurs frequently, consider increasing the buffer count for send packets, specified by <tt>PiaPlugin.InitializeTransportSetting()</tt>. [:handling]
*/
        public static PiaPlugin.Result SendToAll(UInt16 port, byte[] data, UInt32 dataSize)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            PiaPlugin.Result result = SendToAllUnreliableNative(port, handle.AddrOfPinnedObject(), dataSize);
            handle.Free();

            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportUnreliable_Send(UInt16 port, UInt64 destConstantId, IntPtr pData, UInt32 dataSize);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportUnreliable_Send")]
#else
    [DllImport("__Internal", EntryPoint = "TransportUnreliable_Send")]
#endif
    private static extern PiaPlugin.Result SendUnreliableNative(UInt16 port, UInt64 destConstantId, IntPtr pData, UInt32 dataSize);
#endif

/*!
        @brief  Uses <tt>UnreliableProtocol</tt> to send byte array data to specific stations.

        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] destConstantId  The <tt>ConstantId</tt> of the recipient.
        @param[in] data  Byte array data to send. This size must not be greater than the value returned by the <tt>GetSendUnreliableDataSizeMax()</tt> function.
        @param[in] dataSize  Size of the data to send. The value is in bytes.

        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidArgument  One or more arguments are invalid. This error is also returned when the size of the data to send is too large. [:progErr]
        @retval ResultInvalidState  Pia initialization may not have been performed. [:progErr]
        @retval ResultNotInCommunication  Communication is not possible. [:handling]
        @retval ResultTemporaryUnavailable  The API is temporarily unavailable because the session is migrating. (Only returned when the joint session feature is being used.) [:handling]
        @retval ResultNotFound  The specified destination was not found.
        @retval ResultBufferIsFull  There is not enough space in the send buffer. If this occurs frequently, consider increasing the buffer count for send packets, specified by <tt>PiaPlugin.InitializeTransportSetting()</tt>. [:handling]
*/
        public static PiaPlugin.Result Send(UInt16 port, UInt64 destConstantId, byte[] data, UInt32 dataSize)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            PiaPlugin.Result result = SendUnreliableNative(port, destConstantId, handle.AddrOfPinnedObject(), dataSize);
            handle.Free();

            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportUnreliable_Receive(UInt16 port, [Out] out UInt64 srcConstantId, IntPtr pRecvBuf, [Out] out UInt32 recvDataSize, UInt32 recvBufSize);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportUnreliable_Receive")]
#else
    [DllImport("__Internal", EntryPoint = "TransportUnreliable_Receive")]
#endif
    private static extern PiaPlugin.Result ReceiveUnreliableNative(UInt16 port, [Out] out UInt64 srcConstantId, IntPtr pRecvBuf, [Out] out UInt32 recvDataSize, UInt32 recvBufSize);
#endif

/*!
        @brief  Uses UnreliableProtocol to load the received data into a buffer.

        @param[in] port  The port number specifying the ID of the created UnreliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[out] srcConstantId  The station ID of the station that sent the received data.
        @param[out] recvBuf  Buffer that stores the received data.
        @param[out] recvDataSize  The size of the received data. The value is in bytes.

        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.

        @retval ResultInvalidArgument  One or more arguments are invalid. [:progErr]
        @retval ResultInvalidState  Pia initialization may not have been performed. [:progErr]
        @retval ResultBufferShortage  The receiving buffer is too small. Returned when the obtained data is larger than the size of the buffer specified by <tt><var>recvBuf</var></tt>. [:progErr]
        @retval ResultNoData  No data was received. [:handling]
*/
        public static PiaPlugin.Result Receive(UInt16 port, out UInt64 srcConstantId, byte[] recvBuf, out UInt32 recvDataSize)
        {
            int bufferSize = recvBuf.Length;
            GCHandle handle = GCHandle.Alloc(recvBuf, GCHandleType.Pinned);
            PiaPlugin.Result result = ReceiveUnreliableNative(port, out srcConstantId, handle.AddrOfPinnedObject(), out recvDataSize, (UInt32)bufferSize);
            handle.Free();
            return result;
        }
    }

    // -----------------------------------------------------------------------
    //! @brief  This class is a collection of the features that use <tt>ReliableProtocol</tt>.
    // -----------------------------------------------------------------------

    public class Reliable
    {
#if UNITY_EDITOR
        static TransportReliable_Send SendReliableNative;
        static TransportReliable_Receive ReceiveReliableNative;
        static TransportReliable_SetThroughputLimit SetReliableThroughputLimitNative;
        static TransportReliable_ReadySend ReadySendReliableNative;
        static TransportReliable_ReadyReceive ReadyReceiveReliableNative;

        public static void InitializeHooks(IntPtr? plugin_dll)
        {
            IntPtr pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportReliable_Send");
            SendReliableNative = (TransportReliable_Send)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportReliable_Send));
            PiaPluginUtil.UnityLog("InitializeHooks " + SendReliableNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportReliable_Receive");
            ReceiveReliableNative = (TransportReliable_Receive)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportReliable_Receive));
            PiaPluginUtil.UnityLog("InitializeHooks " + ReceiveReliableNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportReliable_SetThroughputLimit");
            SetReliableThroughputLimitNative = (TransportReliable_SetThroughputLimit)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportReliable_SetThroughputLimit));
            PiaPluginUtil.UnityLog("InitializeHooks " + SetReliableThroughputLimitNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportReliable_ReadySend");
            ReadySendReliableNative = (TransportReliable_ReadySend)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportReliable_ReadySend));
            PiaPluginUtil.UnityLog("InitializeHooks " + ReadySendReliableNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportReliable_ReadyReceive");
            ReadyReceiveReliableNative = (TransportReliable_ReadyReceive)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportReliable_ReadyReceive));
            PiaPluginUtil.UnityLog("InitializeHooks " + ReadyReceiveReliableNative);
        }
#endif

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result TransportReliable_Send(UInt16 port, UInt64 destConstantId, IntPtr pData, UInt32 dataSize);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportReliable_Send")]
#else
    [DllImport("__Internal", EntryPoint = "TransportReliable_Send")]
#endif
    private static extern PiaPlugin.Result SendReliableNative(UInt16 port, UInt64 destConstantId, IntPtr pData, UInt32 dataSize);
#endif


/*!
     @brief  Uses <tt>ReliableProtocol</tt>, which performs unicast sending, to send byte array data to specific stations.

     @param[in] port  The port number specifying the ID of the created  ReliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
     @param[in] destConstantId  The station ID of the station to send to.
     @param[in] data  Byte array data to send.
     @param[in] dataSize  Size of the data to send. The value is in bytes.

     @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.

     @retval ResultInvalidState  Pia initialization may not have been performed. [:progErr]
     @retval ResultInvalidArgument  An argument is not valid. [:progErr]
     @retval ResultNotInCommunication  The local station is not in communication with the specified recipient. The specified peer is not in this session. [:handling]
     @retval ResultBufferIsFull  There is not enough space in the send buffer. If this occurs frequently, consider increasing the buffer count for send packets, specified by <tt>PiaPlugin.InitializeTransportSetting()</tt>. [:handling]
     @retval ResultNotFound  The specified destination was not found. [:handling]
*/

    public static PiaPlugin.Result Send(UInt16 port, UInt64 destConstantId, byte[] data, UInt32 dataSize)
    {
        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        PiaPlugin.Result result = SendReliableNative(port, destConstantId, handle.AddrOfPinnedObject(), dataSize);
        handle.Free();

        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result TransportReliable_Receive(UInt16 port, [Out] out UInt64 srcConstantId, IntPtr pRecvBuf, [Out] out UInt32 recvDataSize, UInt32 recvBufSize);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportReliable_Receive")]
#else
    [DllImport("__Internal", EntryPoint = "TransportReliable_Receive")]
#endif
    private static extern PiaPlugin.Result ReceiveReliableNative(UInt16 port, [Out] out UInt64 srcConstantId, IntPtr pRecvBuf, [Out] out UInt32 recvDataSize, UInt32 recvBufSize);
#endif

/*!
    @brief  Uses <tt>ReliableProtocol</tt>, which performs unicast sending, to receive byte array sent using @ref Send.

    @param[in] port  The port number specifying the ID of the created  ReliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
    @param[out] srcConstantId  The station ID of the station that sent the received data.
    @param[out] recvBuf  Buffer for storing received data.
    @param[out] recvDataSize  The size of the received data. The value is in bytes.

    @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.

    @retval ResultInvalidState  Not initialized. [:progErr]
    @retval ResultInvalidArgument  An argument is not valid. [:progErr]
    @retval ResultNoData  No data was received. [:handling]
    @retval ResultBufferShortage  The received data is too large to fit into the receive buffer.
    This result might occur when invalid data is received, such as when a sender is cheating, in which case it requires handling at run time.
    Revise the program so this result is not returned when the sender behavior is appropriate. [:handling]
*/
    public static PiaPlugin.Result Receive(UInt16 port, out UInt64 srcConstantId, byte[] recvBuf, out UInt32 recvDataSize)
    {
        int bufferSize = recvBuf.Length;
        GCHandle handle = GCHandle.Alloc(recvBuf, GCHandleType.Pinned);
        PiaPlugin.Result result = ReceiveReliableNative(port, out srcConstantId, handle.AddrOfPinnedObject(), out recvDataSize, (UInt32)bufferSize);
        handle.Free();
        return result;
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportReliable_SetThroughputLimit(UInt16 port, UInt32 throughputLimit);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportReliable_SetThroughputLimit")]
#else
    [DllImport("__Internal", EntryPoint = "TransportReliable_SetThroughputLimit")]
#endif
    private static extern void SetReliableThroughputLimitNative(UInt16 port, UInt32 throughputLimit);
#endif

/*!
        @brief  Set the maximum amount of data that this protocol sends per call to the <tt>PiaPlugin.Dispatch</tt> function.
        @details  The average amount of data sent per call to <tt>PiaPlugin.Dispatch</tt> by this protocol is limited to less than or equal to the value set with this function.
                                   Data may still momentarily be sent at rates greater than specified.
                                   The amount of data sent via Internet communication using broadcasting counts as the amount sent for one station.
                                   <tt><var>throughputLimit</var></tt> for the sender and receiver do not need to match.
                                   The maximum value that can be set is @ref ThroughputLimitMax. If a value larger than this maximum is set, it is treated as @ref ThroughputLimitMax.
                                   If this function is not called, the default value is @ref ThroughputLimitMax.
        @param[in] port  The port number specifying the ID of the created  ReliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] throughputLimit  The maximum amount of send data to configure (in bytes).
*/
        public static void SetThroughputLimit(UInt16 port, UInt32 throughputLimit)
        {
            SetReliableThroughputLimitNative(port, throughputLimit);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportReliable_ReadySend(UInt16 port, UInt64 destConstantId, UInt32 dataSize);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportReliable_ReadySend")]
#else
    [DllImport("__Internal", EntryPoint = "TransportReliable_ReadySend")]
#endif
    private static extern PiaPlugin.Result ReadySendReliableNative(UInt16 port, UInt64 destConstantId, UInt32 dataSize);
#endif


/*!
        @brief  Checks whether this state is a ready state for sending by <tt>ReliableProtocol</tt>.
        @param[in] port  The port number specifying the ID of the created  ReliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] destConstantId  The ID of the station to send to.
        @param[in] dataSize  The amount of data planned to be sent.
        @return  If sending is possible in this state, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidState  The setup for <tt>session</tt> might not have been completed. [:progErr]
        @retval ResultInvalidArgument  An argument is not valid. [:progErr]
        @retval ResultNotInCommunication  The local station is not in communication with the specified recipient. [:handling]
        @retval ResultBufferIsFull  There is not enough space in the send buffer. If this happens frequently, consider increasing the number of send buffers specified by the <tt>PiaPlugin.InitializeTransportSetting()</tt> function. [:handling]
        @retval ResultTemporaryUnavailable  The API is temporarily unavailable because the session is migrating. (Only returned when the joint session feature is being used.) [:handling]
        @retval ResultNotFound  The specified destination was not found. [:handling]
*/

        public static PiaPlugin.Result ReadySend(UInt16 port, UInt64 destConstantId, UInt32 dataSize)
        {
            PiaPlugin.Result result = ReadySendReliableNative(port, destConstantId, dataSize);
            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportReliable_ReadyReceive(UInt16 port);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportReliable_ReadyReceive")]
#else
    [DllImport("__Internal", EntryPoint = "TransportReliable_ReadyReceive")]
#endif
    private static extern PiaPlugin.Result ReadyReceiveReliableNative(UInt16 port);
#endif


/*!
        @brief  Checks whether this state is a ready state for sending by <tt>ReliableProtocol</tt>.
        @param[in] port  The port number specifying the ID of the created  ReliableProtocol. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @return  If receiving is possible in this state, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidState  Not initialized. [:progErr]
        @retval ResultNoData  No data was received. [:handling]
*/

        public static PiaPlugin.Result ReadyReceive(UInt16 port)
        {
            PiaPlugin.Result result = ReadyReceiveReliableNative(port);
            return result;
        }
    }


// -----------------------------------------------------------------------
//! @brief  This class is a collection of the features that use <tt>BroadcastReliableProtocol</tt>.
// -----------------------------------------------------------------------

public class BroadcastReliable
    {
#if UNITY_EDITOR
        static TransportBroadcastReliable_Send SendBroadcastReliableNative;
        static TransportBroadcastReliable_SendToAll SendToAllBroadcastReliableNative;
        static TransportBroadcastReliable_Receive ReceiveBroadcastReliableNative;
        static TransportBroadcastReliable_SetThroughputLimit SetBroadcastReliableThroughputLimitNative;
        static TransportBroadcastReliable_ReadySend ReadySendBroadcastReliableNative;
        static TransportBroadcastReliable_ReadySendToAll ReadySendToAllBroadcastReliableNative;
        static TransportBroadcastReliable_ReadyReceive ReadyReceiveBroadcastReliableNative;

        public static void InitializeHooks(IntPtr? plugin_dll)
        {
            IntPtr pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportBroadcastReliable_Send");
            SendBroadcastReliableNative = (TransportBroadcastReliable_Send)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportBroadcastReliable_Send));
            PiaPluginUtil.UnityLog("InitializeHooks " + SendBroadcastReliableNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportBroadcastReliable_SendToAll");
            SendToAllBroadcastReliableNative = (TransportBroadcastReliable_SendToAll)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportBroadcastReliable_SendToAll));
            PiaPluginUtil.UnityLog("InitializeHooks " + SendToAllBroadcastReliableNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportBroadcastReliable_Receive");
            ReceiveBroadcastReliableNative = (TransportBroadcastReliable_Receive)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportBroadcastReliable_Receive));
            PiaPluginUtil.UnityLog("InitializeHooks " + ReceiveBroadcastReliableNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportBroadcastReliable_SetThroughputLimit");
            SetBroadcastReliableThroughputLimitNative = (TransportBroadcastReliable_SetThroughputLimit)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportBroadcastReliable_SetThroughputLimit));
            PiaPluginUtil.UnityLog("InitializeHooks " + SetBroadcastReliableThroughputLimitNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportBroadcastReliable_ReadySend");
            ReadySendBroadcastReliableNative = (TransportBroadcastReliable_ReadySend)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportBroadcastReliable_ReadySend));
            PiaPluginUtil.UnityLog("InitializeHooks " + ReadySendBroadcastReliableNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportBroadcastReliable_ReadySendToAll");
            ReadySendToAllBroadcastReliableNative = (TransportBroadcastReliable_ReadySendToAll)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportBroadcastReliable_ReadySendToAll));
            PiaPluginUtil.UnityLog("InitializeHooks " + ReadySendToAllBroadcastReliableNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportBroadcastReliable_ReadyReceive");
            ReadyReceiveBroadcastReliableNative = (TransportBroadcastReliable_ReadyReceive)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportBroadcastReliable_ReadyReceive));
            PiaPluginUtil.UnityLog("InitializeHooks " + ReadyReceiveBroadcastReliableNative);
        }
#endif

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportBroadcastReliable_Send(UInt16 port, UInt64 destConstantId, IntPtr pData, UInt32 dataSize);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportBroadcastReliable_Send")]
#else
    [DllImport("__Internal", EntryPoint = "TransportBroadcastReliable_Send")]
#endif
    private static extern PiaPlugin.Result SendBroadcastReliableNative(UInt16 port, UInt64 destConstantId, IntPtr pData, UInt32 dataSize);
#endif
/*!
        @brief  Uses <tt>BroadcastReliableProtocol</tt> to send a byte array of data to a specific station.

        @param[in] port  The port number that specifies the ID of the <tt>BroadcastReliableProtocol</tt> that is created. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] destConstantId  The station ID of the station to send to.
        @param[in] data  Byte array data to send.
        @param[in] dataSize  Size of the data to send. The value is in bytes.

        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.

        @retval ResultInvalidState  Pia initialization may not have been performed. [:progErr]
        @retval ResultInvalidArgument  An argument is not valid. [:progErr]
        @retval ResultBufferIsFull  There is not enough space in the send buffer. If this occurs frequently, consider increasing the buffer count for send packets, specified by <tt>PiaPlugin.InitializeTransportSetting()</tt>. [:handling]
*/
        public static PiaPlugin.Result Send(UInt16 port, UInt64 destConstantId, byte[] data, UInt32 dataSize)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            PiaPlugin.Result result = SendBroadcastReliableNative(port, destConstantId, handle.AddrOfPinnedObject(), dataSize);
            handle.Free();

            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportBroadcastReliable_SendToAll(UInt16 port, IntPtr pData, UInt32 dataSize);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportBroadcastReliable_SendToAll")]
#else
    [DllImport("__Internal", EntryPoint = "TransportBroadcastReliable_SendToAll")]
#endif
    private static extern PiaPlugin.Result SendToAllBroadcastReliableNative(UInt16 port, IntPtr pData, UInt32 dataSize);
#endif
/*!
        @brief  Uses <tt>BroadcastReliableProtocol</tt> to send byte array data to all stations.

        @param[in] port  The port number that specifies the ID of the <tt>BroadcastReliableProtocol</tt> that is created. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] data  Byte array data to send.
        @param[in] dataSize  Size of the data to send. The value is in bytes.

        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.

        @retval ResultInvalidState  Pia initialization may not have been performed. [:progErr]
        @retval ResultInvalidArgument  An argument is not valid. [:progErr]
        @retval ResultBufferIsFull  There is not enough space in the send buffer. If this occurs frequently, consider increasing the buffer count for send packets, specified by <tt>PiaPlugin.InitializeTransportSetting()</tt>. [:handling]
*/
        public static PiaPlugin.Result SendToAll(UInt16 port, byte[] data, UInt32 dataSize)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            PiaPlugin.Result result = SendToAllBroadcastReliableNative(port, handle.AddrOfPinnedObject(), dataSize);
            handle.Free();

            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportBroadcastReliable_Receive(UInt16 port, [Out] out UInt64 srcConstantId, IntPtr pRecvBuf, [Out] out UInt32 recvDataSize, UInt32 recvBufSize);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportBroadcastReliable_Receive")]
#else
    [DllImport("__Internal", EntryPoint = "TransportBroadcastReliable_Receive")]
#endif
    private static extern PiaPlugin.Result ReceiveBroadcastReliableNative(UInt16 port, [Out] out UInt64 srcConstantId, IntPtr pRecvBuf, [Out] out UInt32 recvDataSize, UInt32 recvBufSize);
#endif

/*!
        @brief  Uses <tt>BroadcastReliableProtocol</tt> to receive the byte array sent using @ref Send.

        @param[in] port  The port number that specifies the ID of the <tt>BroadcastReliableProtocol</tt> that is created. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[out] srcConstantId  The station ID of the station that sent the received data.
        @param[out] recvBuf  Buffer for storing received data.
        @param[out] recvDataSize  The size of the received data. The value is in bytes.

        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.

        @retval ResultInvalidState  Not initialized. [:progErr]
        @retval ResultInvalidArgument  An argument is not valid. [:progErr]
        @retval ResultNoData  No data was received. [:handling]
        @retval ResultBufferShortage  The received data is too large to fit into the receive buffer.
        This result might occur when invalid data is received, such as when a sender is cheating, in which case it requires handling at run time.
        Revise the program so this result is not returned when the sender behavior is appropriate. [:handling]
        @retval ResultAllocationFailed  Failed to allocate resources such as memory. [:progErr]
*/
        public static PiaPlugin.Result Receive(UInt16 port, out UInt64 srcConstantId, byte[] recvBuf, out UInt32 recvDataSize)
        {
            int bufferSize = recvBuf.Length;
            GCHandle handle = GCHandle.Alloc(recvBuf, GCHandleType.Pinned);
            PiaPlugin.Result result = ReceiveBroadcastReliableNative(port, out srcConstantId, handle.AddrOfPinnedObject(), out recvDataSize, (UInt32)bufferSize);
            handle.Free();
            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportBroadcastReliable_SetThroughputLimit(UInt16 port, UInt32 throughputLimit);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportBroadcastReliable_SetThroughputLimit")]
#else
    [DllImport("__Internal", EntryPoint = "TransportBroadcastReliable_SetThroughputLimit")]
#endif
    private static extern void SetBroadcastReliableThroughputLimitNative(UInt16 port, UInt32 throughputLimit);
#endif

/*!
        @brief  Set the maximum amount of data that this protocol sends per call to the <tt>PiaPlugin.Dispatch</tt> function.
        @details  The average amount of data sent per call to <tt>PiaPlugin.Dispatch</tt> by this protocol is limited to less than or equal to the value set with this function.
                                   Data may still momentarily be sent at rates greater than specified.
                                   The amount of data sent via Internet communication using broadcasting counts as the amount sent for one station.
                                   <tt><var>throughputLimit</var></tt> for the sender and receiver do not need to match.
                                   The maximum value that can be set is @ref ThroughputLimitMax. If a value larger than this maximum is set, it is treated as @ref ThroughputLimitMax.
                                   If this function is not called, the default value is @ref ThroughputLimitMax.
        @param[in] port  The port number that specifies the ID of the <tt>BroadcastReliableProtocol</tt> that is created. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] throughputLimit  The maximum amount of send data to configure (in bytes).
*/
        public static void SetThroughputLimit(UInt16 port, UInt32 throughputLimit)
        {
            SetBroadcastReliableThroughputLimitNative(port, throughputLimit);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportBroadcastReliable_ReadySend(UInt16 port, UInt64 destConstantId, UInt32 dataSize);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportBroadcastReliable_ReadySend")]
#else
    [DllImport("__Internal", EntryPoint = "TransportBroadcastReliable_ReadySend")]
#endif
    private static extern PiaPlugin.Result ReadySendBroadcastReliableNative(UInt16 port, UInt64 destConstantId, UInt32 dataSize);
#endif
/*!
        @brief  Checks whether this state is a ready state for sending by <tt>BroadcastReliableProtocol</tt>.

        @param[in] port  The port number that specifies the ID of the <tt>BroadcastReliableProtocol</tt> that is created. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] destConstantId  The ID of the station to send to.
        @param[in] dataSize  The amount of data planned to be sent.
        @return  If sending is possible in this state, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidState  <tt>ReliableProtocol</tt> might not have been initialized, or the session setup might not have been completed. [:progErr]
        @retval ResultInvalidArgument  An argument is not valid. [:progErr]
        @retval ResultNotInCommunication  Stations are not communicating. [:handling]
        @retval ResultNotFound  The specified destination was not found. [:handling]
        @retval ResultBufferIsFull  There is not enough space in the send buffer. If this happens frequently, consider increasing the number of send buffers specified by the <tt>PiaPlugin.InitializeTransportSetting()</tt> function. [:handling]
*/
        public static PiaPlugin.Result ReadySend(UInt16 port, UInt64 destConstantId, UInt32 dataSize)
        {
            PiaPlugin.Result result = ReadySendBroadcastReliableNative(port, destConstantId, dataSize);
            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportBroadcastReliable_ReadySendToAll(UInt16 port, UInt32 dataSize);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportBroadcastReliable_ReadySendToAll")]
#else
    [DllImport("__Internal", EntryPoint = "TransportBroadcastReliable_ReadySendToAll")]
#endif
    private static extern PiaPlugin.Result ReadySendToAllBroadcastReliableNative(UInt16 port, UInt32 dataSize);
#endif
/*!
        @brief  Checks whether this state is a ready state for sending by <tt>BroadcastReliableProtocol</tt>.

        @param[in] port  The port number that specifies the ID of the <tt>BroadcastReliableProtocol</tt> that is created. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] dataSize  The amount of data planned to be sent.
        @return  If sending is possible in this state, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidState  The setup for <tt>session</tt> might not have been completed. [:progErr]
        @retval ResultBufferIsFull  There is not enough space in the send buffer. If this happens frequently, consider increasing the number of send buffers specified by the <tt>PiaPlugin.InitializeTransportSetting()</tt> function. [:handling]
*/
        public static PiaPlugin.Result ReadySendToAll(UInt16 port, UInt32 dataSize)
        {
            PiaPlugin.Result result = ReadySendToAllBroadcastReliableNative(port, dataSize);
            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportBroadcastReliable_ReadyReceive(UInt16 port);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportBroadcastReliable_ReadyReceive")]
#else
    [DllImport("__Internal", EntryPoint = "TransportBroadcastReliable_ReadyReceive")]
#endif
    private static extern PiaPlugin.Result ReadyReceiveBroadcastReliableNative(UInt16 port);
#endif
/*!
        @brief  Checks whether this state is a ready state for receiving by <tt>BroadcastReliableProtocol</tt>.
        @param[in] port  The port number that specifies the ID of the <tt>BroadcastReliableProtocol</tt> that is created. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @return  If receiving is possible in this state, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidState  Not initialized. [:progErr]
        @retval ResultNoData  No data was received. [:handling]
*/
        public static PiaPlugin.Result ReadyReceive(UInt16 port)
        {
            PiaPlugin.Result result = ReadyReceiveBroadcastReliableNative(port);
            return result;
        }
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result Transport_SetEmulationSetting([In, MarshalAs(UnmanagedType.LPStruct)] EmulationSetting setting);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Transport_SetEmulationSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Transport_SetEmulationSetting")]
#endif
    private static extern PiaPlugin.Result SetEmulationSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] EmulationSetting setting);
#endif

/*!
    @brief  Configures settings for latency emulation and packet loss emulation. Fails in the Release build.

    @param[in] setting  The setting for latency emulation and packet loss emulation.

    @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.

    @retval ResultInvalidState  Pia initialization may not have been performed. Or it could be a release build. [:progErr]
    @retval ResultInvalidArgument  An argument is not valid. [:progErr]
*/
    public static PiaPlugin.Result SetEmulationSetting(EmulationSetting setting)
    {
        PiaPlugin.Result result = SetEmulationSettingNative(setting);
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result Transport_UpdateTransportAnalyzer();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Transport_UpdateTransportAnalyzer")]
#else
    [DllImport("__Internal", EntryPoint = "Transport_UpdateTransportAnalyzer")]
#endif
    private static extern PiaPlugin.Result UpdateTransportAnalyzerNative();
#endif

/*!
     @brief  Updates the transport analyzer's data (for debugging).

     @details  This function is for debugging. Do not include it in retail products. The application does not need to call this function if a positive value
     has been set for the <tt><var>measurementInterval</var></tt> member variable of the <tt>PiaPlugin.InitializeTransportSetting</tt> class, and
     automatic printing of the analysis data has been enabled.
     When the application gets the analysis data with the <tt><var>measurementInterval</var></tt> member
     variable set to <tt>0</tt> and the automatic printing feature disabled,
     you must explicitly call this function and update the data.

     @return  If the process is successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>. You must make sure that the implementation of this function in your application does not return any errors.
     @retval ResultNotInitialized  The <tt>TransportAnalyzer</tt> instance is not initialized. [:progErr]
     @retval ResultInvalidState  The function was called at the wrong time. [:progErr]
*/
    public static PiaPlugin.Result UpdateTransportAnalyzer()
    {
        return UpdateTransportAnalyzerNative();
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate TransportAnalysisData Transport_GetTransportAnalysisData();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Transport_GetTransportAnalysisData")]
#else
    [DllImport("__Internal", EntryPoint = "Transport_GetTransportAnalysisData")]
#endif
    private static extern TransportAnalysisData GetTransportAnalysisDataNative();
#endif

/*!
     @cond  PRIVATE
     @brief  Gets the transport analyzer's data (for debugging).
     @details  This function is for debugging. Do not include it in retail products.
     @return  The result of the transport analyzer.
*/
    public static TransportAnalysisData GetTransportAnalysisDataOld()
    {
        return  GetTransportAnalysisDataNative();
    }
    //! @endcond

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void Transport_GetTransportAnalysisData2([MarshalAs(UnmanagedType.LPStruct)] IntPtr packetAnalysisData);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Transport_GetTransportAnalysisData2")]
#else
    [DllImport("__Internal", EntryPoint = "Transport_GetTransportAnalysisData2")]
#endif
    private static extern void GetTransportAnalysisDataNative2([ MarshalAs(UnmanagedType.LPStruct)] IntPtr transportAnalysisData);
#endif

/*!
     @brief  Gets the transport analyzer's data (for debugging).
     @details  This function is for debugging. Do not include it in retail products.
     @return  The result of the transport analyzer.
*/
    public static TransportAnalysisData GetTransportAnalysisData()
    {
        TransportAnalysisData transportAnalysisData;
        int size = Marshal.SizeOf(typeof(TransportAnalysisData));
        IntPtr ptr = Marshal.AllocHGlobal(size);
        GetTransportAnalysisDataNative2(ptr);
        transportAnalysisData = (TransportAnalysisData)Marshal.PtrToStructure(ptr, typeof(TransportAnalysisData));
        Marshal.FreeHGlobal(ptr);
        return transportAnalysisData;
    }

    // -----------------------------------------------------------------------
    //! @brief  Class that is a compilation of the features that use <tt>StreamBroadcastReliableProtocol</tt>.
    // -----------------------------------------------------------------------

    public class StreamBroadcastReliable
    {

/*!
        @brief  Enumerates the operational states.
*/
        public enum State : byte
        {
            None = 0,         //!<  Currently doing nothing.
            Sending,          //!<  Sending.
            WaitingSendAck,   //!<  The state where the data that needs to be sent has already been sent, and now waiting for all ACK to be returned.
            SendSuccess,      //!<  Sending was successful.
            WaitingReceive,   //!<  Waiting to receive.
            Receiving,        //!<  Receiving.
            ReceiveSuccess,   //!<  Receiving was successful.
            RequestFailure,   //!<  The send request failed.
            CancelingSend,    //!<  Canceling send.
            CancelingRequest, //!<  Canceling the send request.
            WaitingCancelAck, //!<  Waiting for the ACK to the cancel.
            CancelSuccess,    //!<  The send or send request was canceled.
            Failure           //!<  Sending failed due to leaving the session or a cancellation notification.
        };


#if UNITY_EDITOR
        static TransportStream_Request Stream_RequestNative;
        static TransportStream_IsRequested Stream_IsRequestedNative;
        static TransportStream_StartSend Stream_StartSendNative;
        static TransportStream_Cancel Stream_CancelNative;
        static TransportStream_GetState Stream_GetStateNative;
        static TransportStream_IsRunning Stream_IsRunningNative;
        static TransportStream_GetProgress Stream_GetProgressNative;
        static TransportStream_SetThroughputLimit Stream_SetThroughputLimitNative;
        static TransportStream_ReadyRequest Stream_ReadyRequestNative;
        static TransportStream_ReadyStartSend Stream_ReadyStartSendNative;

        public static void InitializeHooks(IntPtr? plugin_dll)
        {
            IntPtr pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportStream_Request");
            Stream_RequestNative = (TransportStream_Request)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportStream_Request));
            PiaPluginUtil.UnityLog("InitializeHooks " + Stream_RequestNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportStream_IsRequested");
            Stream_IsRequestedNative = (TransportStream_IsRequested)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportStream_IsRequested));
            PiaPluginUtil.UnityLog("InitializeHooks " + Stream_IsRequestedNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportStream_StartSend");
            Stream_StartSendNative = (TransportStream_StartSend)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportStream_StartSend));
            PiaPluginUtil.UnityLog("InitializeHooks " + Stream_StartSendNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportStream_Cancel");
            Stream_CancelNative = (TransportStream_Cancel)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportStream_Cancel));
            PiaPluginUtil.UnityLog("InitializeHooks " + Stream_CancelNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportStream_GetState");
            Stream_GetStateNative = (TransportStream_GetState)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportStream_GetState));
            PiaPluginUtil.UnityLog("InitializeHooks " + Stream_GetStateNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportStream_IsRunning");
            Stream_IsRunningNative = (TransportStream_IsRunning)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportStream_IsRunning));
            PiaPluginUtil.UnityLog("InitializeHooks " + Stream_IsRunningNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportStream_GetProgress");
            Stream_GetProgressNative = (TransportStream_GetProgress)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportStream_GetProgress));
            PiaPluginUtil.UnityLog("InitializeHooks " + Stream_GetProgressNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportStream_SetThroughputLimit");
            Stream_SetThroughputLimitNative = (TransportStream_SetThroughputLimit)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportStream_SetThroughputLimit));
            PiaPluginUtil.UnityLog("InitializeHooks " + Stream_SetThroughputLimitNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportStream_ReadyRequest");
            Stream_ReadyRequestNative = (TransportStream_ReadyRequest)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportStream_ReadyRequest));
            PiaPluginUtil.UnityLog("InitializeHooks " + Stream_ReadyRequestNative);

            pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "TransportStream_ReadyStartSend");
            Stream_ReadyStartSendNative = (TransportStream_ReadyStartSend)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TransportStream_ReadyStartSend));
        }

#endif

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportStream_Request(UInt16 port, UInt64 destinationConstantId, IntPtr pBuffer, UInt32 bufferSize, byte id);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportStream_Request")]
#else
    [DllImport("__Internal", EntryPoint = "TransportStream_Request")]
#endif
    private static extern PiaPlugin.Result Stream_RequestNative(UInt16 port, UInt64 destinationConstantId, IntPtr pBuffer, UInt32 bufferSize, byte id);
#endif
/*!
        @brief  Requests that data be sent with the specified <tt><var>id</var></tt>.
        @details  If this function succeeds, the state transitions to @ref State.WaitingReceive.
        @param[in] port  The port number specifying the ID of the created <tt>StreamBroadcastReliableProtocol</tt>. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] destinationConstantId  Station the data is being received from.
        @param[out] pBuffer  Buffer that receives the data.
                                         The data cannot be accessed until <tt>GetState()</tt> becomes @ref State.ReceiveSuccess.
        @param[in] id  The data is received if the value of this parameter matches the <tt><var>id</var></tt> specified by @ref StartSend.
        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidState  <tt>@ref IsRunning</tt> is <tt>true</tt>. [:progErr]
        @retval ResultInvalidArgument  An argument is not valid. [:progErr]
        @retval ResultBufferIsFull  There is not enough space in the send buffer. [:handling]
        @retval ResultNotInCommunication  Stations are not communicating. [:handling]
*/

        public static PiaPlugin.Result Request(UInt16 port, UInt64 destinationConstantId, byte[] pBuffer, byte id)
        {
            Int32 bufferSize = pBuffer.Length;
            GCHandle handle = GCHandle.Alloc(pBuffer, GCHandleType.Pinned);
            PiaPlugin.Result result = Stream_RequestNative(port, destinationConstantId, handle.AddrOfPinnedObject(), (UInt32)bufferSize, id);
            handle.Free();
            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool TransportStream_IsRequested(UInt16 port, UInt64 constantId, byte id);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportStream_IsRequested")]
#else
    [DllImport("__Internal", EntryPoint = "TransportStream_IsRequested")]
#endif
    private static extern bool Stream_IsRequestedNative(UInt16 port, UInt64 constantId, byte id);
#endif

/*!
        @brief  Determines whether a request has been received from the specified station.
        @param[in] port  The port number specifying the ID of the created <tt>StreamBroadcastReliableProtocol</tt>. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] constantId  The ID of the station to check for a request.
        @param[in] id  The ID to check for a request.
        @return  Returns <tt>true</tt> if a request has been received form the specified station.
*/

        public static bool IsRequested(UInt16 port, UInt64 constantId, byte id)
        {
            return Stream_IsRequestedNative(port, constantId, id);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportStream_StartSend(UInt16 port, IntPtr cpData, UInt32 dataSize, byte id);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportStream_StartSend")]
#else
    [DllImport("__Internal", EntryPoint = "TransportStream_StartSend")]
#endif
    private static extern PiaPlugin.Result Stream_StartSendNative(UInt16 port, IntPtr cpData,  UInt32 dataSize, byte id);
#endif
/*!
        @brief  Starts sending data.
        @details  Sends data to the station the request was received from.
                 If this function succeeds, the state transitions to @ref State.Sending.
        @param[in] port  The port number specifying the ID of the created <tt>StreamBroadcastReliableProtocol</tt>. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] data  Pointer to the data being sent.
                             The specified buffer must be retained until <tt>GetState()</tt> becomes @ref State.ReceiveSuccess.
        @param[in] dataSize  Size of the data to send.
        @param[in] id  The ID of the data to send.
        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidState  <tt>@ref IsRunning</tt> is <tt>true</tt>. [:progErr]
        @retval ResultInvalidArgument  An argument is not valid. [:progErr]
*/

        public static PiaPlugin.Result StartSend(UInt16 port, byte[] data, UInt32 dataSize, byte id)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            PiaPlugin.Result result = Stream_StartSendNative(port, handle.AddrOfPinnedObject(), dataSize, id);
            handle.Free();

            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportStream_Cancel(UInt16 port);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportStream_Cancel")]
#else
    [DllImport("__Internal", EntryPoint = "TransportStream_Cancel")]
#endif
    private static extern PiaPlugin.Result Stream_CancelNative(UInt16 port);
#endif
/*!
        @brief  Suspends the send or send request.
        @details  If this function succeeds, <tt>GetState()</tt> transitions to @ref State.CancelingSend or @ref State.CancelingRequest.
        @param[in] port  The port number specifying the ID of the created <tt>StreamBroadcastReliableProtocol</tt>. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @return  If successful, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidState  @ref GetState() is some state other than @ref State.Sending or @ref State.WaitingReceive. [:progErr]
*/

        public static PiaPlugin.Result Cancel(UInt16 port)
        {
            PiaPlugin.Result result = Stream_CancelNative(port);

            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate State TransportStream_GetState(UInt16 port);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportStream_GetState")]
#else
    [DllImport("__Internal", EntryPoint = "TransportStream_GetState")]
#endif
    private static extern State Stream_GetStateNative(UInt16 port);
#endif

/*!
        @brief  Gets the current operational state.
        @details  If this function succeeds, <tt>GetState()</tt> transitions to @ref State.CancelingSend or @ref State.CancelingRequest.
        @param[in] port  The port number specifying the ID of the created <tt>StreamBroadcastReliableProtocol</tt>. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @return  Returns the current operational state.
*/

        public static State GetState(UInt16 port)
        {
            return Stream_GetStateNative(port);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool TransportStream_IsRunning(UInt16 port);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportStream_IsRunning")]
#else
    [DllImport("__Internal", EntryPoint = "TransportStream_IsRunning")]
#endif
    private static extern bool Stream_IsRunningNative(UInt16 port);
#endif

/*!
        @brief  Determines whether asynchronous send and receive operations are currently executing.
        @param[in] port  The port number specifying the ID of the created <tt>StreamBroadcastReliableProtocol</tt>. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @return  Returns <tt>true</tt> if asynchronous processing of send and receive operations is in progress.
        @see  GetState
*/

        public static bool IsRunning(UInt16 port)
        {
            return Stream_IsRunningNative(port);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte TransportStream_GetProgress(UInt16 port);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportStream_GetProgress")]
#else
    [DllImport("__Internal", EntryPoint = "TransportStream_GetProgress")]
#endif
    private static extern byte Stream_GetProgressNative(UInt16 port);
#endif

/*!
        @brief  Gets the progress status of the send/receive operation.
        @param[in] port  The port number specifying the ID of the created <tt>StreamBroadcastReliableProtocol</tt>. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @return  Returns a value between <tt>0</tt> and <tt>100</tt> representing the percentage of completion of sending and receiving.
*/
        public static byte GetProgress(UInt16 port)
        {
            return Stream_GetProgressNative(port);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportStream_SetThroughputLimit(UInt16 port, UInt32 throughputLimit);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportStream_SetThroughputLimit")]
#else
    [DllImport("__Internal", EntryPoint = "TransportStream_SetThroughputLimit")]
#endif
    private static extern void Stream_SetThroughputLimitNative(UInt16 port, UInt32 throughputLimit);
#endif

/*!
        @brief  Set the maximum amount of data that this protocol sends per call to the <tt>PiaPlugin.Dispatch</tt> function.
        @details  The average amount of data sent per call to <tt>PiaPlugin.Dispatch</tt> by this protocol is limited to less than or equal to the value set with this function.
                                   Data may still momentarily be sent at rates greater than specified.
                                   The amount of data sent via Internet communication using broadcasting counts as the amount sent for one station.
                                   <tt><var>throughputLimit</var></tt> for the sender and receiver do not need to match.
                                   The maximum value that can be set is @ref ThroughputLimitMax. If a value larger than this maximum is set, it is treated as @ref ThroughputLimitMax.
                                   If this function is not called, the default value is @ref ThroughputLimitMax.
        @param[in] port  The port number specifying the ID of the created <tt>StreamBroadcastReliableProtocol</tt>. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] throughputLimit  The maximum amount of send data to configure (in bytes).
*/
        public static void SetThroughputLimit(UInt16 port, UInt32 throughputLimit)
        {
            Stream_SetThroughputLimitNative(port, throughputLimit);
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportStream_ReadyRequest(UInt16 port, UInt64 destinationConstantId);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportStream_ReadyRequest")]
#else
    [DllImport("__Internal", EntryPoint = "TransportStream_ReadyRequest")]
#endif
    private static extern PiaPlugin.Result Stream_ReadyRequestNative(UInt16 port, UInt64 destinationConstantId);
#endif
/*!
        @brief  Checks whether this state is a ready state for a request.
        @param[in] port  The port number specifying the ID of the created <tt>StreamBroadcastReliableProtocol</tt>. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @param[in] destinationConstantId  The ID of the station for the planned request.
        @return  If a request is possible in this state, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidState  @ref IsRunning() might be <tt>true</tt>. [:progErr]
        @retval ResultInvalidArgument  An argument is not valid. [:progErr]
        @retval ResultBufferIsFull  There is not enough space in the send buffer. [:handling]
        @retval ResultNotInCommunication  Stations are not communicating. [:handling]
        @retval ResultNotFound  The specified destination was not found. [:handling]
*/

        public static PiaPlugin.Result ReadyRequest(UInt16 port, UInt64 destinationConstantId)
        {
            PiaPlugin.Result result = Stream_ReadyRequestNative(port, destinationConstantId);
            return result;
        }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result TransportStream_ReadyStartSend(UInt16 port);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "TransportStream_ReadyStartSend")]
#else
    [DllImport("__Internal", EntryPoint = "TransportStream_ReadyStartSend")]
#endif
    private static extern PiaPlugin.Result Stream_ReadyStartSendNative(UInt16 port);
#endif
/*!
        @brief  Checks whether this state is a ready state for @ref StartSend().
        @param[in] port  The port number specifying the ID of the created <tt>StreamBroadcastReliableProtocol</tt>. Specify a value in the range from <tt>0</tt> to (<em>number created</em> - 1).
        @return  If @ref StartSend() is possible in this state, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> is <tt>true</tt>.
        @retval ResultInvalidState  @ref IsRunning() might be <tt>true</tt>. [:progErr]
*/

        public static PiaPlugin.Result ReadyStartSend(UInt16 port)
        {
            PiaPlugin.Result result = Stream_ReadyStartSendNative(port);
            return result;
        }
    } //StreamBroadcastReliable
}
