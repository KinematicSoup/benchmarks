/* This file was auto-generated. DO NOT MODIFY THIS FILE. */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using KS.Reactor.Client.Unity;
using KS.Reactor;
using KS.Unity;

namespace KSProxies.Scripts.KS.Benchmark.Reactor
{
    [CreateAssetMenu(menuName = ksMenuNames.REACTOR + "SphereRingBenchmarkData", order = ksMenuGroups.SCRIPT_ASSETS)]
    public class SphereRingBenchmarkData : KSProxies.Scripts.KS.Benchmark.Reactor.BaseBenchmarkData
    {

        public Int32 NumAsteroids;
        [Tooltip("True to scale the bounds to keep the density the same as it would be with 1000 asteroids.")]
        public Boolean ScaleBounds;
        public Single Bounds;
        public Single MinScale;
        public Single MaxScale;
        public Single MaxAngularVelocity;
        public Single MinSpeed;
        public Single MaxSpeed;
        [Tooltip("If zero, a random seed will be used.")]
        public Int32 Seed;
        public SphereRingBenchmarkData() : base() 
        {
            NumAsteroids = 1000;
            ScaleBounds = true;
            Bounds = 50f;
            MinScale = 0.25f;
            MaxScale = 1f;
            MaxAngularVelocity = 45f;
            MinSpeed = 1f;
            MaxSpeed = 4f;
            Seed = 0;
        }

    }
}