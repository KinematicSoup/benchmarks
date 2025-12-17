using System;
using System.Collections.Generic;
using UnityEngine;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Core;
using KS.Reactor;

namespace KS.Benchmark.SmartFox2X
{
    public class sfObjectSendManager : MonoBehaviour
    {
        private sfRunner m_runner;
        private List<sfNetId> m_objects = new List<sfNetId>();
        private List<RoomVariable> m_updates = new List<RoomVariable>();
        private int m_nextId = 1;
        private float m_timer;

        private void Awake()
        {
            m_runner = GetComponent<sfRunner>();
            m_runner.OnRoomConnect += HandleConnect;
            m_runner.OnRoomDisconnect += HandleDisconnect;
            enabled = false;    
        }

        private void HandleConnect()
        {
            enabled = m_runner.IsServer;
            if (enabled)
            {
                BaseBenchmark.Get(m_runner.Benchmark).Spawn(m_runner.Benchmark, m_runner.Prefabs, null, Spawn);
            }
        }

        private void HandleDisconnect()
        {
            if (enabled)
            {
                enabled = false;
                m_nextId = 1;
                m_timer = 0;
                for (int i = 0; i < m_objects.Count; i++)
                {
                    Destroy(m_objects[i].gameObject);
                }
                m_objects.Clear();
                m_updates.Clear();
            }
        }

        public void Update()
        {
            if (m_runner.SendRate > 0)
            {
                m_timer -= Time.deltaTime;
                if (m_timer > 0)
                {
                    return;
                }
                m_timer += 1f / m_runner.SendRate;
            }
            SendUpdates();
        }

        public GameObject Spawn(int prefabIndex, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            GameObject go = Instantiate(m_runner.Prefabs[prefabIndex], position, rotation);
            go.transform.localScale = scale;
            sfNetId netId = go.AddComponent<sfNetId>();
            netId.Id = m_nextId++;
            m_objects.Add(netId);

            SFSObject obj = SFSObject.NewInstance();
            obj.PutByte("p", (byte)prefabIndex);
            if (m_runner.InvScalePrecision == 0f)
            {
                obj.PutFloat("sx", scale.x);
                obj.PutFloat("sy", scale.y);
                obj.PutFloat("sz", scale.z);
            }
            else
            {
                obj.PutInt("sx", Quantize(scale.x, m_runner.InvScalePrecision));
                obj.PutInt("sy", Quantize(scale.y, m_runner.InvScalePrecision));
                obj.PutInt("sz", Quantize(scale.z, m_runner.InvScalePrecision));
            }

            m_updates.Add(new SFSRoomVariable("s" + netId.Id, obj));
            return go;
        }

        private void SendUpdates()
        {
            for (int i = 0; i < m_objects.Count; i++)
            {
                sfNetId netId = m_objects[i];
                m_updates.Add(new SFSRoomVariable("u" + netId.Id, GetUpdate(netId.transform)));
            }
            m_runner.SmartFox.Send(new SetRoomVariablesRequest(m_updates));
            m_updates.Clear();
        }

        private SFSObject GetUpdate(Transform t)
        {
            SFSObject obj = SFSObject.NewInstance(); 
            if (m_runner.InvPositionPrecision == 0f)
            {
                obj.PutFloat("x", t.localPosition.x);
                obj.PutFloat("y", t.localPosition.y);
                obj.PutFloat("z", t.localPosition.z);
            }
            else
            {
                obj.PutInt("x", Quantize(t.localPosition.x, m_runner.InvPositionPrecision));
                obj.PutInt("y", Quantize(t.localPosition.y, m_runner.InvPositionPrecision));
                obj.PutInt("z", Quantize(t.localPosition.z, m_runner.InvPositionPrecision));
            }

            if (m_runner.RotationPrecision == 0f)
            {
                obj.PutFloat("q0", t.rotation.x);
                obj.PutFloat("q1", t.rotation.y);
                obj.PutFloat("q2", t.rotation.z);
                obj.PutFloat("q3", t.rotation.w);
            }
            else
            {
                int[] qInt = QuantizeQuaternion(t.rotation);
                obj.PutInt("q0", qInt[0]);
                obj.PutInt("q1", qInt[1]);
                obj.PutInt("q2", qInt[2]);
                obj.PutByte("q3", (byte)qInt[3]);
            }

            return obj;
        }

        private int Quantize(float value, float invPrecision)
        {
            float workValue = value * invPrecision + 0.5f;
            if (workValue > int.MaxValue || workValue < int.MinValue)
            {
                ksLog.Warning(this, "Precision overflow. value = " + value +
                    ", invPrecision = " + invPrecision +
                    ", workValue = " + workValue +
                    ", max = " + (float)int.MaxValue +
                    ", min = " + (float)int.MinValue
                );
            }
            return (int)Mathf.Floor(workValue);
        }

        private int[] QuantizeQuaternion(Quaternion q)
        {
            int[] qInt = new int[4];
            int maxValue = (int)Math.Pow(2, Math.Ceiling(Math.Log(1.0f / m_runner.RotationPrecision, 2)));

            float max = 0f;
            float value = 0f;
            int qMax = 0;
            for (int i = 0; i < 4; i++)
            {
                value = (q[i] < 0) ? -q[i] : q[i];
                if (max < value)
                {
                    max = value;
                    qMax = i;
                }
            }
            qInt[3] = qMax;

            bool isNegative = q[qMax] < 0.0f;
            int qi = 0;
            float maxF = maxValue;
            for (int i = 0; i < 3; i++)
            {
                qi = (int)(maxF * (q[i + (qMax > i ? 0 : 1)]));
                if (isNegative)
                {
                    qi = -qi;
                }
                qInt[i] = qi;
            }

            return qInt;
        }


    }
}