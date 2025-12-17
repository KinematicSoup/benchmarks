using System;
using UnityEngine;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Requests;
using Sfs2X.Entities;
using Sfs2X.Util;
using KS.Reactor;
using KS.Reactor.Client.Unity;
using KS.Benchmark.Reactor;
using static Unity.Collections.Unicode;

namespace KS.Benchmark.SmartFox2X
{
    public class sfRunner : MonoBehaviour
    {
        public event Action OnRoomConnect;
        public event Action OnRoomDisconnect;

        public GameObject[] Prefabs;
        public ksScriptAssetReference<BaseBenchmarkData> Benchmark;

        public float SendRate = 30f;

        public string Host = "localhost";
        public ushort ClientPort = 9933;
        public ushort ServerPort = 9934;

        public float PositionPrecision
        {
            get { return m_positionPrecision; }
        }
        [SerializeField]
        private float m_positionPrecision = .01f;

        public float RotationPrecision
        {
            get { return m_rotationPrecistion; }
        }
        [SerializeField]
        private float m_rotationPrecistion = .001f;

        public float ScalePrecision
        {
            get { return m_scalePrecision; }
        }
        [SerializeField]
        private float m_scalePrecision = .01f;

        public float InvPositionPrecision
        {
            get { return m_invPositionPrecision; }
        }
        private float m_invPositionPrecision;

        public float InvScalePrecision
        {
            get { return m_invScalePrecision; }
        }
        private float m_invScalePrecision;

        public bool IsServer
        {
            get { return m_isServer; }
        }
        private bool m_isServer;

        public bool IsRoomConnected
        {
            get { return m_isRoomConnected; }
            set { m_isRoomConnected = value; }
        }
        private bool m_isRoomConnected;

        public SmartFox SmartFox
        {
            get { return m_smartFox; }
        }
        public SmartFox m_smartFox = new SmartFox();

        private void Awake()
        {
            Application.runInBackground = true;
            Physics.simulationMode = SimulationMode.FixedUpdate;
            if (m_positionPrecision > 0)
            {
                m_invPositionPrecision = 1f / m_positionPrecision;
            }
            if (m_scalePrecision > 0)
            {
                m_invScalePrecision = 1f / m_scalePrecision;
            }

            m_smartFox.AddEventListener(SFSEvent.CONNECTION, HandleConnect);
            m_smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, HandleDisconnect);
            m_smartFox.AddEventListener(SFSEvent.LOGIN, HandleLogin);
            m_smartFox.AddEventListener(SFSEvent.LOGIN_ERROR, HandleLoginError);
            m_smartFox.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, HandleRoomCreationError);
            m_smartFox.AddEventListener(SFSEvent.ROOM_ADD, HandleRoomAdd);
            m_smartFox.AddEventListener(SFSEvent.ROOM_JOIN, HandleJoinRoom);
            m_smartFox.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, HandleJoinRoomError);
            
        }

        private void OnDestroy()
        {
            m_smartFox.Disconnect();
            m_smartFox.RemoveAllEventListeners();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                ConnectAsServer();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                ConnectAsClient();
            }

            m_smartFox.ProcessEvents();
        }

        public void ConnectAsServer()
        {
            if (m_smartFox.IsConnected || m_smartFox.IsConnecting)
            {
                return;
            }
            m_isServer = true;
            m_smartFox.Connect(Host, ServerPort);
        }

        public void ConnectAsClient()
        {
            if (m_smartFox.IsConnected || m_smartFox.IsConnecting)
            {
                return;
            }
            m_isServer = false;
            m_smartFox.Connect(Host, ClientPort);
        }

        private void HandleConnect(BaseEvent ev)
        {
            if (!(bool)ev.Params["success"])
            {
                ksLog.Error("Connection error.");
                return;
            }
            m_smartFox.Send(new LoginRequest(IsServer ? "server" : "client", null, "BasicExamples"));
        }

        private void HandleDisconnect(BaseEvent ev)
        {
            string reason = (string)ev.Params["reason"];
            if (reason != ClientDisconnectionReason.MANUAL)
            {
                ksLog.Error(this, "Disconnected: " + reason);
            }
            else
            {
                ksLog.Info(this, "Disconnected.");
            }
            if (m_isRoomConnected)
            {
                m_isRoomConnected = false;
                if (OnRoomDisconnect != null)
                {
                    OnRoomDisconnect();
                }
            }
        }

        private void HandleLogin(BaseEvent ev)
        {
            if (IsServer)
            {
                ksLog.Info("Logged in as server. Starting room...");
                RoomSettings roomSettings = new RoomSettings(gameObject.name);
                roomSettings.MaxVariables = 10000;
                m_smartFox.Send(new CreateRoomRequest(roomSettings, true));
            }
            else
            {
                ksLog.Info("Logged in as client. Joining room...");
                Room room = m_smartFox.RoomManager.GetRoomByName(gameObject.name);
                if (room == null)
                {
                    ksLog.Warning("Could not find room '" + gameObject.name + "' to join.");
                    m_smartFox.Disconnect();
                }
                else
                {
                    m_smartFox.Send(new JoinRoomRequest(room.Id));
                }
            }
        }

        private void HandleLoginError(BaseEvent ev)
        {
            ksLog.Error(this, "Login error: " + ev.Params["errorMessage"]);
            m_smartFox.Disconnect();
        }

        private void HandleJoinRoom(BaseEvent ev)
        {
            ksLog.Info(this, "Joined room");
            m_isRoomConnected = true;
            if (OnRoomConnect != null)
            {
                OnRoomConnect();
            }
        }

        private void HandleJoinRoomError(BaseEvent ev)
        {
            ksLog.Error(this, "Error joining room: " + ev.Params["errorMessage"]);
            m_smartFox.Disconnect();
        }

        private void HandleRoomCreationError(BaseEvent ev)
        {
            ksLog.Error(this, "Error creating room: " + ev.Params["errorMessage"]);
        }

        private void HandleRoomAdd(BaseEvent ev)
        {
            ksLog.Info(this, "Room added");
        }
    }
}