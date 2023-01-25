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

// When NEX is enabled, only valid for Switch with the editor or a standalone app.
#if UNITY_SWITCH
#define NN_PIA_ENABLE_NEX
#endif


using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

// -----------------------------------------------------------------------
//! @brief  Class that is a compilation of the features used by Pia overall.
// -----------------------------------------------------------------------
public class PiaPlugin
{

/*!
      @brief  Enumerated types that show the general-purpose result values for Pia.
*/
    public enum ResultValue : uint
    {
        ResultAllocationFailed = 0x00010401,                                            //!<  Failed to allocate memory or other resources. Applications must be implemented to ensure this <tt>Result</tt> value is never returned.
        ResultAlreadyInitialized = 0x00010402,                                          //!<  Initialization is already complete. Applications must be implemented to ensure this <tt>Result</tt> value is never returned.
        ResultBufferShortage = 0x00010404,                                              //!<  The buffer passed as an argument is too small. Applications must be implemented to ensure this <tt>Result</tt> value is never returned.
        ResultBrokenData = 0x00002c03,                                                  //!<  Data coming in through the communication line is corrupted or may have been tampered with.
        ResultCancelled = 0x00006c05,                                                   //!<  The asynchronous process received a cancellation request.
        ResultNetworkConnectionIsLost = 0x0000c406,                                     //!<  The connection was already terminated.
        ResultInvalidArgument = 0x00010407,                                             //!<  An invalid argument was passed to a function. One case where this result would be returned is when a null pointer was passed to a function. Applications must be implemented to ensure this <tt>Result</tt> value is never returned.
        ResultInvalidState = 0x00010408,                                                //!<  The function was called at the wrong time. The Pia library was not in the correct state to call the function that returned this <tt>Result</tt> value.
        ResultNoData = 0x00002c09,                                                      //!<  The data was not found.
        ResultNotFound = 0x00002c0a,                                                    //!<  The requested item was not found.
        ResultNotImplemented = 0x00000c0b,                                              //!<  A feature is not implemented. Applications must be implemented to ensure this <tt>Result</tt> value is never returned.
        ResultNotInitialized = 0x0001040c,                                              //!<  The object or module that called the function is not initialized. Applications must be implemented to ensure this <tt>Result</tt> value is never returned.
        ResultBufferIsFull = 0x00004c0d,                                                //!<  The action failed because the buffer required for the action was temporarily full. This may be caused by network congestion. It may succeed if you try again after some time has passed.
        ResultTimeout = 0x00006c0e,                                                     //!<  An asynchronous process timed out.
        ResultAlreadyExists = 0x0001040f,                                               //!<  The object already exists.
        ResultContainerIsFull = 0x00000c10,                                             //!<  The container is full.
        ResultTemporaryUnavailable = 0x00004c11,                                        //!<  The function is temporarily unusable.
        ResultNotSet = 0x00010413,                                                      //!<  Values that are supposed to be set in advance are not set.
        ResultMemoryLeak = 0x00010414,                                                  //!<  There may be a memory leak in the Pia library.
        ResultInvalidNode = 0x00000c16,                                                 //!<  [System Error] The destination node could not be found on the network.
        ResultNegligibleFault = 0x00002c18,                                             //!<  An error that can be ignored occurred.
        ResultInvalidConnection = 0x00006c19,                                           //!<  The connection state is invalid.
        ResultLocalCommunicationInvalidState = 0x00006c1b,                              //!<  A local communication error occurred internally.
        ResultNetworkIsNotFound = 0x00006c1c,                                           //!<  [System Error] The system cannot connect to this network.
        ResultNetworkIsFull = 0x00006c1d,                                               //!<  The network is full.
        ResultLocalCommunicationLowerVersion = 0x0000641e,                              //!<  The local station has a lower version than its peers.
        ResultLocalCommunicationHigherVersion = 0x0000641f,                             //!<  The local station has a higher version than its peers.
        ResultWifiOff = 0x0000c420,                                                     //!<  Result code indicating that wireless functionality is disabled.
        ResultSleep = 0x0000c421,                                                       //!<  Result code indicating sleep mode.
        ResultWirelessControllerCountLimitation = 0x00006422,                           //!<  Result code indicating the limit on the number of connected wireless controllers has been exceeded.
        ResultConnectionFailed = 0x00000c24,                                            //!<  Connecting to the network failed for some reason. (System error.)
        ResultCreateStationFailed = 0x00000c25,                                         //!<  The new <tt>Station</tt> could not be created. (System error.)
        ResultIncompatibleFormat = 0x00010426,                                          //!<  Incompatible communication format.
        ResultNotInCommunication = 0x00002c27,                                          //!<  Stations are not communicating.
        ResultTableIsFull = 0x00000c28,                                                 //!<  No more entries can be added because the table is full. (System error.)
        ResultDataIsNotArrivedYet = 0x00004c2c,                                         //!<  Data has not arrived from all stations. This error is generated when a problem occurs in data synchronization, such as a degenerated communication state. If the function returns this error, have the application stop the progress of the game and wait for the next picture frame. Note that this error is not necessarily encountered by all stations at the same time, or for the same number of times.
        ResultDataIsNotSet = 0x00010432,                                                //!<  The data to send has not been configured.
        ResultNatCheckFailed = 0x00006434,                                              //!<  The NAT check failed.
        ResultInUse = 0x00000c35,                                                       //!<  The object is already being used.
        ResultDnsFailed = 0x00006436,                                                   //!<  DNS resolution failed.
        ResultNexInternalError = 0x0000e437,                                            //!<  An error occurred in NEX.
        ResultJoinRequestDenied = 0x00006c38,                                           //!<  The join request was rejected by the session host.
        ResultStationConnectionFailed = 0x00006439,                                     //!<  Failed to connect stations.
        ResultMeshIsFull = 0x00006c3d,                                                  //!<  Could not join the requested mesh because it was full.
        ResultInvalidSystemMessage = 0x0001043e,                                        //!<  An invalid message was returned as a response to a join request.
        ResultStationConnectionNatTraversalFailedUnknown = 0x0000643f,                  //!<  NAT traversal between stations failed. The NAT type is unknown.
        ResultNatTraversalFailedBothEim = 0x00006442,                                   //!<  NAT traversal between stations failed. The NAT type for both local and remote stations is EIM.
        ResultNatTraversalFailedLocalEimRemoteEdm = 0x00006443,                         //!<  NAT traversal between stations failed. The NAT type was EIM for the local station and EDM for the remote station.
        ResultNatTraversalFailedLocalEdmRemoteEim = 0x00006444,                         //!<  NAT traversal between stations failed. The NAT type was EDM for the local station and EIM for the remote station.
        ResultNatTraversalFailedBothEdm = 0x00006445,                                   //!<  NAT traversal between stations failed. The NAT type for both local and remote stations is EDM.
        ResultRelayFailedNoCandidate = 0x00006446,                                      //!<  The relay connection failed. (There was no relay candidate.)
        ResultRelayFailedRttLimit = 0x00006447,                                         //!<  The relay connection failed. (The RTT limit was exceeded.)
        ResultRelayFailedRelayNumLimit = 0x00006448,                                    //!<  The relay connection failed. (The limit on the number of relays was exceeded.)
        ResultRelayFailedUnknown = 0x00006449,                                          //!<  The relay connection failed. (Details unknown.)
        ResultNatTraversalRequestTimeout = 0x0000644a,                                  //!<  NAT traversal between stations failed. The NAT traversal request timed out.
        ResultSessionIsNotFound = 0x00006c4b,                                           //!<  The session no longer exists.
        ResultMatchmakeSessionIsFull = 0x00006c4c,                                      //!<  Attempted to join a matchmaking session that is full.
        ResultDeniedByParticipant = 0x00006c4d,                                         //!<  The local station is blacklisted by a user in the session.
        ResultParticipantInBlockList = 0x00006c4e,                                      //!<  A user on the local station's blocked-user list is in the session.
        ResultGameServerMaintenance = 0x0000a44f,                                       //!<  The game server is down for maintenance.
        ResultSessionUserPasswordUnmatch = 0x00006c51,                                  //!<  Attempted to join a session set with a user password, but the user password did not match.
        ResultSessionSystemPasswordUnmatch = 0x00006c52,                                //!<  Attempted to join a session set with a system password, but the system password did not match.
        ResultMeshConnectionIsLost = 0x00006c50,                                        //!<  [System Error] The mesh is disconnected.
        ResultSessionIsClosed = 0x00006c53,                                             //!<  Attempted to join a session that is closed.
        ResultCompanionStationIsOffline = 0x00006c54,                                   //!<  (For joint sessions) Attempted to add a station that was not logged in to the server.
        ResultHostIsNotFriend = 0x00006c55,                                             //!<  Attempted to join a session with a host that is not a friend.
        ResultSessionConnectionIsLost = 0x00008c56,                                     //!<  The session was disconnected.
        ResultCompanionStationIsLeft = 0x00006c57,                                      //!<  Attempted to specify companion stations for the joint session that were disconnected.
        ResultSessionMigrationFailed = 0x00006c59,                                      //!<  Processes failed that were necessary for session transitions in joint sessions.
        ResultGameServerProcessAborted = 0x0000ac5a,                                    //!<  The game server process aborted.
        ResultSessionConnectionIsLostByHost = 0x00008c78,                               //!<  The session was destroyed by the host.
        ResultSessionConnectionIsLostByHostMigrationFailure = 0x00008c76,               //!<  Host migration failed and the session was disconnected.
        ResultSessionConnectionIsLostByInconsistentInfo = 0x00008c77,                   //!<  Session data is in an inconsistent state and the session was disconnected.
        ResultKickedOutFromSessionByInconsistentInfo = 0x00008c7a,                      //!<  Session data is in an inconsistent state and the host removed the player from the session.
        ResultKickedOutFromSessionByUser = 0x00008c79,                                  //!<  The host removed the player from the session.
        ResultSessionWrongState = 0x00006c5b,                                           //!<  The state of the joined session was irregular.
        ResultCreateCommunityFailedUpperLimit = 0x00006c5f,                             //!<  Exceeded the upper limit for creating communities.
        ResultJoinCommunityFailedUpperLimit = 0x00006c60,                               //!<  Exceeded the upper limit for joining communities.
        ResultCommunityIsFull = 0x00006c61,                                             //!<  The community you attempted to join is full.
        ResultCommunityIsNotFound = 0x00006c62,                                         //!<  The community does not exist.
        ResultCommunityIsClosed = 0x00006c63,                                           //!<  The community you attempted to join was not in a joinable period.
        ResultCommunityUserPasswordUnmatch = 0x00006c66,                                //!<  The user password specified when joining does not match the user password set for the community.
        ResultAlreadyJoinedCommunity = 0x00002c64,                                      //!<  You already joined the community you attempted to join.
        ResultUserAccountNotExisted = 0x00006c65,                                       //!<  The target user account does not exist.
        ResultNatTraversalFailedBothEimSamePublicAddress = 0x00006467,                  //!<  NAT traversal between stations failed. The NAT type for both local and remote stations is EIM. In addition, the global IP address was the same for both local and remote.
        ResultNatTraversalFailedBothEdmSamePublicAddress = 0x0000646a,                  //!<  NAT traversal between stations failed. The NAT type for both local and remote stations is EDM. In addition, the global IP address was the same for both local and remote.
        ResultNatTraversalFailedLocalEimRemoteEdmSamePublicAddress = 0x00006468,        //!<  NAT traversal between stations failed. The NAT type was EIM for the local station and EDM for the remote station. In addition, the global IP address was the same for both local and remote.
        ResultNatTraversalFailedLocalEdmRemoteEimSamePublicAddress = 0x00006469,        //!<  NAT traversal between stations failed. The NAT type was EDM for the local station and EIM for the remote station. In addition, the global IP address was the same for both local and remote.
        ResultSdkError = 0x0000ec6b,                                                    //!<  The SDK function call failed.
        ResultSdkViewerResultError = 0x0000a46e,                                        //!<  The SDK function call failed. Use the <tt>nn::pia::Result::GetErrorResult()</tt> function to get the SDK result and use the error viewer applet to handle it appropriately.
        ResultCancelledByUser = 0x0000ac6d,                                             //!<  Processing was suspended according to user operations.
        ResultSystemLowerVersion = 0x0000646f,                                          //!<  The local station has a lower version than its peers.
        ResultSystemHigherVersion = 0x00006470,                                         //!<  The local station has a higher version than its peers.
        ResultNetworkConnectionIsLostByDuplicateLogin = 0x0000a471,                     //!<  The station was disconnected from the server because another station is logged in using the same account. Although it is in a disconnected state, you can still call a logout.
        ResultLicenseForNetworkServiceNotAvailable = 0x0000a472,                        //!<  No permissions to use the network service.
        ResultLicenseForNetworkServiceError = 0x0000a473,                               //!<  The network service is unavailable.
        ResultLicenseForNetworkServiceSubscriptionError = 0x0000a474,                   //!<  The network service is unavailable.
        ResultLicenseForNetworkServiceSubscriptionError2 = 0x0000a475,                  //!<  The network service is unavailable.
        ResultOutOfMemory = 0x0000ec7b,                                                 //!<  Result code indicating that there was either an overflow or the heap passed as an argument did not have enough size.
        ResultNetworkInterfaceIsNotFound = 0x00006c7e,                                  //!<  Could not find a network interface with a valid address.
        ResultInvalidClock = 0x00006c7c,                                                //!<  Failed on an invalid clock value.
        ResultIncreaseDataSize = 0x00002c7d,                                            //!<  Failed because the data size was larger after data compression.
        ResultServerAccessLimitExceeded = 0x0001047f,                                   //!<  The frequency of API access to the game server exceeded the limit. Make adjustments so the functions used to create, search, join, leave, and configure settings for sessions are not called in quick succession. This issue does not occur in production ROMs.
        ResultEventDesynchronization = 0x00008c80,                                      //!<  Automatic synchronization of received data by <tt>EventProtocol</tt> and <tt>BroadcastEventProtocol</tt> is not possible.
        ResultSuccess = 0x00000000,                                                     //!<  <tt>Result</tt> instance indicating success.
    }

/*!
      @brief  Trace flags.
      @details  Set to <tt>InitializeFrameworkSetting.traceFlags</tt>.
      @details  The enumeration matches up with Pia, so it includes flags for modules that cannot be used with PiaUnity.
*/
    public enum TraceFlag : short
    {
        Always = 0,                         //!<  Flag to always perform trace output. Not configured from the application.

        Common,                             //!<  Flag to include PiaCommon information in trace output.
        CommonSocketDetail,                 //!<  Flag to include PiaCommon socket processing information in trace output.
        CommonDetail,                       //!<  Flag to include detailed information about PiaCommon in trace output.

        Local,                              //!<  Flag to include PiaLocal information in trace output.
        LocalMigration,                     //!<  Flag to include PiaLocal host migration information in trace output.
        LocalDetail,                        //!<  Flag to include detailed PiaLocal information in trace output.

        Net,                                //!<  Flag to include PiaNet information in trace output.
        NetMigration,                       //!<  Flag to include PiaNet host migration information in trace output.

        Direct,                             //!<  Flag to include PiaDirect information in trace output.
        DirectConnect,                      //!<  Flag to include PiaDirect connection information in trace output.
        DirectDetail,                       //!<  Flag to include detailed PiaDirect information in trace output.

        Nex,                                //!<  Flag to include PiaNex information in trace output.
        NexMigration,                       //!<  Flag to include PiaNex host migration process information in trace output.
        NexJoint,                           //!<  Flag to include PiaNex joint session process information in trace output.
        NexTransport,                       //!<  Flag to include PiaNex transmission information in trace output.
        NexRelay,                           //!<  Flag to include PiaNex server relay information in trace output.
        NexRelayTransport,                  //!<  Flag to include PiaNex server relay transmission information in trace output.
        NexNat,                             //!<  Flag to include PiaNex NAT traversal information in trace output.
        NexNatProbe,                        //!<  Flag to include PiaNex NAT probe information in trace output.

        Nat,                                //!<  Flag to include PiaNat information in trace output.
        NatProbe,                           //!<  Flag to include PiaNat NAT probe information in trace output.

        Cloud,                              //!<  Flag to include PiaCloud information in trace output.
        CloudConnectionDetail,              //!<  Flag to include PiaCloud information in trace output.
        CloudTransport,                     //!<  Flag to include PiaCloud transmission information in trace output.
        CloudHeapDetail,                    //!<  Flag to include the PiaCloud heap size in trace output.
        CloudDetail,                        //!<  Flag to include detailed PiaCloud information in trace output.

        Wan,                                //!<  Flag to include PiaWan information in trace output.
        WanNat,                             //!<  Flag to include PiaWan NAT traversal information in trace output.
        WanTransport,                       //!<  Flag to include PiaWan transmission information in trace output.
        WanDetail,                          //!<  Flag to include detailed PiaWan information in trace output.

        Lan,                                //!<  Flag to include PiaLan information in trace output.
        LanMigration,                       //!<  Flag to include PiaLan host migration process information in trace output.
        LanTransport,                       //!<  Flag to include PiaLan transmission information in trace output.

        Transport,                          //!<  Flag to include PiaTransport information in trace output.
        TransportInit,                      //!<  Flag to include PiaTransport initialization and finalization information in trace output.
        TransportKeepAlive,                 //!<  Flag to include PiaTransport keep-alive information in trace output.
        TransportRtt,                       //!<  Flag to include PiaTransport RTT protocol information in trace output.
        TransportBufferDetail,              //!<  Flag to include PiaTransport buffering information in trace output.
        TransportPacketDetail,              //!<  Flag to include PiaTransport packet transmission information in trace output.
        TransportProtocolDetail,            //!<  Flag to include PiaTransport protocol message transmission information in trace output.
        TransportRelay,                     //!<  Flag to include PiaTransport relay information in trace output.
        TransportRelayDetail,               //!<  Flag to include detailed PiaTransport relay information in trace output.
        TransportReliableDetail,            //!<  Flag to include detailed information for PiaTransport reliable communications in trace output.
        TransportDebug,                     //!<  Flag to include PiaTransport information in trace output for debugging.

        Mesh,                               //!<  Flag to include PiaMesh information in trace output.
        MeshMigration,                      //!<  Flag to include the PiaMesh host migration process information in trace output.
        MeshUpdate,                         //!<  Flag to include PiaMesh mesh update information in trace output.
        MeshClock,                          //!<  Flag to include PiaMesh mesh time synchronization information in trace output.

        Session,                            //!<  Flag to include PiaSession information in trace output.
        SessionMigration,                   //!<  Flag to include PiaSession host migration information in trace output.
        SessionJoint,                       //!<  Flag to include PiaSession joint session information in trace output.
        SessionReserved,                    //!<  This is a reserved region.

        Lobby,                              //!<  Flag to include PiaLobby information in trace output.
        LobbyDetail,                        //!<  Flag to include detailed PiaLobby information in trace output.

        Sync,                               //!<  Flag to include PiaSync information in trace output.
        SyncFrameDrop,                      //!<  Flag to include PiaSync frame drop events in trace output.
        SyncDetail,                         //!<  Flag to include detailed PiaSync information in trace output.

        Clone,                              //!<  Flag to include PiaClone information in trace output.
        CloneUpdate,                        //!<  Flag to include PiaClone UpdateProtocolEvent information in trace output.
        CloneEvent,                         //!<  Flag to include PiaClone EventProtocol and BroadcastEventProtocol information in trace output.
        CloneEventDetail,                   //!<  Flag to include detailed PiaClone EventProtocol and BroadcastEventProtocol information in trace output.
        CloneAtomic,                        //!<  Flag to include PiaClone AtomicProtocol information in trace output.
        CloneClock,                         //!<  Flag to include PiaClone ClockProtocol information in trace output.

        Reckoning,                          //!<  Flag to include PiaReckoning information in trace output.

        Chat,                               //!<  Flag to include PiaChat information in trace output.
        ChatDetail,                         //!<  Flag to include detailed PiaChat information in trace output.

        Framework,                          //!<  Flag to include PiaFramework information in trace output.
        FrameworkDetail,                    //!<  Flag to include detailed PiaFramework information in trace output.

        Emulation,                          //!<  Flag to include PiaEmulation information in trace output.
        EmulationDetail,                    //!<  Flag to include PiaEmulation packet transmission information in trace output.

        Brain,                              //!<  Flag to include PiaBrain information in trace output.
        BrainDetail,                        //!<  Flag to include detailed PiaBrain information in trace output.

        Plugin,                             //!<  Flag to include Pia plug-in information in trace output.

        Report,                             //!<  Flag to include PiaReport information in trace output.

        Terminate,                          //!<  The <tt>enum</tt> termination value.

        Default = Always,                   //!<  The default value for <tt>TraceFlag</tt>.

        All = -1                            //!<  Flag to include all information in trace output.
    }

/*!
      @brief  Enumerates the types of handling expected.
*/
    public enum HandlingType : int
    {
        Void = 0,         //!<  Invalid value.
        Ignorable,        //!<  Can be ignored.
        Retry,            //!<  Try to execute again after some time has passed. This is set only if the process in which the error occurred can be re-executed without user action.
        Cleanup,          //!<  Execute a cleanup process.
        CleanupWithLeave, //!<  Execute session leaving and a cleanup process, in that order.
        LogoutWithLeave,  //!<  Execute session leaving, a cleanup process, and a logout, in that order. The logout process must be executed for Internet communication only.
        ShutdownNetwork,  //!<  Execute a cleanup process, a logout, and a network shutdown, in that order. The logout process must be executed for Internet communication only.
        Finalize,         //!<  Execute a cleanup process, a logout, a network shutdown, and Pia finalization, in that order. The logout process must be executed for Internet communication only.
        ProgrammingError  //!<  Programming error. Make source code revisions.
    }

/*!
      @brief  Enumerates the policies for using the error viewer.
*/
    public enum ViewerType : int
    {
        Void = 0,     //!<  Invalid value.
        ShouldUse,    //!<  Supposed to be used.
        MayUse,       //!<  Okay to use.
        ShouldNotUse  //!<  Do not use.
    }

/*!
      @brief  Enumerated type to represent the types of networks used.
*/
    public enum NetworkType : int
    {
        Local = 0,      //!<  Local communication.
        Internet,       //!<  Internet communication.
        Lan             //!<  LAN communication.
    }

/*!
      @brief  Enumerated type indicating the state of progress of the session join-in process.
*/
    public enum State : int
    {
        NotInitialized = 0,                     //!<  The state before initialization (after finalization) of the server service, network interface, and socket.
        NetworkStartedUp = 1,                   //!<  The state of being logged out with the server service, network interface, and socket initialized and a network connection available.
        LoggedIn = 2,                           //!<  The state of being logged in before initialization (after finalization) of Pia.
        SessionInitialized = 3,                 //!<  The state where Pia is initialized but before startup (after cleanup) of Pia.
        SessionStartedUp = 4,                   //!<  The state where Pia has started but before joining (after leaving) a session.

        SessionJoined = 5,                      //!<  The state where the station has joined the session.
        SessionStartedBrowseMatchmake = 7,      //!<  The state where browse matchmaking has started.

        SessionLeft = 4,                        //!<  The state where Pia has started but before joining (after leaving) a session.
        SessionCleanedUp = 3,                   //!<  The state where Pia is initialized but before startup (after cleanup) of Pia.
        SessionFinalized = 2,                   //!<  The state of being logged in before initialization (after finalization) of Pia.
        LoggedOut = 1,                          //!<  The state of being logged out with the server service, network interface, and socket initialized and a network connection available.
        NetworkCleanedUp = 0,                   //!<  The state before initialization (after finalization) of the server service, network interface, and socket.

        JointSessionJoined = 6,                 //!<  The state of being joined in a joint session.
        JointSessionLeft = 4                    //!<  The state where Pia has started but before joining (after leaving) a joint session.
    }

/*!
      @brief  Enumerated type that identifies the type of the asynchronous process.
*/

    public enum AsyncProcessId : int
    {
        Nothing = 0,                    //!<  A process that does nothing.
        ChangeState,                    //!<  Session joining.
        HandleError,                    //!<  Error handling.
        CreateSession,                  //!<  Create the session.
        BrowseSession,                  //!<  Session search.
        JoinSession,                    //!<  Session joining.
        OpenSession,                    //!<  Open a session to participants.
        UpdateAndOpenSession,           //!<  Updates the settings for a session and opens it to participants.
        CloseSession,                   //!<  Close a session to participants.
        UpdateSessionSetting,           //!<  Updates the session settings.
        RequestSessionProperty,         //!<  Requests information about the session the station is participating in.
        DestroyJointSession,            //!<  Disbands a joint session.
        OpenJointSession,               //!<  Open a joint session to participants.
        UpdateAndOpenJointSession,      //!<  Updates the settings for a joint session and opens it to participants.
        CloseJointSession,              //!<  Close a joint session to participants.
        UpdateJointSessionSetting,      //!<  Updates the joint session settings.
        RequestJointSessionProperty,    //!<  Requests information about the joint session the station is participating in.
    };

    #if NN_PIA_ENABLE_NAT_CHECK
    public enum ServerEnvironment : int
    {
        Invalid = -1, //!<  Invalid server environment settings.
        Develop = 0,  //!<  The server environment for development.
        Producet      //!<  The server environment for production.
    }
#endif

    // pia::InvalidConstantId
    public const UInt64 InvalidConstantId = 0;      //!<  Constant representing an invalid <tt>ConstantId</tt>.
    // pia::StationIndex_Invalid
    public const int InvalidStationIndex = 0xFD;    //!<  ID indicating a station that is not present in the session.

    public const UInt32 PlayerNameLengthMax = 20;   //!<  Specifies the maximum length of a player name.

#if UNITY_ONLY_SWITCH
    public const int LocalCommunicationVersionMin = 0;      //!<  Minimum value of the local communication version.
    public const int LocalCommunicationVersionMax = 32767;  //!<  Maximum value of the local communication version.
#endif
#if UNITY_ONLY_SWITCH
    public const int PlayerInfoSizeMax = 4;                 //!<  Specifies the maximum size for player data. Be careful when implementing because it will be <tt>1</tt> for standalone builds and when running the Editor.
#else
    public const int PlayerInfoSizeMax = 1;                 //!<  Specifies the maximum size for player data.
#endif

/*!
    @cond  PRIVATE
*/
    // DispatchResult
    private static List<PiaPluginSession.SessionEvent> s_SessionEventList = new List<PiaPluginSession.SessionEvent>();    //!<  System variable.
    //! @endcond

/*!
      @brief  Shows processing results.
*/
    [StructLayout(LayoutKind.Sequential)]
    public struct Result
    {
        public ResultValue resultValue      //!  Pia general-purpose result values.
        {
            get; private set;
        }
        private UInt32 innerErrorCode       //!  Error code values.
        {
//            get; //private set;
            get;  set;
        }
        public ViewerType viewerType        //!  Error viewer use policies.
        {
            get; private set;
        }
        public HandlingType handlingType    //!  Expected handling types.
        {
            get; private set;
        }
#if UNITY_ONLY_SWITCH
        private nn.Result sdkViewerResult    //!  The SDK result value.
        {
            get; //private set;
        }
#endif
/*!
        @cond  PRIVATE
        @brief  Constructor.
*/
        public Result(HandlingType _handlingType)
        {
            resultValue = ResultValue.ResultNotImplemented;
            innerErrorCode = 0;
            viewerType = ViewerType.ShouldNotUse;
            handlingType = _handlingType;
#if UNITY_ONLY_SWITCH
            sdkViewerResult = new nn.Result();
#endif
        }
        //! @endcond

/*!
        @cond  PRIVATE
        @brief  Constructor.
*/
        public Result(ResultValue _resultValue, UInt32 _innerErrorCode, ViewerType _viewerType, HandlingType _handlingType)
        {
            resultValue = _resultValue;
            innerErrorCode =_innerErrorCode;
            viewerType = _viewerType;
            handlingType = _handlingType;
#if UNITY_ONLY_SWITCH
            sdkViewerResult =new nn.Result();
#endif
        }
        //! @endcond

/*!
          @brief  Determines whether processing was successful.
          @return  Whether processing was successful.
          @retval true  Success.
          @retval false  Failure.
*/
        public bool IsSuccess()
        {
            return resultValue == ResultValue.ResultSuccess;
        }

/*!
          @brief  Determines whether processing failed.
          @return  Whether processing failed.
          @retval true  Failure.
          @retval false  Success.
*/
        public bool IsFailure()
        {
            return !IsSuccess();
        }

#if UNITY_ONLY_SWITCH
/*!
          @brief  Gets the error code.
          @return  Error code.
*/
        public nn.err.ErrorCode GetErrorCode()
        {
            nn.err.ErrorCode errorCode = new nn.err.ErrorCode();
            errorCode.category = this.innerErrorCode / 10000;
            errorCode.number = this.innerErrorCode % 10000;
            return errorCode;
        }
/*!
          @brief  Gets the <tt>nn.Result</tt> that <tt>nn.err.Error.Show()</tt> displays when <tt>ResultSdkViewerResultError</tt> is returned.
          @return  <tt>nn.Result</tt>.
*/
        public nn.Result GetErrorResult()
        {
            if (resultValue == PiaPlugin.ResultValue.ResultSdkViewerResultError)
            {
                return sdkViewerResult;
            }
            else
            {
                return new nn.Result();
            }
        }
#else
/*!
          @brief  Gets the error code.
          @return  Error code.
*/
        public ErrorCode GetErrorCode()
        {
            ErrorCode errorCode = new ErrorCode();
            errorCode.category = this.innerErrorCode / 10000;
            errorCode.number = this.innerErrorCode % 10000;
            return errorCode;
        }

/*!
        @brief  Shows the error code.
*/
        [StructLayout(LayoutKind.Sequential)]
        public struct ErrorCode
        {
            public UInt32 category;  //!<  Category.
            public UInt32 number;    //!<  Number.
        }
#endif
/*!
          @brief  Prints information that is useful for debugging.
*/
        public void Trace()
        {
            PiaPlugin.Trace(String.Format("Result: resultValue={0}, errorCode=(category:0x{1,0:X8} number:0x{2,0:X8}), viewerType={3}, handlingType={4}", resultValue, GetErrorCode().category, GetErrorCode().number, viewerType, handlingType));
        }
    }

/*!
      @brief  Stores the player information associated with a <tt>Station</tt>.
      @details  The player name and the language type for the player name are shared with the other party you are connected with.@if NIN_DOC The player ID is not shared.@endif
*/
    [Serializable]
    public class PlayerInfo
    {
        public Byte nameStringLanguage;  //!<  The language of the player name.
        public string playerName;        //!<  Player name. The name must be no longer than <tt>PlayerNameLengthMax</tt> (not including the terminating null character).@if CTR_DOC Character encoding must be UTF-16. @endif
#if UNITY_ONLY_SWITCH
        public nn.account.Uid playerId;  //!<  The player ID. The user identifiers registered in the account system must be designated.
#endif
        public PlayerInfo()
        {
            nameStringLanguage = 0;
            playerName = "";
        }

/*!
         @cond  PRIVATE
*/
        internal PlayerInfo(PlayerInfoNative playerInfoNative)
        {
            nameStringLanguage = playerInfoNative.nameStringLanguage;
#if UNITY_EDITOR_OR_STANDALONE
            playerName = UnmanagedMemoryManager.ReadUtf16(playerInfoNative.playerName, playerInfoNative.playerNameSize);
#else
            playerName = UnmanagedMemoryManager.ReadUtf8(playerInfoNative.playerName, playerInfoNative.playerNameSize);
#endif
        }
        //! @endcond
    }

/*!
      @cond  PRIVATE
*/
    [StructLayout(LayoutKind.Sequential)]
    public struct PlayerInfoNative : IDisposable
    {
        public Byte nameStringLanguage;
        public IntPtr playerName;
        public int playerNameSize;
#if UNITY_ONLY_SWITCH
        public nn.account.Uid playerId;
#endif
        internal PlayerInfoNative(PlayerInfo playerInfo)
        {
            int bufferSize = 0;
            nameStringLanguage = playerInfo.nameStringLanguage;
            playerName = UnmanagedMemoryManager.WriteUtf8(playerInfo.playerName, ref bufferSize);
            playerNameSize = playerInfo.playerName.Length;
#if UNITY_ONLY_SWITCH
            playerId = playerInfo.playerId;
#endif
        }

        public void Dispose()
        {
            UnmanagedMemoryManager.Free(playerName);
        }
    }
    //! @endcond

    public static readonly Result SuccessResult = new Result(ResultValue.ResultSuccess, 0, ViewerType.Void, HandlingType.Void); //!<  Result indicating success.
    public static readonly Result ProgrammingErrorResult = new Result(HandlingType.ProgrammingError); //!<  Result indicating a programming error.
    public static readonly Result InvalidArgumentResult = new Result(ResultValue.ResultInvalidArgument, 0, ViewerType.ShouldNotUse, HandlingType.ProgrammingError); //!<  An invalid argument was passed to a function.
    public static readonly Result AllocationFailedResult = new Result(ResultValue.ResultAllocationFailed, 0, ViewerType.ShouldNotUse, HandlingType.ProgrammingError); //!<  Failed to allocate memory or other resources.

#if UNITY_EDITOR_OR_STANDALONE
    private static PiaPluginUtil.PiaAssert PiaAssert = new PiaPluginUtil.PiaAssert();
#endif


/*!
      @cond  PRIVATE
*/
    [StructLayout(LayoutKind.Sequential)]
    internal struct DispatchResultNative : IDisposable
    {
        public Result result { get; private set; }
        public IntPtr pSessionEventArray { get; private set; }
        public int sessionEventNum { get; private set; }

        public void Dispose()
        {
            UnmanagedMemoryManager.Free(pSessionEventArray);
        }
    }
    //! @endcond

/*!
    @cond  PRIVATE
*/
    public struct SessionEventListNative : IDisposable
    {
        public IntPtr pSessionEventArray { get; private set; }
        public int sessionEventNum { get; private set; }
        public void Dispose() { }
    }
    //! @endcond

/*!
    @brief  The structure representing the state of asynchronous processing.
*/
    [StructLayout(LayoutKind.Sequential)]
    public struct AsyncState
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool isCompleted;                  //!<  Whether asynchronous processing has been completed.
        public Result result                      //!  Result of asynchronous processing.
        {
            get; private set;
        }
    }

/*!
     @brief  A structure for encryption settings.
*/
    [StructLayout(LayoutKind.Sequential)]
    public struct CryptoSetting
    {
/*!
         @brief  Indicates the type of encryption algorithm.
*/
        public enum Mode : int
        {
            Nothing = 0,  //!<  None
            Aes128        //!<  AES-128
        }

        public Mode mode;           //!<  The kind of encryption algorithm.
        public IntPtr pKeyData;     //!<  The address of the key data. This setting is not required if there is no encryption (<tt>Mode.Nothing</tt>).
    }


/*!
      @brief  The class for managing the trace flag to pass during framework initialization.
*/
    [StructLayout(LayoutKind.Sequential)]
    public class TraceFlagSetting
    {
        public const uint ArraySize = (uint)PiaPlugin.TraceFlag.Terminate;  //!<  Buffer size. You can only register a buffer of this size.
        public uint count;                                                      //!<  Registered count.
        public TraceFlag[] flags = new TraceFlag[ArraySize];                    //!<  Registration buffer.

/*!
        @brief  Constructor.
*/
        public TraceFlagSetting()
        {
            count = 0;
        }

/*!
          @brief  Registers a trace flag.
          @param[in] value  The <tt>enum</tt> value to set.
          @return  Returns the <tt>TraceFlagSetting</tt> itself. Used to additionally call this function following the return value of this function.
*/

        public TraceFlagSetting add(TraceFlag value)
        {
            if (count< flags.Length)
            {
                flags[count] = value;
                ++count;
            }
            else
            {
                PiaPlugin.Trace("TraceFlagSetting buffer is over. count:"+ count);
                return null;
            }

            return this;
        }
    }

/*!
      @cond  PRIVATE
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class TraceFlagSettingNative : IDisposable
    {
        private const uint ArraySize = (uint)PiaPlugin.TraceFlag.Terminate;  //!<  Buffer size. You can only register a buffer of this number.
        public uint count                                                       //!  Registered count.
        {
            get; private set;
        }
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ArraySize)]
        private TraceFlag[] flags = new TraceFlag[ArraySize];                   //!<  Registration buffer.
        internal TraceFlagSettingNative(TraceFlagSetting setting)
        {
            uint in_count = setting.count <= (uint)flags.Length ? setting.count : (uint)flags.Length;
            for (uint i=0 ; i< in_count; i++)
            {
                flags[i] = setting.flags[i];
//                    PiaPlugin.Trace("TraceFlagSetting flags:" + i+" "+ flags[i]);
            }
            count = in_count;
        }

        public void Dispose() {}
    }
    //! @endcond

/*!
      @brief  Class with initialization parameters for the <tt>framework</tt> library.
*/
    [StructLayout(LayoutKind.Sequential)]
    public class InitializeFrameworkSetting
    {
        public UInt32 piaBufferSize;    //!<  Overall Pia buffer size.
        public NetworkType networkType; //!<  Type of network used.
        public Int32 backgroundThreadPriority = -1; //!<  The priority of the thread used by the background scheduler.
        public TraceFlagSetting traceFlags = new TraceFlagSetting();    //!<  Trace flag settings.
#if NN_PIA_ENABLE_MONITORING_ENV
        public ServerEnvironment serverEnvironment; //!<  Specifies the server environment to use.
#endif
#if UNITY_ONLY_SWITCH
        public enum CoreId
        {
            CoreId_Default, //!<  The default setting. The core on which <tt>nn::pia::common::Initialize</tt> was run is assigned.
            CoreId_0,       //!<  Core 0.
            CoreId_1,       //!<  Core 1.
            CoreId_2        //!<  Core 2.
        }
        public CoreId threadCoreId = CoreId.CoreId_Default; //!<  Specifies the core for the threads.
#endif
    }

/*!
      @cond  PRIVATE
*/
    [StructLayout(LayoutKind.Sequential)]
    public class InitializeFrameworkSettingNative : IDisposable
    {
        public UInt32 piaBufferSize;    //!<  Overall Pia buffer size.
        public int networkType; //!<  Type of network used.
        public Int32 backgroundThreadPriority; //<! The priority of the thread used by the background scheduler.
        public IntPtr unityVersion;
#if UNITY_ONLY_SWITCH
        public int threadCoreId;
#endif
#if NN_PIA_ENABLE_MONITORING_ENV
        public int serverEnvironment; //!<  Specifies the server environment to use.
#endif
        internal InitializeFrameworkSettingNative(InitializeFrameworkSetting setting)
        {
            piaBufferSize = setting.piaBufferSize;
            networkType = (int)setting.networkType;
            backgroundThreadPriority = setting.backgroundThreadPriority;
            int bufferSize = 0;
            unityVersion = UnmanagedMemoryManager.WriteUtf8(Application.unityVersion, ref bufferSize);
#if UNITY_ONLY_SWITCH
            threadCoreId = (int)setting.threadCoreId;
#endif
#if NN_PIA_ENABLE_MONITORING_ENV
            serverEnvironment = setting.serverEnvironment;
#endif
        }

        public void Dispose() { }
    }
    //! @endcond

/*!
      @brief  Class with initialization parameters for <tt>inet</tt>.
*/
//    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class InitializeInternetSetting
    {
        public UInt32 totalMemorySize;                  //!<  Memory size used by NEX.
        public UInt32 emergencyMemorySize;              //!<  Size of memory allocated as emergency memory when the heap is depleted.
        public UInt32 gameId;                           //!<  Game server ID used when logging in to the game server.
        public string accessKey = "";               //!<  Access key used when logging in to the game server.
#if UNITY_ONLY_SWITCH
        public nn.account.UserHandle userHandle;    //!<  Account user handle. If you do not specify an open handle, the account processing for calling Pia fails.
        [MarshalAs(UnmanagedType.U1)]
        public byte nexVersionMajor;                //!<  The major version of NEX used by the application. Set the <tt>NEX_VERSION_MAJOR</tt> macro defined by NEX.
        public byte nexVersionMinor;                //!<  The minor version of NEX used by the application. Set the <tt>NEX_VERSION_MINOR</tt> macro defined by NEX.
        public byte nexVersionMicro;                //!<  The micro version of NEX used by the application. Set the <tt>NEX_VERSION_MICRO</tt> macro defined by NEX.
#endif
    }

/*!
      @cond  PRIVATE
*/
    [StructLayout(LayoutKind.Sequential)]
    public class InitializeInternetSettingNative : IDisposable
    {
        public UInt32 totalMemorySize;              //!<  Memory size used by NEX.
        public UInt32 emergencyMemorySize;          //!<  Size of memory allocated as emergency memory when the heap is depleted.
        public UInt32 gameId;                       //!<  Game server ID used when logging in to the game server.
        public IntPtr accessKey = IntPtr.Zero;      //!<  Access key used when logging in to the game server.
#if UNITY_ONLY_SWITCH
        public nn.account.UserHandle userHandle;    //!<  Account user handle.
        [MarshalAs(UnmanagedType.U1)]
        public byte nexVersionMajor;                //!<  The major version of NEX used by the application. Set the <tt>NEX_VERSION_MAJOR</tt> macro defined by NEX.
        public byte nexVersionMinor;                //!<  The minor version of NEX used by the application. Set the <tt>NEX_VERSION_MINOR</tt> macro defined by NEX.
        public byte nexVersionMicro;                  //!<  The micro version of NEX used by the application. Set the <tt>NEX_VERSION_MICRO</tt> macro defined by NEX.
#endif
        internal InitializeInternetSettingNative(InitializeInternetSetting setting)
        {
            totalMemorySize = setting.totalMemorySize;
            emergencyMemorySize = setting.emergencyMemorySize;
            gameId = setting.gameId;
            int bufferSize = 0;
            accessKey = UnmanagedMemoryManager.WriteUtf8(setting.accessKey, ref bufferSize);
#if UNITY_ONLY_SWITCH
            userHandle = setting.userHandle;
            nexVersionMajor = setting.nexVersionMajor;
            nexVersionMinor = setting.nexVersionMinor;
            nexVersionMicro = setting.nexVersionMicro;
#endif
        }

        public void Dispose()
        {
            UnmanagedMemoryManager.Free(accessKey);
        }
    }
    //! @endcond

/*!
      @brief  Class with initialization parameters for the <tt>lan</tt> library.
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class InitializeLanSetting
    {
        public UInt64 communicationId;      //!<  The communication identifier.
    }

/*!
      @brief  Class with initialization parameters for the <tt>local</tt> library.
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class InitializeLocalSetting
    {
#if UNITY_SWITCH
        public UInt64 communicationId;                  //!<  The local communication identifier.@if NIN_DOC The specified local communication identifier must be registered in the application control data. For more information about application control data, see NintendoSDK Documents. If this value is not set, <tt>PiaPlugin.GetAsyncProcessState(AsyncProcessId <var>id</var>)</tt> returns a <tt>ResultInvalidArgument</tt> error.@endif
        public int applicationCommunicationVersion;     //!<  The local communication version. If the communication specifications are changed, such as by an application update, you can indicate that there is no compatibility between communication types by incrementing the local communication version. Applications running a different version can find this via search but always fail when trying to connect. Specify a value between <tt>nn::ldn::LocalCommunicationVersionMin</tt> and <tt>nn::ldn::LocalCommunicationVersionMax</tt>.

#endif
    }

/*!
        @brief  Class with initialization parameters for the <tt>transport</tt> library.
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class InitializeTransportSetting
    {
        public UInt16 stationNumMax;                                //!<  The maximum number of <tt>Station</tt> instances that can join a single network.
        public int measurementInterval;                             //!<  Distance measured with the transport analysis feature. The unit is seconds. If you specify <tt>0</tt>, measurement is not performed.
        public UInt16 sendProtocolBufferNumPerStation = 8;          //!<  The number of send buffers per station to pass to each protocol at initialization time.
        public UInt16 receiveProtocolBufferNumPerStation = 8;       //!<  The number of receive buffers per station to pass to each protocol at initialization time.
        public UInt16 sendThreadBufferNumPerStation = 0;            //!<  The number of buffers per station that the sending thread in Pia uses to send packets (about 1.5 KB per buffer * <tt>Setting::<var>stationNumMax</var></tt>). You must set this value to <tt>1</tt> or greater.
        public UInt16 receiveThreadBufferNumPerStation = 0;         //!<  The number of buffers per station that the receiving thread in Pia uses to receive packets (about 1.5 KB per buffer * <tt>Setting::<var>stationNumMax</var></tt>). You must set this value to <tt>1</tt> or greater.
        public UInt16 unreliableProtocolNum = 0;                    //!<  The number that generates UnreliableProtocol. To use this, you must set a value of <tt>1</tt> or greater.
        public UInt16 reliableProtocolNum = 0;                      //!<  The number of <tt>ReliableProtocol</tt> instances to create for unicast transmission. To use this, you must set a value of <tt>1</tt> or greater.
        public UInt16 broadcastReliableProtocolNum = 0;             //!<  The number of <tt>BroadcastReliableProtocol</tt> instances to create for broadcast transmission. To use this, you must set a value of <tt>1</tt> or greater.
        public UInt16 streamBroadcastReliableProtocolNum = 0;       //!<  The number of <tt>StreamBroadcastReliableProtocol</tt> instances to create for breaking up the data for broadcast transmission. To use this, you must set a value of <tt>1</tt> or greater.
        [MarshalAs(UnmanagedType.U1)]
        public bool isAnalysisResultPrintEnabled;                   //!<  Indicates whether to send analysis results from the transport analysis feature to the console. When <tt>true</tt> is specified, analysis results are sent at the interval designated by <tt><var>measurementInterval</var></tt>.
    }

/*!
      @brief  Class with initialization parameters for the <tt>clone</tt> library.
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class InitializeCloneSetting
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool isEnable;                                         //!<  Specifies whether to perform the PiaClone initialization process internally. <tt>true</tt> when using PiaClone.
        public UInt16 broadcastEventProtocolNum;                      //!<  Specifies the number of <tt>BroadcastEventProtocol</tt> instances to create. To use this, you must set a value of <tt>1</tt> or greater.
        public UInt16 broadcastEventProtocolBufferNumPerStation;      //!<  The data count for the send and receive buffer used with <tt>BroadcastEventProtocol</tt>. You must set a value of at least <tt>2</tt>. This value is passed to <tt><var>bufferNum</var></tt> in <tt>nn::pia::clone::BroadcastEventProtocol::Initialize(uint16_t <var>bufferNum</var>)</tt>.
        public UInt32 atomicProtocolIdMax = 32;                       //!<  The maximum value that can be specified for <tt><var>id</var></tt> by <tt>AtomicProtocol</tt> class functions like <tt>TryLock</tt>. The value you specify must be less than <tt>0xffffffff</tt>. The default is <tt>32</tt>. This value is passed to <tt><var>maxId</var></tt> in <tt>nn::pia::clone::AtomicProtocol::Initialize(uint32_t <var>maxId</var>)</tt>.
    }

/*!
      @brief  Class with initialization parameters for the <tt>sync</tt> library.
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class InitializeSyncSetting
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool isEnable;                        //!<  Specifies whether to conduct <tt>sync</tt> initialization.
        [MarshalAs(UnmanagedType.U1)]
        public bool isChangeDelayEnabled;                    //!<  Specifies whether to enable the feature to change the input delay while communication is synchronized.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public UInt32[] dataUnitSize = new UInt32[16];       //!<  Sets the size of synchronization data for each data ID. (Specify <tt>0</tt> for unused data IDs. The array size is fixed at <tt>16</tt>.)
    }

/*!
      @brief  The class contains parameters for initializing reckoning.
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class InitializeReckoningSetting
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool isEnable;                      //!<  Whether to perform <tt><var>initializeReckoning</var></tt>.
        public UInt16 reckoning1dProtocolNum;           //!<  Specifies the number of <tt>Reckoning1dProtocol</tt> instances to create.
        public UInt16 reckoning3dProtocolNum;           //!<  Specifies the number of <tt>Reckoning3dProtocol</tt> instances to create.
        public UInt16 reckoningProtocolBufferNum;      //!<  The number of data items in the send and receive buffer used with <tt>ReckoningProtocol</tt>. You must set a value of at least <tt>2</tt>.
    }

/*!
      @brief  Class with initialization parameters for the <tt>session</tt> library.
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class InitializeSessionSetting
    {
        public PiaPluginSession.NetworkTopology networkTopology;    //!<  Configure the network topology.
        public UInt16 browsedSessionPropertyListNum;                    //!<  The number of browsable sessions.
                                                                    //!<  The maximum possible configurable number differs for each network type.
                                                                    //!< @if NIN_DOC
                                                                    //!<  24 for local matchmaking.
                                                                    //!<  10 for LAN matchmaking.
                                                                    //!<  <tt>1000</tt> for Internet matchmaking when <tt>PiaPluginSession.SessionSearchCriteriaParticipant</tt> is passed to @ref PiaPluginSession.BrowseSessionAsync, and <tt>100</tt> otherwise.
                                                                    //!< @endif
    }

/*!
      @brief  Class with initialization parameters for the network interface and socket libraries.
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class StartupNetworkSetting
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool isAutoInitializeNetworkInterface; //!<  Specifies whether to initialize the network interface.
    }

/*!
      @brief  Class with session startup parameters.
*/
    [Serializable]
    public class StartupSessionSetting
    {
        public UInt16 relayRouteNumMax;  //!<  The maximum limit value for the number of forwarding requests that can be assigned to a single station when relay connection is enabled.
        public UInt16 relayRouteRttMax;                    //!<  The maximum limit value (in milliseconds) for the total RTT for one relay connection route when relay connection is enabled.
        public bool isEachPlayerCountedAsParticipant = true;   //!<  If multiple players are associated with the local station, specifies whether the number of players count as the number of participants in the session. Specify <tt>true</tt> to count them this way. The default is <tt>true</tt>. Not supported for local communication.
#if UNITY_SWITCH
        public bool isAutoInitializeLdn;                     //!<  Specifies whether LND is initialized automatically.
#endif
        //#endif
        public UInt32 silenceTimeMax = PiaPluginSession.DefaultSessionSilenceTimeMax;               //!<  Specifies the maximum allowable time without communication (in milliseconds). The default is <tt>PiaPluginSession.SessionSilenceTimeMaxDefault</tt>. The largest value that can be set is <tt>session::SessionSilenceTimeMaxMax</tt>, and the smallest is <tt>session::SessionSilenceTimeMaxMin</tt>.
        public UInt32 keepAliveSendingInterval = PiaPluginSession.DefaultSessionKeepAliveInterval;  //!<  Sets the send interval for keep-alive (in milliseconds). The default is <tt>PiaPluginSession.DefaultSessionKeepAliveInterval</tt>.
        public Int32 updateMeshSendingInterval = PiaPluginSession.UpdateMeshSendingIntervalDefault; //!<  Sets the interval for sending the message to synchronize the participants in the mesh (in milliseconds).

        public PiaPlugin.CryptoSetting cryptoSetting; //!<  The encryption settings to use for communication. Communications must be encrypted because the communication between stations can be easily intercepted. We strongly recommend using an encryption key that is difficult to guess. You must always specify encryption processing. You can specify a no-encryption option for development builds. The session fails to start if nothing is specified, or if no encryption is set in a release build.

        public string localAddress = "";                //!<  Specifies the local IP address to use during LAN communication. This setting is only used when <tt>Framework::RegisterStartupNetworkSetting</tt> is called with @ref StartupNetworkSetting.isAutoInitializeNetworkInterface disabled.
        public UInt32 v4Subnetmask;                     //!<  Specifies the IPv4 subnet mask to use during LAN communication. If left unspecified, <tt>255.255.255.0</tt> is set. This setting is only used when <tt>RegisterStartupNetworkSetting</tt> is called with @ref StartupNetworkSetting.isAutoInitializeNetworkInterface disabled.
        public Int32 v6InterfaceIndex;                  //!<  Specifies the IPv6 network interface index to use during LAN communication. If nothing is specified, <tt>0</tt> is set by default. This setting is only used when <tt>RegisterStartupNetworkSetting</tt> is called with @ref StartupNetworkSetting.isAutoInitializeNetworkInterface disabled.

#if UNITY_EDITOR_OR_STANDALONE
        public bool isLocalhostMatchmakeEnabled;        //!<  (For debugging) Specifies whether to perform communications among multiple processes on the same station when LAN communication is being used.
#endif
        public bool useBroadcastOnSendingToAllStation; //!<  (For debugging) Specifies whether to use broadcasting when LAN communication is being used. Cannot be used at the same time as <tt><var>isLocalhostMatchmakeEnabled</var></tt>. In a wireless LAN environment, specify <tt>false</tt> because otherwise the communications are transmitted from the wireless access point to all stations connected to that wireless AP.
    }

/*!
      @cond  PRIVATE
*/
    [StructLayout(LayoutKind.Sequential)]
    private class StartupSessionSettingNative : IDisposable
    {
        public UInt16 relayRouteNumMax;
        public UInt16 relayRouteRttMax;
#if UNITY_ONLY_SWITCH
        [MarshalAs(UnmanagedType.U1)]
        public bool isAutoInitializeLdn;
#endif
        public UInt32 silenceTimeMax;
        public UInt32 keepAliveSendingInterval;
        public Int32 updateMeshSendingInterval;

        [MarshalAs(UnmanagedType.U1)]
        public bool isEachPlayerCountedAsParticipant;
        public PiaPlugin.CryptoSetting cryptoSetting;

        public IntPtr pLocalAddress;
        public UInt32 v4Subnetmask;
        public Int32 v6InterfaceIndex;

#if UNITY_EDITOR_OR_STANDALONE
        [MarshalAs(UnmanagedType.U1)]
        public bool isLocalhostMatchmakeEnabled;
#endif
        [MarshalAs(UnmanagedType.U1)]
        public bool useBroadcastOnSendingToAllStation;

        internal StartupSessionSettingNative(StartupSessionSetting setting)
        {
            relayRouteNumMax = setting.relayRouteNumMax;
            relayRouteRttMax = setting.relayRouteRttMax;
            isEachPlayerCountedAsParticipant = setting.isEachPlayerCountedAsParticipant;
#if UNITY_ONLY_SWITCH
            isAutoInitializeLdn = setting.isAutoInitializeLdn;
#endif
            silenceTimeMax = setting.silenceTimeMax;
            keepAliveSendingInterval = setting.keepAliveSendingInterval;
            updateMeshSendingInterval = setting.updateMeshSendingInterval;

            cryptoSetting = setting.cryptoSetting;

            int bufferSize = 0;
            pLocalAddress = UnmanagedMemoryManager.WriteUtf8(setting.localAddress, ref bufferSize);
            v4Subnetmask = setting.v4Subnetmask;
            v6InterfaceIndex = setting.v6InterfaceIndex;

#if UNITY_EDITOR_OR_STANDALONE
            isLocalhostMatchmakeEnabled = setting.isLocalhostMatchmakeEnabled;
#endif
            useBroadcastOnSendingToAllStation = setting.useBroadcastOnSendingToAllStation;
        }

        public void Dispose()
        {
            UnmanagedMemoryManager.Free(pLocalAddress);
        }
    }
    //! @endcond

/*!
      @brief  Class with random matchmaking parameters.
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class JoinRandomSessionSetting
    {
        public PiaPluginSession.CreateSessionSetting createSessionSetting;          //!<  Configuration structure used when creating sessions.
        public PiaPluginSession.SessionSearchCriteria[] sessionSearchCriteriaList;  //!<  The array for the configuration structure used when joining sessions. If multiple search criteria are specified, a search will be performed starting with the first criterion specified in the list, and the search will end when participation is possible.
#if NN_PIA_ENABLE_NEX
        [MarshalAs(UnmanagedType.U1)]
        public bool isMyBlockListUsed = true;                                       //!<  Specifies whether to run a check for users who are on your blocked-user list. The default is <tt>true</tt>.
        [MarshalAs(UnmanagedType.U1)]
        public bool isOthersBlockListUsed = true;                                   //!<  Specifies whether to run a check for users who have you on their blocked-user lists. The default is <tt>true</tt>.
        [MarshalAs(UnmanagedType.U1)]
        public bool isUniqueKeywordEnabled = false;                                   //!<  Configures whether to avoid creating sessions with duplicate keywords for keyword matchmaking. The default is <tt>false</tt>.
#endif
    }

/*!
      @brief  The class that holds the NAT debug settings.
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class NatDebugSetting
    {
        public byte natTraversalFailureRatioForHost;        //!<  The probability that NAT traversal with the host fails (<tt>0</tt> to <tt>100</tt>).
        public byte natTraversalFailureRatioForClient;      //!<  The probability that NAT traversal with the client fails (<tt>0</tt> to <tt>100</tt>).
        [MarshalAs(UnmanagedType.U1)]
        public bool isNatTypeCheckFailure;                  //!<  Whether the NAT type determination will fail.
        [MarshalAs(UnmanagedType.U1)]
        public bool isDnsResolutionFailure;                 //!<  Whether the DNS name resolution processing that occurs during NAT type determination will fail.
    }

/*!
      @brief  Class prepared for measuring the peak internal resource usage values in the Pia library (unsupported).
*/
    public class Watermark
    {
        public string name       //!  Gets the name associated with an instance.
        {
            get; private set;
        }
        public Int64 valueMax    //!  Gets the maximum value currently being used by <tt>Update()</tt>.
        {
            get; private set;
        }
        public Int64 valueMin    //!  Gets the minimum value currently being used by <tt>Update()</tt>.
        {
            get; private set;
        }
        public Int64 latestValue //!  Gets the update value passed in the last call to <tt>Update()</tt>.
        {
            get; private set;
        }
        public Int64 updateCount //!  Gets the number of times <tt>Update()</tt> has been called so far.
        {
            get; private set;
        }

        public Watermark()
        {
            name = "";
            valueMax = 0;
            valueMin = 0;
            latestValue = 0;
            updateCount = 0;
        }

/*!
         @cond  PRIVATE
*/
        internal Watermark(WatermarkNative watermarkNative)
        {
#if UNITY_EDITOR_OR_STANDALONE
            name = UnmanagedMemoryManager.ReadUtf16(watermarkNative.pName, watermarkNative.nameSize);
#else
            name = UnmanagedMemoryManager.ReadUtf8(watermarkNative.pName, watermarkNative.nameSize);

#endif
            valueMax = watermarkNative.valueMax;
            valueMin = watermarkNative.valueMin;
            latestValue = watermarkNative.latestValue;
            updateCount = watermarkNative.updateCount;
        }
        //! @endcond
    }

/*!
     @cond  PRIVATE
*/
    [StructLayout(LayoutKind.Sequential)]
    internal class WatermarkNative : IDisposable
    {
        public IntPtr pName { get; private set; }
        public int nameSize { get; private set; }
        public Int64 valueMax { get; private set; }
        public Int64 valueMin { get; private set; }
        public Int64 latestValue { get; private set; }
        public Int64 updateCount { get; private set; }

        public void Dispose() { }
    }
    //! @endcond

/*!
  @brief  Structure representing the date and time.
*/

    public struct DateTime
    {
        public UInt16 year;  //!<  The year value.
        public byte month;   //!<  The month value.
        public byte day;     //!<  The day value.
        public byte hour;    //!<  The hour value.
        public byte minute;  //!<  The minute value.
        public byte second;  //!<  The seconds value.
        [MarshalAs(UnmanagedType.U1)]
        private bool isRegistered;  //!<  Whether the values have been specified.

/*!
        @brief  Instantiates an object with the specified date and time.
        @param[in] year_  Year.
        @param[in] month_  Month.
        @param[in] day_  Day.
        @param[in] hour_  Hour.
        @param[in] minute_  Minute.
        @param[in] second_  Seconds.
*/
        public DateTime(UInt16 year_, byte month_, byte day_, byte hour_, byte minute_, byte second_)
        {
            year = year_;
            month = month_;
            day = day_;
            hour = hour_;
            minute = minute_;
            second = second_;
            isRegistered = true;
        }

/*!
        @brief  Determines whether a value has been specified.
        @return  Returns <tt>true</tt> if a value is specified.
*/
        public bool IsRegistered()
        {
            return isRegistered;
        }
    }

/*!
     @brief  The structure to use to get the IP address.
*/
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class LocalAdressInfo
    {
        public string address6 = "";
        public UInt32 interfaceIndex;
        public string address4 = "";
        public UInt32 subnetMask;

        public LocalAdressInfo()
        {
            address6 = "";
            interfaceIndex = 0;
            address4 = "";
            subnetMask = 0;
        }

        internal LocalAdressInfo(LocalAdressInfoNative localAdressInfoNative)
        {
            address6 = UnmanagedMemoryManager.ReadUtf8(localAdressInfoNative.address6, localAdressInfoNative.address6Length);
            interfaceIndex = localAdressInfoNative.interfaceIndex;
            address4 = UnmanagedMemoryManager.ReadUtf8(localAdressInfoNative.address4, localAdressInfoNative.address4Length);
            subnetMask = localAdressInfoNative.subnetMask;
        }
    }

/*!
  @cond  PRIVATE
*/
    [StructLayout(LayoutKind.Sequential)]
    internal class LocalAdressInfoNative : IDisposable
    {
        public IntPtr address6 { get; private set; }
        public UInt16 address6Length { get; private set; }
        public UInt32 interfaceIndex { get; private set; }
        public IntPtr address4 { get; private set; }
        public UInt16 address4Length { get; private set; }
        public UInt32 subnetMask { get; private set; }

        internal LocalAdressInfoNative()
        {
            Reset();
        }
        internal LocalAdressInfoNative(LocalAdressInfo localAddressInfo)
        {
            Reset();
            int bufferSize = 0;
            address6 = UnmanagedMemoryManager.WriteUtf8(localAddressInfo.address6, ref bufferSize);
            address4 = UnmanagedMemoryManager.WriteUtf8(localAddressInfo.address4, ref bufferSize);
        }

        public void Reset()
        {
            address6 = IntPtr.Zero;
            address6Length = 0;
            interfaceIndex = 0;
            address4 = IntPtr.Zero;
            address4Length = 0;
            subnetMask = 0;
        }

        public void Dispose(){}
    }
    //! @endcond


#if NN_PIA_ENABLE_NAT_CHECK
    //! @cond  PRIVATE
//    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class CheckNatSetting
    {
        public IntPtr pBuffer;        //!<  Passes the buffer to use with Pia. The <tt>CheckNatBufferSizeMin</tt> area is needed. 4-byte alignment is required for 32-bit environments, and 8-byte alignment is required for 64-bit environments.
        public UInt32 bufferSize;     //!<  The <tt><var>pBuffer</var></tt> size.
        public Int32 threadPriorityRelative; //!<  Passes the running thread priority.
        public TraceFlagSetting traceFlags = new TraceFlagSetting();    //!<  Trace flag settings.
        public UInt64 applicationUserData0;    //!<  The data to set for monitoring from the application.
        public UInt64 applicationUserData1;    //!<  The data to set for monitoring from the application.
        public UInt64 applicationUserData2;    //!<  The data to set for monitoring from the application.
        public UInt64 applicationUserData3;    //!<  The data to set for monitoring from the application.
        public string natCheckPrimaryAddress = ""; //!<  PrimaryAddress
        public string natCheckSecondaryAddress = ""; //!<  SecondaryAddress
        public ServerEnvironment serverEnvironment; //!<  Specifies the server environment to use.
    }
    [StructLayout(LayoutKind.Sequential)]
    public class CheckNatSettingNative : IDisposable
    {
        public IntPtr pBuffer;        //!<  Passes the buffer to use with Pia. The <tt>CheckNatBufferSizeMin</tt> area is needed. 4-byte alignment is required for 32-bit environments, and 8-byte alignment is required for 64-bit environments.
        public UInt32 bufferSize;     //!<  The <tt><var>pBuffer</var></tt> size.
        public Int32 threadPriorityRelative; //!<  Passes the running thread priority.
        public UInt64 applicationUserData0;    //!<  The data to set for monitoring from the application.
        public UInt64 applicationUserData1;    //!<  The data to set for monitoring from the application.
        public UInt64 applicationUserData2;    //!<  The data to set for monitoring from the application.
        public UInt64 applicationUserData3;    //!<  The data to set for monitoring from the application.
        public IntPtr natCheckPrimaryAddress; //!<  PrimaryAddress
        public IntPtr natCheckSecondaryAddress; //!<  SecondaryAddress
        public ServerEnvironment serverEnvironment; //!<  Specifies the server environment to use.

        public void Dispose(){}
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CheckNatResult
    {
        public Byte    mapping;           //!<  Mapping type.
        public Byte    filtering;         //!<  Filtering type.
        public UInt16  publicPort;        //!<  External port.
        public UInt32  publicAddress;     //!<  External address.
        public Int32   portIncrement;     //!<  Port increment value.
        public UInt32  rtt;               //!<  RTT value.
        public UInt16  detectionTime;     //!<  The time spent for type check.
        public UInt16  dnsResolutionTime; //!<  The time spent for DNS resolution.
        public UInt16  interfacePort;     //!<  The port number of the interface used for checking.
        public Byte    retryCount;        //!<  Number of retries.
        public Byte    attribute;         //!<  Special attribute settings.
    }
    //! @endcond
#endif

#if UNITY_EDITOR
/*!
    @cond  PRIVATE
*/
    public static class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
    }
    //! @endcond

    static IntPtr? s_PluginDll = null;

    static plugin_InitializeFrameworkNative InitializeFrameworkNative;
    static plugin_FinalizeAllNative FinalizeAllNative;

    static Pia_RegisterPiaLog RegisterPiaLogNative;
    static Pia_UnregisterPiaLog UnregisterPiaLogNative;

#if NN_PIA_ENABLE_NAT_CHECK
    static plugin_InitializeCheckNatThreadNative InitializeCheckNatThreadNative;
    static plugin_IsRunCheckNatCompletedNative IsRunCheckNatCompletedNative;
    static plugin_GetCheckNatResultNative GetCheckNatResultNative;
    static plugin_WaitAndGetCheckNatResultNative WaitAndGetCheckNatResultNative;
    static plugin_FinalizeCheckNatThreadNative FinalizeCheckNatThreadNative;
    static plugin_WaitFinalizeDoneCheckNatThreadNative WaitFinalizeDoneCheckNatThreadNative;
    static plugin_IsFinalizeDoneCheckNatThreadNative IsFinalizeDoneCheckNatThreadNative;
#endif
    static plugin_RegisterInitializeInternetSettingNative RegisterInitializeInternetSettingNative;
    static plugin_RegisterInitializeLanSettingNative RegisterInitializeLanSettingNative;
    static plugin_RegisterInitializeLocalSettingNative RegisterInitializeLocalSettingNative;
    static plugin_RegisterInitializeTransportSettingNative RegisterInitializeTransportSettingNative;
    static plugin_RegisterInitializeCloneSettingNative RegisterInitializeCloneSettingNative;
    static plugin_RegisterInitializeSyncSettingNative RegisterInitializeSyncSettingNative;
    static plugin_RegisterInitializeReckoningSettingNative RegisterInitializeReckoningSettingNative;
    static plugin_RegisterInitializeSessionSettingNative RegisterInitializeSessionSettingNative;
    static plugin_RegisterStartupNetworkSettingNative RegisterStartupNetworkSettingNative;
    static plugin_RegisterStartupSessionSettingNative RegisterStartupSessionSettingNative;
    static plugin_RegisterJoinRandomSessionSettingNative RegisterJoinRandomSessionSettingNative;
    static plugin_RegisterJoinRandomJointSessionSettingNative RegisterJoinRandomJointSessionSettingNative;
    static plugin_RegisterNatDebugSettingNative RegisterNatDebugSettingNative;
    static plugin_UnregisterInitializeInternetSettingNative UnregisterInitializeInternetSettingNative;
    static plugin_UnregisterInitializeLanSettingNative UnregisterInitializeLanSettingNative;
    static plugin_UnregisterInitializeLocalSettingNative UnregisterInitializeLocalSettingNative;
    static plugin_UnregisterInitializeTransportSettingNative UnregisterInitializeTransportSettingNative;
    static plugin_UnregisterInitializeCloneSettingNative UnregisterInitializeCloneSettingNative;
    static plugin_UnregisterInitializeSyncSettingNative UnregisterInitializeSyncSettingNative;
    static plugin_UnregisterInitializeReckoningSettingNative UnregisterInitializeReckoningSettingNative;
    static plugin_UnregisterInitializeSessionSettingNative UnregisterInitializeSessionSettingNative;
    static plugin_UnregisterStartupNetworkSettingNative UnregisterStartupNetworkSettingNative;
    static plugin_UnregisterStartupSessionSettingNative UnregisterStartupSessionSettingNative;
    static plugin_UnregisterJoinRandomSessionSettingNative UnregisterJoinRandomSessionSettingNative;
    static plugin_UnregisterJoinRandomJointSessionSettingNative UnregisterJoinRandomJointSessionSettingNative;
    static plugin_DispatchNative DispatchNative;
    static plugin_UnregisterNatDebugSettingNative UnregisterNatDebugSettingNative;
    static plugin_CheckDispatchErrorNative CheckDispatchErrorNative;
    static plugin_GetSessionEventListNative GetSessionEventListNative;
    static plugin_FinalizeNetworkNative FinalizeNetworkNative;
    static plugin_ChangeStateAsyncNative ChangeStateAsyncNative;
    static plugin_HandleErrorAsyncNative HandleErrorAsyncNative;
    static plugin_GetCurrentHandlingResultNative GetCurrentHandlingResultNative;
    static plugin_ConvertHandlingTypeToStateNative ConvertHandlingTypeToStateNative;
    static plugin_GetJoinProcessStateNative GetJoinProcessStateNative;
    static plugin_IsSessionMigratingNative IsSessionMigratingNative;
    static plugin_GetMemoryUsageNative GetMemoryUsageNative;
    static plugin_GetAsyncProcessStateNative GetAsyncProcessStateNative;
    static plugin_GetAsyncProcessIdNative GetAsyncProcessIdNative;
    static plugin_StartWatermarkNative StartWatermarkNative;
    static plugin_StopWatermarkNative StopWatermarkNative;
    static plugin_GetWatermarkArrayNative GetWatermarkArrayNative;
    static plugin_GetRttNative GetRttNative;
    static plugin_GetCryptoKeySizeNative GetCryptoKeySizeNative;
    static plugin_AssertNative AssertNative;
    static plugin_GetServerTimeNative GetServerTimeNative;
    static plugin_GetLocalAddressNative GetLocalAddressNative;
    static Debug.plugin_EnableBroadcastOnSendingToAllStationForDebugNative EnableBroadcastOnSendingToAllStationForDebugNative;
    static plugin_GetDeviceLocationNameNative GetDeviceLocationNameNative;
    static plugin_TraceNative TraceNative;

    // Maximum number of FreeLibrary repeats for the DLL.
    const int cDllFreeCountLimit = 20;


    static void InitializeHooks()
    {
        IntPtr pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_InitializeFramework");
        InitializeFrameworkNative = (plugin_InitializeFrameworkNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_InitializeFrameworkNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + InitializeFrameworkNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_FinalizeAll");
        FinalizeAllNative = (plugin_FinalizeAllNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_FinalizeAllNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + FinalizeAllNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(s_PluginDll, "Pia_RegisterPiaLog");
        RegisterPiaLogNative = (Pia_RegisterPiaLog)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Pia_RegisterPiaLog));
        PiaPluginUtil.UnityLog("InitializeHooks " + RegisterPiaLogNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(s_PluginDll, "Pia_UnregisterPiaLog");
        UnregisterPiaLogNative = (Pia_UnregisterPiaLog)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Pia_UnregisterPiaLog));
        PiaPluginUtil.UnityLog("InitializeHooks " + UnregisterPiaLogNative);

#if NN_PIA_ENABLE_NAT_CHECK
        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_InitializeCheckNatThread");
        InitializeCheckNatThreadNative = (plugin_InitializeCheckNatThreadNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_InitializeCheckNatThreadNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + InitializeCheckNatThreadNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_RunCheckNatThread");
        RunCheckNatThreadNative = (plugin_RunCheckNatThreadNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_RunCheckNatThreadNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + RunCheckNatThreadNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_IsRunCheckNatCompleted");
        IsRunCheckNatCompletedNative = (plugin_IsRunCheckNatCompletedNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_IsRunCheckNatCompletedNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + IsRunCheckNatCompletedNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_GetCheckNatResult");
        GetCheckNatResultNative = (plugin_GetCheckNatResultNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_GetCheckNatResultNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetCheckNatResultNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_WaitAndGetCheckNatResult");
        WaitAndGetCheckNatResultNative = (plugin_WaitAndGetCheckNatResultNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_WaitAndGetCheckNatResultNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + WaitAndGetCheckNatResultNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_FinalizeCheckNatThread");
        FinalizeCheckNatThreadNative = (plugin_FinalizeCheckNatThreadNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_FinalizeCheckNatThreadNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + FinalizeCheckNatThreadNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_WaitFinalizeDoneCheckNatThread");
        WaitFinalizeDoneCheckNatThreadNative = (plugin_WaitFinalizeDoneCheckNatThreadNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_WaitFinalizeDoneCheckNatThreadNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + WaitFinalizeDoneCheckNatThreadNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_IsFinalizeDoneCheckNatThread");
        IsFinalizeDoneCheckNatThreadNative = (plugin_IsFinalizeDoneCheckNatThreadNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_IsFinalizeDoneCheckNatThreadNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + IsFinalizeDoneCheckNatThreadNative);
#endif
        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_RegisterInitializeInternetSetting");
        RegisterInitializeInternetSettingNative = (plugin_RegisterInitializeInternetSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_RegisterInitializeInternetSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + RegisterInitializeInternetSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_RegisterInitializeLanSetting");
        RegisterInitializeLanSettingNative = (plugin_RegisterInitializeLanSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_RegisterInitializeLanSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + RegisterInitializeLanSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_RegisterInitializeLocalSetting");
        RegisterInitializeLocalSettingNative = (plugin_RegisterInitializeLocalSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_RegisterInitializeLocalSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + RegisterInitializeLocalSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_RegisterInitializeTransportSetting");
        RegisterInitializeTransportSettingNative = (plugin_RegisterInitializeTransportSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_RegisterInitializeTransportSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + RegisterInitializeTransportSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_RegisterInitializeCloneSetting");
        RegisterInitializeCloneSettingNative = (plugin_RegisterInitializeCloneSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_RegisterInitializeCloneSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + RegisterInitializeCloneSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_RegisterInitializeSyncSetting");
        RegisterInitializeSyncSettingNative = (plugin_RegisterInitializeSyncSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_RegisterInitializeSyncSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + RegisterInitializeSyncSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_RegisterInitializeReckoningSetting");
        RegisterInitializeReckoningSettingNative = (plugin_RegisterInitializeReckoningSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_RegisterInitializeReckoningSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + RegisterInitializeReckoningSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_RegisterInitializeSessionSetting");
        RegisterInitializeSessionSettingNative = (plugin_RegisterInitializeSessionSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_RegisterInitializeSessionSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + RegisterInitializeSessionSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_RegisterStartupNetworkSetting");
        RegisterStartupNetworkSettingNative = (plugin_RegisterStartupNetworkSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_RegisterStartupNetworkSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + RegisterStartupNetworkSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_RegisterStartupSessionSetting");
        RegisterStartupSessionSettingNative = (plugin_RegisterStartupSessionSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_RegisterStartupSessionSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + RegisterStartupSessionSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_RegisterJoinRandomSessionSetting");
        RegisterJoinRandomSessionSettingNative = (plugin_RegisterJoinRandomSessionSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_RegisterJoinRandomSessionSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + RegisterJoinRandomSessionSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_RegisterJoinRandomJointSessionSetting");
        RegisterJoinRandomJointSessionSettingNative = (plugin_RegisterJoinRandomJointSessionSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_RegisterJoinRandomJointSessionSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + RegisterJoinRandomJointSessionSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_RegisterNatDebugSetting");
        RegisterNatDebugSettingNative = (plugin_RegisterNatDebugSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_RegisterNatDebugSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + RegisterNatDebugSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_UnregisterInitializeInternetSetting");
        UnregisterInitializeInternetSettingNative = (plugin_UnregisterInitializeInternetSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_UnregisterInitializeInternetSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + UnregisterInitializeInternetSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_UnregisterInitializeLanSetting");
        UnregisterInitializeLanSettingNative = (plugin_UnregisterInitializeLanSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_UnregisterInitializeLanSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + UnregisterInitializeLanSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_UnregisterInitializeLocalSetting");
        UnregisterInitializeLocalSettingNative = (plugin_UnregisterInitializeLocalSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_UnregisterInitializeLocalSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + UnregisterInitializeLocalSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_UnregisterInitializeTransportSetting");
        UnregisterInitializeTransportSettingNative = (plugin_UnregisterInitializeTransportSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_UnregisterInitializeTransportSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + UnregisterInitializeTransportSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_UnregisterInitializeCloneSetting");
        UnregisterInitializeCloneSettingNative = (plugin_UnregisterInitializeCloneSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_UnregisterInitializeCloneSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + UnregisterInitializeCloneSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_UnregisterInitializeSyncSetting");
        UnregisterInitializeSyncSettingNative = (plugin_UnregisterInitializeSyncSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_UnregisterInitializeSyncSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + UnregisterInitializeSyncSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_UnregisterInitializeReckoningSetting");
        UnregisterInitializeReckoningSettingNative = (plugin_UnregisterInitializeReckoningSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_UnregisterInitializeReckoningSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + UnregisterInitializeReckoningSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_UnregisterInitializeSessionSetting");
        UnregisterInitializeSessionSettingNative = (plugin_UnregisterInitializeSessionSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_UnregisterInitializeSessionSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + UnregisterInitializeSessionSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_UnregisterStartupNetworkSetting");
        UnregisterStartupNetworkSettingNative = (plugin_UnregisterStartupNetworkSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_UnregisterStartupNetworkSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + UnregisterStartupNetworkSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_UnregisterStartupSessionSetting");
        UnregisterStartupSessionSettingNative = (plugin_UnregisterStartupSessionSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_UnregisterStartupSessionSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + UnregisterStartupSessionSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_UnregisterJoinRandomSessionSetting");
        UnregisterJoinRandomSessionSettingNative = (plugin_UnregisterJoinRandomSessionSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_UnregisterJoinRandomSessionSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + UnregisterJoinRandomSessionSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_UnregisterJoinRandomJointSessionSetting");
        UnregisterJoinRandomJointSessionSettingNative = (plugin_UnregisterJoinRandomJointSessionSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_UnregisterJoinRandomJointSessionSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + UnregisterJoinRandomJointSessionSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_UnregisterNatDebugSetting");
        UnregisterNatDebugSettingNative = (plugin_UnregisterNatDebugSettingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_UnregisterNatDebugSettingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + UnregisterNatDebugSettingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_Dispatch");
        DispatchNative = (plugin_DispatchNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_DispatchNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + DispatchNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_CheckDispatchError");
        CheckDispatchErrorNative = (plugin_CheckDispatchErrorNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_CheckDispatchErrorNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + CheckDispatchErrorNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_GetSessionEventList");
        GetSessionEventListNative = (plugin_GetSessionEventListNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_GetSessionEventListNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetSessionEventListNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_FinalizeNetwork");
        FinalizeNetworkNative = (plugin_FinalizeNetworkNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_FinalizeNetworkNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + FinalizeNetworkNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_ChangeStateAsync");
        ChangeStateAsyncNative = (plugin_ChangeStateAsyncNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_ChangeStateAsyncNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + ChangeStateAsyncNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_GetAsyncProcessState");
        GetAsyncProcessStateNative = (plugin_GetAsyncProcessStateNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_GetAsyncProcessStateNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetAsyncProcessStateNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_GetAsyncProcessId");
        GetAsyncProcessIdNative = (plugin_GetAsyncProcessIdNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_GetAsyncProcessIdNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetAsyncProcessIdNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_HandleErrorAsync");
        HandleErrorAsyncNative = (plugin_HandleErrorAsyncNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_HandleErrorAsyncNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + HandleErrorAsyncNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_ConvertHandlingTypeToState");
        ConvertHandlingTypeToStateNative = (plugin_ConvertHandlingTypeToStateNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_ConvertHandlingTypeToStateNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + ConvertHandlingTypeToStateNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_GetJoinProcessState");
        GetJoinProcessStateNative = (plugin_GetJoinProcessStateNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_GetJoinProcessStateNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetJoinProcessStateNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_IsSessionMigrating");
        IsSessionMigratingNative = (plugin_IsSessionMigratingNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_IsSessionMigratingNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + IsSessionMigratingNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_GetMemoryUsage");
        GetMemoryUsageNative = (plugin_GetMemoryUsageNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_GetMemoryUsageNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetMemoryUsageNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_StartWatermark");
        StartWatermarkNative = (plugin_StartWatermarkNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_StartWatermarkNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + StartWatermarkNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_StopWatermark");
        StopWatermarkNative = (plugin_StopWatermarkNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_StopWatermarkNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + StopWatermarkNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_StopWatermark");
        StopWatermarkNative = (plugin_StopWatermarkNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_StopWatermarkNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + StopWatermarkNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_GetWatermarkArray");
        GetWatermarkArrayNative = (plugin_GetWatermarkArrayNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_GetWatermarkArrayNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetWatermarkArrayNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_GetRtt");
        GetRttNative = (plugin_GetRttNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_GetRttNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetRttNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_GetCryptoKeySize");
        GetCryptoKeySizeNative = (plugin_GetCryptoKeySizeNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_GetCryptoKeySizeNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetCryptoKeySizeNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_Assert");
        AssertNative = (plugin_AssertNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_AssertNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + AssertNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_GetServerTime");
        GetServerTimeNative = (plugin_GetServerTimeNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_GetServerTimeNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetServerTimeNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_GetCurrentHandlingResult");
        GetCurrentHandlingResultNative = (plugin_GetCurrentHandlingResultNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_GetCurrentHandlingResultNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetCurrentHandlingResultNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_GetLocalAddress");
        GetLocalAddressNative = (plugin_GetLocalAddressNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_GetLocalAddressNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetLocalAddressNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_Debug_EnableBroadcastOnSendingToAllStationForDebug");
        EnableBroadcastOnSendingToAllStationForDebugNative = (Debug.plugin_EnableBroadcastOnSendingToAllStationForDebugNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Debug.plugin_EnableBroadcastOnSendingToAllStationForDebugNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + EnableBroadcastOnSendingToAllStationForDebugNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_GetDeviceLocationName");
        GetDeviceLocationNameNative = (plugin_GetDeviceLocationNameNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_GetDeviceLocationNameNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetDeviceLocationNameNative);

        pAddressOfFunctionToCall = HookUtil(s_PluginDll, "Pia_Trace");
        TraceNative = (plugin_TraceNative)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(plugin_TraceNative));
        PiaPluginUtil.UnityLog("InitializeHooks " + TraceNative);
    }

/*!
    @cond  PRIVATE
*/
    public static void ClosePluginDll()
    {
        if (s_PluginDll == null)
        {
            PiaPluginUtil.UnityLog("Plugin DLL is already closed.");
            return;
        }
        bool result = true;
        int count = cDllFreeCountLimit;
        while(result)
        {
            result = NativeMethods.FreeLibrary(s_PluginDll.Value);
            PiaPluginUtil.UnityLog("Plugin DLL close " + result+" count : "+ (cDllFreeCountLimit-count));
            if (result)
            {
                count--;
                if (count<=0)
                {
                    result = false;
                }
            }
        }

        s_PluginDll = null;
    }

    public static bool IsHookPluginDll()
    {
        return s_PluginDll != null;
    }


    public static IntPtr HookUtil(IntPtr? s_PluginDll, string apiName)
    {
        IntPtr pAddressOfFunctionToCall = NativeMethods.GetProcAddress(s_PluginDll.Value, apiName);
        PiaPluginUtil.UnityLog("pAddressOfFunctionToCall " + pAddressOfFunctionToCall + (pAddressOfFunctionToCall != (IntPtr)0));
        if (pAddressOfFunctionToCall == (IntPtr)0)
        {
            PiaPluginUtil.UnityLog("not found :" + apiName);
        }
        UnityEngine.Debug.Assert (pAddressOfFunctionToCall != (IntPtr)0, "not found "+apiName);
        return pAddressOfFunctionToCall;
    }
    //! @endcond
#endif


#if UNITY_EDITOR
/*!
      @brief  Loads the DLL and hooks the API when the editor is running.
      @details  Normally this function is called when <tt>InitializeFramework()</tt> is called, but it can be called if you need to call the DLL API first.
*/
    static public void OpenAndHookPluginDll()
    {
        if (s_PluginDll==null)
        {
            OpenAndHookPluginDllInner();
        }

    }
    //! @cond  PRIVATE
/*!
    @brief  Loads the DLL and hooks the API when the editor is running.
    @details  For internal processing. (If already loaded, the DLL is first released and then the processing executes.)
*/
    static private void OpenAndHookPluginDllInner()
    {
        ClosePluginDll();

        string dllName = ("./Assets/PiaPlugin/Win/Lib/nn_piaPlugin.dll");
        if (System.IO.File.Exists(dllName))
        {
            PiaPluginUtil.UnityLog("file is exists." + dllName);
        }
        s_PluginDll = NativeMethods.LoadLibrary(dllName);
        int result = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
        PiaPluginUtil.UnityLog("DLL Hooks Start result:" + result + " " + s_PluginDll);
        {
            PiaPluginUtil.UnityLog("InitializeHooks DLL " + s_PluginDll);
            PiaPlugin.InitializeHooks();
            PiaPluginTransport.InitializeHooks(s_PluginDll);
            PiaPluginSession.InitializeHooks(s_PluginDll);
            PiaPluginSync.InitializeHooks(s_PluginDll);
            PiaPluginClone.InitializeHooks(s_PluginDll);
            PiaPluginReckoning.InitializeHooks(s_PluginDll);
            PiaPluginUtil.InitializeHooks(s_PluginDll);
        }
    }
    //! @endcond
#endif

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void Pia_RegisterPiaLog();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RegisterPiaLog")]
#endif
    private static extern void RegisterPiaLogNative();
#endif

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_InitializeFrameworkNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeFrameworkSettingNative setting,
                                                           [In, MarshalAs(UnmanagedType.LPStruct)] TraceFlagSettingNative traceFlags
        );
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_InitializeFramework")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_InitializeFramework")]
#endif
    private static extern Result InitializeFrameworkNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeFrameworkSettingNative setting,
                                                           [In, MarshalAs(UnmanagedType.LPStruct)] TraceFlagSettingNative traceFlags
        );
#endif
/*!
      @brief  Initializes <tt>PiaFramework</tt>.
      @param[in] setting  Initialization settings.
      @return  Returns the execution result.
*/
    public static Result InitializeFramework(InitializeFrameworkSetting setting)
    {
#if UNITY_EDITOR
        OpenAndHookPluginDllInner();
#endif

#if UNITY_EDITOR_OR_STANDALONE
        // Register a trace callback.
        RegisterPiaLogNative();
        // Because the DLL was hooked, checks the Pia log flag and enables printing as soon as PiaCommon is initialized.
        PiaPluginUtil.UpdatePiaLog();
        // Pia assertions now stop Unity with an assertion failure.
        PiaAssert.Enable();
#endif

        using (InitializeFrameworkSettingNative frameworkSetting = new InitializeFrameworkSettingNative(setting))
        {
            using (TraceFlagSettingNative frameworkTraceFlags = new TraceFlagSettingNative(setting.traceFlags))
            {
                PiaPluginUtil.UnityLog("frameworkTraceFlags " + frameworkTraceFlags.count);
                PiaPluginUtil.VersionInfo piaVersionInfo = new PiaPluginUtil.VersionInfo();
                PiaPluginUtil.VersionInfo piaPluginNativeVersionInfo = new PiaPluginUtil.VersionInfo();
                PiaPluginUtil.GetNativeVersionInfo(ref piaVersionInfo, ref piaPluginNativeVersionInfo);
                PiaPluginUtil.UnityLog($"[PiaUnity] {PiaPluginVersion.VersionMajor}.{PiaPluginVersion.VersionMinor}.{PiaPluginVersion.VersionMicro}, {PiaPluginVersion.Date}, r{PiaPluginVersion.Revision}");
                UnityEngine.Debug.Assert(PiaPluginVersion.Revision == piaPluginNativeVersionInfo.revision, $"revision does not match. script:{PiaPluginVersion.Revision} native:{piaPluginNativeVersionInfo.revision}");
                return InitializeFrameworkNative(frameworkSetting, frameworkTraceFlags);
            }
        }
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void Pia_UnregisterPiaLog();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_UnregisterPiaLog")]
#endif
    private static extern void UnregisterPiaLogNative();
#endif

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void plugin_FinalizeAllNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_FinalizeAll")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_FinalizeAll")]
#endif
    private static extern void FinalizeAllNative();
#endif

/*!
     @brief  Finalizes PiaUnity.
     @details  This function does nothing if it is called when PiaUnity is in the uninitialized state.
              It must not be called while asynchronous processes are running.
              As an exception, it can be called during the asynchronous processing of the <tt>ChangeStateAsync()</tt> function.
              If the function is called while the state is transitioning from <tt>PiaPlugin.State.NetworkStartedUp</tt> to <tt>PiaPlugin.State.LoggedIn</tt>,
              the process of checking whether a network service account is available and
              the asynchronous process of logging in to the game server are canceled, and then the module is finalized.
              Otherwise, the function waits for the completion of one transition step and then finalizes the module.
*/
    public static void FinalizeAll()
    {
#if UNITY_EDITOR
        if (s_PluginDll==null)
        {
            return;
        }
#endif
        FinalizeAllNative();
#if UNITY_EDITOR_OR_STANDALONE
        // Unregister the trace callback.
        UnregisterPiaLogNative();
#endif
    }

#if NN_PIA_ENABLE_NAT_CHECK
    //! @cond  PRIVATE

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_InitializeCheckNatThreadNative([In, MarshalAs(UnmanagedType.LPStruct)] CheckNatSettingNative setting,
                                                         [In, MarshalAs(UnmanagedType.LPStruct)] TraceFlagSettingNative traceFlags
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_InitializeCheckNatThread")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_InitializeCheckNatThread")]
#endif
    private static extern Result InitializeCheckNatThreadNative([In, MarshalAs(UnmanagedType.LPStruct)] CheckNatSettingNative setting,
                                                         [In, MarshalAs(UnmanagedType.LPStruct)] TraceFlagSettingNative traceFlags
        );
#endif
/*!
      @brief  Starts the NAT check process.
      @param[in] setting  Runtime settings.
      @return  Returns the execution result.
*/
    public static Result InitializeCheckNatThread(CheckNatSetting setting)
    {
        PiaPluginUtil.UnityLog("InitializeCheckNatThread " + setting);
        CheckNatSettingNative checkNatSetting = new CheckNatSettingNative();
        TraceFlagSettingNative checkNatTraceFlags = new TraceFlagSettingNative(setting.traceFlags);

        checkNatSetting.pBuffer = setting.pBuffer;
        checkNatSetting.bufferSize = setting.bufferSize;
        checkNatSetting.threadPriorityRelative = setting.threadPriorityRelative;
        checkNatSetting.applicationUserData0 = setting.applicationUserData0;
        checkNatSetting.applicationUserData1 = setting.applicationUserData1;
        checkNatSetting.applicationUserData2 = setting.applicationUserData2;
        checkNatSetting.applicationUserData3 = setting.applicationUserData3;
        checkNatSetting.serverEnvironment = setting.serverEnvironment;

        int bufferSize=0;
        checkNatSetting.natCheckPrimaryAddress = UnmanagedMemoryManager.WriteUtf8(setting.natCheckPrimaryAddress, ref bufferSize);
        checkNatSetting.natCheckSecondaryAddress = UnmanagedMemoryManager.WriteUtf8(setting.natCheckSecondaryAddress, ref bufferSize);
        return InitializeCheckNatThreadNative(checkNatSetting, checkNatTraceFlags);
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate  Result plugin_RunCheckNatThreadNative(Int32 reachability);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RunCheckNatThread")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_RunCheckNatThread")]
#endif
    private static extern Result RunCheckNatThreadNative(Int32 reachability);
#endif

/*!
      @brief  Starts the NAT check process.
      @param[in] setting  Runtime settings.
      @return  Returns the execution result.
*/
    public static Result RunCheckNatThread()
    {
        NetworkReachability reachability = Application.internetReachability;
        Int32 reachabilityInt = 3;
        if (reachability == NetworkReachability.NotReachable)
        {
            reachabilityInt = 0;
        }
        else if (reachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        {
            reachabilityInt = 1;
        }
        else if (reachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            reachabilityInt = 2;
        }

        return RunCheckNatThreadNative(reachabilityInt);
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool plugin_IsRunCheckNatCompletedNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_IsRunCheckNatCompleted")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_IsRunCheckNatCompleted")]
#endif
    private static extern bool IsRunCheckNatCompletedNative();
#endif
/*!
      @brief  Checks whether <tt>RunCheckNatThread</tt> processing has completed.
*/
    public static bool IsRunCheckNatCompleted()
    {
        return IsRunCheckNatCompletedNative();
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_GetCheckNatResultNative([Out, MarshalAs(UnmanagedType.LPStruct)] CheckNatResult natResult);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_GetCheckNatResult")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_GetCheckNatResult")]
#endif
    private static extern Result GetCheckNatResultNative([Out, MarshalAs(UnmanagedType.LPStruct)] CheckNatResult natResult);
#endif

/*!
    @brief  This function is used to get the NatCheckServer test results.
*/
    public static Result GetCheckNatResult(out CheckNatResult natResult)
    {
        CheckNatResult tmpResult = new CheckNatResult();
        Result result = GetCheckNatResultNative(tmpResult);
        natResult = tmpResult;
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_WaitAndGetCheckNatResultNative([Out, MarshalAs(UnmanagedType.LPStruct)] CheckNatResult natResult);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_WaitAndGetCheckNatResult")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_WaitAndGetCheckNatResult")]
#endif
    private static extern Result WaitAndGetCheckNatResultNative([Out, MarshalAs(UnmanagedType.LPStruct)] CheckNatResult natResult);
#endif

/*!
    @brief  This function is used to get the NatCheckServer test results.
*/
    public static Result WaitAndGetCheckNatResult(out CheckNatResult natResult)
    {
        CheckNatResult tmpResult = new CheckNatResult();
        Result result = WaitAndGetCheckNatResultNative(tmpResult);
        natResult = tmpResult;
        return result;
    }


#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void plugin_FinalizeCheckNatThreadNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_FinalizeCheckNatThread")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_FinalizeCheckNatThread")]
#endif
    private static extern void FinalizeCheckNatThreadNative();
#endif

/*!
      @brief  Stops the NAT check process.
*/
    public static void FinalizeCheckNatThread()
    {
        PiaPluginUtil.UnityLog("FinalizeCheckNatThreadNative");
        FinalizeCheckNatThreadNative();
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void plugin_WaitFinalizeDoneCheckNatThreadNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_WaitFinalizeDoneCheckNatThread")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_WaitFinalizeDoneCheckNatThread")]
#endif
    private static extern void WaitFinalizeDoneCheckNatThreadNative();
#endif

/*!
      @brief  Stops the NAT check process.
*/
    public static void WaitFinalizeDoneCheckNatThread()
    {
//        PiaPluginUtil.UnityLog("WaitFinalizeDoneCheckNatThreadNativeNative");
        WaitFinalizeDoneCheckNatThreadNative();
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool plugin_IsFinalizeDoneCheckNatThreadNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_IsFinalizeDoneCheckNatThread")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_IsFinalizeDoneCheckNatThread")]
#endif
    private static extern bool IsFinalizeDoneCheckNatThreadNative();
#endif

/*!
      @brief  Stops the NAT check process.
*/
    public static bool IsFinalizeDoneCheckNatThread()
    {
//        PiaPluginUtil.UnityLog("IsFinalizeDoneCheckNatThread");
        return IsFinalizeDoneCheckNatThreadNative();
    }
    //! @endcond

#endif //NN_PIA_ENABLE_NAT_CHECK

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_RegisterInitializeInternetSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeInternetSettingNative setting);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RegisterInitializeInternetSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_RegisterInitializeInternetSetting")]
#endif
    private static extern Result RegisterInitializeInternetSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeInternetSettingNative setting);
#endif

/*!
      @brief  Registers the default parameters for <tt>inet</tt>.
      @param[in] setting  Initialization settings.
      @return  Returns the execution result.
*/
    public static Result RegisterInitializeInternetSetting(InitializeInternetSetting setting)
    {
        using (InitializeInternetSettingNative settingNative = new InitializeInternetSettingNative(setting))
        {
            Result result = RegisterInitializeInternetSettingNative(settingNative);
            return result;
        }
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_RegisterInitializeLanSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeLanSetting setting);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RegisterInitializeLanSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_RegisterInitializeLanSetting")]
#endif
    private static extern Result RegisterInitializeLanSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeLanSetting setting);
#endif

/*!
      @cond  PRIVATE
      @brief  Registers the initialization settings parameter for <tt>lan</tt>.
      @param[in] setting  Initialization settings.
      @return  Returns the execution result.
*/
    public static Result RegisterInitializeLanSetting(InitializeLanSetting setting)
    {
        Result result = RegisterInitializeLanSettingNative(setting);
        return result;
    }
    //! @endcond

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_RegisterInitializeLocalSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeLocalSetting setting);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RegisterInitializeLocalSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_RegisterInitializeLocalSetting")]
#endif
    private static extern Result RegisterInitializeLocalSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeLocalSetting setting);
#endif

/*!
      @brief  Registers the initialization settings parameter for <tt>local</tt>.
      @param[in] setting  Initialization settings.
      @return  Returns the execution result.
*/
    public static Result RegisterInitializeLocalSetting(InitializeLocalSetting setting)
    {
        Result result = RegisterInitializeLocalSettingNative(setting);
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_RegisterInitializeTransportSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeTransportSetting setting);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RegisterInitializeTransportSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_RegisterInitializeTransportSetting")]
#endif
    private static extern Result RegisterInitializeTransportSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeTransportSetting setting);
#endif

/*!
      @brief  Registers the initialization settings parameter for <tt>transport</tt>.
      @param[in] setting  Initialization settings.
      @return  Returns the execution result.
*/
    public static Result RegisterInitializeTransportSetting(InitializeTransportSetting setting)
    {
        Result result = RegisterInitializeTransportSettingNative(setting);
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_RegisterInitializeCloneSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeCloneSetting setting
        );
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RegisterInitializeCloneSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_RegisterInitializeCloneSetting")]
#endif
    private static extern Result RegisterInitializeCloneSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeCloneSetting setting
    );
#endif

/*!
      @brief  Registers the initialization settings parameter for <tt>clone</tt>.
      @param[in] setting  Initialization settings.
      @return  Returns the execution result.
*/
    public static Result RegisterInitializeCloneSetting(InitializeCloneSetting setting)
    {
        Result result = RegisterInitializeCloneSettingNative(setting);
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_RegisterInitializeSyncSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeSyncSetting setting);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RegisterInitializeSyncSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_RegisterInitializeSyncSetting")]
#endif
    private static extern Result RegisterInitializeSyncSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeSyncSetting setting);
#endif

/*!
      @brief  Registers the initialization settings parameter for <tt>sync</tt>.
      @param[in] setting  Initialization settings.
      @return  Returns the execution result.
*/
    public static Result RegisterInitializeSyncSetting(InitializeSyncSetting setting)
    {
        if (setting.dataUnitSize.Length != 16)
        {
            return InvalidArgumentResult;
        }
        Result result = RegisterInitializeSyncSettingNative(setting);
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_RegisterInitializeReckoningSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeReckoningSetting setting);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RegisterInitializeReckoningSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_RegisterInitializeReckoningSetting")]
#endif
    private static extern Result RegisterInitializeReckoningSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeReckoningSetting setting);
#endif

/*!
      @brief  Registers the initialization settings parameter for <tt>reckoning</tt>.
      @param[in] setting  Initialization settings.
      @return  Returns the execution result.
*/
    public static Result RegisterInitializeReckoningSetting(InitializeReckoningSetting setting)
    {
        Result result = RegisterInitializeReckoningSettingNative(setting);
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_RegisterInitializeSessionSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeSessionSetting setting);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RegisterInitializeSessionSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_RegisterInitializeSessionSetting")]
#endif
    private static extern Result RegisterInitializeSessionSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] InitializeSessionSetting setting);
#endif

/*!
      @brief  Registers the initialization settings parameter for <tt>session</tt>.
      @param[in] setting  Initialization settings.
      @return  Returns the execution result.
*/
    public static Result RegisterInitializeSessionSetting(InitializeSessionSetting setting)
    {
        Result result = RegisterInitializeSessionSettingNative(setting);
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_RegisterStartupNetworkSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] StartupNetworkSetting setting);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RegisterStartupNetworkSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_RegisterStartupNetworkSetting")]
#endif
    private static extern Result RegisterStartupNetworkSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] StartupNetworkSetting setting);
#endif

/*!
      @brief  Registers the initialization settings parameter for the network interface and socket.
      @param[in] setting  Initialization settings.
      @return  Returns the execution result.
*/
    public static Result RegisterStartupNetworkSetting(StartupNetworkSetting setting)
    {
        Result result = RegisterStartupNetworkSettingNative(setting);
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_RegisterStartupSessionSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] StartupSessionSettingNative settingNative,
        [In] PiaPlugin.PlayerInfoNative[] playerInfo,
        int infoNum
        );
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RegisterStartupSessionSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_RegisterStartupSessionSetting")]
#endif
    private static extern Result RegisterStartupSessionSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] StartupSessionSettingNative settingNative,
        [In] PiaPlugin.PlayerInfoNative[] playerInfo,
        int infoNum
        );
#endif

/*!
      @brief  Registers the parameters for session startup.
      @details  There is no host migration setting for <tt>StartupSessionSetting</tt>, but startups always occur in an enabled state.
      @param[in] setting  Initialization settings.
      @param[in] playerInfo  The stored player information associated with a station. You can specify multiple pieces of information only with Internet communication and LAN communication, and the maximum is <tt>PlayerInfoSizeMax</tt>.
      @return  Returns the execution result.
*/
    public static Result RegisterStartupSessionSetting(StartupSessionSetting setting, PlayerInfo[] playerInfo)
    {
        PlayerInfoNative[] playerInfoNativeList = new PiaPlugin.PlayerInfoNative[playerInfo.Length];
        for (int i=0; i< playerInfo.Length; i++)
        {
            playerInfoNativeList[i] = new PiaPlugin.PlayerInfoNative(playerInfo[i]);
        }
        using (StartupSessionSettingNative settingNative = new StartupSessionSettingNative(setting))
        {
            Result result = RegisterStartupSessionSettingNative(settingNative, playerInfoNativeList, playerInfo.Length);
            foreach(PlayerInfoNative playerInfoNative in playerInfoNativeList)
            {
                playerInfoNative.Dispose();
            }
            return result;
        }
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate  Result plugin_RegisterJoinRandomSessionSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] PiaPluginSession.CreateSessionSettingNative createSessionSetting,
                                                                        [In] PiaPluginSession.SessionSearchCriteriaNative[] sessionSearchCriteriaList,
                                                                        int sessionSerachCriteriaListSize
#if NN_PIA_ENABLE_NEX
                                                                        ,[In, MarshalAs(UnmanagedType.Bool)] bool isMyBlockListUsed
                                                                        , [In, MarshalAs(UnmanagedType.Bool)] bool isOthersBlockListUsed
                                                                        , [In, MarshalAs(UnmanagedType.Bool)] bool isUniqueKeywordEnabled
#endif
        );
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RegisterJoinRandomSessionSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_RegisterJoinRandomSessionSetting")]
#endif
    private static extern Result RegisterJoinRandomSessionSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] PiaPluginSession.CreateSessionSettingNative createSessionSetting,
                                                                        [In] PiaPluginSession.SessionSearchCriteriaNative[] sessionSearchCriteriaList,
                                                                        int sessionSerachCriteriaListSize
#if NN_PIA_ENABLE_NEX
                                                                        ,[In, MarshalAs(UnmanagedType.Bool)] bool isMyBlockListUsed
                                                                        , [In, MarshalAs(UnmanagedType.Bool)] bool isOthersBlockListUsed
                                                                        , [In, MarshalAs(UnmanagedType.Bool)] bool isUniqueKeywordEnabled
#endif
        );
#endif

/*!
      @brief  Registers the parameters for random matchmaking.
      @param[in] setting  Initialization settings.
      @return  Returns the execution result.
*/
    public static Result RegisterJoinRandomSessionSetting(JoinRandomSessionSetting setting)
    {
        using (PiaPluginSession.CreateSessionSettingNative createSessionSetting = new PiaPluginSession.CreateSessionSettingNative(setting.createSessionSetting))
        {
            PiaPluginSession.SessionSearchCriteriaNative[] sessionSearchCriteriaList = new PiaPluginSession.SessionSearchCriteriaNative[setting.sessionSearchCriteriaList.Length];
            for (int i = 0; i < sessionSearchCriteriaList.Length; ++i)
            {
                sessionSearchCriteriaList[i] = new PiaPluginSession.SessionSearchCriteriaNative(setting.sessionSearchCriteriaList[i]);
            }
            Result result = RegisterJoinRandomSessionSettingNative(createSessionSetting, sessionSearchCriteriaList, sessionSearchCriteriaList.Length
#if NN_PIA_ENABLE_NEX
            , setting.isMyBlockListUsed, setting.isOthersBlockListUsed, setting.isUniqueKeywordEnabled
#endif
            );
            foreach (PiaPluginSession.SessionSearchCriteriaNative sessionSearchCriteriaNative in sessionSearchCriteriaList)
            {
                sessionSearchCriteriaNative.Dispose();
            }
            return result;
        }
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_RegisterJoinRandomJointSessionSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] PiaPluginSession.CreateSessionSettingNative createSessionSetting,
                                                                        [In] PiaPluginSession.SessionSearchCriteriaNative[] sessionSearchCriteriaList,
                                                                        int sessionSerachCriteriaListSize
#if NN_PIA_ENABLE_NEX
                                                                        ,[In, MarshalAs(UnmanagedType.Bool)] bool isMyBlockListUsed
                                                                        , [In, MarshalAs(UnmanagedType.Bool)] bool isOthersBlockListUsed
                                                                        , [In, MarshalAs(UnmanagedType.Bool)] bool isUniqueKeywordEnabled
#endif
        );
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RegisterJoinRandomJointSessionSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_RegisterJoinRandomJointSessionSetting")]
#endif
    private static extern Result RegisterJoinRandomJointSessionSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] PiaPluginSession.CreateSessionSettingNative createSessionSetting,
                                                                        [In] PiaPluginSession.SessionSearchCriteriaNative[] sessionSearchCriteriaList,
                                                                        int sessionSerachCriteriaListSize
#if NN_PIA_ENABLE_NEX
                                                                        ,[In, MarshalAs(UnmanagedType.Bool)] bool isMyBlockListUsed
                                                                        , [In, MarshalAs(UnmanagedType.Bool)] bool isOthersBlockListUsed
                                                                        , [In, MarshalAs(UnmanagedType.Bool)] bool isUniqueKeywordEnabled
#endif
        );
#endif

/*!
      @brief  Registers the parameters for joint sessions.
      @param[in] setting  Initialization settings.
      @return  Returns the execution result.
*/
    public static Result RegisterJoinRandomJointSessionSetting(JoinRandomSessionSetting setting)
    {
        using (PiaPluginSession.CreateSessionSettingNative createSessionSetting = new PiaPluginSession.CreateSessionSettingNative(setting.createSessionSetting))
        {
            PiaPluginSession.SessionSearchCriteriaNative[] sessionSearchCriteriaList = new PiaPluginSession.SessionSearchCriteriaNative[setting.sessionSearchCriteriaList.Length];
            for (int i = 0; i < sessionSearchCriteriaList.Length; ++i)
            {
                sessionSearchCriteriaList[i] = new PiaPluginSession.SessionSearchCriteriaNative(setting.sessionSearchCriteriaList[i]);
            }

            Result result = RegisterJoinRandomJointSessionSettingNative(createSessionSetting, sessionSearchCriteriaList, sessionSearchCriteriaList.Length
#if NN_PIA_ENABLE_NEX
            , setting.isMyBlockListUsed, setting.isOthersBlockListUsed, setting.isUniqueKeywordEnabled
#endif
            );
            foreach (PiaPluginSession.SessionSearchCriteriaNative sessionSearchCriteriaNative in sessionSearchCriteriaList)
            {
                sessionSearchCriteriaNative.Dispose();
            }
            return result;
        }
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_RegisterNatDebugSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] NatDebugSetting setting);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_RegisterNatDebugSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_RegisterNatDebugSetting")]
#endif
    private static extern Result RegisterNatDebugSettingNative([In, MarshalAs(UnmanagedType.LPStruct)] NatDebugSetting setting);
#endif

/*!
      @brief  (For debugging.) Registers the settings for the NAT debugging feature.
      @details  This function enables the use of the feature to intentionally cause NAT traversal to fail and to aid in the testing of the error handling of the session join-in process.
               When you use this function, the error that is returned is selected randomly from among the errors that are returned when NAT traversal fails.
      @param[in] setting  Initialization settings.
      @return  Returns the execution result.
*/
    public static Result RegisterNatDebugSetting(NatDebugSetting setting)
    {
        Result result = RegisterNatDebugSettingNative(setting);
        return result;
    }


#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_UnregisterInitializeInternetSettingNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_UnregisterInitializeInternetSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_UnregisterInitializeInternetSetting")]
#endif
    private static extern Result UnregisterInitializeInternetSettingNative();
#endif

/*!
      @brief  Deletes the registered default parameters for <tt>inet</tt>.
      @return  Returns the execution result.
*/
    public static Result UnregisterInitializeInternetSetting()
    {
        Result result = UnregisterInitializeInternetSettingNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_UnregisterInitializeLanSettingNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_UnregisterInitializeLanSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_UnregisterInitializeLanSetting")]
#endif
    private static extern Result UnregisterInitializeLanSettingNative();
#endif

/*!
      @cond  PRIVATE
      @brief  Unregisters the initialization settings parameter for <tt>lan</tt>.
      @return  Returns the execution result.
*/
    public static Result UnregisterInitializeLanSetting()
    {
        Result result = UnregisterInitializeLanSettingNative();
        return result;
    }
    //! @endcond

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_UnregisterInitializeLocalSettingNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_UnregisterInitializeLocalSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_UnregisterInitializeLocalSetting")]
#endif
    private static extern Result UnregisterInitializeLocalSettingNative();
#endif

/*!
      @brief  Unregisters the initialization settings parameter for <tt>local</tt>.
      @return  Returns the execution result.
*/
    public static Result UnregisterInitializeLocalSetting()
    {
        Result result = UnregisterInitializeLocalSettingNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_UnregisterInitializeTransportSettingNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_UnregisterInitializeTransportSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_UnregisterInitializeTransportSetting")]
#endif
    private static extern Result UnregisterInitializeTransportSettingNative();
#endif

/*!
      @brief  Unregisters the initialization settings parameter for <tt>transport</tt>.
      @return  Returns the execution result.
*/
    public static Result UnregisterInitializeTransportSetting()
    {
        Result result = UnregisterInitializeTransportSettingNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_UnregisterInitializeCloneSettingNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_UnregisterInitializeCloneSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_UnregisterInitializeCloneSetting")]
#endif
    private static extern Result UnregisterInitializeCloneSettingNative();
#endif

/*!
      @brief  Unregisters the initialization settings parameter for <tt>clone</tt>.
      @return  Returns the execution result.
*/
    public static Result UnregisterInitializeCloneSetting()
    {
        Result result = UnregisterInitializeCloneSettingNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_UnregisterInitializeSyncSettingNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_UnregisterInitializeSyncSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_UnregisterInitializeSyncSetting")]
#endif
    private static extern Result UnregisterInitializeSyncSettingNative();
#endif

/*!
      @brief  Unregisters the initialization settings parameter for <tt>sync</tt>.
      @return  Returns the execution result.
*/
    public static Result UnregisterInitializeSyncSetting()
    {
        Result result = UnregisterInitializeSyncSettingNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_UnregisterInitializeReckoningSettingNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_UnregisterInitializeReckoningSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_UnregisterInitializeReckoningSetting")]
#endif
    private static extern Result UnregisterInitializeReckoningSettingNative();
#endif

/*!
      @brief  Unregisters the initialization settings parameter for <tt>reckoning</tt>.
      @return  Returns the execution result.
*/
    public static Result UnregisterInitializeReckoningSetting()
    {
        Result result = UnregisterInitializeReckoningSettingNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_UnregisterInitializeSessionSettingNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_UnregisterInitializeSessionSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_UnregisterInitializeSessionSetting")]
#endif
    private static extern Result UnregisterInitializeSessionSettingNative();
#endif

/*!
      @brief  Unregisters the initialization settings parameter for <tt>session</tt>.
      @return  Returns the execution result.
*/
    public static Result UnregisterInitializeSessionSetting()
    {
        Result result = UnregisterInitializeSessionSettingNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_UnregisterStartupNetworkSettingNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_UnregisterStartupNetworkSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_UnregisterStartupNetworkSetting")]
#endif
    private static extern Result UnregisterStartupNetworkSettingNative();
#endif

/*!
      @brief  Unregisters the initialization settings parameter for the network interface and socket.
      @return  Returns the execution result.
*/
    public static Result UnregisterStartupNetworkSetting()
    {
        Result result = UnregisterStartupNetworkSettingNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_UnregisterStartupSessionSettingNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_UnregisterStartupSessionSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_UnregisterStartupSessionSetting")]
#endif
    private static extern Result UnregisterStartupSessionSettingNative();
#endif

/*!
      @brief  Deletes the registered parameters for session startup.
      @return  Returns the execution result.
*/
    public static Result UnregisterStartupSessionSetting()
    {
        Result result = UnregisterStartupSessionSettingNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_UnregisterJoinRandomSessionSettingNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_UnregisterJoinRandomSessionSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_UnregisterJoinRandomSessionSetting")]
#endif
    private static extern Result UnregisterJoinRandomSessionSettingNative();
#endif

/*!
      @brief  Deletes the registered parameters for random matchmaking.
      @return  Returns the execution result.
*/
    public static Result UnregisterJoinRandomSessionSetting()
    {
        Result result = UnregisterJoinRandomSessionSettingNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_UnregisterJoinRandomJointSessionSettingNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_UnregisterJoinRandomJointSessionSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_UnregisterJoinRandomJointSessionSetting")]
#endif
    private static extern Result UnregisterJoinRandomJointSessionSettingNative();
#endif

/*!
      @brief  Deletes the registered parameters for joint sessions.
      @return  Returns the execution result.
*/
    public static Result UnregisterJoinRandomJointSessionSetting()
    {
        Result result = UnregisterJoinRandomJointSessionSettingNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_UnregisterNatDebugSettingNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_UnregisterNatDebugSetting")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_UnregisterNatDebugSetting")]
#endif
    private static extern Result UnregisterNatDebugSettingNative();
#endif

/*!
      @brief  Unregisters the parameters for the NAT debugging function.
      @return  Returns the execution result.
*/
    public static Result UnregisterNatDebugSetting()
    {
        Result result = UnregisterNatDebugSettingNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void plugin_DispatchNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_Dispatch")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_Dispatch")]
#endif
    private static extern void DispatchNative();
#endif

/*!
      @brief  Executes the Pia or NEX dispatch process.
      @details  Call this function once or twice for each game frame.
      @details  Error handling is not needed from the application because error handling occurs when <tt>Dispatch()</tt> fails during asynchronous processing.
*/
    public static void Dispatch()
    {
#if UNITY_EDITOR
        if (s_PluginDll == null)
        {
            return;
        }
#endif
#if UNITY_EDITOR_OR_STANDALONE
        PiaPluginUtil.UpdatePiaLog();
#endif
        DispatchNative();
    }


#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_CheckDispatchErrorNative();
#else
#if UNITY_STANDALONE
        [DllImport("nn_piaPlugin", EntryPoint = "Pia_CheckDispatchError")]
#else
        [DllImport("__Internal", EntryPoint = "Pia_CheckDispatchError")]
#endif
        private static extern Result CheckDispatchErrorNative();
#endif

/*!
    @brief  Check whether an error has occurred in @ref Dispatch().
    @details  This function does not need to be called for every dispatch, but we recommend calling it at least once per frame.
    @return  If successful, returns @ref ResultSuccess.
*/
    public static Result CheckDispatchError()
    {
#if UNITY_EDITOR
        if (s_PluginDll == null)
        {
            Result dispatchResultNull = SuccessResult;
            return dispatchResultNull;
        }
#endif
        return CheckDispatchErrorNative();
    }


#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate SessionEventListNative plugin_GetSessionEventListNative();
#else
#if UNITY_ANDROID || UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_GetSessionEventList")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_GetSessionEventList")]
#endif
    private static extern SessionEventListNative GetSessionEventListNative();
#endif

/*!
      @brief  Gets the change in the session's state.
      @details  Gets the change in the session's state that has occurred since the last time <tt>GetSessionEventList()</tt> was called.
                    Call this function at least once per frame.
      @return  <tt><var>sessionEventList</var></tt>, the change in the session state.
*/
    public static List<PiaPluginSession.SessionEvent> GetSessionEventList()
    {
        List<PiaPluginSession.SessionEvent> sessionEventList;
        s_SessionEventList.Clear();
#if UNITY_EDITOR
        if (s_PluginDll == null)
        {
            sessionEventList = s_SessionEventList;
            return sessionEventList;
        }
#endif
        SessionEventListNative sessionEventListNative = GetSessionEventListNative();

        if (sessionEventListNative.sessionEventNum > 0)
        {
            UnmanagedMemoryManager.ReadList<PiaPluginSession.SessionEvent>(sessionEventListNative.pSessionEventArray, sessionEventListNative.sessionEventNum, ref s_SessionEventList);
        }
        sessionEventList = s_SessionEventList;

        return sessionEventList;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void plugin_FinalizeNetworkNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_FinalizeNetwork")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_FinalizeNetwork")]
#endif
    private static extern void FinalizeNetworkNative();
#endif

/*!
      @brief  Performs network exit processing.
      @details  This function is specific for code running in Unity Editor and standalone builds.
               Forcibly transitions <tt>PiaPlugin.State</tt> to <tt>State.NetworkCleanedUp</tt>.
               Implements the <tt>OnDestroy()</tt> function in a derived class of <tt>MonoBehaviour</tt> and uses it for finalization when an unexpected termination occurs.
               Be sure to always call <tt>PiaPlugin.FinalizeAll()</tt> after calling this function.
*/
    public static void FinalizeNetwork()
    {
#if UNITY_EDITOR
        if (s_PluginDll==null)
        {
            return;
        }
#endif
        FinalizeNetworkNative();
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_ChangeStateAsyncNative(State state);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_ChangeStateAsync")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_ChangeStateAsync")]
#endif
    private static extern Result ChangeStateAsyncNative(State state);
#endif

/*!
    @brief  Starts transitioning the state of progress in the session-joining process.
    @details  Performs the required processing until reaching the specified state of progression.
    @details  The completion of the process and the result can be checked with @ref GetAsyncProcessState.
    @details  While in a <tt>State.SessionJoined</tt> state, transition to <tt>State.SessionCleanedUp</tt> when <tt>State.SessionStartedUp</tt> or <tt>State.SessionLeft</tt> is specified as the target state.
    @if NIN_DOC
    @details  <tt>nn::nifm::HandleNetworkRequestResult()</tt> is called when the state progresses from <tt>State.SessionInitialized</tt> to <tt>State.NetworkStartedUp</tt>, so the UI for this feature is sometimes shown on the screen. This process blocks during that time.
    @details  <tt>nn::account::EnsureNetworkServiceAccountAvailable()</tt> is called when the state progresses from <tt>State.NetworkStartedUp</tt> to <tt>State.LoggedIn</tt>, so the UI for this feature is shown on the screen when the specified user network service account is not available. This process blocks during that time.
    @details  When the network type is local and <tt>StartupSessionSetting.isAutoInitializeLdn</tt> is <tt>true</tt>, the <tt>ldn</tt> library is initialized internally, but after it is initialized other communication features cannot be used until <tt>ldn</tt> library exit processing is performed. The system may use the communication features, so do not initialize the <tt>ldn</tt> library other than in the required scenes.
             In addition, immediately finalize the <tt>ldn</tt> library after finishing use of local communication.
             The <tt>ldn</tt> library is initialized at the point where the state transitions from <tt>State.SessionInitialized</tt> to <tt>State.SessionStartedUp</tt>.
             The <tt>ldn</tt> library is finalized at the point where the state transitions from <tt>State.SessionStartedUp</tt> to <tt>State.SessionInitialized</tt>.
    @endif
    @details
    @details  The following result occurs when there is a failure using @ref GetAsyncProcessState.
    @details  <tt>ResultInvalidState</tt> indicates that the function call occurred in an invalid state. Make sure that no other asynchronous processes are running. [:progErr]
    @details  The <tt>ResultInvalidArgument</tt> argument is not valid. Returned when the specified <tt><var>transitionState</var></tt> is further along than the current state of progress. [:progErr]
    @param[in] state  The target state for session join processing.
    @return  Returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> returns <tt>true</tt>. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running. [:progErr]
    @retval ResultInvalidArgument  An argument is not valid. Returned when the specified <tt><var>transitionState</var></tt> is further along than the current state of progress. [:progErr]
    @see  GetAsyncProcessState
*/
    public static Result ChangeStateAsync(State state)
    {
        Trace("call " +state);
        Result res = ChangeStateAsyncNative(state);
        return res;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_GetCurrentHandlingResultNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_GetCurrentHandlingResult")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_GetCurrentHandlingResult")]
#endif
    private static extern Result GetCurrentHandlingResultNative();
#endif

/*!
     @brief  Gets the <tt>Result</tt> instance that is currently the subject of error handling by the <tt>HandleErrorAsync()</tt> function.
     @details  If an error occurs during the error handling process, this <tt>Result</tt> instance is updated.
     @return  The <tt>Result</tt> instance that is the subject of error handling.
*/
    public static Result GetCurrentHandlingResult()
    {
        Result res = GetCurrentHandlingResultNative();
        return res;
    }


#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_HandleErrorAsyncNative(Result result);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_HandleErrorAsync")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_HandleErrorAsync")]
#endif
    private static extern Result HandleErrorAsyncNative(Result result);
#endif
/*!
    @brief  Performs error handling, such as when disconnections occur.
    @details  Runs the necessary finalization processes based on the <tt>Result.HandlingType</tt> of the specified <tt>Result</tt> instance.
    @details  If an error occurs inside <tt>Dispatch()</tt> during error handling, the <tt>Result</tt> instance that will be the target of error handling is updated inside this function (at which time <tt>Dispatch()</tt> returns @ref ResultSuccess).
    @details  The completion of the process and the result can be checked with @ref GetAsyncProcessState.
    @details  When <tt>StartupNetworkSetting.isAutoInitializeNetworkInterface</tt> is <tt>false</tt>, the state will not transition to any state earlier than @ref State.LoggedOut.
    @details  If <tt>StartupNetworkSetting.isAutoInitializeNetworkInterface</tt> is <tt>false</tt>, the asynchronous processing of the <tt>HandleErrorAsync()</tt> function might complete while the state is transitioning.
    @details  For this reason, the application must store the <tt>Result.HandlingType</tt> of the Result instance passed to this function, and make the appropriate call to @ref ChangeStateAsync() after the asynchronous processing of the <tt>HandleErrorAsync()</tt> function has completed.
    @details
    @details  The following result occurs when there is a failure using @ref GetAsyncProcessState.
    @details  <tt>ResultInvalidState</tt> indicates that the function call occurred in an invalid state. Make sure that no other asynchronous processes are running. [:progErr]
    @details  <tt>ResultInvalidArgument</tt> cannot transition to <tt>transitionState</tt>. Finalization processing is required before you can perform result handling. However, this result is returned when a <tt>transitionState</tt> is specified that is more advanced than the current state of progress. This value is also returned when a result where <tt>IsSuccess()</tt> is <tt>true</tt> is specified as an argument.
    @param[in] result  The <tt>Result</tt> that is the error handling target.
    @return  Returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> returns <tt>true</tt>. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running. [:progErr]
    @retval ResultInvalidArgument  Cannot transition to <tt><var>transitionState</var></tt>. Finalization processing is required before you can perform result handling. However, this result is returned when a <tt>transitionState</tt> is specified that is more advanced than the current state of progress. This value is also returned when a result where <tt>IsSuccess()</tt> is <tt>true</tt> is specified as an argument.
    @see  GetAsyncProcessState
*/
    public static Result HandleErrorAsync(Result result)
    {
        Result res = HandleErrorAsyncNative(result);
        return res;
    }


#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate State plugin_ConvertHandlingTypeToStateNative(HandlingType handlingType);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_ConvertHandlingTypeToState")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_ConvertHandlingTypeToState")]
#endif
    private static extern State ConvertHandlingTypeToStateNative(HandlingType handlingType);
#endif

/*!
    @brief  Converts the handling type to the state of progress of join session processing.
    @param[in] handlingType  Expected handling types.
    @return  Returns the state of progress of join session processing that the handling type was converted to.
*/
    public static State ConvertHandlingTypeToState(HandlingType handlingType)
    {
        State state = ConvertHandlingTypeToStateNative(handlingType);
        return state;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate State plugin_GetJoinProcessStateNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_GetJoinProcessState")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_GetJoinProcessState")]
#endif
    private static extern State GetJoinProcessStateNative();
#endif

/*!
    @brief  Gets the current state of the join session processing.
    @return  Returns the state of progress of join session processing.
*/
    public static State GetJoinProcessState()
    {
        State state = GetJoinProcessStateNative();
        return state;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate AsyncState plugin_GetAsyncProcessStateNative(AsyncProcessId id);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_GetAsyncProcessState")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_GetAsyncProcessState")]
#endif
    private static extern AsyncState GetAsyncProcessStateNative(AsyncProcessId id);
#endif

/*!
    @brief  Gets the state of the last-called asynchronous process.
    @details  After an asynchronous process started by a function like <tt>ChangeStateAsync()</tt> or @ref PiaPluginSession.CreateSessionAsync() has completed, this function can get the results of that asynchronous process.
    @details  Use the @ref GetAsyncProcessId() function to get the asynchronous process that was called last.
    @param[in] id  ID that identifies the type of the asynchronous process.
    @return  Returns the asynchronous processing status. <tt>true</tt> is returned for <tt>AsyncState.isCompleted</tt> when the asynchronous processing has ended. <tt>false</tt> is returned when no asynchronous processing has occurred or when it is still underway.
    If the asynchronous process has completed and this function is successful, a <tt>Result</tt> instance for which <tt>IsSuccess()</tt> is <tt>true</tt> is returned to @ref AsyncState.result.
    For more information about which result value to return if the process has failed, see the API reference for the function that started the asynchronous process.
*/
    public static AsyncState GetAsyncProcessState(AsyncProcessId id)
    {
        AsyncState asyncState = GetAsyncProcessStateNative(id);
        return asyncState;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate AsyncProcessId plugin_GetAsyncProcessIdNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_GetAsyncProcessId")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_GetAsyncProcessId")]
#endif
    private static extern AsyncProcessId GetAsyncProcessIdNative();
#endif

/*!
    @brief  Gets the type of the asynchronous process that was called last.
    @return  Returns the ID for the asynchronous process that was called last.
*/

    public static AsyncProcessId GetAsyncProcessId()
    {
        return GetAsyncProcessIdNative();
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool plugin_IsSessionMigratingNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_IsSessionMigrating")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_IsSessionMigrating")]
#endif
    private static extern bool IsSessionMigratingNative();
#endif

/*!
    @brief  Determines whether session migration is underway.
    @return  Returns <tt>true</tt> when a session migration is underway.
*/
    public static bool IsSessionMigrating()
    {
        return IsSessionMigratingNative();
    }


#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate UInt32 plugin_GetMemoryUsageNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_GetMemoryUsage")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_GetMemoryUsage")]
#endif
    private static extern UInt32 GetMemoryUsageNative();
#endif

/*!
    @brief  Gets the amount of memory being used by the Pia library at the point that this function was called.
    @return  Returns the amount of memory being used by the Pia library at the point this function was called.
*/
    public static UInt32 GetMemoryUsage()
    {
        UInt32 size = GetMemoryUsageNative();
        return size;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void plugin_StartWatermarkNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_StartWatermark")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_StartWatermark")]
#endif
    private static extern void StartWatermarkNative();
#endif

/*!
    @brief  Start the feature that uses Watermark Manager to monitor the numbers of the various types of buffer elements within Pia.
*/
    public static void StartWatermark()
    {
        StartWatermarkNative();
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void plugin_StopWatermarkNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_StopWatermark")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_StopWatermark")]
#endif
    private static extern void StopWatermarkNative();
#endif

/*!
    @brief  End the feature that uses Watermark Manager to monitor the numbers of the various types of buffer elements within Pia.
*/
    public static void StopWatermark()
    {
        StopWatermarkNative();
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void plugin_GetWatermarkArrayNative([Out] out IntPtr pWatermarkNativeArray, [Out] out int watermarkNativeArrayLength);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_GetWatermarkArray")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_GetWatermarkArray")]
#endif
    private static extern void GetWatermarkArrayNative([Out] out IntPtr pWatermarkNativeArray, [Out] out int watermarkNativeArrayLength);
#endif

/*!
    @brief  Gets all <tt>Watermark</tt> contents.
    @return  The <tt>Watermark</tt> list.
*/
    public static List<Watermark> GetWatermarkList()
    {
        IntPtr pWatermarkNativeArray = IntPtr.Zero;
        int watermarkNativeArrayLength = 0;

        GetWatermarkArrayNative(out pWatermarkNativeArray, out watermarkNativeArrayLength);
        if (watermarkNativeArrayLength == 0)
        {
            return new List<Watermark>();
        }
        WatermarkNative[] watermarkNativeArray = new WatermarkNative[watermarkNativeArrayLength];
        UnmanagedMemoryManager.ReadArray<WatermarkNative>(pWatermarkNativeArray, watermarkNativeArrayLength, ref watermarkNativeArray);

        List<Watermark> watermarkList = new List<Watermark>(watermarkNativeArrayLength);
        for (int i = 0; i < watermarkNativeArrayLength; ++i)
        {
            watermarkList.Add(new Watermark(watermarkNativeArray[i]));
        }
        return watermarkList;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Int32 plugin_GetRttNative(UInt64 constantId);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_GetRtt")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_GetRtt")]
#endif
    private static extern Int32 GetRttNative(UInt64 constantId);
#endif

/*!
    @brief  Gets the round-trip time (RTT) between the local <tt>Station</tt> and this <tt>Station</tt>.
    @details  The system sends pulses to other stations periodically to get the round-trip time (RTT).
                However, at times such as right after a station has connected to a session,
                this process of sending a pulse and getting a response has not yet been performed.
                At these times, the <tt>GetRtt()</tt> function returns <tt>InvalidRtt<</tt>, but if you let some
                time pass and then call the <tt>GetRtt()</tt> the correct value will be returned.
                This function returns the median value of multiple samples.
                As a result, the value obtained with this function is relatively stable
                and not easily affected by minor changes in the network environment.
    @param[in] constantId  Setting for <tt><var>constantId</var></tt> to get the RTT.
    @return  Returns the RTT value. The value is in milliseconds.
    @retval InvalidRtt  A negative value that indicates that the RTT value cannot be obtained in this state.
*/
    public static Int32 GetRtt(UInt64 constantId)
    {
        return GetRttNative(constantId);
    }

#if NN_PIA_ENABLE_NEX
    [DllImport("__Internal", EntryPoint = "Pia_GetNexNgsFacade")]
    private static extern IntPtr GetNexNgsFacadeNative();

/*!
    @brief  Get the NEX <tt>NgsFacade</tt> instance.
    @return  Returns a pointer to an <tt>NgsFacade</tt> instance.
*/
    public static IntPtr GetNexNgsFacade()
    {
        return GetNexNgsFacadeNative();
    }

    [DllImport("__Internal", EntryPoint = "Pia_GetNexCredentials")]
    private static extern IntPtr GetNexCredentialsNative();

/*!
    @brief  Gets the NEX credentials and the client certificate data.
    @details  To get valid credentials, you must be logged in to NGS using <tt>ChangeStateAsync()</tt>.
    @return  Returns a pointer to a <tt>Credentials</tt> instance.
*/
    public static IntPtr GetNexCredentials()
    {
        return GetNexCredentialsNative();
    }

    [DllImport("__Internal", EntryPoint = "Pia_IsRelayed")]
    private static extern bool IsRelayedNative(UInt64 constantId);

/*!
     @brief  Determines whether a relay server is being used for communication with the specified station.
     @param[in] constantId  The <tt><var>constantId</var></tt> of the station to check to determine whether a relay server is being used.
     @return  Returns <tt>true</tt> if a relay server is being used, or <tt>false</tt> if not.
*/
    public static bool IsRelayed(UInt64 constantId)
    {
        return IsRelayedNative(constantId);
    }
#endif

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int plugin_GetCryptoKeySizeNative();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_GetCryptoKeySize")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_GetCryptoKeySize")]
#endif
    private static extern int GetCryptoKeySizeNative();
#endif

/*!
    @brief  Gets the size of encryption key that can be set when LAN matchmaking.
    @return  Returns the size of the key.
*/
    public static int GetCryptoKeySize()
    {
        return GetCryptoKeySizeNative();
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void plugin_AssertNative(bool flag);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_Assert")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_Assert")]
#endif
    private static extern void AssertNative(bool flag);
#endif

/*!
    @brief  Calls a Pia assertion.
    @param[in] flag  A flag that stops the assertion if set to <tt>false</tt>.
*/
    public static void Assert(bool flag)
    {
        AssertNative(flag);
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate PiaPlugin.Result plugin_GetServerTimeNative([Out] out DateTime dateTime);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_GetServerTime")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_GetServerTime")]
#endif
    private static extern PiaPlugin.Result GetServerTimeNative([Out] out DateTime dateTime);
#endif

/*!
    @brief  Gets the server time, in UTC.
    @if NIN_DOC
    @details  Gets the value from the NEX game sever.
    @endif
    @details  Can only be used after Internet communication login. @param[out] dateTime  The server time that is set.
    @return  If successful, returns a <tt>Result</tt> instance for which <tt>IsSuccess()</tt> is <tt>true</tt>.

    @retval ResultInvalidArgument  An argument is not valid. [:progErr]
    @retval ResultInvalidState  The function was called at the wrong time. [:progErr]
*/
    public static PiaPlugin.Result GetServerTime(out DateTime dateTime)
    {
        PiaPlugin.Result result = GetServerTimeNative(out dateTime);

        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Result plugin_GetLocalAddressNative([Out, MarshalAs(UnmanagedType.LPStruct)] LocalAdressInfoNative localAddressInfo);
#else
#if UNITY_ANDROID || UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_GetLocalAddress")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_GetLocalAddress")]
#endif
    private static extern Result GetLocalAddressNative([Out, MarshalAs(UnmanagedType.LPStruct)] LocalAdressInfoNative localAddressInfo);
#endif
/*!
   @brief  Gets the local address.
   @param[out] localAddressInfo  The local address is stored here.
   @return  If the local address is successfully obtained, returns an instance of <tt>Result</tt> for which <tt>IsSuccess()</tt> returns <tt>true</tt>. If the call fails, returns the reason for the failure.
*/
    public static Result GetLocalAddress(ref LocalAdressInfo localAddressInfo)
    {
        using (LocalAdressInfoNative localAddressInfoNative = new LocalAdressInfoNative(localAddressInfo))
        {
            Result result = GetLocalAddressNative(localAddressInfoNative);
            if (result.IsSuccess())
            {
                localAddressInfo = new LocalAdressInfo(localAddressInfoNative);
            }
            return result;
        }
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate UInt32 plugin_GetDeviceLocationNameNative(ref IntPtr locationName);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_GetDeviceLocationName")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_GetDeviceLocationName")]
#endif
    private static extern void GetDeviceLocationNameNative(ref IntPtr locationName);
#endif
/*!
    @cond  PRIVATE
    @brief  Gets the time zone.
    @return  Returns the time zone. If used with a device other than NX, the function returns an empty string.
*/
    public static string GetDeviceLocationName()
    {
        string locationName = "";
        int bufferSize = 0;
        IntPtr pLocationName = UnmanagedMemoryManager.WriteUtf8(locationName, ref bufferSize);
        GetDeviceLocationNameNative(ref pLocationName);
        locationName = UnmanagedMemoryManager.ReadUtf8(pLocationName, 36);

        // Remove invalid strings and then return.
        return System.Text.RegularExpressions.Regex.Replace(locationName, @"[^\w\.@-]", "");
    }
    //! @endcond

    public class Debug
    {
#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result plugin_EnableBroadcastOnSendingToAllStationForDebugNative();
#else
#if UNITY_STANDALONE
        [DllImport("nn_piaPlugin", EntryPoint = "Pia_Debug_EnableBroadcastOnSendingToAllStationForDebug")]
#else
        [DllImport("__Internal", EntryPoint = "Pia_Debug_EnableBroadcastOnSendingToAllStationForDebug")]
#endif
        private static extern PiaPlugin.Result EnableBroadcastOnSendingToAllStationForDebugNative();
#endif

/*!
        @brief  (For debugging) Enables the use of broadcasting when sending to all devices in the session.
*/
        public static PiaPlugin.Result EnableBroadcastOnSendingToAllStationForDebug()
        {
            return EnableBroadcastOnSendingToAllStationForDebugNative();
        }
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void plugin_TraceNative(IntPtr mes);
#else
#if UNITY_ANDROID || UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Pia_Trace")]
#else
    [DllImport("__Internal", EntryPoint = "Pia_Trace")]
#endif
    private static extern void TraceNative(IntPtr mes);
#endif
/*!
     @cond  PRIVATE
*/
/*!
    @brief  Displays a log of the trace of Pia. Displayed if <tt>PiaPlugin.TraceFlag.Plugin</tt> is enabled.
    @details  Not displayed when the development build is disabled.
    @details  If .NET 4.x is not available, the caller's filename and member name are not shown in the trace.
    @param[in] The  message to display in the trace.
*/
    //! @endcond
#if !UNITY_EDITOR
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
#endif
    public static void Trace(string msg
#if NET_4_6
        ,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
        )
    {
        int bufferSize = 0;
#if NET_4_6
        string trcae = Path.GetFileName(sourceFilePath) + " " + memberName + ":" + msg;
#else
        string trcae = msg;
#endif
        IntPtr mesPtr = UnmanagedMemoryManager.WriteUtf8(trcae, ref bufferSize);
        TraceNative(mesPtr);
    }
}
