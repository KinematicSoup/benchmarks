/* This file was auto-generated. DO NOT MODIFY THIS FILE. */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using KS.Reactor.Client.Unity;
using KS.Reactor;
using KS.Unity;

namespace KSProxies.Scripts.KS.Benchmark.Reactor.Server
{
    
    public class srSpawner : ksProxyRoomScript
    {
#if UNITY_EDITOR
        [Tooltip("Prefab to spawn, without number suffix.")]
        public String Prefab;
        [Tooltip("If greater than zero, a number between 1 and this will be appended to the prefab path when spawning entities.")]
        public Int32 PrefabCount;
        public KSProxies.Scripts.KS.Benchmark.Reactor.BaseBenchmarkData Benchmark;
        public srSpawner() : base() 
        {
            Prefab = "asteroid";
            PrefabCount = 8;
            Benchmark = null;
        }
#endif
    }
}