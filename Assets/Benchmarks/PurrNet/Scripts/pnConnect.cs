using UnityEngine;
using UnityEngine.UI;
using PurrNet;
using PurrNet.Transports;

namespace KS.Benchmark.PurrNet
{
    /// <summary>Starts a PurrNet server or connects to one as a client when a button is pressed.</summary>
    [RequireComponent(typeof(NetworkManager))]
    public class pnConnect : MonoBehaviour
    {
        public Button ServerButton;
        public Button ClientButton;

        private NetworkManager m_network;

        void Start()
        {
            m_network = GetComponent<NetworkManager>();
            m_network.onClientConnectionState += OnConnectionChange;
            m_network.onServerConnectionState += OnConnectionChange;
            OnConnectionChange(m_network.clientState == ConnectionState.Disconnected ? 
                m_network.serverState : m_network.clientState);

            ServerButton.onClick.AddListener(m_network.StartServer);
            ClientButton.onClick.AddListener(m_network.StartClient);
        }

        private void OnConnectionChange(ConnectionState state)
        {
            if (ServerButton != null && ClientButton != null)
            {
                ServerButton.gameObject.SetActive(state == ConnectionState.Disconnected);
                ClientButton.gameObject.SetActive(state == ConnectionState.Disconnected);
            }
        }
    }
}