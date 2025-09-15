using UnityEngine;
using PurrNet;
using KS.Reactor.Client.Unity;
using KS.Benchmark.Reactor;

namespace KS.Benchmark.PurrNet
{
    /// <summary>Spawns game objects for a PurrNet benchmark.</summary>
    public class pnAsteroidSpawner : NetworkBehaviour
    {
        public GameObject[] Prefabs;
        public ksScriptAssetReference<BaseBenchmarkData> Benchmark;

        protected override void OnSpawned()
        {
            if (isServer)
            {
                Physics.simulationMode = SimulationMode.FixedUpdate;
                BaseBenchmark.Get(Benchmark).Spawn(Benchmark, Prefabs);
            }
        }
    }
}