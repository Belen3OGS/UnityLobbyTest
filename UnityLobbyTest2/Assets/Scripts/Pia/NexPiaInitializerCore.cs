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


// Whether to set the IP address manually.
//#define SET_LAN_LOCAL_ADDRESS_MANUAL

// Whether to use the NAT traversal failure emulation feature.
//#define NAT_TRAVERSAL_FAILURE_TEST

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

// -----------------------------------------------------------------------
//! @brief  Classes that execute initialization processing for each module.
// -----------------------------------------------------------------------
public class NexPiaInitializerCore : MonoBehaviour
{
    public enum LocalMultiPlayType
    {
        None = 0,        //!<  Only one player is joining from the same terminal.
        Participants,    //!<  Multiple players are joining a matchmaking session from the same terminal.
        Stations,        //!<  Multiple players are joining a matchmaking session from the same terminal, which is only counted once, however, within the matchmaking session.
        MultiLogin,      //!<  Multiple players are logged in and joining a matchmaking session from the same terminal. (Not implemented.)
        Max
    };

    private bool m_IsSetNetworkType = false;
    private int frameCount;
    private PiaPlugin.NetworkType s_NetworkType;
    private const UInt64 CommunicationId = 0x0100289000012000; // ID for local communication and LAN communication.


#if UNITY_ONLY_SWITCH
    private const byte NexVersionMajor = 4;
    private const byte NexVersionMinor = 5;
    private const byte NexVersionMicro = 0;
#endif

    //    PiaPlugin.CheckNatSetting settings;
    //    int[] buf;

    void Start()
    {
        frameCount = 0;
    }

    void Update()
    {
        ++frameCount;
    }

/*!
    @brief  Initializes <tt>PiaFramework</tt> and registers the default parameters for modules.
*/
    public PiaPlugin.Result InitializeFramework(bool isAutoPrintTransportAnalysisData = true, LocalMultiPlayType type = LocalMultiPlayType.None)
    {
        PiaPlugin.Result result;

        PiaPlugin.InitializeFrameworkSetting initializeFrameworkSetting = new PiaPlugin.InitializeFrameworkSetting();
        PiaPlugin.InitializeInternetSetting initializeInternetSetting = new PiaPlugin.InitializeInternetSetting();
        PiaPlugin.InitializeLanSetting initializeLanSetting = new PiaPlugin.InitializeLanSetting();
        PiaPlugin.InitializeLocalSetting initializeLocalSetting = new PiaPlugin.InitializeLocalSetting();
        PiaPlugin.InitializeTransportSetting transportInitializeSetting = new PiaPlugin.InitializeTransportSetting();
        PiaPlugin.InitializeCloneSetting cloneInitializeSetting = new PiaPlugin.InitializeCloneSetting();
        PiaPlugin.InitializeSyncSetting syncInitializeSetting = new PiaPlugin.InitializeSyncSetting();
        PiaPlugin.InitializeReckoningSetting reckoningInitializeSetting = new PiaPlugin.InitializeReckoningSetting();
        PiaPlugin.InitializeSessionSetting initializeSessionSetting = new PiaPlugin.InitializeSessionSetting();
        PiaPlugin.StartupNetworkSetting startupNetworkSetting = new PiaPlugin.StartupNetworkSetting();
        PiaPlugin.StartupSessionSetting startupSessionSetting = new PiaPlugin.StartupSessionSetting();

#if NAT_TRAVERSAL_FAILURE_TEST
        PiaPlugin.NatDebugSetting natDebugSetting = new PiaPlugin.NatDebugSetting();
#endif


        // Add settings needed for PiaFramework initialization.
        {
            if (!m_IsSetNetworkType)
            {
                initializeFrameworkSetting.networkType = PiaPlugin.NetworkType.Internet;
            }

            initializeFrameworkSetting.piaBufferSize = 15 * 1024 * 1024;

            initializeFrameworkSetting.traceFlags
            .add(PiaPlugin.TraceFlag.Common)
            .add(PiaPlugin.TraceFlag.Cloud)
            .add(PiaPlugin.TraceFlag.Clone)
            .add(PiaPlugin.TraceFlag.Sync)
            .add(PiaPlugin.TraceFlag.Session)
            .add(PiaPlugin.TraceFlag.SessionMigration)
            .add(PiaPlugin.TraceFlag.Transport)
            .add(PiaPlugin.TraceFlag.TransportInit)
            .add(PiaPlugin.TraceFlag.Local)
            .add(PiaPlugin.TraceFlag.LocalMigration)
            .add(PiaPlugin.TraceFlag.Nex)
            .add(PiaPlugin.TraceFlag.Lan)
            .add(PiaPlugin.TraceFlag.Framework)
            .add(PiaPlugin.TraceFlag.Reckoning)
            .add(PiaPlugin.TraceFlag.Plugin);
            initializeFrameworkSetting.networkType = s_NetworkType;
            result = PiaPlugin.InitializeFramework(initializeFrameworkSetting);
            if (result.IsFailure())
            {
                return result;
            }
        }

        // Add settings needed for PiaNex initialization and logins.
        {
            initializeInternetSetting.emergencyMemorySize = 50 * 1024;

#if UNITY_ONLY_SWITCH
            initializeInternetSetting.gameId = 0x27851C01;
            initializeInternetSetting.accessKey = "12345678";
            initializeInternetSetting.userHandle = NNAccountSetup.UserHandle;
            initializeInternetSetting.totalMemorySize = (1024 * 1024) + initializeInternetSetting.emergencyMemorySize;
            initializeInternetSetting.nexVersionMajor = NexVersionMajor;
            initializeInternetSetting.nexVersionMinor = NexVersionMinor;
            initializeInternetSetting.nexVersionMicro = NexVersionMicro;
#endif
            PiaPlugin.RegisterInitializeInternetSetting(initializeInternetSetting);
        }

        // Register the settings required for initializing PiaLan.
        {
            initializeLanSetting.communicationId = CommunicationId;
            PiaPlugin.RegisterInitializeLanSetting(initializeLanSetting);
        }

        // Add settings needed for PiaLocal initialization.
        {
#if UNITY_SWITCH
            initializeLocalSetting.communicationId = CommunicationId;
#endif
            PiaPlugin.RegisterInitializeLocalSetting(initializeLocalSetting);
        }

        // Add settings needed for PiaTransport(PiaPluginTransport) initialization.
        {
            transportInitializeSetting.stationNumMax = 8;
            transportInitializeSetting.sendProtocolBufferNumPerStation = 8;
            transportInitializeSetting.receiveProtocolBufferNumPerStation = 8;
            transportInitializeSetting.sendThreadBufferNumPerStation = 8;
            transportInitializeSetting.receiveThreadBufferNumPerStation = 8;

            transportInitializeSetting.unreliableProtocolNum = 1;
            transportInitializeSetting.reliableProtocolNum = 1;
            transportInitializeSetting.broadcastReliableProtocolNum = 1;
            transportInitializeSetting.streamBroadcastReliableProtocolNum = 1;
            if (isAutoPrintTransportAnalysisData)
            {
                transportInitializeSetting.measurementInterval = 10;
                transportInitializeSetting.isAnalysisResultPrintEnabled = true;
            }
            else
            {
                transportInitializeSetting.measurementInterval = 0;
                transportInitializeSetting.isAnalysisResultPrintEnabled = false;
            }
            PiaPlugin.RegisterInitializeTransportSetting(transportInitializeSetting);
        }

        // Add settings needed for PiaClone(PiaPluginClone) initialization.
        {
            cloneInitializeSetting.isEnable = true;
            cloneInitializeSetting.broadcastEventProtocolNum = 3;
            cloneInitializeSetting.broadcastEventProtocolBufferNumPerStation = 48;
            cloneInitializeSetting.atomicProtocolIdMax = 32;

            PiaPlugin.RegisterInitializeCloneSetting(cloneInitializeSetting);
        }

        // Add settings needed for PiaSync(PiaPluginSync) initialization.
        {
            syncInitializeSetting.isEnable = true;
            // Specify whether to use the change delay feature while communication is synchronized.
            syncInitializeSetting.isChangeDelayEnabled = true;
            for (int i = 0; i < 4; ++i)
            {
                syncInitializeSetting.dataUnitSize[i] = 4;
            }
            for (int i = 4; i < syncInitializeSetting.dataUnitSize.Length; ++i)
            {
                syncInitializeSetting.dataUnitSize[i] = 12;
            }
            PiaPlugin.RegisterInitializeSyncSetting(syncInitializeSetting);
        }

        // Add settings needed for PiaSync(PiaPluginReckoning) initialization.
        {
            reckoningInitializeSetting.isEnable = true;
            reckoningInitializeSetting.reckoning3dProtocolNum = 1;
            reckoningInitializeSetting.reckoning1dProtocolNum = 1;
            reckoningInitializeSetting.reckoningProtocolBufferNum = 10;
            PiaPlugin.RegisterInitializeReckoningSetting(reckoningInitializeSetting);
        }

        // Add settings needed for PiaSession(PiaPluginSession) initialization.
        {
            initializeSessionSetting.networkTopology = PiaPluginSession.NetworkTopology.FullMesh;
            initializeSessionSetting.browsedSessionPropertyListNum = 4;

            PiaPlugin.RegisterInitializeSessionSetting(initializeSessionSetting);
        }

        // Add settings needed for the network interface and socket initialization.
        {
#if SET_LAN_LOCAL_ADDRESS_MANUAL
            startupNetworkSetting.isAutoInitializeNetworkInterface = false;
#else
            startupNetworkSetting.isAutoInitializeNetworkInterface = true;
#endif
            PiaPlugin.RegisterStartupNetworkSetting(startupNetworkSetting);
        }

        // Add parameters for starting up sessions.
        {
            int playernum = 1;
#if UNITY_ONLY_SWITCH
            switch (type)
            {
                case LocalMultiPlayType.None:
                    playernum = 1;
                    startupSessionSetting.isEachPlayerCountedAsParticipant = false;
                    break;
                case LocalMultiPlayType.Participants:
                    playernum = 2;
                    startupSessionSetting.isEachPlayerCountedAsParticipant = true;
                    break;
                case LocalMultiPlayType.Stations:
                    playernum = 2;
                    startupSessionSetting.isEachPlayerCountedAsParticipant = false;
                    break;
            }
#endif
            PiaPlugin.PlayerInfo[] playerInfo = new PiaPlugin.PlayerInfo[playernum];

            startupSessionSetting.relayRouteNumMax = 10;
            startupSessionSetting.relayRouteRttMax = 400;
            //            for (int i = 0; i < PiaPlugin.PlayerInfoSizeMax; i++)
            for (int i = 0; i < playerInfo.Length ; i++)
            {
                playerInfo[i] = new PiaPlugin.PlayerInfo();
#if UNITY_ONLY_SWITCH
                playerInfo[i].playerId = NNAccountSetup.Uid;
#endif
                playerInfo[i].nameStringLanguage = 0x1;
                playerInfo[i].playerName = "Sample";
            }
            startupSessionSetting.silenceTimeMax = 10000;
            startupSessionSetting.keepAliveSendingInterval = 500;
#if UNITY_ONLY_SWITCH
            startupSessionSetting.isAutoInitializeLdn = true;
#endif

            // Encryption settings.
            // This sample demo uses a simple key to show an example of the encryption settings.
            // We recommend, however, that applications use keys that are difficult to guess.
            PiaPlugin.CryptoSetting cryptoSetting = new PiaPlugin.CryptoSetting();
            byte[] cryptoKeyData = new byte[PiaPlugin.GetCryptoKeySize()];
            for (byte i = 0; i < cryptoKeyData.Length; ++i)
            {
                cryptoKeyData[cryptoKeyData.Length - 1 - i] = i;
            }
            SampleUtility.PinnedKey cryptoPinnedKey = new SampleUtility.PinnedKey(cryptoKeyData);
            cryptoSetting.mode = PiaPlugin.CryptoSetting.Mode.Aes128;
            cryptoSetting.pKeyData = cryptoPinnedKey.GetKeyDataPtr();

#if UNITY_EDITOR_OR_STANDALONE
            startupSessionSetting.isLocalhostMatchmakeEnabled = true;
#endif
            startupSessionSetting.useBroadcastOnSendingToAllStation = true;

            // Enable only when startupNetworkSetting.isAutoInitializeNetworkInterface = false.
#if SET_LAN_LOCAL_ADDRESS_MANUAL
            cryptoSetting.mode = PiaPlugin.CryptoSetting.Mode.Nothing;
            PiaPlugin.LocalAdressInfo localAddressInfo = new PiaPlugin.LocalAdressInfo();
            PiaPlugin.GetLocalAddress(ref localAddressInfo);
            startupSessionSetting.localAddress = localAddressInfo.address4;
            //startupSessionSetting.localAddress = localAddressInfo.address6;
            startupSessionSetting.v4Subnetmask = localAddressInfo.subnetMask;
            startupSessionSetting.v6InterfaceIndex = (int)localAddressInfo.interfaceIndex;
#endif
            startupSessionSetting.cryptoSetting = cryptoSetting;
            PiaPlugin.RegisterStartupSessionSetting(startupSessionSetting, playerInfo);
        }

#if NAT_TRAVERSAL_FAILURE_TEST
        // Registers the settings for NAT traversal failure emulation.
        {
            natDebugSetting.natTraversalFailureRatioForHost = 20;
            natDebugSetting.natTraversalFailureRatioForClient = 10;
            natDebugSetting.isNatTypeCheckFailure = true;
            natDebugSetting.isDnsResolutionFailure = true;
            PiaPlugin.RegisterNatDebugSetting(natDebugSetting);
        }
#endif

        return result;
    }

/*!
    @brief  Sets the type of network being used.
    @param[in] networkType  Type of network used.
*/
    public void SetNetworkType(PiaPlugin.NetworkType networkType)
    {
        s_NetworkType = networkType;
        m_IsSetNetworkType = true;
    }

/*!
    @brief  Performs network exit processing.
    @brief  Terminate Pia when starting the Editor or a standalone build.
*/
    public void OnApplicationQuit()
    {
    }

    // Implement OnDestroy() for crashes on Win.
#if UNITY_EDITOR_OR_STANDALONE
    public void OnDestroy()
    {
        // Forcibly transition PiaPlugin.State to NetworkCleanedUp.
        PiaPlugin.FinalizeNetwork();
        PiaPluginUtil.UnityLog("FinalizeNetwork.");
        // Pia finalization.
        PiaPlugin.FinalizeAll();
        PiaPluginUtil.DisablePiaLog();
        PiaPluginUtil.UnityLog("FinalizeAll.");
    }
#endif
}
