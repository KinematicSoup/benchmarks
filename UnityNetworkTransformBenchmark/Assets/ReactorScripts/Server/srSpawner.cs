using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using KS.Reactor.Server;
using KS.Reactor;

namespace KS.Benchmark.Reactor.Server
{
    /// <summary>Spawns entities for a benchmark.</summary>
    public class srSpawner : ksServerRoomScript
    {
        /// <summary>Prefab to spawn, without number suffix.</summary>
        [ksEditable]
        [ksUnityTag("[Tooltip(\"Prefab to spawn, without number suffix.\")]")]
        public string Prefab = "asteroid";
        /// <summary>
        /// If greater than zero, a number between 1 and this will be appended to the prefab path when spawning entities.
        /// </summary>
        [ksEditable]
        [ksUnityTag("[Tooltip(\"If greater than zero, a number between 1 and this will be appended to the prefab path when spawning entities.\")]")]
        public int PrefabCount = 8;
        [ksEditable]
        public BaseBenchmarkData Benchmark;

        public override void Initialize()
        {
#if REACTOR_LOCAL_SERVER
            Properties[Props.LOCAL_SERVER] = true;
#endif

            // Register an Update handler at time -1 (before physics) so we can stop physics from updating if there are
            // no connected players.
            Room.OnUpdate[-1] += Update;

            sBaseBenchmark.Get(Benchmark).Spawn(Room, Benchmark, Prefab, PrefabCount);
        }

        public override void Detached()
        {
            Room.OnUpdate[-1] -= Update;
        }

        private void Update()
        {
            // Skip further script updates and don't update physics if there are no connected players.
            Room.SkipFrameUpdates = Room.ConnectedPlayerCount == 0;
            Time.TimeScale = Room.SkipFrameUpdates ? 0f : 1f;
        }
    }
}