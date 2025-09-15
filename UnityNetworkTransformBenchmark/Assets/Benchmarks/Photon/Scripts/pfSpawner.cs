using UnityEngine;
#if FUSION2
using Fusion;
#endif
using KS.Benchmark.Reactor;
using KS.Reactor.Client.Unity;

namespace KS.Benchmark.Photon
{
    /// <summary>Spawns game objects for a Photon Fusion benchmark.</summary>
    public class pfSpawner : MonoBehaviour
    {
        public GameObject[] Prefabs;
        public ksScriptAssetReference<BaseBenchmarkData> Benchmark;

#if !FUSION2
        private void Start()
        {
            Debug.LogWarning("Photon Fusion 2 is not installed.");
        }
#else
#if UNITY_WEBGL
        private void Start()
        {
            Debug.Log("If the server does not start, trying changing the build profile to something other than WebGL.");
        }
#endif

        public void OnConnect(NetworkRunner runner)
        {
            if (runner.IsServer)
            {
                Physics.simulationMode = SimulationMode.FixedUpdate;
                BaseBenchmark.Get(Benchmark).Spawn(Benchmark, Prefabs, null, (GameObject go, Vector3 pos, Quaternion rot) =>
                    runner.Spawn(go, pos, rot).gameObject);
            }
        }
#endif
    }
}