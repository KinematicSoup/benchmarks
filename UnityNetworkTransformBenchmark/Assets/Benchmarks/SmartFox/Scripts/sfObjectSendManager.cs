using System;
using System.Collections.Generic;
using UnityEngine;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using KS.Reactor;
using KS.Reactor.Client.Unity;
using KS.Benchmark.Reactor;

namespace KS.Benchmark.SmartFox2X
{
    /// <summary>
    /// Spawns game objects for an SFS benchmark when connected to SFS as a server, and manages the sending of spawn
    /// data and transform updates. The data for each game object is synced as 2 SFSObject room variables - one
    /// containing the data that never changes (prefab, scale) and the other containing the data that changes
    /// (position, rotation). Changing scale after spawning is not supported. Transform data is sent as quantized ints
    /// and the quaternion uses smallest-3 encoding, unless the transform precision values configured on
    /// <see cref="sfRunner"/> are set to 0, in which cause it sends the full float values.
    /// </summary>
    public class sfObjectSendManager : MonoBehaviour
    {
        public ksScriptAssetReference<BaseBenchmarkData> Benchmark;
        public float SendRate = 30f;

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
                BaseBenchmark.Get(Benchmark).Spawn(Benchmark, m_runner.Prefabs, null, Spawn);
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
            if (SendRate > 0)
            {
                m_timer -= Time.deltaTime;
                if (m_timer > 0)
                {
                    return;
                }
                m_timer += 1f / SendRate;
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
            obj.PutByte(sfVars.PREFAB, (byte)prefabIndex);
            if (m_runner.InvScalePrecision == 0f)
            {
                obj.PutFloat(sfVars.SCALE_X, scale.x);
                obj.PutFloat(sfVars.SCALE_Y, scale.y);
                obj.PutFloat(sfVars.SCALE_Z, scale.z);
            }
            else
            {
                obj.PutInt(sfVars.SCALE_X, Quantize(scale.x, m_runner.InvScalePrecision));
                obj.PutInt(sfVars.SCALE_Y, Quantize(scale.y, m_runner.InvScalePrecision));
                obj.PutInt(sfVars.SCALE_Z, Quantize(scale.z, m_runner.InvScalePrecision));
            }

            m_updates.Add(new SFSRoomVariable(sfIdPrefix.SPAWN + netId.Id, obj));
            return go;
        }

        private void SendUpdates()
        {
            for (int i = 0; i < m_objects.Count; i++)
            {
                sfNetId netId = m_objects[i];
                m_updates.Add(new SFSRoomVariable(sfIdPrefix.UPDATE + netId.Id, GetUpdate(netId.transform)));
            }
            m_runner.SmartFox.Send(new SetRoomVariablesRequest(m_updates));
            m_updates.Clear();
        }

        private SFSObject GetUpdate(Transform t)
        {
            SFSObject obj = SFSObject.NewInstance(); 
            if (m_runner.InvPositionPrecision == 0f)
            {
                obj.PutFloat(sfVars.POS_X, t.localPosition.x);
                obj.PutFloat(sfVars.POS_Y, t.localPosition.y);
                obj.PutFloat(sfVars.POS_Z, t.localPosition.z);
            }
            else
            {
                obj.PutInt(sfVars.POS_X, Quantize(t.localPosition.x, m_runner.InvPositionPrecision));
                obj.PutInt(sfVars.POS_Y, Quantize(t.localPosition.y, m_runner.InvPositionPrecision));
                obj.PutInt(sfVars.POS_Z, Quantize(t.localPosition.z, m_runner.InvPositionPrecision));
            }

            if (m_runner.RotationPrecision == 0f)
            {
                obj.PutFloat(sfVars.Q0, t.rotation.x);
                obj.PutFloat(sfVars.Q1, t.rotation.y);
                obj.PutFloat(sfVars.Q2, t.rotation.z);
                obj.PutFloat(sfVars.Q3, t.rotation.w);
            }
            else
            {
                int[] qInt = EncodeQuaternion(t.rotation);
                obj.PutInt(sfVars.Q0, qInt[0]);
                obj.PutInt(sfVars.Q1, qInt[1]);
                obj.PutInt(sfVars.Q2, qInt[2]);
                obj.PutByte(sfVars.Q3, (byte)qInt[3]);
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

        private int[] EncodeQuaternion(Quaternion q)
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