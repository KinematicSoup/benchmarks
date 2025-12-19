using System;
using System.Collections.Generic;
using UnityEngine;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Core;

namespace KS.Benchmark.SmartFox2X
{
    /// <summary>
    /// Applies server transform data stored in SFSObjects room variables to game objects when connected to SFS as a
    /// client.
    /// </summary>
    public class sfObjectReceiveManager : MonoBehaviour
    {
        private sfRunner m_runner;
        private Dictionary<int, sfNetId> m_objects = new Dictionary<int, sfNetId>();

        private void Awake()
        {
            m_runner = GetComponent<sfRunner>();
            m_runner.OnRoomConnect += HandleConnect;
            m_runner.OnRoomDisconnect += HandleDisconnect;
            enabled = false;
        }

        private void HandleConnect()
        {
            if (m_runner.IsServer)
            {
                return;
            }
            enabled = true;
            m_runner.SmartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, HandleUpdate);
        }

        private void HandleDisconnect()
        {
            if (enabled)
            {
                enabled = false;
                m_runner.SmartFox.RemoveEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, HandleUpdate);

                foreach (sfNetId obj in m_objects.Values)
                {
                    Destroy(obj.gameObject);
                }
                m_objects.Clear();
            }
        }

        private void HandleUpdate(BaseEvent ev)
        {
            List<string> changedNames = (List<string>)ev.Params["changedVars"];
            for (int i = 0; i < changedNames.Count; i++)
            {
                string name = changedNames[i];
                if (!name.StartsWith(sfIdPrefix.UPDATE))
                {
                    continue;
                }
                int id;
                if (!int.TryParse(name.Substring(sfIdPrefix.UPDATE.Length), out id))
                {
                    continue;
                }
                sfNetId netId;
                ISFSObject obj;
                if (!m_objects.TryGetValue(id, out netId))
                {
                    // We didn't find an object with this id, so spawn a new one. We get the immutable spawn data for
                    // this object from a different SFSObject than the one with the update data.
                    obj = GetSFObject(sfIdPrefix.SPAWN + id);
                    if (obj == null)
                    {
                        continue;
                    }
                    int prefabIndex = obj.GetByte(sfVars.PREFAB);
                    GameObject go = Instantiate(m_runner.Prefabs[prefabIndex]);
                    netId = go.AddComponent<sfNetId>();
                    netId.Id = id;
                    m_objects[id] = netId;

                    Vector3 scale = new Vector3();
                    if (m_runner.ScalePrecision == 0f)
                    {
                        scale.x = obj.GetFloat(sfVars.SCALE_X);
                        scale.y = obj.GetFloat(sfVars.SCALE_Y);
                        scale.z = obj.GetFloat(sfVars.SCALE_Z);
                    }
                    else
                    {
                        scale.x = obj.GetInt(sfVars.SCALE_X) * m_runner.ScalePrecision;
                        scale.y = obj.GetInt(sfVars.SCALE_Y) * m_runner.ScalePrecision;
                        scale.z = obj.GetInt(sfVars.SCALE_Z) * m_runner.ScalePrecision;
                    }
                    go.transform.localScale = scale;
                }

                obj = GetSFObject(name);
                if (obj == null)
                {
                    continue;
                }
                Vector3 position = new Vector3();
                if (m_runner.PositionPrecision == 0f)
                {
                    position.x = obj.GetFloat(sfVars.POS_X);
                    position.y = obj.GetFloat(sfVars.POS_Y);
                    position.z = obj.GetFloat(sfVars.POS_Z);
                }
                else
                {
                    position.x = obj.GetInt(sfVars.POS_X) * m_runner.PositionPrecision;
                    position.y = obj.GetInt(sfVars.POS_Y) * m_runner.PositionPrecision;
                    position.z = obj.GetInt(sfVars.POS_Z) * m_runner.PositionPrecision;
                }
                netId.transform.localPosition = position;

                if (m_runner.RotationPrecision == 0f)
                {
                    Quaternion rot = new Quaternion();
                    rot.x = obj.GetFloat(sfVars.Q0);
                    rot.y = obj.GetFloat(sfVars.Q1);
                    rot.z = obj.GetFloat(sfVars.Q2);
                    rot.w = obj.GetFloat(sfVars.Q3);
                    netId.transform.localRotation = rot;
                }
                else
                {
                    int[] qInt = new int[4];
                    qInt[0] = obj.GetInt(sfVars.Q0);
                    qInt[1] = obj.GetInt(sfVars.Q1);
                    qInt[2] = obj.GetInt(sfVars.Q2);
                    qInt[3] = obj.GetByte(sfVars.Q3);
                    netId.transform.localRotation = DecodeQuaternion(qInt);
                }
            }
        }

        private ISFSObject GetSFObject(string name)
        {
            RoomVariable var = m_runner.SmartFox.LastJoinedRoom.GetVariable(name);
            if (var == null)
            {
                return null;
            }
            return var.GetSFSObjectValue();
        }

        private Quaternion DecodeQuaternion(int[] qInt)
        {
            int maxValue = (int)Math.Pow(2, Math.Ceiling(Math.Log(1 / m_runner.RotationPrecision, 2)));
            Quaternion q = new Quaternion();
            float squareSum = 0f;
            if (maxValue != 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float f = (float)((double)qInt[i] / (double)maxValue);
                    q[i + (qInt[3] <= i ? 1 : 0)] = f;
                    squareSum += f * f;
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    int index = i + (qInt[3] <= i ? 1 : 0);
                    q[index] = BitConverter.ToSingle(BitConverter.GetBytes(qInt[i]), 0);
                    squareSum += q[index] * q[index];
                }
            }
            q[qInt[3]] = squareSum > 1f ? 0f : (float)Math.Sqrt(1f - squareSum);
            q.Normalize();
            return q;
        }
    }
}