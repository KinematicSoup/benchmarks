using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor;

namespace KS.Benchmark.Reactor
{
    /// <summary>Configurable data for the asteroid sphere-ring benchmark.</summary>
    [ksSharedData]
    public class SphereRingBenchmarkData : BaseBenchmarkData
    {
        [ksEditable]
        public int NumAsteroids = 1000;
        /// <summary>
        /// True to scale the bounds to keep the density the same as it would be with 1000 asteroids.
        /// </summary>
        [ksEditable]
        [ksUnityTag("[Tooltip(\"True to scale the bounds to keep the density the same as it would be with 1000 asteroids.\")]")]
        public bool ScaleBounds = true;
        [ksEditable]
        public float Bounds = 50f;
        [ksEditable]
        public float MinScale = .25f;
        [ksEditable]
        public float MaxScale = 1f;
        [ksEditable]
        public float MaxAngularVelocity = 45f;
        [ksEditable]
        public float MinSpeed = 1f;
        [ksEditable]
        public float MaxSpeed = 4f;
        /// <summary>If zero, a random seed will be used.</summary>
        [ksEditable]
        [ksUnityTag("[Tooltip(\"If zero, a random seed will be used.\")]")]
        public int Seed = 0;
    }
}