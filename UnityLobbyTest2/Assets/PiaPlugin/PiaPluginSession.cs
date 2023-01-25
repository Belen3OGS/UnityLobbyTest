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
//! @brief  Class that is a compilation of the features used by <tt>PiaSession</tt>.
// -----------------------------------------------------------------------
public class PiaPluginSession
{
    // nex::AttributeSizeMax
    const int AttributeSizeMax = 6;  //!<  The number of attributes that can be specified in a session.
    // session::DefaultSessionKeepAliveInterval
    public const int DefaultSessionKeepAliveInterval = 1000;  //!<  The default value for the sending interval (in milliseconds) for keep-alive.
    // session::SessionSilenceTimeMaxDefault
    public const int DefaultSessionSilenceTimeMax = 10000;    //!<  The default value for the amount of time without communication (in milliseconds) after which it is determined that the communication with the station has been disconnected.

    public const int LanApplicationDataBufferSizeMax = 384;         //!<  Maximum size that can be set as application defined data during LAN communication.

    public const int UpdateMeshSendingIntervalDefault = 1000;  //!<  The default send interval for the message for synchronizing the participants in the mesh (in milliseconds).

#if UNITY_ONLY_SWITCH
    // n2908: A user password of up to 32 characters can be set for Internet communication, but set the size to match that for local communication.
    private const int SessionUserPasswordLengthMax = 8;                 //!<  The maximum number of characters that can be set for the user password.
    public const int InetApplicationDataBufferSizeMax = 512;            //!<  The maximum size that can be set as application-defined data during Internet communication.
    public const int LocalApplicationDataBufferSizeMax = 360;           //!<  The maximum size that can be set as application-defined data during local communication.
//    public const int ApplicationDataBufferSizeMax = 364; //! <  The maximum size that can be set as application-defined data.
    private const int ApplicationDataSystemBufferSizeMax = 512;         //!<  The maximum size that can be set as application-defined data. (For use by the system.)
#else
    private const int SessionUserPasswordLengthMax = 8;                 //!<  The maximum number of characters that can be set for the user password.
    public const int InetApplicationDataBufferSizeMax = 364;            //!<  The maximum size that can be set as application-defined data during Internet communication.
    public const int LocalApplicationDataBufferSizeMax = 150;           //!<  The maximum size that can be set as application-defined data during local communication.
//  public const int ApplicationDataBufferSizeMax = 364; //! <  The maximum size that can be set as application-defined data.
    private const int ApplicationDataSystemBufferSizeMax = 384;         //!<  The maximum size that can be set as application-defined data. (For use by the system.)
#endif

#if UNITY_ONLY_SWITCH
    private const int SessionMatchmakeKeywordLength = 32;       //!<  The length of the keyword string that can be configured during keyword matchmaking.
#endif

/*!
    @brief  Expresses the content of state changes generated in a session.
*/
    public enum EventType : int
    {
        EventJoin = 0,                 //!<  A station has joined.
        EventLeave,                    //!<  A station has left.
        SessionHostChanged = 2,        //!<  The host of the session has changed. (Only occurs when host migration is enabled.)
        JointSessionHostChanged = 4,   //!<  The host of the joint session has changed. (Only occurs when using the joint session feature with host migration enabled.)
        StartSessionCreateJoint = 11,  //!<  The process of creating a joint session has started. (Only occurs when using the joint session feature.)
        EndSessionCreateJoint,         //!<  The process of creating a joint session has finished. (Only occurs when using the joint session feature.)
        StartSessionJoinJoint,         //!<  The process of joining a joint session has started. (Only occurs when using the joint session feature.)
        EndSessionJoinJoint,           //!<  The process of joining a joint session has finished. (Only occurs when using the joint session feature.)
        StartSessionJointRandom,       //!<  A joint session has started through random matchmaking. (Only occurs when using the joint session feature.)
        EndSessionJointRandom,         //!<  The random matchmaking joint session has ended. (Only occurs when using the joint session feature.)
        StartSessionDestroyJoint,      //!<  The process of disbanding a joint session has started. (Only occurs when using the joint session feature.)
        EndSessionDestroyJoint,        //!<  The process of disbanding a joint session has finished. (Only occurs when using the joint session feature.)
        StartSessionLeaveJoint,        //!<  The process of leaving a joint session has started. (Only occurs when using the joint session feature.)
        EndSessionLeaveJoint,          //!<  The process of leaving a joint session has finished. (Only occurs when using the joint session feature.)
        SetSessionSystemPassword = 22, //!<  A system password was set on the session.
        ClearSessionSystemPassword,    //!<  The system password of the session was cleared.
        InconsistentNotice             //!<  A notification was received from the client that the station information does not match.
    }

/*!
    @brief  Enumerates session states.
*/
    public enum Status : int
    {
        NotConnected,           //!<  Not communicating.
        ConnectedSession,       //!<  Joined a session.
        MigratingSession,       //!<  Being transferred to a session by the joint session feature. (Includes a period with no communication.)
        ConnectedJointSession,  //!<  Joined a joint session.
        DisconnectedSession,    //!<  Disconnected from the session.
        DisconnectedNetwork     //!<  Disconnected from the network. Log out and then log in again.
    }

/*!
    @brief  Enumerated type representing the reasons that a <tt>Session</tt> object cannot communicate.
*/
    public enum DisconnectReason : int
    {
        UnknownReason = 0,   //!<  Specifies that the reason for the disconnection is unknown. Switches to this value when cleaning up a session (transition from <tt>PiaPlugin.State.SessionStartedUp</tt> to <tt>PiaPlugin.State.SessionCleanedUp</tt>) during communication (without destroying or leaving the session).
        NotYetCommunicated,  //!<  Specifies that no communication has been performed. Switches to this value when no sessions are created or joined after the state transitions to <tt>PiaPlugin.State.SessionStartedUp</tt>.
        OperationOfOwn,      //!<  The disconnection was caused by an operation by the local station.
        OperationOfOther,    //!<  Specifies that the disconnection was caused by another station in the session. One way to get this value is when the local station is a client, and the host destroys the session.
        KickoutByUser,       //!<  Specifies that the station was kicked out of a session by calling <tt>PiaPluginSession.KickoutStation</tt>.
        KickoutBySystem,     //!<  Specifies that the disconnection from the session was caused by the library. One way to get this value is when relaying is enabled, and the library disconnects you from the session because it restricts the number of relay requests.
        InconsistentInfo,    //!<  Specifies that the disconnection was caused by a desync. (The session information was no longer consistent between participants.) One way to get this value is when the station counts do not match.
        MigrationFail,       //!<  Disconnection was caused by a failed host migration.
        ExternalFactor,      //!<  Specifies that the disconnection was caused by something other than the session. For example, this value is returned when the reason is some kind of network failure.
        MigrationFatalError  //!<  Specifies that a fatal error occurred during host migration. This error can occur when the library detects that a session has broken up.
    }

/*!
    @brief  Enumerated type for configuring the network topology of a mesh created with the <tt>Session</tt> class or the <tt>Mesh</tt> class.
    @details  This feature will be added to the <tt>Session</tt> class and the <tt>Mesh</tt> class in a future release.
*/
    public enum NetworkTopology : int
    {
        FullMesh = 0,  //!<  Full-mesh network topology.
        RelayMesh      //!<  Mesh network topology with relay connections enabled.
    }

/*!
    @brief  Enumerates the selection methods for joining sessions when random matchmaking is in force.
*/
    public enum SelectionMethod : int
    {
        Random = 0,                         //!<  Random selection.
        BroadenRangeWithSelectionPriority,  //!<  Selected by range broadening and matchmaking priority.
        ScoreBased                          //!<  Selected by score-conversion format.
    }

/*!
    @brief  Enumerates session open/close states.
    @details  You can get this information using the @ref GetSessionOpenStatus and @ref GetJointSessionOpenStatus functions.
*/
    public enum SessionOpenStatus : int
    {
        Unknown = 0,                        //!<  The recruitment state is unknown (client only).
        Open,                               //!<  Recruitment open state.
        Close                               //!<  Recruitment closed state.
    }


/*!
    @brief  Expresses that the session status has changed.
*/
    [StructLayout(LayoutKind.Sequential)]
    public class SessionEvent
    {
        public EventType eventType;  //!<  The event type.
        public UInt64 constantId;     //!<  The station ID.
        public int stationIndex;     //!<  The station index.
    }

/*!
    @brief  Class that manages settings for session creation.
*/
    [Serializable]
    public class CreateSessionSetting
    {

/*!
        @cond  PRIVATE
        @brief  The items corresponding to each bit in the bit mask indicating the specified items.
*/
        private enum CondMask : int
        {
            GameMode = 0,                                                                    //!<  Game mode.
            ParticipantNumMin,                                                               //!<  Matchmaking session option.
            ParticipantNumMax,                                                               //!<  Matchmaking session option.
            ApplicationData,                                                                 //!<  Application-defined data.
            OpenSession,                                                                     //!<  Whether participation is possible.
            MatchmakeSessionOption,                                                          //!<  Matchmaking session option.
            ScoreBasedSettingIndex,                                                          //!<  The score conversion method settings index, when using score-based matchmaking.
            ScoreBasedRatingValue,                                                           //!<  Rating value, when using score-based matchmaking.
            ScoreBasedDisconnectionRate,                                                     //!<  Disconnection rate, when using score-based matchmaking.
            ScoreBasedViolationRate,                                                         //!<  Violation rate, when using score-based matchmaking.
            ScoreBasedCountryCode,                                                           //!<  Country code, when using score-based matchmaking.
            ScoreBasedGeoIp,                                                                 //!<  Position data, when using score-based matchmaking.
            SessionUserPassword,                                                             //!<  User password.
            MatchmakeKeyword,                                                                //!<  Keyword.
            Attribute,                                                                       //!<  Attribute.
            Max
        }
/*! @endcond */

/*!
        @brief  Class used for specifying session attributes.
*/
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public class Attribute
        {
            public UInt32 index;  //!<  Attribute index.
            public UInt32 value;  //!<  Attribute value.
        }

/*!
        @cond  PRIVATE
*/
        public UInt32 setCondMask = 0;                                                 //!<  Manage the configured criteria.
/*! @endcond */

        private UInt16 participantNumMin;                                              //!<  Sets the minimum number of participants.
        private UInt16 participantNumMax;                                              //!<  Sets the maximum number of people who can participate.
        private UInt16 gameMode;                                                       //!<  The game mode.
        private List<Attribute> attributeList = new List<Attribute>();                  //!<  Specifies a value to use when searching for each attribute. If a value has not been set for a particular index attribute, the search looks for any value.
        private Byte[] applicationData = (Byte[])System.Linq.Enumerable.Empty<Byte>(); //!<  Sets application-defined data.
        private bool isOpenSession;                                                    //!<  Allows participation as soon as the session is created.
        private UInt32 matchmakeSessionOption;                                         //!<  The matchmaking session options.
        private string countryCode = "";                                               //!<  Sets the country code to use for comparison when score-based matchmaking is specified. The country code is a string of two uppercase letters that is an ISO 3166-1 alpha-2 country code.
        private UInt32 ratingValue;                                                    //!<  Sets the rating value to use for comparison when score-based matchmaking is specified.
        private UInt32 disconnectionRate;                                              //!<  Sets the disconnection rate to use for comparison when score-based matchmaking is specified.
        private UInt32 violationRate;                                                  //!<  Sets the violation rate to use for comparison when score-based matchmaking is specified.
        private bool isGeoIpUsed;                                                      //!<  Sets whether to use the location information (latitude/longitude and country code) for comparison when score-based matchmaking is specified.
        private string wirelessCryptoKey;                                              //!<  The passphrase used to encrypt the wireless layer. Necessary for local communication.
#if UNITY_ONLY_SWITCH
        private string sessionUserPassword;                                            //!<  User password to set in the session. Can be used for local communication@if NIN_DOC and internet communication@endif .
        private string matchmakeKeyword;                                                //!<  The keyword.
        private UInt16 localCommunicationChannel;                                       //!<  The local communication channel specification.
#endif

/*!
        @brief  Sets the minimum number of participants.
        @details  For Internet communication and LAN communication, if a search condition includes the minimum number of participants, matching is performed. This is ignored for local communication.
        @param[in] participantNumMin  The minimum number of people who can participate.
*/
        public void SetParticipantNumMin(UInt16 participantNumMin)
        {
            setCondMask |= 1 << (int)CondMask.ParticipantNumMin;
            this.participantNumMin = participantNumMin;
        }

/*!
        @brief  Gets the minimum number of participants.
        @return  Minimum number of participants
*/
        public UInt16 GetParticipantNumMin()
        {
            return this.participantNumMin;
        }

/*!
        @brief  Sets the maximum number of people who can participate.
        @param[in] participantNumMax  The maximum number of people who can participate.
*/
        public void SetParticipantNumMax(UInt16 participantNumMax)
        {
            setCondMask |= 1 << (int)CondMask.ParticipantNumMax;
            this.participantNumMax = participantNumMax;
        }

/*!
        @brief  Gets the maximum number of people who can participate.
        @return  Maximum number of participants.
*/
        public UInt16 GetParticipantNumMax()
        {
            return this.participantNumMax;
        }
/*!
        @brief  The game mode.
        @param[in] gameMode  Game mode.
*/
        public void SetGameMode(UInt16 gameMode)
        {
            setCondMask |= 1 << (int)CondMask.GameMode;
            this.gameMode = gameMode;
        }

/*!
        @brief  Gets the game mode.
        @return  Game mode.
*/
        public UInt16 GetGameMode()
        {
            return this.gameMode;
        }
/*!
        @brief  Sets application-defined data.
        @param[in] applicationData  The application data.
*/
        public void SetApplicationData(Byte[] applicationData)
        {
            setCondMask |= 1 << (int)CondMask.ApplicationData;
            this.applicationData = applicationData;
        }

/*!
        @brief  Gets application-defined data.
        @return  The application data.
*/
        public Byte[] GetApplicationData()
        {
            return this.applicationData;
        }
/*!
        @brief  Allows participation as soon as the session is created.
        @details  Can be used with Internet communication and LAN communication.
        @param[in] isOpenSession  Whether to open the session to participation.
*/
        public void SetOpenSession(bool isOpenSession)
        {
            setCondMask |= 1 << (int)CondMask.OpenSession;
            this.isOpenSession = isOpenSession;
        }

/*!
        @brief  Determines whether participation is allowed as soon as the session is created.
        @details  Can be used with Internet communication and LAN communication.
        @return  Whether the session is open to participation.
*/
        public bool IsOpenSession()
        {
            return this.isOpenSession;
        }

/*!
        @cond  PRIVATE
        @brief  The matchmaking session options.
*/
        public void SetMatchmakeSessionOption(UInt32 matchmakeSessionOption)
        {
            setCondMask |= 1 << (int)CondMask.MatchmakeSessionOption;
            this.matchmakeSessionOption = matchmakeSessionOption;
        }
/*! @endcond */

/*!
        @cond  PRIVATE
        @brief  Gets the matchmaking session options.
*/
        public UInt32 GetMatchmakeSessionOption()
        {
            return this.matchmakeSessionOption;
        }
/*! @endcond */

/*!
        @brief  Sets the country code to use for comparison when score-based matchmaking is specified. The country code is a string of two uppercase letters that is an ISO 3166-1 alpha-2 country code.
        @param[in] countryCode  Country code.
*/
        public void SetCountryCode(string countryCode)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedCountryCode;
            this.countryCode = countryCode;
        }

/*!
        @brief  Gets the country code.
        @return  Country code.
*/
        public string GetCountryCode()
        {
            return this.countryCode;
        }

/*!
        @brief  Sets the rating value to use for comparison when score-based matchmaking is specified.
        @param[in] ratingValue  Rating value.
*/
        public void SetRatingValue(UInt32 ratingValue)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedRatingValue;
            this.ratingValue = ratingValue;
        }

/*!
        @brief  Gets the rating value.
        @return  Rating value.
*/
        public UInt32 GetRatingValue()
        {
            return this.ratingValue;
        }

/*!
        @brief  Sets the disconnection rate to use for comparison when score-based matchmaking is specified.
        @details  Can be used with Internet communication.
        @param[in] disconnectionRate  Disconnection rate.
*/
        public void SetDisconnectionRate(UInt32 disconnectionRate)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedDisconnectionRate;
            this.disconnectionRate = disconnectionRate;
        }

/*!
        @brief  Gets the specified disconnection rate.
        @details  Can be used with Internet communication.
        @return  Disconnection rate.
*/
        public UInt32 GetDisconnectionRate()
        {
            return this.disconnectionRate;
        }

/*!
        @brief  Sets the violation rate to use for comparison when score-based matchmaking is specified.
        @details  Can be used with Internet communication.
        @param[in] violationRate  Violation rate.
*/
        public void SetViolationRate(UInt32 violationRate)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedViolationRate;
            this.violationRate = violationRate;
        }

/*!
        @brief  Gets the specified violation rate.
        @details  Can be used with Internet communication.
        @return  Violation rate.
*/
        public UInt32 GetViolationRate()
        {
            return this.violationRate;
        }

/*!
        @brief  Sets whether to use the location information (latitude/longitude and country code) for comparison when score-based matchmaking is specified.
        @details  Can be used with Internet communication.
        @param[in] isGeoIpUsed  Whether to use location information (longitude/latitude and country code).
*/
        public void SetUseGeoIp(bool isGeoIpUsed)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedGeoIp;
            this.isGeoIpUsed = isGeoIpUsed;
        }

/*!
        @brief  Determines whether location information (latitude/longitude and country code) is used.
        @details  Can be used with Internet communication.
        @return  The setting for whether to use location information.
*/
        public bool IsGeoIpUsed()
        {
            return this.isGeoIpUsed;
        }

/*!
        @brief  The passphrase used to encrypt the wireless layer. Necessary for local communication.
        @param[in] wirelessCryptoKey  The encryption key.
*/
        public void SetWirelessCryptoKey(string wirelessCryptoKey)
        {
            this.wirelessCryptoKey = wirelessCryptoKey;
        }

/*!
        @brief  Gets the passphrase used for encrypting the configured wireless layer.
        @return  The encryption key.
*/
        public string GetWirelessCryptoKey()
        {
            return this.wirelessCryptoKey;
        }

/*!
        @brief  Sets the attribute list.
        @details  Cannot be used with <tt><var>matchmakeKeyword</var></tt>. Whichever is set first is valid. [:disable-local]
        @param[in] attributeList  Attribute list.
*/
        public PiaPlugin.Result SetAttributeList(List<Attribute> attributeList)
        {
            if (0 != (setCondMask & (1 << (int)CondMask.MatchmakeKeyword)))
            {
                PiaPlugin.Trace("MatchmakeKeyword is already set. Attribute can not be used with MatchmakeKeyword.");
                return PiaPlugin.InvalidArgumentResult;
            }

            setCondMask |= 1 << (int)CondMask.Attribute;
            this.attributeList = attributeList;
            return PiaPlugin.SuccessResult;
        }

/*!
        @brief  Gets a list of set attributes.
        @details  [:disable-local]
        @return  Attribute list.
*/
        public List<Attribute> GetAttributeList()
        {
            return attributeList;
        }

#if UNITY_ONLY_SWITCH
/*!
        @brief  Sets a user password for the created session. Can be used for local communication@if NIN_DOC and internet communication@endif .
        @param[in] userPassword  The user password to set. The password string must be no longer than <tt>SessionUserPasswordLengthMax</tt>.
        @return  Returns a value that indicates success when the user password is successfully set.
        @retval PiaPlugin.InvalidArgumentResult  The string is null, the encoding is not valid, or the user password exceeds the maximum length. [:progErr]
*/
        public PiaPlugin.Result SetSessionUserPassword(string userPassword)
        {
            if (userPassword == null)
            {
                PiaPlugin.Trace("UserPassword is null.");
                return PiaPlugin.InvalidArgumentResult;
            }
            if (userPassword.Length == 0)
            {
                PiaPlugin.Trace("PasswordLength is zero.");
                return PiaPlugin.InvalidArgumentResult;
            }
            if (userPassword.Length > SessionUserPasswordLengthMax)
            {
                PiaPlugin.Trace("PasswordLength is exceed length. (need PasswordLength <= " + SessionUserPasswordLengthMax + ")");
                return PiaPlugin.InvalidArgumentResult;
            }
            setCondMask |= 1 << (int)CondMask.SessionUserPassword;
            this.sessionUserPassword = userPassword;
            return PiaPlugin.SuccessResult;
        }

/*!
        @brief  Gets the user password for the created session.
        @return  The user password.
*/
        public string GetSessionUserPassword()
        {
            return this.sessionUserPassword;
        }

/*!
        @brief  Sets the keyword for the created session.
        @param[in] keyword  The keyword for keyword matchmaking. The keyword string must be no longer than <tt>SessionMatchmakeKeywordLength</tt>.
        @return  Returns a successful result when the keyword is successfully specified.
        @retval PiaPlugin.InvalidArgumentResult  Either the string is null, the encoding is invalid, or the keyword exceeds the maximum length. [:progErr]
*/
        public PiaPlugin.Result SetSessionMatchmakeKeyword(string keyword)
        {
            if (keyword == null)
            {
                PiaPlugin.Trace("keyword is null.");
                return PiaPlugin.InvalidArgumentResult;
            }
            if (keyword.Length == 0)
            {
                PiaPlugin.Trace("keyword is zero.");
                return PiaPlugin.InvalidArgumentResult;
            }
            if (keyword.Length > SessionMatchmakeKeywordLength)
            {
                PiaPlugin.Trace("KeywordLength is exceed length. (need Keyword Length <= " + SessionMatchmakeKeywordLength + ")");
                return PiaPlugin.InvalidArgumentResult;
            }
            if (0 != (setCondMask & (1 << (int)CondMask.Attribute)))
            {
                PiaPlugin.Trace("Attribute is already set. MatchmakeKeyword can not be used with Attribute.");
                return PiaPlugin.InvalidArgumentResult;
            }
            setCondMask |= 1 << (int)CondMask.MatchmakeKeyword;
            this.matchmakeKeyword = keyword;
            return PiaPlugin.SuccessResult;
        }

/*!
        @brief  Sets the keyword for the created session.
        @return  The user password.
*/
        public string GetSessionMatchmakeKeyword()
        {
            return this.matchmakeKeyword;
        }

/*!
        @brief  (For debugging) Sets the channel to use for communication by the session being created.
        @param[in] localCommunicationChannel  The channel to use for communication. You must specify <tt>0</tt> (automatic), <tt>1</tt>, <tt>6</tt>, or <tt>11ch</tt>. This setting is ignored when specified for a retail device, in which case it is always <tt>0</tt> (automatic).
        You can specify channels 36, 40, 44, and 48 for the 5-GHz bandwidth on development hardware. When using the 5-GHz band, use bandwidth within the range stipulated by each country in its laws about using the radio spectrum. We strongly recommend only using this range in environments like shielded rooms where the radio signals cannot escape.
*/
        public void SetLocalCommunicationChannel(UInt16 localCommunicationChannel)
        {
            this.localCommunicationChannel = localCommunicationChannel;
        }

/*!
        @brief  Gets the channel to use for communication by the session that is being created and configured for debugging.
        @return  The channel to use for communication by the session that is being created and configured for debugging.
*/
        public UInt16 GetLocalCommunicationChannel()
        {
            return this.localCommunicationChannel;
        }
#endif
    }

/*!
     @cond  PRIVATE
     @brief  <tt>CreateSessionSetting</tt> available for <tt>Native</tt> use.
*/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CreateSessionSettingNative : IDisposable
    {
        public UInt32 setCondMask = 0;
        public UInt16 participantNumMin;
        public UInt16 participantNumMax;
        public UInt16 gameMode;
        public IntPtr pAttributeArray;
        public int attributeNum;
        public IntPtr pApplicationData;
        public int applicationDataSize;
        [MarshalAs(UnmanagedType.U1)]
        public bool isOpenSession;                                                     //!<  Allows participation as soon as the session is created.
        public UInt32 matchmakeSessionOption;                                          //!<  Matchmaking session option.
        public IntPtr pCountryCode;                                                    //!<  Sets the country code to use for comparison when score-based matchmaking is specified. The country code is a string of two uppercase letters that is an ISO 3166-1 alpha-2 country code. Used when GeoIp is disabled and the GeoIp location information cannot be obtained.
        public UInt32 ratingValue;                                                     //!<  Sets the rating value to use for comparison when score-based matchmaking is specified.
        private UInt32 disconnectionRate;                                              //!<  Sets the disconnection rate to use for comparison when score-based matchmaking is specified.
        private UInt32 violationRate;                                                  //!<  Sets the violation rate to use for comparison when score-based matchmaking is specified.
        [MarshalAs(UnmanagedType.U1)]
        public bool isGeoIpUsed;                                                       //!<  Sets whether to use the location information (latitude/longitude and country code) for comparison when score-based matchmaking is specified.
        public IntPtr pWirelessCryptoKey;                                              //!<  The passphrase used to encrypt the wireless layer.
#if UNITY_ONLY_SWITCH
        public IntPtr pSessionUserPassword;                                            //!<  User password to set in the session. Can be used for local communication@if NIN_DOC and internet communication@endif .
        private IntPtr pMmatchmakeKeyword;                                                //!<  The keyword set for the session.
        private UInt16 localCommunicationChannel;                                         //!<  The local communication channel specification.
#endif

        internal CreateSessionSettingNative()
        {
            setCondMask = 0;
            participantNumMin = 0;
            participantNumMax = 0;
            gameMode = 0;
            pAttributeArray = IntPtr.Zero;
            attributeNum = 0;
            pApplicationData = IntPtr.Zero;
            isOpenSession = false;
            matchmakeSessionOption = 0;
            pCountryCode = IntPtr.Zero;
            ratingValue = 0;
            disconnectionRate = 0;
            violationRate = 0;
            isGeoIpUsed = false;
            pWirelessCryptoKey = IntPtr.Zero;
#if UNITY_ONLY_SWITCH
            pSessionUserPassword = IntPtr.Zero;
            pMmatchmakeKeyword = IntPtr.Zero;
            localCommunicationChannel = 0;
#endif
        }

        internal CreateSessionSettingNative(CreateSessionSetting setting)
        {
            setCondMask = setting.setCondMask;
            participantNumMin = setting.GetParticipantNumMin();
            participantNumMax = setting.GetParticipantNumMax();
            gameMode = setting.GetGameMode();
            attributeNum = setting.GetAttributeList().Count;
            int bufferSize = 0;
            if (setting.GetAttributeList().Count != 0)
            {
                pAttributeArray = UnmanagedMemoryManager.WriteList<CreateSessionSetting.Attribute>(setting.GetAttributeList(), ref bufferSize);
            }
            pApplicationData = UnmanagedMemoryManager.WriteArray<Byte>(setting.GetApplicationData(), ref bufferSize);
            applicationDataSize = setting.GetApplicationData().Length;
            PiaPlugin.Assert(applicationDataSize <= ApplicationDataSystemBufferSizeMax);
            isOpenSession = setting.IsOpenSession();
            matchmakeSessionOption = setting.GetMatchmakeSessionOption();
            pCountryCode = UnmanagedMemoryManager.WriteUtf8(setting.GetCountryCode(), ref bufferSize);
            ratingValue = setting.GetRatingValue();
            disconnectionRate = setting.GetDisconnectionRate();
            violationRate = setting.GetViolationRate();
            isGeoIpUsed = setting.IsGeoIpUsed();
            if (setting.GetWirelessCryptoKey() != null)
            {
                pWirelessCryptoKey = UnmanagedMemoryManager.WriteUtf8(setting.GetWirelessCryptoKey(), ref bufferSize);
            }
#if UNITY_ONLY_SWITCH
            if (setting.GetSessionUserPassword() != null)
            {
                pSessionUserPassword = UnmanagedMemoryManager.WriteUtf8(setting.GetSessionUserPassword(), ref bufferSize);
            }
            if (setting.GetSessionMatchmakeKeyword() != null)
            {
                pMmatchmakeKeyword = UnmanagedMemoryManager.WriteUtf8(setting.GetSessionMatchmakeKeyword(), ref bufferSize);
            }
            localCommunicationChannel = setting.GetLocalCommunicationChannel();
            PiaPlugin.Trace("localCommunicationChannel "+localCommunicationChannel);
#endif
        }

        public void Dispose()
        {
            UnmanagedMemoryManager.Free(pAttributeArray);
            UnmanagedMemoryManager.Free(pApplicationData);
            UnmanagedMemoryManager.Free(pCountryCode);
            UnmanagedMemoryManager.Free(pWirelessCryptoKey);
#if UNITY_ONLY_SWITCH
            UnmanagedMemoryManager.Free(pSessionUserPassword);
            UnmanagedMemoryManager.Free(pMmatchmakeKeyword);
#endif
        }
    }
    //! @endcond

/*!
    @brief  The search condition class used when searching for a session.
*/
    [Serializable]
    public class SessionSearchCriteria
    {
/*!
        @cond  PRIVATE
        @brief  The items that correspond to each bit in the bit mask representing the specified search criteria.
*/
        enum CondMask
        {
            ParticipantNumMin = 0,                                                                       //!<  Minimum number of participants
            ParticipantNumMax,                                                                           //!<  Maximum number of participants.
            OpenedOnly,                                                                                 //!<  Whether to search only for sessions that are open to participants.
            VacantOnly,                                                                                 //!<  Whether to search only for sessions that have openings.
            GameMode,                                                                                   //!<  Game mode.
            RandomSessionSelectionMethod,                                                               //!<  The selection methods for joining sessions when random matchmaking is in effect.
            ScoreBasedSettingIndex,                                                                     //!<  The score conversion method settings index, when using score-based matchmaking.
            ScoreBasedRatingValue,                                                                      //!<  Rating value, when using score-based matchmaking.
            ScoreBasedDisconnectionRate,                                                                //!<  Disconnection rate, when using score-based matchmaking.
            ScoreBasedViolationRate,                                                                    //!<  Violation rate, when using score-based matchmaking.
            ScoreBasedCountryCode,                                                                      //!<  Country code, when using score-based matchmaking.
            ScoreBasedGeoIp,                                                                            //!<  Position data, when using score-based matchmaking.
            ExcludeUserPasswordSet,                                                                     //!<  Whether to exclude sessions set with user passwords.
            MatchmakeKeyword,                                                                           //!<  Keyword.
            Attribute,                                                                                  //!<  Attribute.
            AttributeRange,                                                                             //!<  Attribute (when using with a range specified).
            Max
        };
        //! @endcond

/*!
        @brief  Class used for specifying session attributes.
*/
        [Serializable]
        public class Attribute
        {
            public UInt32 index; //!<  Attribute index.
            public List<UInt32> valueList = new List<UInt32>(); //!<  Attribute value. More than one can be set.
        }

/*!
        @brief  Class used for specifying session attributes. Sets the range of values for indexes.
*/
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public class AttributeRange
        {
            public UInt32 index;          //!<  Attribute index.
            public UInt32 min;            //!<  Minimum attribute value.
            public UInt32 max;            //!<  Maximum attribute value.
        }

/*!
        @cond  PRIVATE
*/
        public UInt32 setCondMask = 0;                                                 //!<  The mask for managing the configured search criteria.
        //! @endcond

        private UInt16 participantNumMin;                                              //!<  Sets a minimum number of participants in the search criteria.
        private UInt16 participantNumMax;                                              //!<  Sets the maximum number of players in the search criteria.
        private bool isOpenedOnly;                                                     //!<  Sets whether to search only for sessions that are open to participants as one of the search criteria.
        private bool isVacantOnly;                                                     //!<  Sets whether to search only for sessions that have openings as one of the search criteria.
        private UInt16 gameMode;                                                       //!<  Sets a game mode in the search criteria.
        private List<Attribute> attributeList = new List<Attribute>();                 //!<  Specifies a value to use when searching for each attribute. If a value has not been set for a particular index attribute, the search looks for any value.
        private List<AttributeRange> attributeRangeList = new List<AttributeRange>();  //!<  Specifies the range of values used when searching for each attribute. If a value has not been set for a particular index attribute, the search looks for any value.
        public UInt32[] resultRange = new UInt32[2];                                   //!<  Sets the range of search results to get. Sets the 0th element to the starting position of the entry group retrieved as the result of searching the entry list matching the search conditions, and sets the first element to the number of results retrieved.
        private string countryCode = "";                                               //!<  Sets the country code to use for comparison when score-based matchmaking is specified. The country code is a string of two uppercase letters that is an ISO 3166-1 alpha-2 country code.
        private UInt32 ratingValue;                                                    //!<  Sets the rating value to use for comparison when score-based matchmaking is specified.
        private UInt32 disconnectionRate;                                              //!<  Sets the disconnection rate to use for comparison when score-based matchmaking is specified.
        private UInt32 violationRate;                                                  //!<  Sets the violation rate to use for comparison when score-based matchmaking is specified.
        private bool isGeoIpUsed;                                                      //!<  Sets whether to use the location information (latitude/longitude and country code) for comparison when score-based matchmaking is specified.
        private UInt32 scoreSettingIndex;                                              //!<  Sets the index of the score conversion method settings to use for comparison when score-based matchmaking is specified.
        private SelectionMethod selectionMethod;                                       //!<  The method for selecting which session to join when random matchmaking is in effect.
#if UNITY_ONLY_SWITCH
        private string matchmakeKeyword;                                                //!<  The keyword.
#endif
        private bool isExcludeUserPasswordSet;                                         //!<  Sets whether to exclude sessions for which a user password has been set.

/*!
        @brief  Sets a minimum number of participants in the search criteria.
        @param[in] participantNumMin  The minimum number of participants to set in the search criteria.
*/
        public void SetParticipantNumMin(UInt16 participantNumMin)
        {
            setCondMask |= 1 << (int)CondMask.ParticipantNumMin;
            this.participantNumMin = participantNumMin;

        }

/*!
        @brief  Gets the minimum number of participants in the specified search criteria.
        @return  Minimum number of participants
*/
        public UInt16 GetParticipantNumMin()
        {
            return this.participantNumMin;

        }

/*!
        @brief  Sets the maximum number of players in the search criteria.
        @param[in] participantNumMax  The maximum number of participants to set in the search criteria.
*/
        public void SetParticipantNumMax(UInt16 participantNumMax)
        {
            setCondMask |= 1 << (int)CondMask.ParticipantNumMax;
            this.participantNumMax = participantNumMax;

        }

/*!
        @brief  Gets the maximum number of participants in the specified search criteria.
        @return  Maximum number of participants.
*/
        public UInt16 GetParticipantNumMax()
        {
            return this.participantNumMax;

        }

/*!
        @brief  Sets whether to search only for sessions that are open to participants as one of the search criteria.
        @details  Can be used with Internet communication and LAN communication.
        @param[in] isOpenedOnly  <tt>true</tt> to search only for sessions that are open to participants.
*/
        public void SetOpenedOnly(bool isOpenedOnly)
        {
            setCondMask |= 1 << (int)CondMask.OpenedOnly;
            this.isOpenedOnly = isOpenedOnly;

        }

/*!
        @brief  Determines whether the search criteria have been configured to search only for sessions that are open to participants.
        @details  Can be used with Internet communication and LAN communication.
        @return  Whether to search only for sessions that are open to participants.
*/
        public bool IsOpenedOnly()
        {
            return this.isOpenedOnly;

        }

/*!
        @brief  Sets whether to search only for sessions that have openings as one of the search criteria.
        @details  Can be used with Internet communication and LAN communication.
        @param[in] isVacantOnly  <tt>true</tt> to search only for sessions that have openings.
*/
        public void SetVacantOnly(bool isVacantOnly)
        {
            setCondMask |= 1 << (int)CondMask.VacantOnly;
            this.isVacantOnly = isVacantOnly;

        }

/*!
        @brief  Determines whether the search criteria include the specification to search only for sessions that have openings.
        @details  Can be used with Internet communication and LAN communication.
        @return  Whether to search only for sessions that have openings.
*/
        public bool IsVacantOnly()
        {
            return this.isVacantOnly;

        }

/*!
        @brief  Sets a game mode in the search criteria.
        @param[in] gameMode  Game mode.
*/
        public void SetGameMode(UInt16 gameMode)
        {
            setCondMask |= 1 << (int)CondMask.GameMode;
            this.gameMode = gameMode;

        }

/*!
        @brief  Gets the specified game mode.
        @return  Game mode.
*/
        public UInt16 GetGameMode()
        {
            return this.gameMode;

        }

/*!
        @brief  Sets the country code to use for comparison when score-based matchmaking is specified. The country code is a string of two uppercase letters that is an ISO 3166-1 alpha-2 country code.
        @details  Can be used with Internet communication.
        @param[in] countryCode  Country code. You must specify a string of two uppercase letters that is an ISO 3166-1 alpha-2 country code.
*/
        public void SetCountryCode(string countryCode)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedCountryCode;
            this.countryCode = countryCode;

        }

/*!
        @brief  Gets the country code.
        @details  Can be used with Internet communication.
        @return  Country code.
*/
        public string GetCountryCode()
        {
            return this.countryCode;

        }

/*!
        @brief  Sets the rating value to use for comparison when score-based matchmaking is specified.
        @details  Can be used with Internet communication.
        @param[in] ratingValue  Rating value.
*/
        public void SetRatingValue(UInt32 ratingValue)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedRatingValue;
            this.ratingValue = ratingValue;

        }

/*!
        @brief  Gets the rating value.
        @details  Can be used with Internet communication.
        @return  Rating value.
*/
        public UInt32 GetRatingValue()
        {
            return this.ratingValue;

        }

/*!
        @brief  Sets the disconnection rate to use for comparison when score-based matchmaking is specified.
        @details  Can be used with Internet communication.
        @param[in] disconnectionRate  Disconnection rate.
*/
        public void SetDisconnectionRate(UInt32 disconnectionRate)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedDisconnectionRate;
            this.disconnectionRate = disconnectionRate;
        }

/*!
        @brief  Gets the specified disconnection rate.
        @details  Can be used with Internet communication.
        @return  Disconnection rate.
*/
        public UInt32 GetDisconnectionRate()
        {
            return this.disconnectionRate;
        }

/*!
        @brief  Sets the violation rate to use for comparison when score-based matchmaking is specified.
        @details  Can be used with Internet communication.
        @param[in] violationRate  Violation rate.
*/
        public void SetViolationRate(UInt32 violationRate)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedViolationRate;
            this.violationRate = violationRate;
        }

/*!
        @brief  Gets the specified violation rate.
        @details  Can be used with Internet communication.
        @return  Violation rate.
*/
        public UInt32 GetViolationRate()
        {
            return this.violationRate;

        }

/*!
        @brief  Sets whether to use the location information (latitude/longitude and country code) for comparison when score-based matchmaking is specified.
        @param[in] isGeoIpUsed  Whether to use position information.
*/
        public void SetUseGeoIp(bool isGeoIpUsed)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedGeoIp;
            this.isGeoIpUsed = isGeoIpUsed;

        }

/*!
        @brief  Determines whether to use location information (latitude/longitude and country code).
        @return  Whether to use location information (longitude/latitude and country code).
*/
        public bool IsGeoIpUsed()
        {
            return this.isGeoIpUsed;

        }

/*!
        @brief  Sets the index of the score conversion method settings to use for comparison when score-based matchmaking is specified.
        @details  Can be used with Internet communication.
        @param[in] scoreSettingIndex  The score conversion settings index.
*/
        public void SetScoreSettingIndex(UInt32 scoreSettingIndex)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedSettingIndex;
            this.scoreSettingIndex = scoreSettingIndex;

        }


/*!
        @brief  Gets the index of the score conversion method settings that was set to use for comparisons.
        @details  Can be used with Internet communication.
        @return  The score conversion method settings index.
*/
        public UInt32 GetScoreSettingIndex()
        {
            return this.scoreSettingIndex;

        }

/*!
        @brief  The method for selecting which session to join when random matchmaking is in effect.
        @details  Can be used with Internet communication.
        @param[in] selectionMethod  The method for selecting which session to join when random matchmaking is in effect.
*/
        public void SetSelectionMethod(SelectionMethod selectionMethod)
        {
            setCondMask |= 1 << (int)CondMask.RandomSessionSelectionMethod;
            this.selectionMethod = selectionMethod;

        }

/*!
        @brief  Gets the session selection method.
        @details  Can be used with Internet communication.
        @return  The session selection method.
*/
        public SelectionMethod GetSelectionMethod()
        {
            return this.selectionMethod;

        }

/*!
        @brief  Sets the attribute list.
        @details  Cannot be used with <tt><var>matchmakeKeyword</var></tt>. Whichever is set first is valid. [:disable-local]
        @param[in] attributeList  Attribute list.
*/
        public PiaPlugin.Result SetAttributeList(List<Attribute> attributeList)
        {
            if (0 != (setCondMask & (1 << (int)CondMask.MatchmakeKeyword)))
            {
                PiaPlugin.Trace("MatchmakeKeyword is already set. Attribute can not be used with MatchmakeKeyword.");
                return PiaPlugin.InvalidArgumentResult;
            }

            setCondMask |= 1 << (int)CondMask.Attribute;
            this.attributeList = attributeList;
            return PiaPlugin.SuccessResult;
        }

/*!
        @brief  Gets a list of set attributes.
        @details  [:disable-local]
        @return  Attribute list.
*/
        public List<Attribute> GetAttributeList()
        {
            return attributeList;
        }

/*!
        @brief  Sets the attribute list when using with a range specified.
        @details  Cannot be used with <tt><var>matchmakeKeyword</var></tt>. Whichever is set first is valid. [:disable-local]
        @param[in] attributeList  Attribute list.
*/
        public PiaPlugin.Result SetAttributeRangeList(List<AttributeRange> attributeRangeList)
        {
            if (0 != (setCondMask & (1 << (int)CondMask.MatchmakeKeyword)))
            {
                PiaPlugin.Trace("MatchmakeKeyword is already set. AttributeRange can not be used with MatchmakeKeyword.");
                return PiaPlugin.InvalidArgumentResult;
            }

            setCondMask |= 1 << (int)CondMask.AttributeRange;
            this.attributeRangeList = attributeRangeList;
            return PiaPlugin.SuccessResult;
        }

/*!
        @brief  Gets a list of set attributes when using with a range specified.
        @details  [:disable-local]
        @return  Attribute list.
*/
        public List<AttributeRange> GetAttributeRangeList()
        {
            return attributeRangeList;
        }

#if UNITY_ONLY_SWITCH
/*!
        @brief  Sets the keyword for the session to search for.
        @details  Can be used with Internet communication.
        @param[in] keyword  The keyword for keyword matchmaking. The keyword string must be no longer than <tt>SessionMatchmakeKeywordLength</tt>.
        @return  Returns a successful result when the keyword is successfully specified.
        @retval PiaPlugin.InvalidArgumentResult  Either the string is null, the encoding is invalid, or the keyword exceeds the maximum length. [:progErr]
*/
        public PiaPlugin.Result SetSessionMatchmakeKeyword(string keyword)
        {
            if (keyword == null)
            {
                PiaPlugin.Trace("keyword is null.");
                return PiaPlugin.InvalidArgumentResult;
            }
            if (keyword.Length == 0)
            {
                PiaPlugin.Trace("keyword is zero.");
                return PiaPlugin.InvalidArgumentResult;
            }
            if (keyword.Length > SessionMatchmakeKeywordLength)
            {
                PiaPlugin.Trace("KeywordLength is exceed length. (need Keyword Length <= " + SessionMatchmakeKeywordLength + ")");
                return PiaPlugin.InvalidArgumentResult;
            }
            if (0 != (setCondMask & (1 << (int)CondMask.Attribute)))
            {
                PiaPlugin.Trace("Attribute is already set. MatchmakeKeyword can not be used with Attribute.");
                return PiaPlugin.InvalidArgumentResult;
            }
            if (0 != (setCondMask & (1 << (int)CondMask.AttributeRange)))
            {
                PiaPlugin.Trace("AttributeRange is already set. MatchmakeKeyword can not be used with AttributeRange.");
                return PiaPlugin.InvalidArgumentResult;
            }
            setCondMask |= 1 << (int)CondMask.MatchmakeKeyword;
            this.matchmakeKeyword = keyword;
            return PiaPlugin.SuccessResult;
        }

/*!
        @brief  Gets the user password for the created session.
        @details  Can be used with Internet communication.
        @return  The user password.
*/
        public string GetSessionMatchmakeKeyword()
        {
            return this.matchmakeKeyword;
        }
#endif

/*!
        @brief  Sets whether to exclude sessions for which a user password has been set.
        @details  Can be used with Internet communication.
        @param[in] isExcludeUserPasswordSet  Whether to exclude sessions for which a user password was set from the search. Specify <tt>true</tt> to enable.
*/
        public void SetExcludeUserPasswordSet(bool isExcludeUserPasswordSet)
        {
            setCondMask |= 1 << (int)CondMask.ExcludeUserPasswordSet;
            this.isExcludeUserPasswordSet = isExcludeUserPasswordSet;
        }

/*!
        @brief  Gets the setting for whether to exclude sessions for which a user password has been set.
        @details  Can be used with Internet communication.
        @return  Whether to exclude sessions set with user passwords.
*/
        public bool IsExcludeUserPasswordSet()
        {
            return this.isExcludeUserPasswordSet;
        }
    }

/*!
    @cond  PRIVATE
    @brief  <tt>SessionSearchCriteria</tt> available for <tt>Native</tt> use.
*/
    [StructLayout(LayoutKind.Sequential)]
    public struct SessionSearchCriteriaNative : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public class AttributeNative : IDisposable
        {
            public UInt32 index;
            public IntPtr pValueArray;
            public int valueNum;

            public AttributeNative()
            {
                index = 0;
                pValueArray = IntPtr.Zero;
                valueNum = 0;
            }

            public AttributeNative(SessionSearchCriteria.Attribute attribute)
            {
                index = attribute.index;
                int bufferSize = 0;
                pValueArray = UnmanagedMemoryManager.WriteList<UInt32>(attribute.valueList, ref bufferSize);
                valueNum = attribute.valueList.Count;
            }

            public void Dispose()
            {
                UnmanagedMemoryManager.Free(pValueArray);
            }
        }

        UInt32 setCondMask;

        public UInt16 participantNumMin;
        public UInt16 participantNumMax;
        [MarshalAs(UnmanagedType.U1)]
        public bool isOpenedOnly;
        [MarshalAs(UnmanagedType.U1)]
        public bool isVacantOnly;
        public UInt16 gameMode;
        public IntPtr pAttributeNativeArray;
        public int attributeNativeNum;
        public IntPtr pAttributeRangeArray;
        public int attributeRangeNum;
        public UInt32 resultOffset;
        public UInt32 resultSize;
        public IntPtr pCountryCode;                                                    //!<  Sets the country code to use for comparison when score-based matchmaking is specified. The country code is a string of two uppercase letters that is an ISO 3166-1 alpha-2 country code. Used when GeoIp is disabled and the GeoIp location information cannot be obtained.
        public UInt32 ratingValue;                                                     //!<  Sets the rating value to use for comparison when score-based matchmaking is specified.
        private UInt32 disconnectionRate;                                              //!<  Sets the disconnection rate to use for comparison when score-based matchmaking is specified.
        private UInt32 violationRate;                                                  //!<  Sets the violation rate to use for comparison when score-based matchmaking is specified.
        [MarshalAs(UnmanagedType.U1)]
        public bool isGeoIpUsed;                                                       //!<  Sets whether to use the location information (latitude/longitude and country code) for comparison when score-based matchmaking is specified.
        public UInt32 scoreSettingIndex;                                               //!<  Sets the index of the score conversion method settings to use for comparison when score-based matchmaking is specified.
        public int selectionMethod;                                                    //!<  The method for selecting which session to join when random matchmaking is in effect.
#if UNITY_ONLY_SWITCH
        private IntPtr pMmatchmakeKeyword;                                             //!<  The keyword set for the session.
#endif
        [MarshalAs(UnmanagedType.U1)]
        public bool isExcludeUserPasswordSet;                                         //!<  Sets whether to exclude sessions for which a user password has been set.

        public void Reset()
        {
            participantNumMin = 0;
            participantNumMax = 0;
            isOpenedOnly = true;
            isVacantOnly = true;
            gameMode = 0;
            pAttributeNativeArray = IntPtr.Zero;
            attributeNativeNum = 0;
            pAttributeRangeArray = IntPtr.Zero;
            attributeRangeNum = 0;
            resultOffset = 0;
            resultSize = 0;
            ratingValue = 0;
            disconnectionRate = 0;
            violationRate = 0;
            isGeoIpUsed = false;
            scoreSettingIndex = 0;
            selectionMethod = (int)SelectionMethod.Random;
            setCondMask = 0;
#if UNITY_ONLY_SWITCH
            pMmatchmakeKeyword = IntPtr.Zero;
#endif
            isExcludeUserPasswordSet = true;
        }

        internal SessionSearchCriteriaNative(SessionSearchCriteria criteria)
        {
            setCondMask = criteria.setCondMask;
            participantNumMin = criteria.GetParticipantNumMin();
            participantNumMax = criteria.GetParticipantNumMax();
            isOpenedOnly = criteria.IsOpenedOnly();
            isVacantOnly = criteria.IsVacantOnly();

            gameMode = criteria.GetGameMode();

            int bufferSize = 0;
            attributeNativeNum = criteria.GetAttributeList().Count;

            List<AttributeNative> attributeNativeList = new List<AttributeNative>();
            foreach (var attribute in criteria.GetAttributeList())
            {
                attributeNativeList.Add(new AttributeNative(attribute));
            }
            pAttributeNativeArray = UnmanagedMemoryManager.WriteList<AttributeNative>(attributeNativeList, ref bufferSize);

            attributeRangeNum = criteria.GetAttributeRangeList().Count;
            pAttributeRangeArray = UnmanagedMemoryManager.WriteList<SessionSearchCriteria.AttributeRange>(criteria.GetAttributeRangeList(), ref bufferSize);

            resultOffset = criteria.resultRange[0];
            resultSize = criteria.resultRange[1];

            pCountryCode = UnmanagedMemoryManager.WriteUtf8(criteria.GetCountryCode(), ref bufferSize);
            ratingValue = criteria.GetRatingValue();
            disconnectionRate = criteria.GetDisconnectionRate();
            violationRate = criteria.GetViolationRate();
            isGeoIpUsed = criteria.IsGeoIpUsed();
            scoreSettingIndex = criteria.GetScoreSettingIndex();
            selectionMethod = (int)criteria.GetSelectionMethod();
#if UNITY_ONLY_SWITCH
            if (criteria.GetSessionMatchmakeKeyword() != null)
            {
                pMmatchmakeKeyword = UnmanagedMemoryManager.WriteUtf8(criteria.GetSessionMatchmakeKeyword(), ref bufferSize);
            }
            else
            {
                pMmatchmakeKeyword = IntPtr.Zero;
            }

#endif
            isExcludeUserPasswordSet = criteria.IsExcludeUserPasswordSet();
        }

        public void Dispose()
        {
            List<AttributeNative> attributeNativeList = new List<AttributeNative>();
            UnmanagedMemoryManager.ReadList<AttributeNative>(pAttributeNativeArray, attributeNativeNum, ref attributeNativeList);
            foreach (var attributeNative in attributeNativeList)
            {
                attributeNative.Dispose();
            }
            UnmanagedMemoryManager.Free(pAttributeNativeArray);
            UnmanagedMemoryManager.Free(pAttributeRangeArray);
            UnmanagedMemoryManager.Free(pCountryCode);
#if UNITY_ONLY_SWITCH
            UnmanagedMemoryManager.Free(pMmatchmakeKeyword);
#endif
        }
    }
    //! @endcond

/*!
    @brief  The class that specifies the matchmaking session owner (normally the same as the session host) as the search condition. (Can be set only for <tt>Inet</tt>.)
*/
    [Serializable]
    public class SessionSearchCriteriaOwner
    {
        private UInt64 ownerConstantId;                                              //!<  Set the constant ID of the owner to specify as the search condition.
        public UInt32[] resultRange = new UInt32[2];                                 //!<  Sets the range of search results to get. Sets the 0th element to the starting position of the entry group retrieved as the result of searching the entry list matching the search conditions, and sets the first element to the number of results retrieved.

/*!
        @brief  Set the constant ID of the owner to specify as the search condition.
        @param[in] ownerConstantId  The constant ID of the owner to specify as the search condition.
*/
        public void SetOwnerConstantId(UInt64 ownerConstantId)
        {
            this.ownerConstantId = ownerConstantId;
        }

/*!
        @brief  Gets the constant ID of the owner specified as the search condition.
        @return  The constant ID of the owner.
*/
        public UInt64 GetOwnerConstantId()
        {
            return this.ownerConstantId;

        }
    }

/*!
    @cond  PRIVATE
    @brief  <tt>SessionSearchCriteriaOwner</tt> available for <tt>Native</tt> use.
*/
    [StructLayout(LayoutKind.Sequential)]
    public class SessionSearchCriteriaOwnerNative : IDisposable
    {
        public UInt64 ownerConstantId;
        public UInt32 resultOffset;
        public UInt32 resultSize;

        internal SessionSearchCriteriaOwnerNative()
        {
            ownerConstantId = 0;
            resultOffset = 0;
            resultSize = 0;
        }

        internal SessionSearchCriteriaOwnerNative(SessionSearchCriteriaOwner criteria)
        {
            ownerConstantId = criteria.GetOwnerConstantId();
            resultOffset = criteria.resultRange[0];
            resultSize = criteria.resultRange[1];
        }

        public void Dispose(){}
    }
    //! @endcond

/*!
    @brief  The class that specifies constant IDs as session search criteria.
*/
    [Serializable]
    public class SessionSearchCriteriaParticipant
    {
        public List<UInt64> participantConstantIdList = new List<UInt64>();                    //!<  Set the constant IDs of the peers to specify as search criteria. If a session that includes a specified ID is found, you can get that ID in the <tt><var>targetConstantId</var></tt> attribute of the <tt>SessionProperty</tt> class.
        public bool isApplicationDataEnabled = false;                                          //!<  Sets whether to include application-defined data in the search results. The default is <tt>false</tt> to alleviate the server load.
    }

/*!
    @cond  PRIVATE
    @brief  <tt>SessionSearchCriteriaParticipant</tt> available for <tt>Native</tt> use.
*/
    [StructLayout(LayoutKind.Sequential)]
    public class SessionSearchCriteriaParticipantNative : IDisposable
    {
        public IntPtr pConstantIdList;
        public int constantIdNum;
        public bool isApplicationDataEnabled = false;

        internal SessionSearchCriteriaParticipantNative()
        {
            pConstantIdList = IntPtr.Zero;
            constantIdNum = 0;
            isApplicationDataEnabled = false;
        }

        internal SessionSearchCriteriaParticipantNative(SessionSearchCriteriaParticipant criteria)
        {
            int bufferSize = 0;
            constantIdNum = criteria.participantConstantIdList.Count;
            pConstantIdList = UnmanagedMemoryManager.WriteList<UInt64>(criteria.participantConstantIdList, ref bufferSize);
            isApplicationDataEnabled = criteria.isApplicationDataEnabled;
        }

        public void Dispose()
        {
            UnmanagedMemoryManager.Free(pConstantIdList);
        }
    }
    //! @endcond

/*!
     @brief  The class that specifies session IDs as session search criteria.
*/
    public class SessionSearchCriteriaSessionId
    {
        public UInt32[] sessionIdArray;                 //!<  The list of session IDs to specify as search criteria.
    }

/*!
     @cond  PRIVATE
*/
    public struct SessionSearchCriteriaSessionIdNative : IDisposable
    {
        IntPtr pSessionIdArray;
        UInt32 sessionIdArraySize;

        internal SessionSearchCriteriaSessionIdNative(SessionSearchCriteriaSessionId criteria)
        {
            int bufferSize = 0;
            pSessionIdArray = UnmanagedMemoryManager.WriteArray<UInt32>(criteria.sessionIdArray, ref bufferSize);
            sessionIdArraySize = (UInt32)criteria.sessionIdArray.Length;
        }

        public void Dispose()
        {
            UnmanagedMemoryManager.Free(pSessionIdArray);
        }
    }
    //! @endcond

/*!
        @brief  Represents information about a station that is participating in a session.
*/
    public class StationInfo
    {
        public UInt64 constantId { get; private set; }           //!<  The station ID.
        public int stationIndex { get; private set; }           //!<  The station index.
        public UInt16 playerNum { get; private set; }              //!<  The number of players on the local station. During joint session migration, this number is temporarily <tt>0</tt>.
        public List<PiaPlugin.PlayerInfo> playerInfoList { get; private set; } //!<  The information for the players on the local station. During joint session migration, the number of elements is temporarily <tt>0</tt>.
        public int rtt { get; private set; }                    //!<  The round trip time.
        public float unicastPacketLossRate { get; private set; }       //!<  The packet loss rate for packets sent and received using unicasting. Valid only when <tt>PiaPlugin.InitializeTransportSetting.measurementInterval</tt> is set to something other than <tt>0</tt>. Becomes <tt>-1</tt> when measurement cannot be done because the number of packets received from a peer station is <tt>0</tt>.
        public float broadcastPacketLossRate { get; private set; }       //!<  The packet loss rate for packets sent and received using broadcasting. Valid only when <tt>PiaPlugin.InitializeTransportSetting.measurementInterval</tt> is set to something other than <tt>0</tt>. Becomes <tt>-1</tt> when measurement cannot be done because the number of packets received from a peer station is <tt>0</tt>.

        public StationInfo()
        {
            constantId = 0;
            stationIndex = 0;
            playerNum = 0;
            playerInfoList = null;
            rtt = 0;
            unicastPacketLossRate = 0;
            broadcastPacketLossRate = 0;
        }

/*!
         @cond  PRIVATE
*/
        internal StationInfo(StationInfoNative stationInfoNative)
        {
            constantId = stationInfoNative.constantId;
            stationIndex = stationInfoNative.stationIndex;
            playerNum = stationInfoNative.playerNum;
            playerInfoList = new List<PiaPlugin.PlayerInfo>();
            if (playerNum > 0)
            {
                PiaPlugin.PlayerInfoNative[] playerInfoNativeArray = new PiaPlugin.PlayerInfoNative[playerNum];
                UnmanagedMemoryManager.ReadArray(stationInfoNative.pStationInfoArray, playerNum, ref playerInfoNativeArray);

                for (int i = 0; i < playerNum; ++i)
                {
                    playerInfoList.Add(new PiaPlugin.PlayerInfo(playerInfoNativeArray[i]));
                }
            }
            rtt = stationInfoNative.rtt;
            unicastPacketLossRate = stationInfoNative.unicastPacketLossRate;
            broadcastPacketLossRate = stationInfoNative.broadcastPacketLossRate;
        }
        //! @endcond
    }

/*!
        @cond  PRIVATE
        @brief  <tt>StationInfo</tt> available for <tt>Native</tt> use.
*/
    [StructLayout(LayoutKind.Sequential)]
    internal struct StationInfoNative : IDisposable
    {
        public UInt64 constantId { get; private set; }
        public int stationIndex { get; private set; }
        public UInt16 playerNum { get; private set; }
        public IntPtr pStationInfoArray { get; private set; }
        public int rtt { get; private set; }
        public float unicastPacketLossRate { get; private set; }
        public float broadcastPacketLossRate { get; private set; }

        public void Dispose() { }
    }
    //! @endcond

/*!
        @brief  The structure with the collection of session information.
*/
    public class SessionStatus
    {
        public UInt32 sessionId                   //!<  The ID of the joined session.
        {
            get; private set;
        }

        public UInt16 stationNum                  //!<  The active number of stations connected to the currently joined session. During joint session migration, this value is temporarily undefined.
        {
            get; private set;
        }
        public UInt16 playerNum                  //!<  The number of connected players. During joint session migration, this value is temporarily undefined.
        {
            get; private set;
        }
        public UInt16 matchmakeSessionStationNum                  //!<  The number of stations participating in the matchmaking session. During joint session migration, this value is temporarily undefined.
        {
            get; private set;
        }
        public UInt16 matchmakeSessionParticipantNum                  //!<  The number of matchmaking session participants that are connecting in the currently joined session. During joint session migration, this value is temporarily undefined.
        {
            get; private set;
        }

        public UInt64 hostConstantId               //!<  The <tt>ConstantId</tt> representing the host of the session in which the local station is participating.
        {
            get; private set;
        }

        public UInt64 localConstantId              //!<  The <tt>ConstantId</tt> representing the local station in the session.
        {
            get; private set;
        }

        public UInt32 jointSessionId                //!<  The ID of the joined joint session.
        {
            get; private set;
        }

        public UInt64 jointSessionHostConstantId //!<  The <tt>ConstantId</tt> representing the host of the joint session in which the local station is participating.
        {
            get; private set;
        }

        public List<StationInfo> stationInfoList  //!<  Represents information about a station that is participating in a session.
        {
            get; private set;
        }

        public Status status                      //!<  The status of the joined session.
        {
            get; private set;
        }

        public DisconnectReason disconnectReason  //!<  The reasons that communication became impossible (in situations where <tt>Session</tt> is in a state where communication is not possible).
        {
            get; private set;
        }

        public SessionStatus()
        {
            sessionId = 0;
            stationNum = 0;
            playerNum = 0;
            matchmakeSessionStationNum = 0;
            matchmakeSessionParticipantNum = 0;
            hostConstantId = 0;
            localConstantId = 0;
            jointSessionId = 0;
            jointSessionHostConstantId = 0;
            stationInfoList = null;
            status = Status.NotConnected;
            disconnectReason = DisconnectReason.ExternalFactor;
        }

/*!
         @cond  PRIVATE
*/
        internal SessionStatus(SessionStatusNative sessionNative)
        {
            sessionId = sessionNative.sessionId;
            stationNum = sessionNative.stationNum;
            hostConstantId = sessionNative.hostConstantId;
            localConstantId = sessionNative.localConstantId;
            jointSessionId = sessionNative.jointSessionId;
            jointSessionHostConstantId = sessionNative.jointSessionHostConstantId;
            playerNum = sessionNative.playerNum;
            matchmakeSessionStationNum = sessionNative.matchmakeSessionStationNum;
            matchmakeSessionParticipantNum = sessionNative.matchmakeSessionParticipantNum;
            stationInfoList = new List<StationInfo>();
            if (stationNum > 0)
            {
                StationInfoNative[] stationInfoNativeArray = new StationInfoNative[stationNum];
                UnmanagedMemoryManager.ReadArray<StationInfoNative>(sessionNative.pStationInfoList, stationNum, ref stationInfoNativeArray);
                for (int i = 0; i < stationNum; ++i)
                {
                    stationInfoList.Add(new StationInfo(stationInfoNativeArray[i]));
                }

            }
            status = sessionNative.status;
            disconnectReason = sessionNative.disconnectReason;
        }
        //! @endcond
    }

/*!
     @cond  PRIVATE
*/
    [StructLayout(LayoutKind.Sequential)]
    internal struct SessionStatusNative : IDisposable
    {
        public UInt32 sessionId { get; private set; }
        public UInt16 stationNum { get; private set; }
        public UInt16 playerNum { get; private set; }
        public UInt16 matchmakeSessionStationNum { get; private set; }
        public UInt16 matchmakeSessionParticipantNum { get; private set; }
        public UInt64 hostConstantId { get; private set; }
        public UInt64 localConstantId { get; private set; }
        public UInt32 jointSessionId { get; private set; }
        public UInt64 jointSessionHostConstantId { get; private set; }
        public IntPtr pStationInfoList { get; private set; }
        public Status status { get; private set; }
        public DisconnectReason disconnectReason { get; private set; }

        internal SessionStatusNative(SessionStatus sessionStatus)
        {
            sessionId = sessionStatus.sessionId;
            stationNum = sessionStatus.stationNum;
            playerNum = sessionStatus.playerNum;
            matchmakeSessionStationNum = sessionStatus.matchmakeSessionStationNum;
            matchmakeSessionParticipantNum = sessionStatus.matchmakeSessionParticipantNum;
            hostConstantId = sessionStatus.hostConstantId;
            localConstantId = sessionStatus.localConstantId;
            jointSessionId = sessionStatus.jointSessionId;
            jointSessionHostConstantId = sessionStatus.jointSessionHostConstantId;
            int bufferSize = 0;
            pStationInfoList = IntPtr.Zero;
            if (sessionStatus.stationInfoList != null)
            {
                pStationInfoList = UnmanagedMemoryManager.WriteList<StationInfo>(sessionStatus.stationInfoList, ref bufferSize);
            }
            status = sessionStatus.status;
            disconnectReason = sessionStatus.disconnectReason;
        }

        public void Dispose() { }
    }
    //! @endcond

/*!
        @brief  The class that manages the settings for joining sessions.
*/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class JoinSessionSetting
    {
        public UInt32 selectJoinSessionNum;                  //!<  Specifies which of the sessions in the session search results to join. Ignored if <tt><var>sessionId</var></tt> is specified.
        public UInt32 sessionId;                             //!<  Specifies the ID of the session to join. This takes precedence if <tt><var>selectJoinSessionNum</var></tt> is also specified. Only enabled for Internet communication.
        public string wirelessCryptoKey;                     //!<  The passphrase used to encrypt the wireless layer. Necessary for local communication.
#if UNITY_ONLY_SWITCH
        public string sessionUserPassword;                   //!<  User password to set in the session. Can be used for local communication@if NIN_DOC and internet communication@endif .
#endif
    };

/*!
       @cond  PRIVATE
       @brief  <tt>JoinSessionSetting</tt> available for <tt>Native</tt> use.
*/
    [StructLayout(LayoutKind.Sequential)]
    internal class JoinSessionSettingNative : IDisposable
    {
        public UInt32 selectJoinSessionNum;                  //!<  Specifies which of the sessions ('n'th) in the session search results to join.
        public UInt32 sessionId;                             //!<  Specifies the ID of the session to join.
        public IntPtr pWirelessCryptoKey;                    //!<  The passphrase used to encrypt the wireless layer.
#if UNITY_ONLY_SWITCH
        public IntPtr pSessionUserPassword;  //!<  User password to set in the session.
#endif

        internal JoinSessionSettingNative(JoinSessionSetting setting)
        {
            selectJoinSessionNum = setting.selectJoinSessionNum;
            sessionId = setting.sessionId;
            int bufferSize = 0;
            if (setting.wirelessCryptoKey != null)
            {
                pWirelessCryptoKey = UnmanagedMemoryManager.WriteUtf8(setting.wirelessCryptoKey, ref bufferSize);
            }
#if UNITY_ONLY_SWITCH
            if (setting.sessionUserPassword != null)
            {
                pSessionUserPassword = UnmanagedMemoryManager.WriteUtf8(setting.sessionUserPassword, ref bufferSize);
            }
#endif
        }
        public void Dispose()
        {
            UnmanagedMemoryManager.Free(pWirelessCryptoKey);
#if UNITY_ONLY_SWITCH
            UnmanagedMemoryManager.Free(pSessionUserPassword);
#endif
        }
    };
    //! @endcond

/*!
       @brief  Information needed when updating the settings for the specified session.
*/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class UpdateSessionSetting
    {
/*!
        @cond  PRIVATE
        @brief  The items corresponding to each bit in the bit mask indicating the specified items.
*/
        enum CondMask
        {
            ApplicationData = 0,            //!<  Application-defined data.
            SelectionPriority,              //!<  Matchmaking priority when session of the same criteria is found.
            ParticipantNumMin,              //!<  Minimum number of participants
            ParticipantNumMax,              //!<  Maximum number of participants.
            ScoreBasedRatingValue,          //!<  Rating value, when using score-based matchmaking.
            ScoreBasedDisconnectionRate,    //!<  Disconnection rate, when using score-based matchmaking.
            ScoreBasedViolationRate,        //!<  Violation rate, when using score-based matchmaking.
            ScoreBasedCountryCode,          //!<  Country code, when using score-based matchmaking.
            ScoreBasedUpdateGeoIp,          //!<  Update position information (when using score-based matchmaking).
            MatchmakeKeyword,               //!<  Keyword.
            Attribute,                      //!<  Attribute.
            Max
        };
        public UInt32 setCondMask = 0;                                                //!<  Manage the set conditions.
        //! @endcond

/*!
        @brief  Class used for specifying session attributes.
*/
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public class Attribute
        {
            public UInt32 index;  //!<  Attribute index.
            public UInt32 value;  //!<  Attribute value.
        }

        private Byte[] applicationData = (Byte[])System.Linq.Enumerable.Empty<Byte>(); //!<  Sets application-defined data.
        private int applicationDataSize = 0;                                           //!<  The size of the application definition data.
        public Byte priority;                                                         //!<  Sets the matchmaking priority when a session with the same conditions is found.

        // nex::NexUpdateSessionSetting
        private UInt16 participantNumMin;                                              //!<  Sets the minimum number of participants.
        private UInt16 participantNumMax;                                              //!<  Sets the maximum number of people who can participate.

        private List<Attribute> attributeList = new List<Attribute>();                 //!<  Specifies a value to use when searching for each attribute. If a value has not been set for a particular index attribute, the search looks for any value. If other parameters are set, a value must be set for every index.
        private string countryCode = "";                                               //!<  Sets the country code to use for comparison when score-based matchmaking is specified. The country code is a string of two uppercase letters that is an ISO 3166-1 alpha-2 country code.
        private UInt32 ratingValue;                                                    //!<  Sets the rating value to use for comparison when score-based matchmaking is specified.
        private UInt32 disconnectionRate;                                              //!<  Sets the disconnection rate to use for comparison when score-based matchmaking is specified.
        private UInt32 violationRate;                                                  //!<  Sets the violation rate to use for comparison when score-based matchmaking is specified.
        private bool isUpdateGeoIp;                                                    //!<  Sets whether to update the location information (latitude/longitude and country code) for comparison when score-based matchmaking is specified.
        private PiaPlugin.DateTime startedTime;                                        //!<  Sets the start-time information to use for comparison when score-based matchmaking is specified.

#if UNITY_ONLY_SWITCH
        private string sessionUserPassword = "";                                       //!<  User password to set in the session.
        private string matchmakeKeyword;                                               //!<  The keyword.
#endif

/*!
        @brief  Sets application-defined data.
        @param[in] applicationData  Pointer to the buffer for the application-defined data for the session.
        @param[in] applicationDataSize  Size of the application-defined data for the session.
*/
        public void SetApplicationData(Byte[] applicationData, int applicationDataSize)
        {
            setCondMask |= 1 << (int)CondMask.ApplicationData;
            this.applicationData = applicationData;
            this.applicationDataSize = applicationDataSize;

        }

/*!
        @brief  Gets application-defined data.
        @return  The data set by the application.
*/
        public Byte[] GetApplicationData()
        {
            return this.applicationData;
        }

/*!
        @brief  Gets the size of the application-defined data.
        @return  The size of the data set by the application.
*/
        public int GetApplicationDataSize()
        {
            return this.applicationDataSize;
        }

/*!
        @brief  Sets the matchmaking priority when a session with the same conditions is found.
                The maximum value that can be set is <tt>nn::pia::nex::NexSelectionPriorityMax</tt>.
        @param[in] selectionPriority  Matchmaking priority.
*/
        public void SetSelectionPriority(Byte selectionPriority)
        {
            setCondMask |= 1 << (int)CondMask.SelectionPriority;
            this.priority = selectionPriority;
        }

/*!
        @brief  Gets the matchmaking priority when a session with the same requirements is found.
        @return  Matchmaking priority.
*/
        public Byte GetSelectionPriority()
        {
            return this.priority;
        }

/*!
        @brief  Sets the minimum number of participants.
        @param[in] participantNumMin  The minimum number of participants to set in the search criteria.
*/
        public void SetParticipantNumMin(UInt16 participantNumMin)
        {
            setCondMask |= 1 << (int)CondMask.ParticipantNumMin;
            this.participantNumMin = participantNumMin;
            PiaPlugin.Trace("SetParticipantNumMin " + this.participantNumMin);
        }

/*!
        @brief  Gets the minimum number of people who can participate.
        @return  Minimum number of participants
*/
        public UInt16 GetParticipantNumMin()
        {
            return this.participantNumMin;
        }

/*!
        @brief  Sets the maximum number of people who can participate.
        @param[in] participantNumMax  The maximum number of participants to set in the search criteria.
*/
        public void SetParticipantNumMax(UInt16 participantNumMax)
        {
            setCondMask |= 1 << (int)CondMask.ParticipantNumMax;
            this.participantNumMax = participantNumMax;
            PiaPlugin.Trace("SetParticipantNumMax " + this.participantNumMax);
        }

/*!
        @brief  Gets the maximum number of people who can participate.
        @return  Maximum number of participants.
*/
        public UInt16 GetParticipantNumMax()
        {
            return this.participantNumMax;
        }

/*!
        @brief  Sets the country code to use for comparison when score-based matchmaking is specified. The country code is a string of two uppercase letters that is an ISO 3166-1 alpha-2 country code.
        @param[in] countryCode  Country code. You must specify a string of two uppercase letters that is an ISO 3166-1 alpha-2 country code.
*/
        public void SetCountryCode(string countryCode)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedCountryCode;
            this.countryCode = countryCode;
        }

/*!
        @brief  Gets the country code.
        @return  Country code.
*/
        public string GetCountryCode()
        {
            return this.countryCode;
        }

/*!
        @brief  Sets the rating value to use for comparison when score-based matchmaking is specified.
        @details  Can be used with Internet communication.
        @param[in] ratingValue  Rating value.
*/
        public void SetRatingValue(UInt32 ratingValue)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedRatingValue;
            this.ratingValue = ratingValue;
            PiaPlugin.Trace("SetRatingValue " + this.ratingValue);
        }

/*!
        @brief  Gets the rating value.
        @details  Can be used with Internet communication.
        @return  Rating value.
*/
        public UInt32 GetRatingValue()
        {
            return this.ratingValue;
        }

/*!
        @brief  Sets the disconnection rate to use for comparison when score-based matchmaking is specified.
        @param[in] disconnectionRate  Disconnection rate.
*/
        public void SetDisconnectionRate(UInt32 disconnectionRate)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedDisconnectionRate;
            this.disconnectionRate = disconnectionRate;
            PiaPlugin.Trace("SetDisconnectionRate " + this.disconnectionRate);
        }

/*!
        @brief  Gets the specified disconnection rate.
        @return  Disconnection rate.
*/
        public UInt32 GetDisconnectionRate()
        {
            return this.disconnectionRate;
        }

/*!
        @brief  Sets the violation rate to use for comparison when score-based matchmaking is specified.
        @param[in] violationRate  Violation rate.
*/
        public void SetViolationRate(UInt32 violationRate)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedViolationRate;
            this.violationRate = violationRate;
            PiaPlugin.Trace("SetViolationRate " + this.violationRate);
        }

/*!
        @brief  Gets the specified violation rate.
        @return  Violation rate.
*/
        public UInt32 GetViolationRate()
        {
            return this.violationRate;
        }

/*!
        @brief  Sets whether to update the location information (latitude/longitude and country code) for comparison when score-based matchmaking is specified.
        @param[in] isUpdateGeoIp  <tt>true</tt> to update the location information (latitude/longitude and country code).
*/
        public void SetUpdateGeoIp(bool isUpdateGeoIp)
        {
            setCondMask |= 1 << (int)CondMask.ScoreBasedUpdateGeoIp;
            this.isUpdateGeoIp = isUpdateGeoIp;
        }

/*!
        @brief  Gets the setting for whether to update the location information (latitude/longitude and country code).
        @return  Whether to update location information.
*/
        public bool IsUpdateGeoIp()
        {
            return this.isUpdateGeoIp;
        }

/*!
        @brief  Sets the attribute list.
        @details  Cannot be used with <tt><var>matchmakeKeyword</var></tt>. Whichever is set first is valid. [:disable-local]
        @param[in] attributeList  Attribute list.
*/
        public PiaPlugin.Result SetAttributeList(List<Attribute> attributeList)
        {
            if (0 != (setCondMask & (1 << (int)CondMask.MatchmakeKeyword)))
            {
                PiaPlugin.Trace("MatchmakeKeyword is already set. Attribute can not be used with MatchmakeKeyword.");
                return PiaPlugin.InvalidArgumentResult;
            }

            setCondMask |= 1 << (int)CondMask.Attribute;
            this.attributeList = attributeList;
            return PiaPlugin.SuccessResult;
        }

/*!
        @brief  Gets a list of set attributes.
        @return  Attribute list.
*/
        public List<Attribute> GetAttributeList()
        {
            return attributeList;
        }

#if UNITY_ONLY_SWITCH
/*!
        @brief  Sets a user password for the created session. Can be used for local communication@if NIN_DOC and internet communication@endif .
        @param[in] userPassword  The user password to set. The password string must be no longer than <tt>SessionUserPasswordLengthMax</tt>.
        @return  Returns a value that indicates success when the user password is successfully set.
        @retval PiaPlugin.InvalidArgumentResult  The string is null, the encoding is not valid, or the user password exceeds the maximum length. [:progErr]
*/
        public PiaPlugin.Result SetSessionUserPassword(string userPassword)
        {
            if (userPassword == null)
            {
                PiaPlugin.Trace("UserPassword is null.");
                return PiaPlugin.InvalidArgumentResult;
            }
            if (userPassword.Length == 0)
            {
                PiaPlugin.Trace("PasswordLength is zero.");
                return PiaPlugin.InvalidArgumentResult;
            }
            if (userPassword.Length > SessionUserPasswordLengthMax)
            {
                PiaPlugin.Trace("PasswordLength is exceed length. (need PasswordLength <= " + SessionUserPasswordLengthMax + ")");
                return PiaPlugin.InvalidArgumentResult;
            }
            this.sessionUserPassword = userPassword;
            return PiaPlugin.SuccessResult;
        }

/*!
        @brief  Gets the user password for the created session.
        @return  The user password.
*/
        public string GetSessionUserPassword()
        {
            return this.sessionUserPassword;
        }

/*!
        @brief  Sets the keyword for the created session.
        @details  Can be used with Internet communication.
        @param[in] keyword  The keyword for keyword matchmaking. The keyword string must be no longer than <tt>SessionMatchmakeKeywordLength</tt>.
        @return  Returns a successful result when the keyword is successfully specified.
        @retval PiaPlugin.InvalidArgumentResult  Either the string is null, the encoding is invalid, or the keyword exceeds the maximum length. [:progErr]
*/
        public PiaPlugin.Result SetSessionMatchmakeKeyword(string keyword)
        {
            if (keyword == null)
            {
                PiaPlugin.Trace("keyword is null.");
                return PiaPlugin.InvalidArgumentResult;
            }
            if (keyword.Length == 0)
            {
                PiaPlugin.Trace("keyword is zero.");
                return PiaPlugin.InvalidArgumentResult;
            }
            if (keyword.Length > SessionMatchmakeKeywordLength)
            {
                PiaPlugin.Trace("KeywordLength is exceed length. (need Keyword Length <= " + SessionMatchmakeKeywordLength + ")");
                return PiaPlugin.InvalidArgumentResult;
            }
             if (0 != (setCondMask & (1 << (int)CondMask.Attribute)))
            {
                PiaPlugin.Trace("Attribute is already set. MatchmakeKeyword can not be used with Attribute.");
                return PiaPlugin.InvalidArgumentResult;
            }
            setCondMask |= 1 << (int)CondMask.MatchmakeKeyword;
            this.matchmakeKeyword = keyword;
            return PiaPlugin.SuccessResult;
        }

/*!
        @brief  Gets the user password for the created session.
        @details  Can be used with Internet communication.
        @return  The user password.
*/
        public string GetSessionMatchmakeKeyword()
        {
            return this.matchmakeKeyword;
        }

/*!
        @brief  Sets the updated date and time for the session start date and time.
        @param[in] dateTime  The start time to set.
        @return  Returns a result value indicating success when the start time has been successfully set.
        @retval PiaPlugin.ResultInvalidArgument  The date and time have not been set or have been set incorrectly. [:progErr]
*/
        public PiaPlugin.Result SetStartedTime(PiaPlugin.DateTime dateTime)
        {
            startedTime = dateTime;

            return PiaPlugin.SuccessResult;
        }
#endif
/*!
        @brief  Gets the configured start time.
        @return  Starting timestamp.
*/
        public PiaPlugin.DateTime GetStartedTime()
        {
            return this.startedTime;
        }

    };

/*!
       @cond  PRIVATE
       @brief  <tt>UpdateSessionSetting</tt> available for <tt>Native</tt> use.
*/
    [StructLayout(LayoutKind.Sequential)]
    internal class UpdateSessionSettingNative : IDisposable
    {
        public UInt32 setCondMask = 0;  //!< @cond  PRIVATE Manage the set conditions. @endcond
        public IntPtr pApplicationData;  //!<  Sets application-defined data.
        public int applicationDataSize; //!<  The size of application definition data.
        public Byte priority;           //!<  Sets the matchmaking priority when a session with the same conditions is found.

        // nex::NexUpdateSessionSetting
        public UInt16 participantNumMin;
        public UInt16 participantNumMax;

        public IntPtr pAttributeArray;      //!<  The attribute value.
        public int attributeNum;
        public UInt32 ratingValue;          //!<  Sets the rating value to use for comparison when score-based matchmaking is specified.
        public UInt32 disconnectionRate;    //!<  Sets the disconnection rate to use for comparison when score-based matchmaking is specified.
        public UInt32 violationRate;        //!<  Sets the violation rate to use for comparison when score-based matchmaking is specified.

        public IntPtr pCountryCode;         //!<  Sets the country code to use for comparison when score-based matchmaking is specified. The country code is a string of two uppercase letters that is an ISO 3166-1 alpha-2 country code. Used when GeoIp is disabled and the GeoIp location information cannot be obtained.
        [MarshalAs(UnmanagedType.U1)]
        public bool isUpdateGeoIp;          //!<  Sets whether to update the location information (latitude/longitude and country code) for comparison when score-based matchmaking is specified.

#if UNITY_ONLY_SWITCH
        public IntPtr pSessionUserPassword; //!<  The user password.
        private IntPtr pMmatchmakeKeyword;                                                //!<  The keyword set for the session.
#endif

        internal UpdateSessionSettingNative()
        {
            setCondMask = 0;
            pApplicationData = IntPtr.Zero;

            participantNumMin = 0;
            participantNumMax = 0;
            pAttributeArray = IntPtr.Zero;
            attributeNum = 0;
            pCountryCode = IntPtr.Zero;
            ratingValue = 0;
            disconnectionRate = 0;
            violationRate = 0;
            isUpdateGeoIp = false;
#if UNITY_ONLY_SWITCH
            pSessionUserPassword = IntPtr.Zero;
            pMmatchmakeKeyword = IntPtr.Zero;
#endif
        }

        internal UpdateSessionSettingNative(UpdateSessionSetting setting)
        {
            setCondMask = setting.setCondMask;
            int bufferSize = 0;
            pApplicationData = UnmanagedMemoryManager.WriteArray<Byte>(setting.GetApplicationData(), ref bufferSize);
            applicationDataSize = setting.GetApplicationDataSize();
            priority = setting.priority;

            attributeNum = setting.GetAttributeList().Count;
            if (attributeNum != 0)
            {
                pAttributeArray = UnmanagedMemoryManager.WriteList<UpdateSessionSetting.Attribute>(setting.GetAttributeList(), ref bufferSize);
            }

            participantNumMin = setting.GetParticipantNumMin();
            participantNumMax = setting.GetParticipantNumMax();
            pCountryCode = UnmanagedMemoryManager.WriteUtf8(setting.GetCountryCode(), ref bufferSize);
            ratingValue = setting.GetRatingValue();
            disconnectionRate = setting.GetDisconnectionRate();
            violationRate = setting.GetViolationRate();
#if UNITY_ONLY_SWITCH
            if (setting.GetSessionUserPassword().Length>0)
            {
                pSessionUserPassword = UnmanagedMemoryManager.WriteUtf8(setting.GetSessionUserPassword(), ref bufferSize);
            }
            if (setting.GetSessionMatchmakeKeyword() != null)
            {
                pMmatchmakeKeyword = UnmanagedMemoryManager.WriteUtf8(setting.GetSessionMatchmakeKeyword(), ref bufferSize);
            }
#endif
        }
        public void Dispose()
        {
            UnmanagedMemoryManager.Free(pAttributeArray);
            UnmanagedMemoryManager.Free(pApplicationData);
            UnmanagedMemoryManager.Free(pCountryCode);
#if UNITY_ONLY_SWITCH
            UnmanagedMemoryManager.Free(pSessionUserPassword);
            UnmanagedMemoryManager.Free(pMmatchmakeKeyword);
#endif
        }
    };
    //! @endcond

/*!
        @brief  Information on browsed sessions.
*/
    [StructLayout(LayoutKind.Sequential)]
    public class SessionProperty
    {
        public UInt16 gameMode;                 //!<  Game mode specified in the session being built.
        public UInt32 sessionId;                //!<  The session ID.
        public UInt16 currentParticipantNum;    //!<  The number of people participating in a session.
        public UInt16 participantNumMin;        //!<  The minimum number of people who can participate in a session.
        public UInt16 participantNumMax;        //!<  The maximum number of people who can participate in a session.
        [MarshalAs(UnmanagedType.U1)]
        public bool isOpened;                   //!<  Becomes true when a session is open for recruitment. Can get the session host only.
        [MarshalAs(UnmanagedType.U1)]
        public bool isRestrictedByUserPassword; //!<  Determines whether a user password is set for the session. Returns <tt>true</tt> if a user password is set.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ApplicationDataSystemBufferSizeMax)]
        public Byte[] applicationData;          //!<  Application-defined data for a session.
        public UInt32 applicationDataSize;      //!<  The size of the application-defined data for a session.
#if UNITY_ONLY_SWITCH
        public UInt64 targetConstantId;        //!<  The constant ID specified in the session search criteria. Can only get when specified as a search term using <tt>SessionSearchCriteriaParticipant</tt>.
#endif
        public SessionProperty()
        {
            gameMode = 0;
            sessionId = 0;
            currentParticipantNum = 0;
            participantNumMin = 0;
            participantNumMax = 0;
            isOpened = true;
            isRestrictedByUserPassword = true;
            applicationData = new Byte[ApplicationDataSystemBufferSizeMax];
            applicationDataSize = 0;
#if UNITY_ONLY_SWITCH
            UInt64 targetConstantId = 0;
#endif
        }

/*!
         @cond  PRIVATE
*/
        internal SessionProperty(SessionPropertyNative sessionPropertyNative)
        {
            gameMode = sessionPropertyNative.gameMode;
            sessionId = sessionPropertyNative.sessionId;
            currentParticipantNum = sessionPropertyNative.currentParticipantNum;
            participantNumMin = sessionPropertyNative.participantNumMin;
            participantNumMax = sessionPropertyNative.participantNumMax;
            isOpened = sessionPropertyNative.isOpened;
            isRestrictedByUserPassword = sessionPropertyNative.isRestrictedByUserPassword;
            applicationDataSize = sessionPropertyNative.applicationDataSize;
            applicationData = new Byte[ApplicationDataSystemBufferSizeMax];
            if (applicationDataSize > 0)
            {
                Array.Copy(sessionPropertyNative.applicationData, 0, applicationData, 0, applicationDataSize);

            }
#if UNITY_ONLY_SWITCH
            targetConstantId = sessionPropertyNative.targetConstantId;
#endif
        }
        //! @endcond
    };

/*!
      @cond  PRIVATE
*/
    [StructLayout(LayoutKind.Sequential)]
    internal struct SessionPropertyNative : IDisposable
    {
        public UInt16 gameMode;                 //!<  Game mode specified in the session being built.
        public UInt32 sessionId;                //!<  The session ID.
        public UInt16 currentParticipantNum;    //!<  The number of people participating in a session.
        public UInt16 participantNumMin;        //!<  The minimum number of people who can participate in a session.
        public UInt16 participantNumMax;        //!<  The maximum number of people who can participate in a session.
        [MarshalAs(UnmanagedType.U1)]
        public bool isOpened;                   //!<  Becomes true when a session is open for recruitment.
        [MarshalAs(UnmanagedType.U1)]
        public bool isRestrictedByUserPassword; //!<  Determines whether a user password is set for the session. Returns <tt>true</tt> if a user password is set.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ApplicationDataSystemBufferSizeMax)]
        public Byte[] applicationData;          //!<  Application-defined data for a session.
        public UInt32 applicationDataSize;      //!<  The size of the application-defined data for a session.
#if UNITY_ONLY_SWITCH
        public UInt64 targetConstantId;        //!<  The constant ID specified in the session search criteria.
#endif
        public void Dispose(){}
    }
    //! @endcond

#if UNITY_EDITOR
    static Session_OpenSessionAsync OpenSessionAsyncNative;
    static Session_UpdateAndOpenSessionAsync UpdateAndOpenSessionAsyncNative;
    static Session_CloseSessionAsync CloseSessionAsyncNative;
    static Session_GetSessionStatus GetSessionStatusNative;
    static Session_KickoutStation KickoutStationNative;
    static Session_BrowseSessionAsync BrowseSessionAsyncNative;
    static Session_BrowseSessionAsyncWithOwner BrowseSessionAsyncWithOwnerNative;
    static Session_BrowseSessionAsyncWithParticipant BrowseSessionAsyncWithParticipantNative;
    static Session_BrowseSessionAsyncWithSessionId BrowseSessionAsyncWithSessionIdNative;
    static Session_CreateSessionAsync CreateSessionAsyncNative;
    static Session_JoinSessionAsync JoinSessionAsyncNative;
    static Session_UpdateSessionSettingAsync UpdateSessionSettingAsyncNative;
    static Session_GetSessionProperty GetSessionPropertyNative;
    static Session_RequestSessionPropertyAsync RequestSessionPropertyAsyncNative;
    static Session_GetBrowsedSessionPropertyListSize GetBrowsedSessionPropertyListSizeNative;
    static Session_GetBrowsedSessionProperty GetBrowsedSessionPropertyNative;
    static Session_GetConstantIdList GetConstantIdListNative;
    static Session_DestroyJointSessionAsync DestroyJointSessionAsyncNative;
    static Session_OpenJointSessionAsync OpenJointSessionAsyncNative;
    static Session_UpdateAndOpenJointSessionAsync UpdateAndOpenJointSessionAsyncNative;
    static Session_CloseJointSessionAsync CloseJointSessionAsyncNative;
    static Session_UpdateJointSessionSettingAsync UpdateJointSessionSettingAsyncNative;
    static Session_GetJointSessionProperty GetJointSessionPropertyNative;
    static Session_RequestJointSessionPropertyAsync RequestJointSessionPropertyAsyncNative;
    static Session_IsHost IsHostNative;
    static Session_IsJointSessionHost IsJointSessionHostNative;
    static Session_GetSessionOpenStatus GetSessionOpenStatusNative;
    static Session_GetJointSessionOpenStatus GetJointSessionOpenStatusNative;
    static Session_GetInetSearchCriteriaListSizeMax GetInetSearchCriteriaListSizeMaxNative;
    static Session_GetLanSearchCriteriaListSizeMax GetLanSearchCriteriaListSizeMaxNative;
    public static void InitializeHooks(IntPtr? plugin_dll)
    {
        IntPtr pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_OpenSessionAsync");
        OpenSessionAsyncNative = (Session_OpenSessionAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_OpenSessionAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + OpenSessionAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_UpdateAndOpenSessionAsync");
        UpdateAndOpenSessionAsyncNative = (Session_UpdateAndOpenSessionAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_UpdateAndOpenSessionAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + UpdateAndOpenSessionAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_CloseSessionAsync");
        CloseSessionAsyncNative = (Session_CloseSessionAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_CloseSessionAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + CloseSessionAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_GetSessionStatus");
        GetSessionStatusNative = (Session_GetSessionStatus)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_GetSessionStatus));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetSessionStatusNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_KickoutStation");
        KickoutStationNative = (Session_KickoutStation)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_KickoutStation));
        PiaPluginUtil.UnityLog("InitializeHooks " + KickoutStationNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_BrowseSessionAsync");
        BrowseSessionAsyncNative = (Session_BrowseSessionAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_BrowseSessionAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + BrowseSessionAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_BrowseSessionAsyncWithOwner");
        BrowseSessionAsyncWithOwnerNative = (Session_BrowseSessionAsyncWithOwner)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_BrowseSessionAsyncWithOwner));
        PiaPluginUtil.UnityLog("InitializeHooks " + BrowseSessionAsyncWithOwnerNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_BrowseSessionAsyncWithParticipant");
        BrowseSessionAsyncWithParticipantNative = (Session_BrowseSessionAsyncWithParticipant)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_BrowseSessionAsyncWithParticipant));
        PiaPluginUtil.UnityLog("InitializeHooks " + BrowseSessionAsyncWithParticipantNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_BrowseSessionAsyncWithSessionId");
        BrowseSessionAsyncWithSessionIdNative = (Session_BrowseSessionAsyncWithSessionId)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_BrowseSessionAsyncWithSessionId));
        PiaPluginUtil.UnityLog("InitializeHooks " + BrowseSessionAsyncWithSessionIdNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_CreateSessionAsync");
        CreateSessionAsyncNative = (Session_CreateSessionAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_CreateSessionAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + CreateSessionAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_JoinSessionAsync");
        JoinSessionAsyncNative = (Session_JoinSessionAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_JoinSessionAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + JoinSessionAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_UpdateSessionSettingAsync");
        UpdateSessionSettingAsyncNative = (Session_UpdateSessionSettingAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_UpdateSessionSettingAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + UpdateSessionSettingAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_GetSessionProperty");
        GetSessionPropertyNative = (Session_GetSessionProperty)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_GetSessionProperty));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetSessionPropertyNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_RequestSessionPropertyAsync");
        RequestSessionPropertyAsyncNative = (Session_RequestSessionPropertyAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_RequestSessionPropertyAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + RequestSessionPropertyAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_GetBrowsedSessionPropertyListSize");
        GetBrowsedSessionPropertyListSizeNative = (Session_GetBrowsedSessionPropertyListSize)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_GetBrowsedSessionPropertyListSize));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetBrowsedSessionPropertyListSizeNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_GetBrowsedSessionProperty");
        GetBrowsedSessionPropertyNative = (Session_GetBrowsedSessionProperty)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_GetBrowsedSessionProperty));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetBrowsedSessionPropertyNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_GetConstantIdList");
        GetConstantIdListNative = (Session_GetConstantIdList)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_GetConstantIdList));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetConstantIdListNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_DestroyJointSessionAsync");
        DestroyJointSessionAsyncNative = (Session_DestroyJointSessionAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_DestroyJointSessionAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + DestroyJointSessionAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_OpenJointSessionAsync");
        OpenJointSessionAsyncNative = (Session_OpenJointSessionAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_OpenJointSessionAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + OpenJointSessionAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_UpdateAndOpenJointSessionAsync");
        UpdateAndOpenJointSessionAsyncNative = (Session_UpdateAndOpenJointSessionAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_UpdateAndOpenJointSessionAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + OpenJointSessionAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_OpenJointSessionAsync");
        OpenJointSessionAsyncNative = (Session_OpenJointSessionAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_OpenJointSessionAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + OpenJointSessionAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_CloseJointSessionAsync");
        CloseJointSessionAsyncNative = (Session_CloseJointSessionAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_CloseJointSessionAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + CloseJointSessionAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_UpdateJointSessionSettingAsync");
        UpdateJointSessionSettingAsyncNative = (Session_UpdateJointSessionSettingAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_UpdateJointSessionSettingAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + UpdateJointSessionSettingAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_GetJointSessionProperty");
        GetJointSessionPropertyNative = (Session_GetJointSessionProperty)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_GetJointSessionProperty));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetJointSessionPropertyNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_RequestJointSessionPropertyAsync");
        RequestJointSessionPropertyAsyncNative = (Session_RequestJointSessionPropertyAsync)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_RequestJointSessionPropertyAsync));
        PiaPluginUtil.UnityLog("InitializeHooks " + RequestJointSessionPropertyAsyncNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_IsHost");
        IsHostNative = (Session_IsHost)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_IsHost));
        PiaPluginUtil.UnityLog("InitializeHooks " + IsHostNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_IsJointSessionHost");
        IsJointSessionHostNative = (Session_IsJointSessionHost)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_IsJointSessionHost));
        PiaPluginUtil.UnityLog("InitializeHooks " + IsJointSessionHostNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_GetSessionOpenStatus");
        GetSessionOpenStatusNative = (Session_GetSessionOpenStatus)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_GetSessionOpenStatus));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetSessionOpenStatusNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_GetJointSessionOpenStatus");
        GetJointSessionOpenStatusNative = (Session_GetJointSessionOpenStatus)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_GetJointSessionOpenStatus));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetJointSessionOpenStatusNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_GetInetSearchCriteriaListSizeMax");
        GetInetSearchCriteriaListSizeMaxNative = (Session_GetInetSearchCriteriaListSizeMax)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_GetInetSearchCriteriaListSizeMax));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetInetSearchCriteriaListSizeMaxNative);

        pAddressOfFunctionToCall = PiaPlugin.HookUtil(plugin_dll, "Session_GetLanSearchCriteriaListSizeMax");
        GetLanSearchCriteriaListSizeMaxNative = (Session_GetLanSearchCriteriaListSizeMax)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Session_GetLanSearchCriteriaListSizeMax));
        PiaPluginUtil.UnityLog("InitializeHooks " + GetLanSearchCriteriaListSizeMaxNative);

    }
#endif


#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result Session_OpenSessionAsync();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_OpenSessionAsync")]
#else
    [DllImport("__Internal", EntryPoint = "Session_OpenSessionAsync")]
#endif
    private static extern PiaPlugin.Result OpenSessionAsyncNative();
#endif

/*!
    @brief  Starts accepting participants in the session.
    @details

           This function starts an asynchronous process that accepts participants.
           Only the session host can execute this function.
           Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidState</tt> indicates that the function call occurred in an invalid state. Make sure that no other asynchronous processes are running. [:progErr]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  <tt>ResultSessionConnectionIsLost</tt> indicates that the station was disconnected from the mesh. [:handling-leave]
    @details  <tt>ResultSessionIsNotFound</tt> indicates that the target session disappeared during the asynchronous process. [:handling-cleanup]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @details  <tt>ResultGameServerProcessAborted</tt> indicates that a game server process failed. [:handling-logout]
    @return  Returns a result value that indicates success if the asynchronous process starts successfully. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running, <tt>Startup</tt> has been called, the session is in participating state, and the local station is the session host. [:progErr]
    @retval ResultNetworkConnectionIsLost  The network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
*/
    public static PiaPlugin.Result OpenSessionAsync()
    {
        PiaPlugin.Result result = OpenSessionAsyncNative();
        return result;
    }


#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate PiaPlugin.Result Session_UpdateAndOpenSessionAsync([In, MarshalAs(UnmanagedType.LPStruct)] UpdateSessionSettingNative setting, [In, MarshalAs(UnmanagedType.LPStruct)] PiaPlugin.DateTime startedTime);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_UpdateAndOpenSessionAsync")]
#else
    [DllImport("__Internal", EntryPoint = "Session_UpdateAndOpenSessionAsync")]
#endif
    private static extern PiaPlugin.Result UpdateAndOpenSessionAsyncNative([In, MarshalAs(UnmanagedType.LPStruct)] UpdateSessionSettingNative setting, [In, MarshalAs(UnmanagedType.LPStruct)] PiaPlugin.DateTime startedTime);
#endif

/*!
    @brief  Starts the asynchronous process of accepting session participants and simultaneously updating the session setting.
    @details

           Starts the asynchronous process of accepting session participants and simultaneously updating the session setting.
           Only the session host can execute this function.
           Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidState</tt> indicates that the function call occurred in an invalid state. Make sure that no other asynchronous processes are running, <tt>Startup</tt> has been called, the session is in participating state, and the local station is the session host. [:progErr]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @param[in] setting  The settings data to specify when changing.
    @return  Returns a result value that indicates success if the asynchronous process starts successfully. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running, <tt>Startup</tt> has been called, the session is in participating state, and the local station is the session host. [:progErr]
    @retval ResultNetworkConnectionIsLost  The network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
*/
    public static PiaPlugin.Result UpdateAndOpenSessionAsync(UpdateSessionSetting setting)
    {
        using (UpdateSessionSettingNative updateSessionSettingNative = new UpdateSessionSettingNative(setting))
        {
            PiaPlugin.Result result = UpdateAndOpenSessionAsyncNative(updateSessionSettingNative, setting.GetStartedTime());
            return result;
        }
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Session_CloseSessionAsync();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_CloseSessionAsync")]
#else
    [DllImport("__Internal", EntryPoint = "Session_CloseSessionAsync")]
#endif
    private static extern PiaPlugin.Result CloseSessionAsyncNative();
#endif

/*!
    @brief  Closes a session to new participants.
    @details

           This function starts an asynchronous process that closes a session to new participants.
           Only the session host can execute this function.
           Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.

           Hypothetically, a station could attempt to join the session immediately before this function is called and end up joining after the asynchronous process has completed.
           For this reason, the asynchronous process ends after waiting for the session to close and for the number of participants in the matchmaking session to match the number of participants in the mesh.
           The asynchronous process also ends if the numbers do not match after a certain amount of time (15 seconds). In this case, @ref PiaPlugin.GetAsyncProcessState
           returns <tt>ResultNegligibleFault</tt>. There could be attempts to join after the asynchronous process has ended.
           Handle these attempts as appropriate in the application.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidState</tt> indicates that the function call occurred in an invalid state. Make sure that no other asynchronous processes are running. [:progErr]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  <tt>ResultSessionConnectionIsLost</tt> indicates that the station was disconnected from the mesh. [:handling-leave]
    @details  <tt>ResultSessionIsNotFound</tt> indicates that the target session disappeared during the asynchronous process. [:handling-cleanup]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @details  <tt>ResultGameServerProcessAborted</tt> indicates that a game server process failed. [:handling-logout]
    @details  <tt>ResultNegligibleFault</tt> indicates that a certain amount of time passed without the number of participants in the matchmaking sessions matching the number in the mesh. [:handling]
    @return  Returns a result value that indicates success if the asynchronous process starts successfully. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running, <tt>Startup</tt> has been called, the session is in participating state, and the local station is the session host. [:progErr]
    @retval ResultNetworkConnectionIsLost  The network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
*/
    public static PiaPlugin.Result CloseSessionAsync()
    {
        PiaPlugin.Result result = CloseSessionAsyncNative();
        return result;
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PiaPlugin.Result Session_GetSessionStatus(ref SessionStatusNative sessionStatusNative);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_GetSessionStatus")]
#else
    [DllImport("__Internal", EntryPoint = "Session_GetSessionStatus")]
#endif
    private static extern PiaPlugin.Result GetSessionStatusNative(ref SessionStatusNative sessionStatusNative);
#endif

/*!
    @brief  Gets the structure that contains <tt><var>constantId</var></tt>, <tt><var>stationNum</var></tt>, and so on.
    @details  During session migration when the joint session feature is being used, there may be times when the information cannot be obtained and @ref PiaPluginSession.SessionStatus.stationInfoList contains null information.
    @param[in] sessionStatus  The variable that stored <tt>SessionStatus</tt>.
    @return  If successfully obtained, returns an instance of <tt>Result</tt> indicating success. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultNotInitialized  Either Pia or PiaUnity or both are not initialized.
*/
    public static PiaPlugin.Result GetSessionStatus(ref SessionStatus sessionStatus)
    {
        PiaPlugin.Result result;
        SessionStatusNative sessionStatusNative = new SessionStatusNative();
        result = GetSessionStatusNative(ref sessionStatusNative);
        sessionStatus = new SessionStatus(sessionStatusNative);
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Session_KickoutStation(UInt64 constantId);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_KickoutStation")]
#else
    [DllImport("__Internal", EntryPoint = "Session_KickoutStation")]
#endif
    private static extern PiaPlugin.Result KickoutStationNative(UInt64 constantId);
#endif

/*!
    @brief  Kicks a station from the mesh.
    @details  Only the mesh host can call this function.

            A kick out notification is sent to the station assigned the <tt>ConstantId</tt> specified in the argument,
            and that station leaves the mesh when it receives the notification.
            Because of latency in transmitting the disconnect notification and other factors, there may be a delay between successfully calling this function, and the station leaving the mesh.
    @param[in] constantId  The <tt>ConstantId</tt> assigned to the station being kicked.
    @return  Returns a <tt>Result</tt> value for which <tt>IsSuccess()</tt> is <tt>true</tt>. if the station is successfully kicked from the mesh. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidState  A node other than the mesh host called the function. [:progErr]
    @retval ResultInvalidArgument  The <tt>ConstantId</tt> specified in the arguments belongs to the mesh host, or no station in the mesh has that ID. [:progErr]
    @retval ResultInProgress  The process to kick the station with the <tt>ConstantId</tt> specified in the arguments has already started. [:handling]
    @retval ResultBufferIsFull  The buffer for kicking stations or the buffer for sending kick notifications is full. It may succeed if you try again after some time has passed. [:handling]
*/
    public static PiaPlugin.Result KickoutStation(UInt64 constantId)
    {
        PiaPlugin.Result result = KickoutStationNative(constantId);
        return result;
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Session_BrowseSessionAsync([In, MarshalAs(UnmanagedType.LPStruct)] SessionSearchCriteriaNative criteria);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", CharSet = CharSet.Ansi, EntryPoint = "Session_BrowseSessionAsync")]
#else
    [DllImport("__Internal", CharSet = CharSet.Ansi, EntryPoint = "Session_BrowseSessionAsync")]
#endif
    private static extern PiaPlugin.Result BrowseSessionAsyncNative([In, MarshalAs(UnmanagedType.LPStruct)] SessionSearchCriteriaNative criteria);
#endif

/*!
    @brief  Searches for sessions.
    @details

           This function starts an asynchronous process that searches for sessions.
           Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.
           If the search process succeeds, the list of retrieved sessions is updated and can be obtained using @ref GetBrowsedSessionProperty.
           The order of the sessions in the search list depends on the specifications of other network libraries used.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidArgument</tt> indicates that an unexpected argument was passed at the start of the asynchronous process. [:progErr]
    @details  <tt>ResultInvalidState</tt> indicates that the asynchronous process has not completed or has not started. [:progErr]
    @details  <tt>ResultCancelled</tt> indicates that the running asynchronous process was canceled. [:handling-cleanup]
    @details  <tt>ResultGameServerMaintenance</tt> indicates that the game server is under maintenance. [:viewer] [:handling-logout]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @details  <tt>ResultErrorOccurred</tt> indicates an internal error. This result is only returned during local communication. [:handling-cleanup]
    @param[in] criteria  Search criteria to use when searching for sessions.
    @return  Returns a result value that indicates success if the asynchronous process starts successfully.
    @retval ResultInvalidArgument  Unexpected arguments were passed to the function when starting the asynchronous process. [:progErr]
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running, that <tt>Startup</tt> has been called, and that communication is not already taking place. [:progErr]
    @retval ResultNetworkConnectionIsLost  The network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
*/
    public static PiaPlugin.Result BrowseSessionAsync(SessionSearchCriteria criteria)
    {
        PiaPlugin.Trace("BrowseSessionAsync called");
        using (SessionSearchCriteriaNative criteriaNative = new SessionSearchCriteriaNative(criteria))
        {
            PiaPlugin.Result result = BrowseSessionAsyncNative(criteriaNative);
            return result;
        }
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Session_BrowseSessionAsyncWithOwner([In, MarshalAs(UnmanagedType.LPStruct)] SessionSearchCriteriaOwnerNative criteria);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", CharSet = CharSet.Ansi, EntryPoint = "Session_BrowseSessionAsyncWithOwner")]
#else
    [DllImport("__Internal", CharSet = CharSet.Ansi, EntryPoint = "Session_BrowseSessionAsyncWithOwner")]
#endif
    private static extern PiaPlugin.Result BrowseSessionAsyncWithOwnerNative([In, MarshalAs(UnmanagedType.LPStruct)] SessionSearchCriteriaOwnerNative criteria);
#endif

/*!
    @brief  Searches for sessions.
    @details

           This function starts an asynchronous process that searches for sessions.
           Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.
           If the search process succeeds, the list of retrieved sessions is updated and can be obtained using @ref GetBrowsedSessionProperty.
           The order of the sessions in the search list depends on the specifications of other network libraries used.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidArgument</tt> indicates that an unexpected argument was passed at the start of the asynchronous process. [:progErr]
    @details  <tt>ResultInvalidState</tt> indicates that the asynchronous process has not completed or has not started. [:progErr]
    @details  <tt>ResultCancelled</tt> indicates that the running asynchronous process was canceled. [:handling-cleanup]
    @details  <tt>ResultGameServerMaintenance</tt> indicates that the game server is under maintenance. [:viewer] [:handling-logout]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @details  <tt>ResultErrorOccurred</tt> indicates an internal error. This result is only returned during local communication. [:handling-cleanup]
    @param[in] criteria  Conditions of the owner when searching sessions.
    @return  Returns a result value that indicates success if the asynchronous process starts successfully.
    @retval ResultInvalidArgument  Unexpected arguments were passed to the function when starting the asynchronous process. [:progErr]
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running, that <tt>Startup</tt> has been called, and that communication is not already taking place. [:progErr]
    @retval ResultNetworkConnectionIsLost  The network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
*/
    public static PiaPlugin.Result BrowseSessionAsync(SessionSearchCriteriaOwner criteria)
    {
        PiaPlugin.Trace("BrowseSessionAsync called");
        using (SessionSearchCriteriaOwnerNative criteriaNative = new SessionSearchCriteriaOwnerNative(criteria))
        {
            PiaPlugin.Result result = BrowseSessionAsyncWithOwnerNative(criteriaNative);
            return result;
        }
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Session_BrowseSessionAsyncWithParticipant([In, MarshalAs(UnmanagedType.LPStruct)] SessionSearchCriteriaParticipantNative criteria);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", CharSet = CharSet.Ansi, EntryPoint = "Session_BrowseSessionAsyncWithParticipant")]
#else
    [DllImport("__Internal", CharSet = CharSet.Ansi, EntryPoint = "Session_BrowseSessionAsyncWithParticipant")]
#endif
    private static extern PiaPlugin.Result BrowseSessionAsyncWithParticipantNative([In, MarshalAs(UnmanagedType.LPStruct)] SessionSearchCriteriaParticipantNative criteria);
#endif

/*!
    @brief  Searches for sessions.
    @details

           This function starts an asynchronous process that searches for sessions.
           Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.
           If the search process succeeds, the list of retrieved sessions is updated and can be obtained using @ref GetBrowsedSessionProperty.
           The order of the sessions in the search list depends on the specifications of other network libraries used.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidArgument</tt> indicates that an unexpected argument was passed at the start of the asynchronous process. [:progErr]
    @details  <tt>ResultInvalidState</tt> indicates that the asynchronous process has not completed or has not started. [:progErr]
    @details  <tt>ResultCancelled</tt> indicates that the running asynchronous process was canceled. [:handling-cleanup]
    @details  <tt>ResultGameServerMaintenance</tt> indicates that the game server is under maintenance. [:viewer] [:handling-logout]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @details  <tt>ResultErrorOccurred</tt> indicates an internal error. This result is only returned during local communication. [:handling-cleanup]
    @param[in] criteria  Participant conditions when searching sessions.
    @return  Returns a result value that indicates success if the asynchronous process starts successfully.
    @retval ResultInvalidArgument  Unexpected arguments were passed to the function when starting the asynchronous process. [:progErr]
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running, that <tt>Startup</tt> has been called, and that communication is not already taking place. [:progErr]
    @retval ResultNetworkConnectionIsLost  The network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
*/
    public static PiaPlugin.Result BrowseSessionAsync(SessionSearchCriteriaParticipant criteria)
    {
        PiaPlugin.Trace("BrowseSessionAsync SessionSearchCriteriaParticipant called");
        using (SessionSearchCriteriaParticipantNative criteriaNative = new SessionSearchCriteriaParticipantNative(criteria))
        {
            PiaPlugin.Result result = BrowseSessionAsyncWithParticipantNative(criteriaNative);
            return result;
        }
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PiaPlugin.Result Session_BrowseSessionAsyncWithSessionId([In, MarshalAs(UnmanagedType.LPStruct)] SessionSearchCriteriaSessionIdNative criteria);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", CharSet = CharSet.Ansi, EntryPoint = "Session_BrowseSessionAsyncWithSessionId")]
#else
    [DllImport("__Internal", CharSet = CharSet.Ansi, EntryPoint = "Session_BrowseSessionAsyncWithSessionId")]
#endif
    private static extern PiaPlugin.Result BrowseSessionAsyncWithSessionIdNative([In, MarshalAs(UnmanagedType.LPStruct)] SessionSearchCriteriaSessionIdNative criteria);
#endif

/*!
    @brief  Searches for sessions.
    @details

           This function starts an asynchronous process that searches for sessions.
           Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.
           If the search process succeeds, the list of retrieved sessions is updated and can be obtained using @ref GetBrowsedSessionProperty.
           The order of the sessions in the search list depends on the specifications of other network libraries used.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidArgument</tt> indicates that an unexpected argument was passed at the start of the asynchronous process. [:progErr]
    @details  <tt>ResultInvalidState</tt> indicates that the asynchronous process has not completed or has not started. [:progErr]
    @details  <tt>ResultCancelled</tt> indicates that the running asynchronous process was canceled. [:handling-cleanup]
    @details  <tt>ResultGameServerMaintenance</tt> indicates that the game server is under maintenance. [:viewer] [:handling-logout]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @details  <tt>ResultErrorOccurred</tt> indicates an internal error. This result is only returned during local communication. [:handling-cleanup]
    @param[in] criteria  Participant conditions when searching sessions.
    @return  Returns an instance of <tt>Result</tt> that indicates success if the asynchronous process starts successfully.
    @retval ResultInvalidArgument  Unexpected arguments were passed to the function when starting the asynchronous process. [:progErr]
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running, that <tt>Startup</tt> has been called, and that communication is not already taking place. [:progErr]
    @retval ResultNetworkConnectionIsLost  The network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
*/
    public static PiaPlugin.Result BrowseSessionAsync(SessionSearchCriteriaSessionId criteria)
    {
        PiaPlugin.Trace("BrowseSessionAsync SessionSearchCriteriaSessionId called");
        using (SessionSearchCriteriaSessionIdNative criteriaNative = new SessionSearchCriteriaSessionIdNative(criteria))
        {
            PiaPlugin.Result result = BrowseSessionAsyncWithSessionIdNative(criteriaNative);
            return result;
        }
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Session_CreateSessionAsync([In, MarshalAs(UnmanagedType.LPStruct)] CreateSessionSettingNative setting);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", CharSet = CharSet.Ansi, EntryPoint = "Session_CreateSessionAsync")]
#else
    [DllImport("__Internal", CharSet = CharSet.Ansi, EntryPoint = "Session_CreateSessionAsync")]
#endif
    private static extern PiaPlugin.Result CreateSessionAsyncNative([In, MarshalAs(UnmanagedType.LPStruct)] CreateSessionSettingNative setting);
#endif

/*!
    @brief  Creates a session.
    @details

           This function starts an asynchronous process that creates a session.
           The local station becomes the host of the created session.
           Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidArgument</tt> indicates that an unexpected argument was passed at the start of the asynchronous process. [:progErr]
    @details  <tt>ResultInvalidState</tt> indicates that the internal state is not a state where the asynchronous process can proceed. This result is also returned when the asynchronous process has not completed or has not started. [:progErr]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  <tt>ResultGameServerMaintenance</tt> indicates that the game server is under maintenance. [:viewer] [:handling-logout]
    @details  <tt>ResultDnsFailed</tt> indicates that DNS resolution failed. [:viewer] [:handling-cleanup]
    @details  <tt>ResultNatCheckFailed</tt> indicates that the NAT check failed. [:viewer] [:handling-cleanup]
    @details  <tt>ResultCancelled</tt> indicates that the asynchronous process was canceled. [:handling-cleanup]
    @details  <tt>ResultTimeOut</tt> indicates that the process timed out. This result is only returned during local communication. [:handling-cleanup]
    @details  <tt>ResultErrorOccurred</tt> indicates an internal error. This result is only returned during local communication. [:handling-cleanup]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @details  <tt>ResultGameServerProcessAborted</tt> indicates that a game server process failed. [:handling-logout]
    @details  <tt>ResultSessionWrongState</tt> indicates that the joined session is in the wrong state. [:handling-cleanup]
    @param[in] setting  Information needed when creating a session.
    @return  Returns a result value that indicates success if the asynchronous process starts successfully. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidArgument  Unexpected arguments were passed to the function when starting the asynchronous process. [:progErr]
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running, that <tt>Startup</tt> has been called, and that communication is not already taking place. [:progErr]
    @retval ResultNetworkConnectionIsLost  The network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
*/
    public static PiaPlugin.Result CreateSessionAsync(CreateSessionSetting setting)
    {
        PiaPlugin.Trace("CreateSessionAsync called");
        using (CreateSessionSettingNative createSessionSettingNative = new CreateSessionSettingNative(setting))
        {
            PiaPlugin.Result result = CreateSessionAsyncNative(createSessionSettingNative);
            return result;
        }
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PiaPlugin.Result Session_JoinSessionAsync([In, MarshalAs(UnmanagedType.LPStruct)] JoinSessionSettingNative setting);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", CharSet = CharSet.Ansi, EntryPoint = "Session_JoinSessionAsync")]
#else
    [DllImport("__Internal", CharSet = CharSet.Ansi, EntryPoint = "Session_JoinSessionAsync")]
#endif
    private static extern PiaPlugin.Result JoinSessionAsyncNative([In, MarshalAs(UnmanagedType.LPStruct)] JoinSessionSettingNative setting);
#endif

/*!
    @brief  Joins a session.
    @details

           This function starts an asynchronous process that joins a session.
           Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidArgument</tt> indicates that an unexpected argument was passed at the start of the asynchronous process. [:progErr]
    @details  <tt>ResultInvalidState</tt> indicates that the asynchronous process has not completed or has not started. [:progErr]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  <tt>ResultSessionIsNotFound</tt> indicates that the target session disappeared or was closed during the asynchronous process. Returned if the password does not match during local communication.
    @details  <tt>ResultMatchmakeSessionIsFull</tt> indicates that the matchmaking session is full. [:handling-cleanup]
    @details  <tt>ResultDeniedByParticipant</tt> indicates that the local station is blacklisted by a session participant. [:handling-cleanup]
    @details  <tt>ResultParticipantInBlockList</tt> indicates that a participant on the local station's blocked-user list is in the session. [:handling-cleanup]
    @details  <tt>ResultGameServerMaintenance</tt> indicates that the game server is under maintenance. [:viewer] [:handling-logout]
    @details  <tt>ResultNatCheckFailed</tt> indicates that the NAT check failed. [:viewer] [:handling-cleanup]
    @details  <tt>ResultDnsFailed</tt> indicates that DNS resolution failed. [:viewer] [:handling-cleanup]
    @details  <tt>ResultCancelled</tt> indicates that the asynchronous process was canceled. [:handling-cleanup]
    @details  <tt>ResultInvalidState</tt> indicates that the station is not in a connection-ready state. This result is also returned when the asynchronous process has not completed or has not started. [:progErr]
    @details  <tt>ResultStationConnectionFailed</tt> indicates that the connection process failed. [:viewer] [:viewer-local] [:handling-cleanup]
    @details  <tt>ResultStationConnectionNatTraversalFailedUnknown</tt> indicates that the NAT traversal connection process failed. The NAT type is unknown. [:viewer] [:viewer-local] [:handling-cleanup]
    @details  <tt>ResultNatTraversalFailedBothEim</tt> indicates that the NAT traversal connection process failed. The NAT type was EIM for both the local station and the peer to which the connection failed. [:viewer] [:viewer-local] [:handling-cleanup]
    @details  <tt>ResultNatTraversalFailedLocalEimRemoteEdm</tt> indicates that the NAT traversal connection process failed. The NAT type was EIM for the local station and EDM for the peer to which the connection failed. [:viewer] [:viewer-local] [:handling-cleanup]
    @details  <tt>ResultNatTraversalFailedLocalEdmRemoteEim</tt> indicates that the NAT traversal connection process failed. The NAT type was EDM for the local station and EIM for the peer to which the connection failed. [:viewer] [:viewer-local] [:handling-cleanup]
    @details  <tt>ResultNatTraversalFailedBothEdm</tt> indicates that the NAT traversal connection process failed. The NAT type was EDM for both the local station and the peer to which the connection failed. [:viewer] [:viewer-local] [:handling-cleanup]
    @details  <tt>ResultNatTraversalFailedBothEimSamePublicAddress</tt> indicates that the NAT traversal connection process failed. The NAT type was EIM for both the local station and the peer to which the connection failed. In addition, the global IP address was the same for the local station and the peer to which the connection failed. [:viewer] [:viewer-local] [:handling-cleanup]
    @details  <tt>ResultNatTraversalFailedLocalEimRemoteEdmSamePublicAddress</tt> indicates that the NAT traversal connection process failed. The NAT type was EIM for the local station and EDM for the peer to which the connection failed. In addition, the global IP address was the same for the local station and the peer to which the connection failed. [:viewer] [:viewer-local] [:handling-cleanup]
    @details  <tt>ResultNatTraversalFailedLocalEdmRemoteEimSamePublicAddress</tt> indicates that the NAT traversal connection process failed. The NAT type was EDM for the local station and EIM for the peer to which the connection failed. In addition, the global IP address was the same for the local station and the peer to which the connection failed. [:viewer] [:viewer-local] [:handling-cleanup]
    @details  <tt>ResultNatTraversalFailedBothEdmSamePublicAddress</tt> indicates that the NAT traversal connection process failed. The NAT type was EDM for both the local station and the peer to which the connection failed. In addition, the global IP address was the same for the local station and the peer to which the connection failed. [:viewer] [:viewer-local] [:handling-cleanup]
    @details  <tt>ResultNatTraversalRequestTimeout</tt> indicates that the NAT traversal connection process failed. The NAT traversal request timed out. [:viewer] [:viewer-local] [:handling-cleanup]
    @details  <tt>ResultJoinRequestDenied</tt> indicates that the session host denied the connection request. [:handling-cleanup] [:viewer-handling]
    @details  <tt>ResultMeshIsFull</tt> indicates that the desired mesh is full and cannot be joined. [:handling-cleanup] [:viewer-handling]
    @details  <tt>ResultRelayFailedNoCandidate</tt> indicates that the relay connection failed because there was no relay candidate. [:handling-cleanup] [:viewer-handling]
    @details  <tt>ResultRelayFailedRttLimit</tt> indicates that the relay connection failed because the relay round-trip time limit was exceeded. [:handling-cleanup] [:viewer-handling]
    @details  <tt>ResultRelayFailedRelayNumLimit</tt> indicates that the relay connection failed because the number of relay requests exceeded the limit. [:handling-cleanup] [:viewer-handling]
    @details  <tt>ResultRelayFailedUnknown</tt> indicates that the relay connection failed. (Details unknown.) [:handling-cleanup] [:viewer-handling]
    @details  <tt>ResultInvalidSystemMessage</tt> indicates that the process was canceled because an invalid response was received from the session host. [:progErr]
    @details  <tt>ResultSessionUserPasswordUnmatch</tt> indicates that the user password specified when joining does not match the user password set on the session. [:handling-cleanup]
    @details  <tt>ResultSessionSystemPasswordUnmatch</tt> indicates that the system password specified when joining does not match the system password set for the session. [:handling-cleanup]
    @details  <tt>ResultSessionIsClosed</tt> indicates that the session that the station tried to join is closed. [:handling-cleanup] [:viewer-handling]
    @details  <tt>ResultHostIsNotFriend</tt> indicates that the host of the session that the station tried to join is not a friend. [:handling-cleanup]
    @details  <tt>ResultCompanionStationIsOffline</tt> indicates that a companion client is not logged in to the game server. [:handling-cleanup]
    @details  <tt>ResultIncompatibleFormat</tt> indicates that the peer has an incompatible communication format. Returned when you try to communicate with a ROM that uses a different version of Pia that does not have communication compatibility. [:progErr]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @details  <tt>ResultGameServerProcessAborted</tt> indicates that a game server process failed. [:handling-logout]
    @details  <tt>ResultSessionWrongState</tt> indicates that the joined session is in the wrong state. [:handling-cleanup]
    @details  <tt>ResultNetworkIsFull</tt> indicates that the network is full. This result is only returned during local communication. [:handling-cleanup]
    @details  <tt>ResultTimeOut</tt> indicates that the process timed out. [:handling-cleanup]
    @details  <tt>ResultDifferentVersion</tt> indicates that there was an attempt to join a session with a different local network version or application version. This result is only returned during local communication. [:handling-cleanup]
    @details  <tt>ResultErrorOccurred</tt> indicates an internal error. This result is only returned during local communication. [:handling-cleanup]
    @param[in] setting  Information needed when joining a session.
    @return  Returns a result value that indicates success if the asynchronous process starts successfully. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidArgument  Unexpected arguments were passed to the function when starting the asynchronous process. [:progErr]
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running, that <tt>Startup</tt> has been called, and that communication is not already taking place. [:progErr]
    @retval ResultNetworkConnectionIsLost  The network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
*/
    public static PiaPlugin.Result JoinSessionAsync(JoinSessionSetting setting)
    {
        PiaPlugin.Trace("JoinSessionAsync called");
        using (JoinSessionSettingNative joinSessionSettingNative = new JoinSessionSettingNative(setting))
        {
            PiaPlugin.Result result = JoinSessionAsyncNative(joinSessionSettingNative);
            return result;
        }
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PiaPlugin.Result Session_UpdateSessionSettingAsync([In, MarshalAs(UnmanagedType.LPStruct)] UpdateSessionSettingNative setting, [In, MarshalAs(UnmanagedType.LPStruct)] PiaPlugin.DateTime startedTime);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", CharSet = CharSet.Ansi, EntryPoint = "Session_UpdateSessionSettingAsync")]
#else
    [DllImport("__Internal", CharSet = CharSet.Ansi, EntryPoint = "Session_UpdateSessionSettingAsync")]
#endif
    private static extern PiaPlugin.Result UpdateSessionSettingAsyncNative([In, MarshalAs(UnmanagedType.LPStruct)] UpdateSessionSettingNative setting, [In, MarshalAs(UnmanagedType.LPStruct)] PiaPlugin.DateTime startedTime);
#endif

/*!
    @brief  Updates the joined session settings.
    @details  Asynchronously updates the joined session settings.
              Only the session host can execute this function.
              Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidState</tt> indicates that the function call occurred in an invalid state. Make sure that no other asynchronous processes are running. [:progErr]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  <tt>ResultSessionIsNotFound</tt> indicates that the target session disappeared during the asynchronous process. [:handling-cleanup]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @details  <tt>ResultGameServerProcessAborted</tt> indicates that a game server process failed. [:handling-logout]
    @param[in] setting  The settings data to specify when changing.
    @return  Returns a result value that indicates success if the asynchronous process starts successfully. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running and that the station is the host. [:progErr]
    @retval ResultInvalidArgument  <tt>UpdateSessionSetting</tt> was set for some attributes and another parameter. To update an attribute and another parameter at the same time, set a value for the index of every attribute. [:progErr]
    @retval ResultNetworkConnectionIsLost  The network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost during Internet matchmaking or LAN matchmaking. [:viewer] [:handling-shutdownNetwork]
    @retval ResultWifiOff  Wireless is off. This result is only returned during local communication. [:viewer] [:handling-shutdownNetwork]
    @retval ResultSleep  The system is in sleep mode. This result is only returned during local communication. [:viewer] [:handling-shutdownNetwork]
*/
    public static PiaPlugin.Result UpdateSessionSettingAsync(UpdateSessionSetting setting)
    {
        using (UpdateSessionSettingNative updateSessionSettingNative = new UpdateSessionSettingNative(setting))
        {
            PiaPlugin.Result result = UpdateSessionSettingAsyncNative(updateSessionSettingNative, setting.GetStartedTime());
            return result;
        }
    }


#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate SessionPropertyNative Session_GetSessionProperty();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_GetSessionProperty")]
#else
    [DllImport("__Internal", EntryPoint = "Session_GetSessionProperty")]
#endif
    private static extern SessionPropertyNative GetSessionPropertyNative();
#endif

/*!
    @brief  Gets information for the session that you have joined.
    @details  Session information is not updated automatically. You must call @ref RequestSessionPropertyAsync to update the session information.
    @return  Returns the information for the session that you have joined.
*/
    public static SessionProperty GetSessionProperty()
    {
        SessionPropertyNative sessionPropertyNative = GetSessionPropertyNative();
        SessionProperty sessionProperty = new SessionProperty(sessionPropertyNative);
        return sessionProperty;
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Session_RequestSessionPropertyAsync();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_RequestSessionPropertyAsync")]
#else
    [DllImport("__Internal", EntryPoint = "Session_RequestSessionPropertyAsync")]
#endif
    private static extern PiaPlugin.Result RequestSessionPropertyAsyncNative();
#endif

/*!
    @brief  Requests information for the session that you have joined.
    @details  Starts asynchronous processing to request the information for the session that you have joined.
    Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.

    Game server access occurs during Internet communication. Do not call this function more than 10 times in one minute.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidState</tt> indicates that the asynchronous process has not completed or has not started. [:progErr]
    @details  <tt>ResultSessionIsNotFound</tt> indicates that the target session disappeared during the asynchronous process. [:handling-cleanup]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @return  Returns a result value that indicates success if the asynchronous process starts successfully. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running and that the session is not joined. [:progErr]
*/
    public static PiaPlugin.Result RequestSessionPropertyAsync()
    {
        return RequestSessionPropertyAsyncNative();
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt32 Session_GetBrowsedSessionPropertyListSize();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_GetBrowsedSessionPropertyListSize")]
#else
    [DllImport("__Internal", EntryPoint = "Session_GetBrowsedSessionPropertyListSize")]
#endif
    private static extern UInt32 GetBrowsedSessionPropertyListSizeNative();
#endif

/*!
    @brief  Gets the list size for sessions found with the <tt>BrowseSessionAsync</tt> asynchronous process.
    @details  You can use this function to get the session list size after the asynchronous process started by @ref BrowseSessionAsync ends and the result from @ref PiaPlugin.GetAsyncProcessState indicates success.
    @return  Returns the list size of the session.
*/
    public static UInt32 GetBrowsedSessionPropertyListSize()
    {
        return GetBrowsedSessionPropertyListSizeNative();
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate SessionPropertyNative Session_GetBrowsedSessionProperty([In, MarshalAs(UnmanagedType.LPStruct)] UInt32 listIndex);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_GetBrowsedSessionProperty")]
#else
    [DllImport("__Internal", EntryPoint = "Session_GetBrowsedSessionProperty")]
#endif
    private static extern SessionPropertyNative GetBrowsedSessionPropertyNative([In, MarshalAs(UnmanagedType.LPStruct)] UInt32 listIndex);
#endif

/*!
    @brief  Gets the session information of the specified index value from the list size for sessions found with the <tt>BrowseSessionAsync</tt> asynchronous process.
    @details  You can use this function to get the session information in the list after the asynchronous process started by @ref BrowseSessionAsync ends and the result from @ref PiaPlugin.GetAsyncProcessState indicates success.
    @details  The list is cleared when the call to @ref CreateSessionAsync, @ref BrowseSessionAsync, @ref JoinSessionAsync, or @ref JoinRandomSessionAsync has succeeded.

    @param[in] listIndex  The obtained list number.
    @return  Returns the information for the specified session.
*/
    public static SessionProperty GetBrowsedSessionProperty(UInt32 listIndex)
    {
        SessionPropertyNative sessionPropertyNative = GetBrowsedSessionPropertyNative(listIndex);
        SessionProperty sessionProperty = new SessionProperty(sessionPropertyNative);
        return sessionProperty;
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Session_GetConstantIdList([Out] out IntPtr pConstantIdNativeList, [Out] out int constantIdNativeListLength);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_GetConstantIdList")]
#else
    [DllImport("__Internal", EntryPoint = "Session_GetConstantIdList")]
#endif
    private static extern PiaPlugin.Result GetConstantIdListNative([Out] out IntPtr pConstantIdNativeList, [Out] out int constantIdNativeListLength);
#endif

/*!
    @brief  Gets a list of all current station IDs.
    @param[in] constantIdList  List storing the Station IDs.
    @return  If the process is successful, the function returns an instance of <tt>Result</tt> that indicates success. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidArgument  An invalid argument was specified. [:progErr]
    @retval ResultTemporaryUnavailable  The API is temporarily unavailable because the session is migrating. (Only returned when the joint session feature is being used.) [:handling]
*/
    public static PiaPlugin.Result GetConstantIdList(List<UInt64> constantIdList)
    {
        IntPtr pConstantIdNativeList = IntPtr.Zero;
        int constantIdNativeListLength = 0;
        PiaPlugin.Result result;
        result = GetConstantIdListNative(out pConstantIdNativeList, out constantIdNativeListLength);

        UInt64[] constantIdNativeList = new UInt64[constantIdNativeListLength];
        UnmanagedMemoryManager.ReadArray<UInt64>(pConstantIdNativeList, constantIdNativeListLength, ref constantIdNativeList);

        for (int i = 0; i < constantIdNativeListLength; ++i)
        {
            constantIdList.Add(constantIdNativeList[i]);
        }

        return result;
    }


#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Session_DestroyJointSessionAsync();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_DestroyJointSessionAsync")]
#else
    [DllImport("__Internal", EntryPoint = "Session_DestroyJointSessionAsync")]
#endif
    private static extern PiaPlugin.Result DestroyJointSessionAsyncNative();
#endif

/*!
        @brief  Disbands a joint session.
        @details

               Starts the asynchronous process to disband a joint session.
               It can be used only by the joint session host.
               Check for the completion of the asynchronous process and the result using @ref PiaPlugin.GetAsyncProcessState.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidState</tt> indicates that processing is not possible in this state. This result is also returned when the asynchronous process has not completed or has not started. [:progErr]
    @details  <tt>ResultInvalidArgument</tt> indicates that an unexpected argument was passed at the start of the asynchronous process. [:progErr]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  <tt>ResultGameServerMaintenance</tt> indicates that the game server is under maintenance. [:viewer] [:handling-logout]
    @details  <tt>ResultCompanionStationIsOffline</tt> indicates that a companion client is not logged in to the game server. [:handling-cleanup]
    @details  <tt>ResultCompanionStationIsLeft</tt> indicates that the connection with a companion client has been dropped. [:handling-cleanup]
    @details  <tt>ResultSessionMigrationFailed</tt> indicates that session migration failed. [:handling-cleanup]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @details  <tt>ResultGameServerProcessAborted</tt> indicates that a game server process failed. [:handling-logout]
    @if NIN_DOC
    @details  <tt>ResultUserAccountNotExisted</tt> indicates that the user account did not exist. [:handling-cleanup]
    @details  <tt>ResultNetworkConnectionIsLostByDuplicateLogin</tt> indicates that the station was disconnected from the game server and the asynchronous process was interrupted because another station is logged in using the same account. [:viewer] [:handling-cleanup]
    @endif
        @return  Returns a result value that indicates success if the asynchronous process starts successfully. If the call fails, the function returns one of the following <tt>Result</tt> instances.
        @retval ResultInvalidArgument  Unexpected arguments were passed to the function when starting the asynchronous process. [:progErr]
        @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running, that <tt>Startup</tt> has been called, and that communication is not already taking place. [:progErr]
*/
    public static PiaPlugin.Result DestroyJointSessionAsync()
    {
        return DestroyJointSessionAsyncNative();
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Session_OpenJointSessionAsync();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_OpenJointSessionAsync")]
#else
    [DllImport("__Internal", EntryPoint = "Session_OpenJointSessionAsync")]
#endif
    private static extern PiaPlugin.Result OpenJointSessionAsyncNative();
#endif
/*!
    @brief  Starts accepting participants in the joint session.
    @details

           This function starts an asynchronous process that accepts joint session participants.
           Only the joint session host can execute this function.
           Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidState</tt> indicates that the function call occurred in an invalid state. Make sure that no other asynchronous processes are running. [:progErr]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  <tt>ResultSessionConnectionIsLost</tt> indicates that the station was disconnected from the mesh. [:handling-leave]
    @details  <tt>ResultSessionIsNotFound</tt> indicates that the target session disappeared during the asynchronous process. [:handling-cleanup]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @details  <tt>ResultGameServerProcessAborted</tt> indicates that a game server process failed. [:handling-logout]
    @return  Returns a result value that indicates success if the asynchronous process starts successfully. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running, <tt>Startup</tt> has been called, the session is in participating state, and the local station is the session host. [:progErr]
    @retval ResultNetworkConnectionIsLost  The network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
*/
    public static PiaPlugin.Result OpenJointSessionAsync()
    {
        PiaPlugin.Result result = OpenJointSessionAsyncNative();
        return result;
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate PiaPlugin.Result Session_UpdateAndOpenJointSessionAsync([In, MarshalAs(UnmanagedType.LPStruct)] UpdateSessionSettingNative setting, [In, MarshalAs(UnmanagedType.LPStruct)] PiaPlugin.DateTime startedTime);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_UpdateAndOpenJointSessionAsync")]
#else
    [DllImport("__Internal", EntryPoint = "Session_UpdateAndOpenJointSessionAsync")]
#endif
    private static extern PiaPlugin.Result UpdateAndOpenJointSessionAsyncNative([In, MarshalAs(UnmanagedType.LPStruct)] UpdateSessionSettingNative setting, [In, MarshalAs(UnmanagedType.LPStruct)] PiaPlugin.DateTime startedTime);
#endif
/*!
    @brief  Starts the asynchronous process to accept participants in the joint session and update the session settings simultaneously.
    @details

           This function starts an asynchronous process that accepts joint session participants.
           Only the joint session host can execute this function.
           Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidState</tt> indicates that the function call occurred in an invalid state. Make sure that no other asynchronous processes are running. [:progErr]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  <tt>ResultSessionConnectionIsLost</tt> indicates that the station was disconnected from the mesh. [:handling-leave]
    @details  <tt>ResultSessionIsNotFound</tt> indicates that the target session disappeared during the asynchronous process. [:handling-cleanup]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @details  <tt>ResultGameServerProcessAborted</tt> indicates that a game server process failed. [:handling-logout]
    @param[in] setting  The settings data to specify when changing.
    @return  Returns a result value that indicates success if the asynchronous process starts successfully. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running, <tt>Startup</tt> has been called, the session is in participating state, and the local station is the session host. [:progErr]
    @retval ResultNetworkConnectionIsLost  The network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
*/
    public static PiaPlugin.Result UpdateAndOpenJointSessionAsync(UpdateSessionSetting setting)
    {
        using (UpdateSessionSettingNative updateSessionSettingNative = new UpdateSessionSettingNative(setting))
        {
            PiaPlugin.Result result = UpdateAndOpenJointSessionAsyncNative(updateSessionSettingNative, setting.GetStartedTime());
            return result;
        }
    }

#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Session_CloseJointSessionAsync();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_CloseJointSessionAsync")]
#else
    [DllImport("__Internal", EntryPoint = "Session_CloseJointSessionAsync")]
#endif
    private static extern PiaPlugin.Result CloseJointSessionAsyncNative();
#endif

/*!
    @brief  Closes a joint session to new participants.
    @details

           This function starts an asynchronous process that closes a joint session to new participants.
           Only the joint session host can execute this function.

           Hypothetically, a station could attempt to join the joint session immediately before this function is called and end up joining after the asynchronous process has completed.
           For this reason, the asynchronous process ends after waiting for the joint session to close and for the number of participants in the matchmaking session to match the number of participants in the mesh.
           The asynchronous process also ends if the numbers do not match after a certain amount of time (15 seconds). In this case, <tt>PiaPlugin.GetAsyncProcessState</tt>
           returns <tt>ResultNegligibleFault</tt>. There could be attempts to join after the asynchronous process has ended.
           Handle these attempts as appropriate in the application.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidState</tt> indicates that the function call occurred in an invalid state. Make sure that no other asynchronous processes are running. [:progErr]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  <tt>ResultSessionConnectionIsLost</tt> indicates that the station was disconnected from the mesh. [:handling-leave]
    @details  <tt>ResultSessionIsNotFound</tt> indicates that the target session disappeared during the asynchronous process. [:handling-cleanup]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @details  <tt>ResultGameServerProcessAborted</tt> indicates that a game server process failed. [:handling-logout]
    @details  <tt>ResultNegligibleFault</tt> indicates that a certain amount of time passed without the number of participants in the matchmaking sessions matching the number in the mesh. [:handling]
    @return  Returns a result value that indicates success if the asynchronous process starts successfully. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running, <tt>Startup</tt> has been called, the session is in participating state, and the local station is the session host. [:progErr]
    @retval ResultNetworkConnectionIsLost  The network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
*/
    public static PiaPlugin.Result CloseJointSessionAsync()
    {
        PiaPlugin.Result result = CloseJointSessionAsyncNative();
        return result;
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate PiaPlugin.Result Session_UpdateJointSessionSettingAsync([In, MarshalAs(UnmanagedType.LPStruct)] UpdateSessionSettingNative setting, [In, MarshalAs(UnmanagedType.LPStruct)] PiaPlugin.DateTime startedTime);
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", CharSet = CharSet.Ansi, EntryPoint = "Session_UpdateJointSessionSettingAsync")]
#else
    [DllImport("__Internal", CharSet = CharSet.Ansi, EntryPoint = "Session_UpdateJointSessionSettingAsync")]
#endif
    private static extern PiaPlugin.Result UpdateJointSessionSettingAsyncNative([In, MarshalAs(UnmanagedType.LPStruct)] UpdateSessionSettingNative setting, [In, MarshalAs(UnmanagedType.LPStruct)] PiaPlugin.DateTime startedTime);
#endif

/*!
    @brief  Updates the settings for the joined joint session.
    @details  Asynchronously updates the settings for the joined joint session.
              Only the joint session host can execute this function.
              Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidState</tt> indicates that the function call occurred in an invalid state. Make sure that no other asynchronous processes are running. [:progErr]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  <tt>ResultSessionIsNotFound</tt> indicates that the target session disappeared during the asynchronous process. [:handling-cleanup]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @details  <tt>ResultGameServerProcessAborted</tt> indicates that a game server process failed. [:handling-logout]
    @param[in] setting  The settings data to specify when changing.
    @return  Returns a result value that indicates success if the asynchronous process starts successfully. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running and that the station is the host. [:progErr]
    @retval ResultInvalidArgument  <tt>UpdateSessionSetting</tt> was set for some attributes and another parameter. To update an attribute and another parameter at the same time, set a value for the index of every attribute. [:progErr]
    @retval ResultNetworkConnectionIsLost  The network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost during Internet matchmaking or LAN matchmaking. [:viewer] [:handling-shutdownNetwork]
    @retval ResultWifiOff  Wireless is off. This result is only returned during local communication. [:viewer] [:handling-shutdownNetwork]
    @retval ResultSleep  The system is in sleep mode. This result is only returned during local communication. [:viewer] [:handling-shutdownNetwork]
*/
    public static PiaPlugin.Result UpdateJointSessionSettingAsync(UpdateSessionSetting setting)
    {
        using (UpdateSessionSettingNative updateJointSessionSettingNative = new UpdateSessionSettingNative(setting))
        {
            PiaPlugin.Result result = UpdateJointSessionSettingAsyncNative(updateJointSessionSettingNative, setting.GetStartedTime());
            return result;
        }
    }


#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate SessionPropertyNative Session_GetJointSessionProperty();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_GetJointSessionProperty")]
#else
    [DllImport("__Internal", EntryPoint = "Session_GetJointSessionProperty")]
#endif
    private static extern SessionPropertyNative GetJointSessionPropertyNative();
#endif

/*!
    @brief  Gets the information for the joined joint session.
    @details  Joint session information is not automatically updated. You must use @ref RequestJointSessionPropertyAsync to update joint session information.
    @return  Returns the information for the session that you have joined.
*/
    public static SessionProperty GetJointSessionProperty()
    {
        SessionPropertyNative sessionPropertyNative = GetJointSessionPropertyNative();
        SessionProperty sessionProperty = new SessionProperty(sessionPropertyNative);
        return sessionProperty;
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate PiaPlugin.Result Session_RequestJointSessionPropertyAsync();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_RequestJointSessionPropertyAsync")]
#else
    [DllImport("__Internal", EntryPoint = "Session_RequestJointSessionPropertyAsync")]
#endif
    private static extern PiaPlugin.Result RequestJointSessionPropertyAsyncNative();
#endif

/*!
    @brief  Requests the information for the joined joint session.
    @details  Starts asynchronous processing to request the information for the joint session that you have joined.
    Check for the completion of the process and the result using @ref PiaPlugin.GetAsyncProcessState.

    Game server access occurs during Internet communication. Do not call this function more than 10 times in one minute.
    @details
    @details  If @ref PiaPlugin.GetAsyncProcessState fails, the following <tt>Result</tt> instances are possible.
    @details  <tt>ResultInvalidState</tt> indicates that the asynchronous process has not completed or has not started. [:progErr]
    @details  <tt>ResultSessionIsNotFound</tt> indicates that the target session disappeared during the asynchronous process. [:handling-cleanup]
    @details  <tt>ResultNetworkConnectionIsLost</tt> indicates that the network is not available. The wireless switch may be turned off, there may be a problem with the access point, or the connection to the game server may have been lost. [:viewer] [:handling-shutdownNetwork]
    @details  ResultNexInternalError [:nexInternalError][:viewer][:handling]
    @return  Returns a result value that indicates success if the asynchronous process starts successfully. If the call fails, the function returns one of the following <tt>Result</tt> instances.
    @retval ResultInvalidState  The instance was not in a valid state when the function was called. Make sure that no other asynchronous processes are running and that the joint session has not been joined. [:progErr]
*/
    public static PiaPlugin.Result RequestJointSessionPropertyAsync()
    {
        return RequestJointSessionPropertyAsyncNative();
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool Session_IsHost();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_IsHost")]
#else
    [DllImport("__Internal", EntryPoint = "Session_IsHost")]
#endif
    private static extern bool IsHostNative();
#endif

/*!
    @brief  Determines whether the local station is the session host.
    @details  This function returns whether the local station is the host of a standard session.

    @return  Returns <tt>true</tt> if the local station is the host, or <tt>false</tt> otherwise.
*/
    public static bool IsHost()
    {
        return IsHostNative();
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool Session_IsJointSessionHost();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_IsJointSessionHost")]
#else
    [DllImport("__Internal", EntryPoint = "Session_IsJointSessionHost")]
#endif
    private static extern bool IsJointSessionHostNative();
#endif
/*!
    @brief  Determines whether the local station is the joint session host.
    @details  This function returns whether the local station is the host of the joint session.

    @return  Returns <tt>true</tt> if the local station is the host, or <tt>false</tt> otherwise.
*/
    public static bool IsJointSessionHost()
    {
        return IsJointSessionHostNative();
    }



#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt32 Session_GetSessionOpenStatus();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_GetSessionOpenStatus")]
#else
    [DllImport("__Internal", EntryPoint = "Session_GetSessionOpenStatus")]
#endif
    private static extern UInt32 GetSessionOpenStatusNative();
#endif

/*!
    @brief  Gets the session recruitment state.
    @details  This function is only available to the session host.

    @return  Session open recruitment state.
*/
    public static SessionOpenStatus GetSessionOpenStatus()
    {
        switch(GetSessionOpenStatusNative())
        {
            case 0:
                return SessionOpenStatus.Unknown;
            case 1:
                return SessionOpenStatus.Open;
            case 2:
                return SessionOpenStatus.Close;
            default:
                PiaPlugin.Trace("Undefined value.");
                return SessionOpenStatus.Unknown;
        }
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt32 Session_GetJointSessionOpenStatus();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_GetJointSessionOpenStatus")]
#else
    [DllImport("__Internal", EntryPoint = "Session_GetJointSessionOpenStatus")]
#endif
    private static extern UInt32 GetJointSessionOpenStatusNative();
#endif

/*!
    @brief  Gets the joint session recruitment state.
    @details  This function is only available to the joint session host.

    @return  Joint session recruitment state.
*/
    public static SessionOpenStatus GetJointSessionOpenStatus()
    {
        switch (GetJointSessionOpenStatusNative())
        {
            case 0:
                return SessionOpenStatus.Unknown;
            case 1:
                return SessionOpenStatus.Open;
            case 2:
                return SessionOpenStatus.Close;
            default:
                PiaPlugin.Trace("Undefined value.");
                return SessionOpenStatus.Unknown;
        }
    }




#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Session_GetInetSearchCriteriaListSizeMax();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_GetInetSearchCriteriaListSizeMax")]
#else
    [DllImport("__Internal", EntryPoint = "Session_GetInetSearchCriteriaListSizeMax")]
#endif
    private static extern int GetInetSearchCriteriaListSizeMaxNative();
#endif

/*!
    @brief  Gets the maximum number of search conditions that can be set during random matchmaking with Internet communication.
    @details  Shows the maximum length of the array that can be set in <tt>PiaPlugin.JoinRandomSessionSetting.<var>sessionSearchCriteriaList</var></tt>.
    @return  The maximum number of search conditions that can be specified during random matchmaking.
*/
    public static int GetInetSearchCriteriaListSizeMax()
    {
        return GetInetSearchCriteriaListSizeMaxNative();
    }

#if UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Session_GetLanSearchCriteriaListSizeMax();
#else
#if UNITY_STANDALONE
    [DllImport("nn_piaPlugin", EntryPoint = "Session_GetLanSearchCriteriaListSizeMax")]
#else
    [DllImport("__Internal", EntryPoint = "Session_GetLanSearchCriteriaListSizeMax")]
#endif
    private static extern int GetLanSearchCriteriaListSizeMaxNative();
#endif

/*!
    @brief  Gets the maximum number of search conditions that can be set during random matchmaking with LAN communication.
    @details  Returns the maximum length of the array that can be set in <tt>PiaPlugin.JoinRandomSessionSetting.<var>sessionSearchCriteriaList</var></tt>.
    @return  The maximum number of search conditions that can be specified during random matchmaking.
*/
    public static int GetLanSearchCriteriaListSizeMax()
    {
        return GetLanSearchCriteriaListSizeMaxNative();
    }
}

