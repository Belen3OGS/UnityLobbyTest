using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Collections;
using Mirror;

namespace Multiplayer.LobbyManagement
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField]
        private InputField lobbyName;
        [SerializeField]
        private Text foundLobby;
        public bool IsConnectedToLobby { get; private set; } = false;
        public Player LoggedInPlayer { get; private set; }
        public Guid PlayerAllocationId { get; private set; }

        public event Action<string> OnLobbyJoined;
        public event Action OnLobbyCreated;

        private Lobby _currentLobby;
        private List<Lobby> _currentLobbyList;
        private bool _unityServicesInitialized = false;


#if UNITY_SWITCH
        [SerializeField]
        private NexPiaInitializerCore m_InitializerCore;

        private PiaPlugin.NetworkType m_NetworkType;
        private bool m_IsHost;
        private uint m_SelectJoinSessionNum; // Specify which session to join.
        private PiaPluginSession.SessionStatus m_SessionStatus;
        private GameState m_GameState;
        private GameState m_NextGameState; // The next transition destination when asynchronous processing was successfully executed.
        private NexPiaInitializerCore.LocalMultiPlayType m_LocalMultiPlayType = NexPiaInitializerCore.LocalMultiPlayType.None;
        private PiaPluginUtil.VersionInfo m_PiaVersionInfo;
        private PiaPluginUtil.VersionInfo m_PiaUnityVersionInfo;
        private const ushort GameMode = 12370; // Change according to the game mode.
        public const int PlayerNumMax = 8; // Maximum number of player participants.
        private const string WirelessCryptoKey = "Unity_PiaBump_WirelessCryptoKey";
        private const int ApplicationDataBufferSize = 10; // Must be less than the value of PiaPluginSession.ApplicationDataBufferSizeMax.
        private byte[] m_ApplicationDataBuffer = new byte[ApplicationDataBufferSize]; // Application-defined data send/receive buffer.
        private int m_ApplicationDataBufferSize = ApplicationDataBufferSize;
        uint listSize = 0;
        private bool joinSession = false;
        private bool createSession = false;

        public enum GameState
        {
            ChangeStateAsync,
            Title,
            NexPiaInitialize,
            NetworkStartedUp,
            PiaStartedUp,
            BrowseSession,
            WaitingBrowseSessionCompleted,
            CreateOrJoinSession,
            JoinProcess,
            Game,
            Logout,
            PiaFrameworkFinalize,
            ErrorHandling,
            Max
        }

        class ApplicationDataStruct
        {
            public System.UInt32 testNum;
            public string testName;

            public int Serialize(byte[] destinationBuf)
            {
                int offset = 0;
                byte[] buffer;
                buffer = System.BitConverter.GetBytes(testNum);
                buffer.CopyTo(destinationBuf, offset);
                offset += buffer.Length;
                buffer = System.Text.Encoding.ASCII.GetBytes(testName);
                buffer.CopyTo(destinationBuf, offset);
                offset += buffer.Length;
                return offset;
            }

            public void Deserialize(byte[] buffer, uint dataSize)
            {
                int offset = 0;
                testNum = System.BitConverter.ToUInt32(buffer, 0);
                offset += 4;
                testName = System.Text.Encoding.ASCII.GetString(buffer, offset, (int)dataSize - offset);
            }
        }

#endif

        #region Initialization

        void Start()
        {

#if UNITY_SWITCH
            m_NetworkType = PiaPlugin.NetworkType.Local;
            m_IsHost = true;
            m_SelectJoinSessionNum = 0;
            m_SessionStatus = new PiaPluginSession.SessionStatus();
#endif

#if UNITY_ONLY_SWITCH
        // Continue the process even if the application moves to the background.
        UnityEngine.Switch.Notification.SetFocusHandlingMode(UnityEngine.Switch.Notification.FocusHandlingMode.Notify);
#endif
            m_PiaUnityVersionInfo = new PiaPluginUtil.VersionInfo();
            m_PiaVersionInfo = new PiaPluginUtil.VersionInfo();
            //StartCoroutine(InitializeUnityServices());

            //UILogManager.log.Write("Logged in as: " + LoggedInPlayer.Id);

        }

        void Update()
        {
            PiaPlugin.Result result;
#if UNITY_ONLY_SWITCH
        nn.Result nnResult;
#endif
            PiaPlugin.AsyncState asyncState;

            // Update NexPlugin.
#if PIA_ENABLE_NEX_PLUGIN
        if (m_NetworkType == PiaPlugin.NetworkType.Internet)
        {
            NexPlugin.Common.Dispatch(3);
            if (!NexPlugin.Common.UpdateAsyncResult())
            {
                if (NexPlugin.Common.IsInitializedNex())
                {
                    PiaPluginUtil.UnityLog("NexPlugin.Common.UpdateAsyncResult failed.");
                }
            }
        }
#endif

            // Update Pia.
            PiaPlugin.Dispatch();
            PiaPlugin.Result dispatchResult = PiaPlugin.CheckDispatchError();
            if (dispatchResult.IsFailure())
            {
                PiaPluginUtil.UnityLog(string.Format("Update is failed state:{0}", m_GameState));
                dispatchResult.Trace();
                if (m_GameState != GameState.ChangeStateAsync && m_GameState != GameState.JoinProcess && m_GameState != GameState.ErrorHandling)
                {
                    // Start error handing if other asynchronous processing is underway.
                    HandleError(dispatchResult);
                }
            }

            //m_GameManager.CheckSessionEvent();

            switch (m_GameState)
            {
                case GameState.ChangeStateAsync:
                    // Check whether asynchronous processing was successfully executed. If there were no problems, transition to the specified GameState.
                    asyncState = PiaPlugin.GetAsyncProcessState(PiaPlugin.AsyncProcessId.ChangeState);
                    if (asyncState.isCompleted)
                    {
                        if (asyncState.result.IsFailure())
                        {
                            PiaPluginUtil.UnityLog("ChangeState is failed");
                            asyncState.result.Trace();
                            HandleError(asyncState.result);
                            break;
                        }
                        //m_GameManager.Reset();
                        m_GameState = m_NextGameState;
                    }
                    break;

                case GameState.Title:

                        m_GameState = GameState.NexPiaInitialize;
                        //m_IsStreamActive = false;
                        //m_IsStreamReseted = true;
                        // Enable Pia logging.
#if NN_PIA_ENABLE_PIA_LOG && UNITY_EDITOR_OR_STANDALONE
                    PiaPluginUtil.EnablePiaLog();
#endif                  
                    break;

                case GameState.NexPiaInitialize:
                    // Select the user and change to the open state.
//#if UNITY_ONLY_SWITCH
//                NNAccountSetup.OpenUserWithShowUserSelector();
//#endif

                    {
                        PiaPluginUtil.UnityLog("m_NetworkType is" + m_NetworkType);
                        m_InitializerCore.SetNetworkType(m_NetworkType);
                        // Framework initialization processing.
                        result = m_InitializerCore.InitializeFramework(false, m_LocalMultiPlayType);
                        if (result.IsFailure())
                        {
                            PiaPluginUtil.UnityLog("InitializeFramework is failed " + result.resultValue.ToString());
                            m_GameState = GameState.Title;
                            return;
                        }
#if UNITY_EDITOR
                        PiaPluginUtil.GetNativeVersionInfo(ref m_PiaVersionInfo, ref m_PiaUnityVersionInfo);
#endif
                    }
                    // Transition to a state in which NetworkInterface is initialized.
                    result = PiaPlugin.ChangeStateAsync(PiaPlugin.State.NetworkStartedUp);
                    if (result.IsFailure())
                    {
                        PiaPluginUtil.UnityLog("ChangeStateAsync is failed");
                        result.Trace();
                        HandleError(result);
                        break;
                    }
                    m_GameState = GameState.ChangeStateAsync;
                    m_NextGameState = GameState.NetworkStartedUp;
                    break;

                case GameState.NetworkStartedUp:
#if PIA_ENABLE_NEX_PLUGIN
                if (m_NetworkType == PiaPlugin.NetworkType.Internet && !NexPlugin.Common.IsInitializedNex())
                {
                    if (isNexAsyncFunctionCompleted)
                    {
                        // When used together with NexPlugin, NexPlugin initialization processing is performed after Pia has finished initializing NEX.
                        if (!NexPlugin.Common.InitializeNexPlugin((1024 * 2) * 1024))
                        {
                            PiaPluginUtil.UnityLog("NexPlugin.Common.InitializeNexPlugin is failed");
                        }
                        InitializeNex();
                    }
                    break;
                }
                PiaPluginUtil.UnityLog("InitializeNex is Completed.");
#endif
                    // Transition to a state in which Pia has started.
                    result = PiaPlugin.ChangeStateAsync(PiaPlugin.State.SessionStartedUp);
                    if (result.IsFailure())
                    {
                        PiaPluginUtil.UnityLog("ChangeStateAsync is failed");
                        result.Trace();
                        HandleError(result);
                        break;
                    }
                    m_GameState = GameState.ChangeStateAsync;
                    m_NextGameState = GameState.PiaStartedUp;
                    break;
                case GameState.PiaStartedUp:
                    m_GameState = GameState.BrowseSession;
                    break;
                case GameState.BrowseSession:
                    // Search for sessions.
                    {
#if SESSION_SEARCH_CRITERIA_OWNER_TEST
                    if (m_HostConstantId != 0 && m_NetworkType == PiaPlugin.NetworkType.Internet)
                    {
                        // For checking PiaPluginSession.SessionSearchCriteriaOwner operation.
                        PiaPluginUtil.UnityLog("Use PiaPluginSession.SessionSearchCriteriaOwner.");
                        PiaPluginSession.SessionSearchCriteriaOwner criteria = new PiaPluginSession.SessionSearchCriteriaOwner();
                        criteria.SetOwnerConstantId(m_HostConstantId);
                        criteria.resultRange = new System.UInt32[2] { 0, 4 };
                        result = PiaPluginSession.BrowseSessionAsync(criteria);
                    }else
#elif SESSION_SEARCH_CRITERIA_PATICIPANT_TEST
                    if (m_HostConstantId != 0 && m_NetworkType == PiaPlugin.NetworkType.Internet)
                    {
                        // For checking PiaPluginSession.SessionSearchCriteriaParticipant operation.
                        PiaPluginUtil.UnityLog("Use PiaPluginSession.SessionSearchCriteriaParticipant.");
                        PiaPluginSession.SessionSearchCriteriaParticipant criteria = new PiaPluginSession.SessionSearchCriteriaParticipant();
                        criteria.participantConstantIdList.Add(m_HostConstantId);
                        criteria.isApplicationDataEnabled = true;
                        result = PiaPluginSession.BrowseSessionAsync(criteria);
                    }else
#endif
                        {
                            PiaPluginSession.SessionSearchCriteria criteria = new PiaPluginSession.SessionSearchCriteria();
                            criteria.SetGameMode(GameMode);
                            criteria.SetParticipantNumMax(PlayerNumMax);
                            criteria.resultRange = new System.UInt32[] { 0, 4 };
                            result = PiaPluginSession.BrowseSessionAsync(criteria);
                        }
                        if (result.IsFailure())
                        {
                            PiaPluginUtil.UnityLog("BrowseSessionAsync is failed");
                            result.Trace();
                            HandleError(result);
                            break;
                        }
                        m_SelectJoinSessionNum = 0;
                        m_GameState = GameState.WaitingBrowseSessionCompleted;
                    }
                    break;
                case GameState.WaitingBrowseSessionCompleted:
                    // Wait until session search is completed.
                    asyncState = PiaPlugin.GetAsyncProcessState(PiaPlugin.AsyncProcessId.BrowseSession);
                    if (asyncState.isCompleted)
                    {
                        if (asyncState.result.IsFailure())
                        {
                            PiaPluginUtil.UnityLog("GetBrowseSessionResult is failed");
                            asyncState.result.Trace();
                            HandleError(asyncState.result);
                            break;
                        }
                        m_GameState = GameState.CreateOrJoinSession;
                    }
                    break;
                case GameState.CreateOrJoinSession:
                    // The search results determine whether to create a session or join a session.
                    {
                        if (PiaPlugin.GetAsyncProcessState(PiaPlugin.AsyncProcessId.BrowseSession).isCompleted)
                        {
                            if (PiaPlugin.GetAsyncProcessState(PiaPlugin.AsyncProcessId.BrowseSession).result.IsSuccess())
                            {
                                listSize = PiaPluginSession.GetBrowsedSessionPropertyListSize();
                            }
                        }

                        if (true)
                        {
                            if (joinSession)
                            {
                                PiaPluginSession.JoinSessionSetting setting = new PiaPluginSession.JoinSessionSetting();
                                setting.selectJoinSessionNum = m_SelectJoinSessionNum;
                                setting.wirelessCryptoKey = WirelessCryptoKey;

                                // Join the session for the found ('n'th) argument.
                                result = PiaPluginSession.JoinSessionAsync(setting);

                                m_IsHost = false;
                                joinSession = false;

                                if (result.IsFailure())
                                {
                                    PiaPluginUtil.UnityLog("JoinSessionAsync or CreateSessionAsync is failed");
                                    result.Trace();
                                    HandleError(result);
                                    break;
                                }

                            }
                            else if(createSession)
                            {
                                // If a session is not found, create a new one.
                                PiaPluginSession.CreateSessionSetting createSessionSetting = new PiaPluginSession.CreateSessionSetting();
                                createSessionSetting.SetGameMode(GameMode);
                                createSessionSetting.SetParticipantNumMax(PlayerNumMax);
                                createSessionSetting.SetWirelessCryptoKey(WirelessCryptoKey);
                                createSessionSetting.SetOpenSession(true);
                                ApplicationDataStruct applicationData = new ApplicationDataStruct();
                                applicationData.testNum = 254;
                                applicationData.testName = "test";
                                int size = applicationData.Serialize(m_ApplicationDataBuffer);
                                PiaPlugin.Assert(size <= ApplicationDataBufferSize);
                                createSessionSetting.SetApplicationData(m_ApplicationDataBuffer);
                                PiaPluginSession.CreateSessionSetting.Attribute attribute = new PiaPluginSession.CreateSessionSetting.Attribute();
                                attribute.index = 0;
                                attribute.value = 1;
                                List<PiaPluginSession.CreateSessionSetting.Attribute> attributeList = new List<PiaPluginSession.CreateSessionSetting.Attribute>();
                                attributeList.Add(attribute);
                                createSessionSetting.SetAttributeList(attributeList);
                                result = PiaPluginSession.CreateSessionAsync(createSessionSetting);
                                m_IsHost = true;
                                createSession = false;

                                if (result.IsFailure())
                                {
                                    PiaPluginUtil.UnityLog("JoinSessionAsync or CreateSessionAsync is failed");
                                    result.Trace();
                                    HandleError(result);
                                    break;
                                }

                            }


                            m_GameState = GameState.JoinProcess;
                        }
                    }
                    break;
                case GameState.JoinProcess:
                    result = PiaPlugin.ProgrammingErrorResult;
                    // Verify whether a session was successfully created or joined.
                    if (!m_IsHost)
                    {
                        asyncState = PiaPlugin.GetAsyncProcessState(PiaPlugin.AsyncProcessId.JoinSession);
                        if (asyncState.isCompleted)
                        {
                            result = asyncState.result;
                        }
                    }
                    else
                    {
                        asyncState = PiaPlugin.GetAsyncProcessState(PiaPlugin.AsyncProcessId.CreateSession);
                        if (asyncState.isCompleted)
                        {
                            result = asyncState.result;
                        }
                    }

                    // If there were no problems, move to the game scene.
                    if (asyncState.isCompleted)
                    {
                        PiaPluginUtil.UnityLog("IsJoinSessionCompleted or IsCreateSessionCompleted is completed");
                        if (result.IsSuccess())
                        {
                            //m_GameManager.Reset();
                            m_GameState = GameState.Game;
                            // Clear the information about the completion of asynchronous processes checked during the game.
                            //m_IsAsyncProcessing = false;
                        }
                        else
                        {
                            PiaPluginUtil.UnityLog("GetJoinSessionResult or GetCreateSessionResult is failed");
                            result.Trace();
                            HandleError(result);
                            break;
                        }
                    }
                    break;
//                case GameState.Game:
//                    PiaPluginSession.GetSessionStatus(ref m_SessionStatus);
//                    m_IsHost = (m_SessionStatus.localConstantId == m_SessionStatus.hostConstantId);

//                    // Common process for all key assignments.
//                    //Log out.
//                    if (GamepadFacade.gamepad.L.isDown)
//                    {
//#if SESSION_SEARCH_CRITERIA_OWNER_TEST || SESSION_SEARCH_CRITERIA_PATICIPANT_TEST
//                    if (!m_IsHost)
//                    {
//                        m_HostConstantId = m_SessionStatus.hostConstantId;
//                    }
//#endif
//                        m_GameState = GameState.Logout;
//                        break;
//                    }
//                    // Switch key assignments.
//                    if (GamepadFacade.gamepad.R.isDown)
//                    {
//                        int nextKeyAssign = (int)m_KeyAssign + 1;
//                        m_KeyAssign = (KeyAssign)(nextKeyAssign % (int)KeyAssign.AssignNum);
//                    }

//                    // Process for each key assignment.
//                    switch (m_KeyAssign)
//                    {
//                        case KeyAssign.Session:
//                            // Start PiaSync and the game.
//                            if (GamepadFacade.gamepad.A.isDown)
//                            {
//                                StartGame();
//                            }
//                            // End synchronous communication.
//                            if (GamepadFacade.gamepad.B.isDown)
//                            {
//                                EndGame();
//                            }
//                            // Open and close session.
//                            if (GamepadFacade.gamepad.X.isDown)
//                            {
//                                SwitchSessionOpenClose();
//                            }
//                            // Update the session settings.
//                            if (GamepadFacade.gamepad.Y.isDown)
//                            {
//                                UpdateSessionSetting();
//                            }
//                            break;
//                        case KeyAssign.StreamBroadcast:
//                            // Start StreamBroadcastReliable sending.
//                            if (GamepadFacade.gamepad.A.isDown)
//                            {
//                                SendStreamBroadcast();
//                            }
//                            //Cancel during StreamBroadcastReliable sending.
//                            if (GamepadFacade.gamepad.B.isDown)
//                            {
//                                CancelStreamBroadcast();
//                            }
//                            // Request StreamBroadcastReliable sending.
//                            if (GamepadFacade.gamepad.X.isDown)
//                            {
//                                RequestStreamBroadcast();
//                            }
//                            //  Output log of session information.
//                            if (GamepadFacade.gamepad.Y.isDown)
//                            {
//                                RequestSessionProperty();
//                            }
//                            break;
//                    }

//                    // Check whether asynchronous processing is underway.
//                    if (m_IsAsyncProcessing)
//                    {
//                        PiaPlugin.AsyncProcessId asyncProcessId = PiaPlugin.GetAsyncProcessId();
//                        if (PiaPlugin.GetAsyncProcessState(asyncProcessId).isCompleted)
//                        {
//                            PiaPluginUtil.UnityLog(asyncProcessId.ToString() + " completed.");
//                            m_IsAsyncProcessing = false;

//                            if (asyncProcessId == PiaPlugin.AsyncProcessId.RequestSessionProperty)
//                            {
//                                PiaPluginSession.SessionProperty sessionInfo = PiaPluginSession.GetSessionProperty();
//                                m_ApplicationDataBufferSize = (int)sessionInfo.applicationDataSize;
//                                PiaPluginUtil.UnityLog(String.Format(
//                                    "--GetSessionInfo--\n"
//                                    + "applicationDataSize : {0} \n"
//                                    + "gameMode : {1} \n"
//                                    + "participantNumMax : {2} \n"
//                                    + "participantNumMin : {3} \n"
//                                    + "currentParticipantNum : {4} \n"
//                                    + "isOpened ; {5} \n",
//                                    sessionInfo.applicationDataSize,
//                                    sessionInfo.gameMode,
//                                    sessionInfo.participantNumMax,
//                                    sessionInfo.participantNumMin,
//                                    sessionInfo.currentParticipantNum,
//                                    sessionInfo.isOpened
//                                    ));
//                            }
//                        }
//                    }

//                    // Advances one frame.
//                    result = PiaPluginSync.Step();
//                    if (result.IsFailure())
//                    {
//                        if (result.resultValue == PiaPlugin.ResultValue.ResultDataIsNotArrivedYet)
//                        {
//                            // Occurs when the error was not a fatal error and data have still not been received.
//                            ++m_FrameDropCount;
//                        }
//                        break;
//                    }

//                    // Runs the reset process when frameNo is 0.
//                    if (PiaPluginSync.GetFrameNo() == 0)
//                    {
//                        m_GameManager.Reset();
//                        m_FrameDropCount = 0;
//                        if (m_IsStreamReseted == false)
//                        {
//                            m_IsStreamReseted = true;
//                            m_IsStreamActive = false;
//                        }
//                    }
//                    else
//                    {
//                        if (m_IsStreamReseted && m_IsStreamActive)
//                        {
//                            m_IsStreamReseted = false;
//                        }
//                    }

//                    // Game progress. PiaPluginSync.GetData and PiaPluginSync.SetData are performed even if not in State.Synchronized.
//                    m_GameManager.GameUpdate();

//                    // Update TransportAnalyzer and get data.
//                    if (m_isAutoPrintTransportAnalysisData)
//                    {
//                        m_TimeForTransportAnalyzer += Time.deltaTime;
//                        if ((int)m_TimeForTransportAnalyzer >= 5.0f)
//                        {
//                            PrintTransportAnalysisData();
//                            m_TimeForTransportAnalyzer = 0.0f;
//                        }
//                    }
//                    break;
                case GameState.Logout:
                    // Transition to a state in which nothing is initialized.
                    result = PiaPlugin.ChangeStateAsync(PiaPlugin.State.LoggedOut);
                    if (result.IsFailure())
                    {
                        PiaPluginUtil.UnityLog("ChangeStateAsync is failed");
                        result.Trace();
                        break;
                    }
                    m_GameState = GameState.ChangeStateAsync;
                    m_NextGameState = GameState.PiaFrameworkFinalize;
                    break;
                case GameState.PiaFrameworkFinalize:
#if PIA_ENABLE_NEX_PLUGIN
                // When used together with NexPlugin, PiaPlugin termination processing is performed after NexPlugin termination processing is finished.
                // Only when error handling was performed when it was NetworkCleanedUp. In this case, NEX termination processing should have been completed, so it is not performed here.
                if (m_NetworkType == PiaPlugin.NetworkType.Internet && (PiaPlugin.GetJoinProcessState() != PiaPlugin.State.NetworkCleanedUp))
                {
                    // Only when error handling was performed when it was NetworkCleanedUp. In this case, NEX termination processing should have been completed, so it is not performed here.
                    if (PiaPlugin.GetJoinProcessState() != PiaPlugin.State.NetworkCleanedUp)
                    {
                        PiaPluginUtil.UnityLog("[Bump] FinalizeNex");
                        NexPlugin.Common.FinalizeNex();
                    }

                    PiaPluginUtil.UnityLog("[Bump] FinalizeNexPlugin");
                    NexPlugin.Common.FinalizeNexPlugin();
                }
#endif
                    PiaPlugin.FinalizeAll();

#if UNITY_ONLY_SWITCH
                // Change the user to the closed state.
                NNAccountSetup.CloseUser();
#endif
                    m_GameState = GameState.Title;
                    break;

                case GameState.ErrorHandling:
                    asyncState = PiaPlugin.GetAsyncProcessState(PiaPlugin.AsyncProcessId.HandleError);
                    if (asyncState.isCompleted)
                    {
                        PiaPluginUtil.UnityLog("HandleError is completed");

                        if (asyncState.result.IsFailure())
                        {
                            PiaPluginUtil.UnityLog("GetHandleErrorResult is failed");
                            asyncState.result.Trace();
                            HandleError(asyncState.result);
                            break;
                        }

                        //m_GameManager.Reset();
                        if (PiaPlugin.GetJoinProcessState() == PiaPlugin.State.LoggedOut)
                        {
                            // If logged out, transition to PiaFrameworkFinalize.
                            // Pia's error handling only transitions to LoggedOut, so manually proceed to NetworkCleanedUp.
#if PIA_ENABLE_NEX_PLUGIN
                        // You must call FinalizeNex() before removing the settings for the NEX memory allocation/release functions when proceeding to NetworkCleanedUp.
                        PiaPluginUtil.UnityLog("[Bump] FinalizeNex");
                        NexPlugin.Common.FinalizeNex();
#endif
                            result = PiaPlugin.ChangeStateAsync(PiaPlugin.State.NetworkCleanedUp);
                            if (result.IsFailure())
                            {
                                PiaPluginUtil.UnityLog("ChangeStateAsync(NetworkCleanedUp) is failed");
                                result.Trace();
                                HandleError(result);
                                break;
                            }
                            m_GameState = GameState.ChangeStateAsync;
                            m_NextGameState = GameState.PiaFrameworkFinalize;
                        }
                        else if (PiaPlugin.GetJoinProcessState() != PiaPlugin.State.NetworkCleanedUp)
                        {
                            // If the logged-in state is maintained, advance Pia to the started-up state.
                            PiaPluginUtil.UnityLog("ChangeStateAsync(State_PiaStartedUp)");
                            result = PiaPlugin.ChangeStateAsync(PiaPlugin.State.SessionStartedUp);
                            if (result.IsFailure())
                            {
                                PiaPluginUtil.UnityLog("ChangeStateAsync(State_PiaStartedUp) is failed");
                                result.Trace();
                                HandleError(result);
                                break;
                            }
                            m_GameState = GameState.ChangeStateAsync;
                            m_NextGameState = GameState.PiaStartedUp;
                        }
                        else
                        {
                            // Do nothing because it never transitions to NetworkCleanedUp to finish error handling.
                        }
                    }
                    break;
            }

            //++updateCount;
            //if (updateCount % 10 == 0)
            //{
            //    m_Fps = (int)(1 / Time.deltaTime);
            //    updateCount = 0;
            //}
        }


        void HandleError(PiaPlugin.Result errorResult)
        {
            PiaPluginUtil.UnityLog("HandleError called");
            errorResult.Trace();
            PiaPlugin.Assert(errorResult.IsFailure());

            if (errorResult.viewerType == PiaPlugin.ViewerType.ShouldUse)
            {
#if UNITY_ONLY_SWITCH
            if (errorResult.resultValue == PiaPlugin.ResultValue.ResultSdkViewerResultError)
            {
                // When ResultSdkViewerResultError is returned, get the nn.Result that will display the error.
                nn.Result sdkResult = errorResult.GetErrorResult();
                nn.err.Error.Show(sdkResult);
            }
            else
            {
                nn.err.Error.Show(errorResult.GetErrorCode());
            }
#endif
            }

            PiaPlugin.State nextSessionState = PiaPlugin.ConvertHandlingTypeToState(errorResult.handlingType);

            PiaPlugin.Result result = PiaPlugin.HandleErrorAsync(errorResult);
            if (result.IsFailure())
            {
                PiaPluginUtil.UnityLog("HandleErrorAsync failed.");
                result.Trace();
                PiaPlugin.Assert(false);
                return;
            }

            m_GameState = GameState.ErrorHandling;
        }

        public bool IsUnityServicesInitialized()
        {
            if (!_unityServicesInitialized)
            {
                Debug.LogError("Unity Services have not been initialized!");
                return false;
            }
            else
            {
                return true;
            }
        }

        private IEnumerator InitializeUnityServices()
        {
            //Initialize Unity Services
            var initTask = UnityServices.InitializeAsync();
            while (!initTask.IsCompleted)
            {
                yield return null;
            }
            if (initTask.IsFaulted)
            {
                Debug.LogError("Unity Services Initialization Failed");
                yield break;
            }

            //Log in player
            var logInTask = GetPlayerFromAnonymousLoginAsync();
            while (!logInTask.IsCompleted)
            {
                yield return null;
            }
            if (logInTask.IsFaulted)
            {
                Debug.LogError("Failed Player Log-in!");
                yield break;
            }
            else
                LoggedInPlayer = logInTask.Result;

            _unityServicesInitialized = true;

            UILogManager.log.Write("Logged in as: " + LoggedInPlayer.Id);
        }
        #endregion

        #region Button Functions
        public void FindLobbysButton()
        {
            if (m_SelectJoinSessionNum < listSize)
            {
                ++m_SelectJoinSessionNum;
            }
            else
                m_SelectJoinSessionNum = 0;

            PiaPluginSession.SessionProperty sessionProperty = PiaPluginSession.GetBrowsedSessionProperty(m_SelectJoinSessionNum);
            foundLobby.text = String.Format("-> JoinSession : {0} (ListSize: {1}, IsRestrictedByUserPassword: {2})", sessionProperty.sessionId, listSize, sessionProperty.isRestrictedByUserPassword);

#if !UNITY_SWITCH
            StartCoroutine(FindLobbys());
#endif
        }

        public void JoinLobbyButton()
        {
            joinSession = true;
#if !UNITY_SWITCH
            StartCoroutine(JoinFirstLobby());
#endif

        }

        public void CreatePiaSessionButton()
        {
            createSession = true;
        }

        #endregion

        #region Lobby creation and joining
        public IEnumerator CreateLobby(string address, int maxPlayers, bool isPrivate = false)
        {
            if (!IsUnityServicesInitialized()) yield break;

            // Add some data to our player
            // This data will be included in a lobby under players -> player.data
            LoggedInPlayer.Data.Add("Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "No"));

            //Creating Lobby Data
            var lobbyData = new Dictionary<string, DataObject>()
            {
                ["Test"] = new DataObject(DataObject.VisibilityOptions.Public, "true", DataObject.IndexOptions.S1),
                ["GameMode"] = new DataObject(DataObject.VisibilityOptions.Public, "ctf", DataObject.IndexOptions.S2),
                ["Skill"] = new DataObject(DataObject.VisibilityOptions.Public, Random.Range(1, 51).ToString(), DataObject.IndexOptions.N1),
                ["Rank"] = new DataObject(DataObject.VisibilityOptions.Public, Random.Range(1, 51).ToString()),
                ["Address"] = new DataObject(DataObject.VisibilityOptions.Member, address, DataObject.IndexOptions.S3),
            };

            // Create a new lobby
            var createLobby = LobbyService.Instance.CreateLobbyAsync(
                lobbyName: lobbyName.text,
                maxPlayers: maxPlayers,
                options: new CreateLobbyOptions()
                {
                    Data = lobbyData,
                    IsPrivate = isPrivate,
                    Player = LoggedInPlayer
                });
            while (!createLobby.IsCompleted)
                yield return null;
            if (createLobby.IsFaulted)
            {
                Debug.LogError("Lobby Creation Failed!");
                yield break;
            }
            _currentLobby = createLobby.Result;
            StartCoroutine(HeartbeatLobbyCoroutine(_currentLobby.Id, 15));
            IsConnectedToLobby = true;
            OnLobbyCreated?.Invoke();

            Debug.Log(LoggedInPlayer.Id); 

            yield break;
        }

        private IEnumerator FindLobbys()
        {
            if (!IsUnityServicesInitialized()) yield break;

            List<QueryFilter> queryFilters = new List<QueryFilter>
        {
            // Let's search for games with open slots (AvailableSlots greater than 0)
            new QueryFilter(
                field: QueryFilter.FieldOptions.AvailableSlots,
                op: QueryFilter.OpOptions.GT,
                value: "0"),
            new QueryFilter(QueryFilter.FieldOptions.Name,lobbyName.text,QueryFilter.OpOptions.EQ)

        };
            List<QueryOrder> queryOrdering = new List<QueryOrder>
        {
            new QueryOrder(true, QueryOrder.FieldOptions.AvailableSlots),
            new QueryOrder(false, QueryOrder.FieldOptions.Created),
            new QueryOrder(false, QueryOrder.FieldOptions.Name),
        };

            //Find Lobby Query
            var findLobbyQuery = LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions()
            {
                Count = 100, // Override default number of results to return
                Filters = queryFilters,
                Order = queryOrdering,
            });
            while (!findLobbyQuery.IsCompleted)
            {
                yield return null;
            }
            if (findLobbyQuery.IsFaulted)
            {
                Debug.LogError("Lobby list retrieval failed!");
            }
            QueryResponse response = findLobbyQuery.Result;

            _currentLobbyList = response.Results;
        }

        private IEnumerator JoinFirstLobby()
        {
            if (!IsUnityServicesInitialized()) yield break;

            if (_currentLobbyList.Count > 0)
            {
                var joinLobbyTask = LobbyService.Instance.JoinLobbyByIdAsync(
                    lobbyId: _currentLobbyList[0].Id,
                    options: new JoinLobbyByIdOptions()
                    {
                        Player = LoggedInPlayer
                    });
                while (!joinLobbyTask.IsCompleted)
                {
                    yield return null;
                }
                if (joinLobbyTask.IsFaulted)
                {
                    Debug.LogError("Join Lobby request failed");
                    yield break;
                }
                _currentLobby = joinLobbyTask.Result;
                string adress = _currentLobby.Data["Address"].Value;
                OnLobbyJoined?.Invoke(adress);
            }
        }
#endregion

#region Helper Functions
        public async void DisconnectPlayer(string playerId)
        {
            await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, playerId);
        }

        public async Task<string> GetLastJoinedPlayerId()
        {
            _currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);
            return _currentLobby.Players[_currentLobby.Players.Count - 1].Id;
        }

        private async Task<Player> GetPlayerFromAnonymousLoginAsync()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                // Use Unity Authentication to log in
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    throw new InvalidOperationException("Player was not signed in successfully; unable to continue without a logged in player");
                }
            }

            // Player objects have Get-only properties, so you need to initialize the data bag here if you want to use it
            return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject>());
        }

        private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            var delay = new WaitForSecondsRealtime(waitTimeSeconds);
            while (true)
            {
                Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return delay;
            }
        }
#endregion

#region Disposal

        public void DisconnectFromLobby()
        {
            if (IsConnectedToLobby && _currentLobby != null)
            {
                LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, LoggedInPlayer.Id);
                IsConnectedToLobby = false;
            }
        }

        public void StopLobby()
        {
            StopAllCoroutines();
            if (IsConnectedToLobby && _currentLobby != null && _currentLobby.HostId == LoggedInPlayer.Id)
            {
                Lobbies.Instance.DeleteLobbyAsync(_currentLobby.Id);
                IsConnectedToLobby = false;
            }
        }

#endregion
    }
}
