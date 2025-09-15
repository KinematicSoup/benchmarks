using System;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor;
using KS.Reactor.Client;
using KS.Reactor.Client.Unity;

namespace KS.Benchmark.Reactor.Client
{
    public class ConnectHandler : MonoBehaviour
    {
        private RoomPingInfo[] m_roomPings;

        private void Start()
        {
            HUD.Get().GameHud.SetActive(false);
            HUD.Get().ConnectHud.SetActive(true);
        }

        private void Update()
        {
            if (m_roomPings == null)
            {
                return;
            }
            // Connect to the first room we get a ping response from.
            for (int i = 0; i < m_roomPings.Length; i++)
            {
                RoomPingInfo roomPing = m_roomPings[i];
                if (roomPing.Order == 0)
                {
                    GetComponent<ksConnect>().Connect(roomPing.RoomInfo);
                    m_roomPings = null;
                    break;
                }
            }
        }

        public void OnGetRooms(ksConnect.GetRoomsEvent ev)
        {
            if (ev.Rooms.Count == 0)
            {
                HUD.Get().Connecting.SetActive(false);
                if (string.IsNullOrEmpty(ev.Error))
                {
                    HUD.Get().ErrorMessage.text = "No rooms found.";
                }
                else
                {
                    HUD.Get().ErrorMessage.text = "Error finding rooms: " + ev.Error;
                }
                return;
            }
            // If there's only one room, connect to it. If there's more than one, ping the rooms and connect to the one
            // with the first response.
            if (ev.Rooms.Count == 1)
            {
                ev.Caller.Connect(ev.Rooms[0]);
            }
            else
            {
                m_roomPings = new RoomPingInfo[ev.Rooms.Count];
                for (int i = 0; i < ev.Rooms.Count; i++)
                {
                    RoomPingInfo ping = new RoomPingInfo(ev.Rooms [i]);
                    m_roomPings[i] = ping;
                    ping.Ping();
                }
            }
        }

        public void OnConnect(ksConnect.ConnectEvent ev)
        {
            if (ev.Status == ksBaseRoom.ConnectStatus.SUCCESS)
            {
                HUD.Get().ConnectHud.SetActive(false);
                HUD.Get().GameHud.SetActive(true);

                ksAddress address = ev.Caller.Room.Info.GetAddress(ev.Caller.Room.Protocol);
                HUD.Get().Server.text = ev.Caller.Room.Info.Name;
                HUD.Get().Host.text = address.Host;
                HUD.Get().Port.text = address.Port.ToString();
                HUD.Get().Protocol.text = GetProtocolName(ev.Caller.Room.Protocol);
                HUD.Get().ResizeWidth(HUD.Get().Room);
            }
            else
            {
                HUD.Get().Connecting.SetActive(false);
                HUD.Get().ErrorMessage.text = "Connection Error: " + ev.Status;
            }
        }

        public void OnDisconnect(ksConnect.DisconnectEvent ev)
        {
            HUD.Get().ConnectHud.SetActive(true);
            HUD.Get().GameHud.SetActive(false);
            HUD.Get().Connecting.SetActive(false);
            HUD.Get().ErrorMessage.text = "Disconnected: " + ev.Status;
        }

        private string GetProtocolName(ksConnectionProtocols protocol)
        {
            switch (protocol)
            {
                case ksConnectionProtocols.WEBSOCKETS: return "WebSockets";
                case ksConnectionProtocols.DIRECT: return "Direct";
                default: return protocol.ToString();
            }
        }
    }
}