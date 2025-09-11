using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using KS.Benchmark.Reactor;
using KS.Reactor.Client.Unity;

namespace KS.Benchmark.NGO
{
    /// <summary>Spawns game objects for an NGO benchmark.</summary>
    public class ngoSpawner : NetworkBehaviour
    {
        public GameObject[] Prefabs;
        public ksScriptAssetReference<BaseBenchmarkData> Benchmark;
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                Physics.simulationMode = SimulationMode.FixedUpdate;
                BaseBenchmark.Get(Benchmark).Spawn(Benchmark, Prefabs, (GameObject go) => 
                {
                    go.GetComponent<NetworkObject>().Spawn();
                });
            }
        }
    }
}