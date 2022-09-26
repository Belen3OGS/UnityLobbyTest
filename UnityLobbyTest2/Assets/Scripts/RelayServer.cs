using UnityEngine;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System.Threading.Tasks;
using System;
using Unity.Networking.Transport;
using Unity.Collections;
using System.Collections;

namespace Multiplayer.RelayManagement
{
    public class RelayServer : MonoBehaviour
    {
        public int MaxPacketSize;
        public RelayHelper.RelayHostData HostData
        {
            get { return _hostData; }
            private set { _hostData = value; }
        }
        public RelayServerData ServerData { get; private set; }
        public bool IsRelayServerConnected { get; private set; }
        public JoinAllocation JoinAllocation { get; private set; }
        public Guid PlayerAllocationId { get; private set; }

        public event Action<string> OnServerReady;
        public event Action<int> OnServerConnected;
        public event Action<int> OnServerDisconnected;
        public event Action<int, ArraySegment<byte>, int> OnServerDataReceived;
        public event Action<int, ArraySegment<byte>, int> OnServerDataSent;

        private RelayHelper.RelayHostData _hostData;
        private NetworkDriver _serverDriver;
        private NativeList<NetworkConnection> _connections;

        public IEnumerator InitHost(int maxPlayers)
        {
            _connections = new NativeList<NetworkConnection>(maxPlayers, Allocator.Persistent);

            var createAllocationTask = RelayService.Instance.CreateAllocationAsync(maxPlayers);
            while (!createAllocationTask.IsCompleted)
                yield return null;
            if (createAllocationTask.IsFaulted)
            {
                Debug.LogError("Could not create Relay server Allocation");
                yield break;
            }
            Allocation allocation = createAllocationTask.Result;

            HostData = new RelayHelper.RelayHostData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            var getJoinCodeTask = RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            while (!getJoinCodeTask.IsCompleted)
                yield return null;
            if (getJoinCodeTask.IsFaulted)
            {
                Debug.LogError("Could not obtain Relay server Join Code");
                yield break;
            }
            // Retrieve JoinCode, with this you can join later
            _hostData.JoinCode = getJoinCodeTask.Result;

            ServerData = RelayHelper.HostRelayData(allocation, "udp");

            var serverBindAsHotTask = ServerBindAndListenAsHostPlayer(ServerData);
            while (!serverBindAsHotTask.IsCompleted)
                yield return null;
            if (serverBindAsHotTask.IsFaulted)
            {
                Debug.LogError("Failed to Bind to Server as Host");
                yield break;
            }

            IsRelayServerConnected = true;
            OnServerReady?.Invoke(HostData.JoinCode);
        }

        public void HostEarlyUpdate()
        {
            if (!(_serverDriver.IsCreated && IsRelayServerConnected)) return;

            _serverDriver.ScheduleUpdate().Complete();

            //Accept incoming client connections
            NetworkConnection incomingConnection;
            while ((incomingConnection = _serverDriver.Accept()) != default(NetworkConnection))
            {
                _connections.Add(incomingConnection);
                OnServerConnected?.Invoke(incomingConnection.InternalId);
            }

            //Process events from all connections
            for (int i = 0; i < _connections.Length; i++)
            {
                DataStreamReader stream;

                if (_connections[i].IsCreated)
                {
                    NetworkEvent.Type eventType;
                    while ((eventType = _serverDriver.PopEventForConnection(_connections[i], out stream)) != NetworkEvent.Type.Empty)
                    {
                        if (eventType == NetworkEvent.Type.Disconnect)
                        {
                            DisconnectPlayer(i);
                        }
                        if (eventType == NetworkEvent.Type.Connect)
                        {
                        }
                        else if (eventType == NetworkEvent.Type.Data)
                        {
                            byte[] array = new byte[stream.Length];
                            for (int j = 0; j < array.Length; j++)
                            {
                                array[j] = stream.ReadByte();
                            }
                            ArraySegment<byte> segment = new ArraySegment<byte>(array);
                            OnServerDataReceived?.Invoke(i, segment, 0);
                        }
                    }
                }

            }
        }
        public void HostLateUpdate()
        {
            if (!(_serverDriver.IsCreated && IsRelayServerConnected)) return;
        }

        public void SendToClient(int i, ArraySegment<byte> segment, int channelId)
        {
            if (!_connections[i].IsCreated)
            {
                Debug.LogError("Client already disconnected");
                return;
            }
            DataStreamWriter writer;
            _serverDriver.BeginSend(_connections[i], out writer);
            foreach (byte b in segment)
                writer.WriteByte(b);
            _serverDriver.EndSend(writer);
            OnServerDataSent?.Invoke(i, segment, 0);
        }

        private async Task ServerBindAndListenAsHostPlayer(RelayServerData relayNetworkParameter)
        {
            // Create the NetworkSettings with Relay parameters
            var networkSettings = new NetworkSettings();
            networkSettings.WithRelayParameters(serverData: ref relayNetworkParameter);

            // Create the NetworkDriver using NetworkSettings
            _serverDriver = NetworkDriver.Create(networkSettings);

            // Bind the NetworkDriver to the local endpoint
            if (_serverDriver.Bind(NetworkEndPoint.AnyIpv4) != 0)
            {
                Debug.LogError("Server failed to bind");
            }
            else
            {
                // The binding process is an async operation; wait until bound
                while (!_serverDriver.Bound)
                {
                    _serverDriver.ScheduleUpdate().Complete();
                    await Task.Delay(10);
                }

                // Once the driver is bound you can start listening for connection requests
                if (_serverDriver.Listen() != 0)
                {
                    Debug.LogError("Server failed to listen");
                }
                else
                {
                    IsRelayServerConnected = true;
                }
            }
        }

        public void DisconnectPlayer(int i)
        {
            _connections[i] = default(NetworkConnection);
            OnServerDisconnected?.Invoke(i);
        }

        public void Shutdown()
        {
            if (_serverDriver.IsCreated)
            {
                _serverDriver.ScheduleUpdate().Complete();
                _serverDriver.Dispose();
            }
            if (_connections.IsCreated)
            {
                _connections.Dispose();
            }
            IsRelayServerConnected = false;
        }
    } 
}
