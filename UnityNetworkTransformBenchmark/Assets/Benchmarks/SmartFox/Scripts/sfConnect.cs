using UnityEngine;
using UnityEngine.UI;

namespace KS.Benchmark.SmartFox2X
{
    /// <summary>Connects to SFS as a server or a client when a button is pressed.</summary>
    public class sfConnect : MonoBehaviour
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        public Button ServerButton;
        public Button ClientButton;

        private sfRunner m_runner;

        void Start()
        {
            m_runner = GetComponent<sfRunner>();
            m_runner.OnRoomConnect += OnConnectionChange;
            m_runner.OnRoomDisconnect += OnConnectionChange;
            OnConnectionChange();

            ServerButton.onClick.AddListener(m_runner.ConnectAsServer);
            ClientButton.onClick.AddListener(m_runner.ConnectAsClient);
        }

        private void OnConnectionChange()
        {
            if (ServerButton != null && ClientButton != null)
            {
                ServerButton.gameObject.SetActive(!m_runner.IsRoomConnected);
                ClientButton.gameObject.SetActive(!m_runner.IsRoomConnected);
            }
        }
#endif
    }
}