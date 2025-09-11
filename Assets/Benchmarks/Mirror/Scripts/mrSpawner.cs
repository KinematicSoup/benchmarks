using UnityEngine;
using Mirror;
using KS.Benchmark.Reactor;
using KS.Reactor.Client.Unity;

namespace KS.Benchmark.Mirror
{
    /// <summary>Spawns game objects for a Mirror benchmark.</summary>
    public class mrSpawner : NetworkBehaviour
    {
        public GameObject[] Prefabs;
        public ksScriptAssetReference<BaseBenchmarkData> Benchmark;

        public override void OnStartServer()
        {
            Physics.simulationMode = SimulationMode.FixedUpdate;
            BaseBenchmark.Get(Benchmark).Spawn(Benchmark, Prefabs, (GameObject go) => NetworkServer.Spawn(go));
        }
    }
}