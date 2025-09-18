using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

namespace KS.Benchmark.Reactor.Client
{
    /// <summary>Monitors bandwidth and sync rate and updates the values in the HUD.</summary>
    public class crPerforanceMonitor : ksRoomScript
    {
        public float UpdateInterval = 2f;

        private float m_timer;
        private int m_frames;
        private ulong m_lastServerFrame;
        

        // Called after all other scripts/entities are attached/spawned.
        public override void Initialize()
        {
            HUD.Get().LocalServerWarning.SetActive(Properties[Props.LOCAL_SERVER]);

            m_timer = 0f;
            m_lastServerFrame = Time.Frame;
            UpdateValues(-1f, -1f);
        }
        
        // Called when the script is detached.
        public override void Detached()
        {
            
        }
        
        // Called every frame.
        private void Update()
        {
            m_frames++;
            m_timer += Time.UnscaledDelta;
            if (m_timer < UpdateInterval)
            {
                return;
            }
            float bw = ksNetCounters.Get(ksNetCounters.CounterType.RX_TOTAL) / (m_timer * 1000);
            //ksLog.Info("bandwidth: " + bw + ", fps: " + (m_frames / m_timer));
            float syncRate = (Time.Frame - m_lastServerFrame) / (Time.FramesPerSync * m_timer);
            m_lastServerFrame = Time.Frame;
            m_timer = 0;
            m_frames = 0;
            ksNetCounters.Clear();
            UpdateValues(bw, syncRate);
        }

        private void UpdateValues(float bandwidth, float syncRate)
        {
            HUD.Get().Objects.text = Room.DynamicEntities.Count.ToString();
            HUD.Get().Players.text = Room.Players.Count.ToString();
            HUD.Get().Goodput.text = bandwidth < 0f ? "-" : (bandwidth.ToString("0.00") + " kB/s");
            HUD.Get().Bandwidth.text = bandwidth < 0f ? "-" : ((bandwidth + GetOverhead()).ToString("0") + " kB/s");
            HUD.Get().Syncs.text = syncRate < 0f ? "-" : syncRate.ToString("0.0") + " syncs/s";
            HUD.Get().ResizeWidth(HUD.Get().Stats, 330f);
        }

        private float GetOverhead()
        {
            switch (Room.Protocol)
            {
                case ksConnectionProtocols.TCP: return 4.5f;
                case ksConnectionProtocols.RUDP: return 9f;
                case ksConnectionProtocols.WEBSOCKETS: return 5f;
                case ksConnectionProtocols.DIRECT: return 0f;
                default: return 5f;
            }
        }
    }
}