using System;
using System.Linq;
using System.Net;
using UnityEngine;
using Mirror;
using Unity.Collections;
using Unity.Networking.Transport;
using kcp2k;
using Unity.Networking.Transport.Relay;

public interface INetworkStreamDriverConstructor
{
    /// <summary>
    /// Creates the internal NetworkDriver
    /// </summary>
    /// <param name="transport">The owner transport</param>
    /// <param name="driver">The driver</param>
    /// <param name="unreliableFragmentedPipeline">The UnreliableFragmented NetworkPipeline</param>
    /// <param name="unreliableSequencedFragmentedPipeline">The UnreliableSequencedFragmented NetworkPipeline</param>
    /// <param name="reliableSequencedPipeline">The ReliableSequenced NetworkPipeline</param>
    void CreateDriver(
        Transport transport,
        out NetworkDriver driver,
        out NetworkPipeline unreliableFragmentedPipeline,
        out NetworkPipeline unreliableSequencedFragmentedPipeline,
        out NetworkPipeline reliableSequencedPipeline);
}

public partial class RelayTransportProtocol : Transport, INetworkStreamDriverConstructor
{
    // scheme used by this transport
    public const string Scheme = "kcp";

    // common
    [Header("Transport Configuration")]
    public ushort Port = 7777;
    [Tooltip("DualMode listens to IPv6 and IPv4 simultaneously. Disable if the platform only supports IPv4.")]
    public bool DualMode = true;
    [Tooltip("NoDelay is recommended to reduce latency. This also scales better without buffers getting full.")]
    public bool NoDelay = true;
    [Tooltip("KCP internal update interval. 100ms is KCP default, but a lower interval is recommended to minimize latency and to scale to more networked entities.")]
    public uint Interval = 10;
    [Tooltip("KCP timeout in milliseconds. Note that KCP sends a ping automatically.")]
    public int Timeout = 10000;

    [Header("Advanced")]
    [Tooltip("KCP fastresend parameter. Faster resend for the cost of higher bandwidth. 0 in normal mode, 2 in turbo mode.")]
    public int FastResend = 2;
    [Tooltip("KCP congestion window. Enabled in normal mode, disabled in turbo mode. Disable this for high scale games if connections get choked regularly.")]
    public bool CongestionWindow = false; // KCP 'NoCongestionWindow' is false by default. here we negate it for ease of use.
    [Tooltip("KCP window size can be modified to support higher loads.")]
    public uint SendWindowSize = 4096; //Kcp.WND_SND; 32 by default. Mirror sends a lot, so we need a lot more.
    [Tooltip("KCP window size can be modified to support higher loads. This also increases max message size.")]
    public uint ReceiveWindowSize = 4096; //Kcp.WND_RCV; 128 by default. Mirror sends a lot, so we need a lot more.
    [Tooltip("KCP will try to retransmit lost messages up to MaxRetransmit (aka dead_link) before disconnecting.")]
    public uint MaxRetransmit = Kcp.DEADLINK * 2; // default prematurely disconnects a lot of people (#3022). use 2x.
    [Tooltip("Enable to use where-allocation NonAlloc KcpServer/Client/Connection versions. Highly recommended on all Unity platforms.")]
    public bool NonAlloc = true;
    [Tooltip("Enable to automatically set client & server send/recv buffers to OS limit. Avoids issues with too small buffers under heavy load, potentially dropping connections. Increase the OS limit if this is still too small.")]
    public bool MaximizeSendReceiveBuffersToOSLimit = true;

    [Header("Calculated Max (based on Receive Window Size)")]
    [Tooltip("KCP reliable max message size shown for convenience. Can be changed via ReceiveWindowSize.")]
    [ReadOnly] public int ReliableMaxMessageSize = 0; // readonly, displayed from OnValidate
    [Tooltip("KCP unreliable channel max message size for convenience. Not changeable.")]
    [ReadOnly] public int UnreliableMaxMessageSize = 0; // readonly, displayed from OnValidate

    // server & client (where-allocation NonAlloc versions)
    KcpServer server;
    KcpClient client;

    // debugging
    [Header("Debug")]
    public bool debugLog;
    // show statistics in OnGUI
    public bool statisticsGUI;
    // log statistics for headless servers that can't show them in GUI
    public bool statisticsLog;

    private NetworkSettings m_NetworkSettings;
    private NetworkDriver m_Driver;
    private ulong m_ServerClientId;
    public ulong ServerClientId => m_ServerClientId;
    private RelayServerData m_RelayServerData;
    private int m_HeartbeatTimeoutMS;
    public INetworkStreamDriverConstructor DriverConstructor => s_DriverConstructor ?? this;
    public static INetworkStreamDriverConstructor s_DriverConstructor;
    private NetworkPipeline m_UnreliableFragmentedPipeline;
    private NetworkPipeline m_UnreliableSequencedFragmentedPipeline;
    private NetworkPipeline m_ReliableSequencedPipeline;
    private State m_State;

    private enum State
    {
        Disconnected,
        Listening,
        Connected,
    }

    public override bool Available()
    {
        throw new NotImplementedException();
    }

    public override void ClientConnect(string address)
    {
        throw new NotImplementedException();
    }

    public override bool ClientConnected()
    {
        throw new NotImplementedException();
    }

    public override void ClientDisconnect()
    {
        throw new NotImplementedException();
    }

    public override void ClientSend(ArraySegment<byte> segment, int channelId = 0)
    {
        throw new NotImplementedException();
    }

    public override int GetMaxPacketSize(int channelId = 0)
    {
        throw new NotImplementedException();
    }

    public override bool ServerActive()
    {
        throw new NotImplementedException();
    }

    public override void ServerDisconnect(int connectionId)
    {
        throw new NotImplementedException();
    }

    public override string ServerGetClientAddress(int connectionId)
    {
        throw new NotImplementedException();
    }

    public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId = 0)
    {
        throw new NotImplementedException();
    }

    public override void ServerStart()
    {
        if (m_Driver.IsCreated)
        {
            return;
        }

        bool succeeded;
        succeeded = StartRelayServer();
        if (!succeeded)
        {
            Shutdown();
        }
    }
    private bool StartRelayServer()
    {
        //This comparison is currently slow since RelayServerData does not implement a custom comparison operator that doesn't use
        //reflection, but this does not live in the context of a performance-critical loop, it runs once at initial connection time.
        if (m_RelayServerData.Equals(default(RelayServerData)))
        {
            Debug.LogError("You must call SetRelayServerData() at least once before calling StartRelayServer.");
            return false;
        }
        else
        {
            m_NetworkSettings.WithRelayParameters(ref m_RelayServerData, m_HeartbeatTimeoutMS);
            return ServerBindAndListen(NetworkEndPoint.AnyIpv4);
        }
    }
    private bool ServerBindAndListen(NetworkEndPoint endPoint)
    {
        InitDriver();

        int result = m_Driver.Bind(endPoint);
        if (result != 0)
        {
            Debug.LogError("Server failed to bind");
            return false;
        }

        result = m_Driver.Listen();
        if (result != 0)
        {
            Debug.LogError("Server failed to listen");
            return false;
        }

        m_State = State.Listening;
        return true;
    }

    private void InitDriver()
    {
        DriverConstructor.CreateDriver(
            this,
            out m_Driver,
            out m_UnreliableFragmentedPipeline,
            out m_UnreliableSequencedFragmentedPipeline,
            out m_ReliableSequencedPipeline);
    }


    public override void ServerStop()
    {
        throw new NotImplementedException();
    }

    public override Uri ServerUri()
    {
        throw new NotImplementedException();
    }

    public override void Shutdown()
    {
        if (!m_Driver.IsCreated)
        {
            return;
        }

        // The above flush only puts the message in UTP internal buffers, need an update to
        // actually get the messages on the wire. (Normally a flush send would be sufficient,
        // but there might be disconnect messages and those require an update call.)
        m_Driver.ScheduleUpdate().Complete();

        DisposeInternals();

        // We must reset this to zero because UTP actually re-uses clientIds if there is a clean disconnect
        m_ServerClientId = 0;
    }
    /// <summary>Set the relay server data for the host.</summary>
    /// <param name="ipAddress">IP address of the relay server.</param>
    /// <param name="port">UDP port of the relay server.</param>
    /// <param name="allocationId">Allocation ID as a byte array.</param>
    /// <param name="key">Allocation key as a byte array.</param>
    /// <param name="connectionData">Connection data as a byte array.</param>
    /// <param name="isSecure">Whether the connection is secure (uses DTLS).</param>
    public void SetHostRelayData(string ipAddress, ushort port, byte[] allocationId, byte[] key, byte[] connectionData, bool isSecure = false)
    {
        SetRelayServerData(ipAddress, port, allocationId, key, connectionData, null, isSecure);
    }
    public void SetRelayServerData(string ipv4Address, ushort port, byte[] allocationIdBytes, byte[] keyBytes, byte[] connectionDataBytes, byte[] hostConnectionDataBytes = null, bool isSecure = false)
    {
        RelayConnectionData hostConnectionData;

        if (!NetworkEndPoint.TryParse(ipv4Address, port, out var serverEndpoint))
        {
            Debug.LogError($"Invalid address {ipv4Address}:{port}");

            // We set this to default to cause other checks to fail to state you need to call this
            // function again.
            m_RelayServerData = default;
            return;
        }
        /*
        var allocationId = ConvertFromAllocationIdBytes(allocationIdBytes);
        var key = ConvertFromHMAC(keyBytes);
        var connectionData = ConvertConnectionData(connectionDataBytes);

        if (hostConnectionDataBytes != null)
        {
            hostConnectionData = ConvertConnectionData(hostConnectionDataBytes);
        }
        else
        {
            hostConnectionData = connectionData;
        }

        m_RelayServerData = new RelayServerData(ref serverEndpoint, 0, ref allocationId, ref connectionData, ref hostConnectionData, ref key, isSecure);
        m_RelayServerData.ComputeNewNonce();

        SetProtocol(ProtocolType.RelayUnityTransport);
        */
    }

    private void DisposeInternals()
    {
        if (m_Driver.IsCreated)
        {
            m_Driver.Dispose();
        }

        m_NetworkSettings.Dispose();
    }

    public void CreateDriver(Transport transport, out NetworkDriver driver, out NetworkPipeline unreliableFragmentedPipeline, out NetworkPipeline unreliableSequencedFragmentedPipeline, out NetworkPipeline reliableSequencedPipeline)
    {
        throw new NotImplementedException();
    }
}
