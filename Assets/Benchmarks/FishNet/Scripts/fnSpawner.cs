using UnityEngine;
using FishNet.Object;
using KS.Benchmark.Reactor;
using KS.Reactor.Client.Unity;

namespace KS.Benchmark.FishNet
{
    /// <summary>Spawns game objects for a FishNet benchmark.</summary>
    public class fnSpawner : NetworkBehaviour
    {
        public GameObject[] Prefabs;
        public ksScriptAssetReference<BaseBenchmarkData> Benchmark;
        

        public override void OnStartServer()
        {
            BaseBenchmark.Get(Benchmark).Spawn(Benchmark, Prefabs, (GameObject go) => Spawn(go));
        }
    }
}
