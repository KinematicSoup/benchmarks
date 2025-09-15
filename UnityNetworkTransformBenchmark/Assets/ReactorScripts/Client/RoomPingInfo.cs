using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;
using System.Net.NetworkInformation;
using UnityEngine;

using Ping = System.Net.NetworkInformation.Ping;
using AdaptorsRoom = KS.Reactor.Client.Adaptors.ksRoom;

namespace KS.Benchmark.Reactor.Client
{
    /// <summary>Pings a room</summary>
    public class RoomPingInfo
    {
        public ksRoomInfo RoomInfo;

        public bool IsPingComplete
        {
            get { return m_order >= 0; }
        }

        /// <summary>
        /// The first room to get a ping response will have order 0, and the second will have order 1, etc. -1 if we
        /// haven't got a ping response yet.
        /// </summary>
        public int Order
        {
            get { return m_order; }
        }

        private int m_order = -1;

        private static int m_pingCount;
        private static object m_pingLock = new object();

        public RoomPingInfo(ksRoomInfo roomInfo)
        {
            RoomInfo = roomInfo;
        }

        public void Ping()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            AdaptorsRoom room = new AdaptorsRoom(RoomInfo);
            room.Protocol = ksConnectionProtocols.WEBSOCKETS;
            room.OnStateChange += OnStateChange;
            ksReactor.Service.JoinRoom(room, null);
#else
            Ping ping = new Ping();
            ping.PingCompleted += OnPing;
            ping.SendAsync(RoomInfo.GetAddress(ksConnectionProtocols.TCP).Host, null);
#endif
        }

        private void OnPing(object sender, PingCompletedEventArgs args)
        {
            lock (m_pingLock)
            {
                m_order = m_pingCount;
                m_pingCount++;
            }
        }

        private void OnStateChange(ksBaseRoom room, ksConnectionStates status)
        {
            if (status == ksConnectionStates.HANDSHAKE)
            {
                m_order = m_pingCount;
                m_pingCount++;
                room.OnStateChange -= OnStateChange;
                ksReactor.Service.LeaveRoom(room);
            }
        }
    }
}