using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using KS.Reactor.Server;
using KS.Reactor;

namespace KS.Benchmark.Reactor.Server
{
    /// <summary>
    /// Base class for benchmarks that run on the Reactor server. There is an equivalent BaseBenchmark for benchmarks
    /// that run in Unity. Reactor benchmarks and Unity benchmarks both load their data from a <see cref="BaseBenchmarkData"/>
    /// script asset The type of <see cref="BaseBenchmarkData"/> determines which benchmark to run. If you create a new benchmark,
    /// add a mapping from your <see cref="BaseBenchmarkData"/> type to your <see cref="sBaseBenchmark"/> type in the 
    /// <see cref="sBaseBenchmark"/> static constructor.
    /// </summary>
    public abstract class sBaseBenchmark
    {
        private static Dictionary<Type, Type> m_typeMap = new Dictionary<Type, Type>();
        private static string LOG_CHANNEL = typeof(sBaseBenchmark).FullName;

        /// <summary>Called once to spawn the entities at the start of the benchmark.</summary>
        /// <param name="room"></param>
        /// <param name="data"></param>
        /// <param name="prefab">Path of prefab to spawn.</param>
        /// <param name="prefabCount">
        /// The number of prefabs to choose from when spawning. If greater than zero, a number between 1 and the
        /// prefab count must be appended to the prefab path to spawn.
        /// </param>
        public abstract void Spawn(ksIServerRoom room, BaseBenchmarkData data, string prefab, int prefabCount);

        static sBaseBenchmark()
        {
            // Map BaseBenchmarkData types to the sBaseBenchmark types to run.
            m_typeMap[typeof(SphereRingBenchmarkData)] = typeof(sSphereRingBenchmark);
        }

        /// <summary>Gets the <see cref="sBaseBenchmark"/> to run for a <see cref="BaseBenchmarkData"/>.</summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static sBaseBenchmark Get(BaseBenchmarkData data)
        {
            Type type;
            if (!m_typeMap.TryGetValue(data.GetType(), out type))
            {
                ksLog.Error(LOG_CHANNEL, "No sBaseBenchmark type registered for " + data.GetType());
                return null;
            }
            try
            {
                return (sBaseBenchmark)Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                ksLog.Error(LOG_CHANNEL, "Error creating " + type.Name, e);
                return null;
            }
        }
    }
}