using System;
using System.Collections.Generic;
using UnityEngine;
using KS.Benchmark.Reactor;
using KS.Reactor;

namespace KS.Benchmark
{
    /// <summary>
    /// Base class for benchmarks that run in Unity. There is an equivalent sBaseBenchmark for benchmarks that run on
    /// the Reactor server. Reactor benchmarks and Unity benchmarks both load their data from a <see cref="BaseBenchmarkData"/>
    /// script asset The type of <see cref="BaseBenchmarkData"/> determines which benchmark to run. If you create a new benchmark,
    /// add a mapping from your <see cref="BaseBenchmarkData"/> type to your <see cref="BaseBenchmark"/> type in the 
    /// <see cref="BaseBenchmark"/> static constructor.
    /// </summary>
    public abstract class BaseBenchmark
    {
        public delegate GameObject SpawnHandler(GameObject prefab, Vector3 pos, Quaternion rotation);

        private static Dictionary<Type, Type> m_typeMap = new Dictionary<Type, Type>();
        private static string LOG_CHANNEL = typeof(BaseBenchmark).FullName;

        /// <summary>Called once to spawn the game objects at the start of the benchmark.</summary>
        /// <param name="data"></param>
        /// <param name="prefabs"></param>
        /// <param name="callback">Callback to call on each spawned game object.</param>
        /// <param name="spawnHandler">
        /// Callback to spawn a game object from a prefab. If null, 
        /// <see cref="UnityEngine.Object.Instantiate(UnityEngine.Object)"/> will be used.
        /// </param>
        public abstract void Spawn(BaseBenchmarkData data,
            GameObject[] prefabs,
            Action<GameObject> callback = null,
            SpawnHandler spawnHandler = null);

        static BaseBenchmark()
        {
            // Map BaseBenchmarkData types to the BaseBenchmark types to run.
            m_typeMap[typeof(SphereRingBenchmarkData)] = typeof(SphereRingBenchmark);
        }

        /// <summary>
        /// Gets the <see cref="BaseBenchmark"/> to run for a <see cref="BaseBenchmarkData"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static BaseBenchmark Get(BaseBenchmarkData data)
        {
            Type type;
            if (!m_typeMap.TryGetValue(data.GetType(), out type))
            {
                ksLog.Error(LOG_CHANNEL, "No BaseBenchmark type registered for " + data.GetType());
                return null;
            }
            try
            {
                return (BaseBenchmark)Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                ksLog.Error(LOG_CHANNEL, "Error creating " + type.Name, e);
                return null;
            }
        }
    }
}