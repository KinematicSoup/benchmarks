using System;
using KS.Benchmark;
using KS.Reactor;
using UnityEngine;
using KS.Benchmark.Reactor;

namespace KS.Benchmark
{
    /// <summary>
    /// Benchmark that spawns asteroids within a sphere that accelerate/decelerate to maintain a random target speed.
    /// Asteroids change directions when they collide or are outside the bounds of the sphere. The simulation 
    /// eventually settles into a ring formation with most asteroids travelling in the same direction around the sphere.
    /// 
    /// The is the Unity implementation of the benchmark. There is an equivalent Reactor server implementation in
    /// sSphereRingBenchmark. Any changes to one should also be changed in the other.
    /// 
    /// The benchmark gets its data from a <see cref="SphereRingBenchmarkData"/> script asset.
    /// </summary>
    public class SphereRingBenchmark : BaseBenchmark
    {
        public override void Spawn(
            BaseBenchmarkData data,
            GameObject[] prefabs,
            Action<GameObject> callback = null,
            SpawnHandler spawnHandler = null)
        {
            SphereRingBenchmarkData d = (SphereRingBenchmarkData)data;

            // If ScaleBounds is true, scale the bounds to keep the density the same as it would be if there were 1000
            // asteroids.
            float bounds = d.ScaleBounds ? d.Bounds * ksMath.Pow(d.NumAsteroids / 1000f, 1f / 3f) : d.Bounds;

            ksRandom rand = d.Seed == 0 ? new ksRandom() : new ksRandom(d.Seed);
            for (int i = 0; i < d.NumAsteroids; i++)
            {
                Vector3 pos = rand.NextVector3() * bounds;
                Quaternion rotation = rand.NextQuaternion();
                float scale = rand.NextFloat(d.MinScale, d.MaxScale);

                // Check the asteroid will not spawn overlapping. If there is an overlap, move the spawn point away
                // from the origin and try again.
                Vector3 direction = pos.normalized;
                if (direction == Vector3.zero)
                {
                    direction = Vector3.right;
                }
                while (Physics.OverlapSphere(pos, 2f * scale).Length > 0)
                {
                    pos += direction * 2f * scale;
                }

                GameObject go;
                if (spawnHandler == null)
                {
                    go = GameObject.Instantiate(prefabs[rand.Next(prefabs.Length)], pos, rotation);
                }
                else
                {
                    go = spawnHandler(prefabs[rand.Next(prefabs.Length)], pos, rotation);
                }
                go.transform.localScale = new Vector3(scale, scale, scale);

                Rigidbody rb = go.GetComponent<Rigidbody>();
                rb.angularVelocity = rand.NextVector3() * d.MaxAngularVelocity * ksMath.FDEGREES_TO_RADIANS;
                float speed = rand.NextFloat(d.MinSpeed, d.MaxSpeed);
                rb.linearVelocity = rand.NextUnitVector3() * speed;
                rb.mass = scale * scale * scale;

                // The velocity bounds check script maintains the target speed and keeps the asteroids within the
                // sphere bounds.
                VelocityBoundsCheck check = go.gameObject.AddComponent<VelocityBoundsCheck>();
                check.TargetSpeed = speed;
                check.Bounds = bounds;

                if (callback != null)
                {
                    callback(go);
                }
            }
        }
    }
}