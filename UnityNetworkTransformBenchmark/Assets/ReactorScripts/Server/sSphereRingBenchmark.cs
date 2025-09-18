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
    /// Benchmark that spawns asteroids within a sphere that accelerate/decelerate to maintain a random target speed.
    /// Asteroids change directions when they collide or are outside the bounds of the sphere. The simulation 
    /// eventually settles into a ring formation with most asteroids travelling in the same direction around the sphere.
    /// 
    /// The is the Reactor server implementation of the benchmark. There is an equivalent Unity implementation in
    /// SphereRingBenchmark. Any changes to one should also be changed in the other.
    /// 
    /// The benchmark gets its data from a <see cref="SphereRingBenchmarkData"/> script asset.
    /// </summary>
    public class sSphereRingBenchmark : sBaseBenchmark
    {
        public override void Spawn(ksIServerRoom room, BaseBenchmarkData data, string prefab, int prefabCount)
        {
            if (room.DynamicEntities.Count == data.ObjectCount)
            {
                return;
            }

            SphereRingBenchmarkData d = (SphereRingBenchmarkData)data;

            // If ScaleBounds is true, scale the bounds to keep the density the same as it would be if there were 1000
            // asteroids.
            float bounds = d.ScaleBounds ? 
                Math.Max(d.MinScaledBounds, d.Bounds * ksMath.Pow(d.ObjectCount / 1000f, 1f / 3f)) :
                d.Bounds;
            sVelocityBoundsCheck.Bounds = bounds;

            // If there are more dynamic entities than requested, destroy entities until we reach the object count.
            if (room.DynamicEntities.Count >= data.ObjectCount)
            {
                for (int i = data.ObjectCount; i < room.DynamicEntities.Count; i++)
                {
                    room.DynamicEntities[i].Destroy();
                }
                return;
            }

            ksSphere sphere = new ksSphere(2f);
            ksOverlapParams overlapParams = new ksOverlapParams();
            overlapParams.Shape = sphere;

            ksSpawnParams spawnParams = new ksSpawnParams();

            ksRandom rand = d.Seed == 0 ? new ksRandom() : new ksRandom(d.Seed);
            for (int i = room.DynamicEntities.Count; i < d.ObjectCount; i++)
            {
                spawnParams.EntityType = prefab;
                if (prefabCount > 0)
                {
                    spawnParams.EntityType += rand.Next(prefabCount) + 1;
                }

                spawnParams.Transform.Position = rand.NextVector3() * bounds;
                spawnParams.Transform.Rotation = rand.NextQuaternion();
                float scale = rand.NextFloat(d.MinScale, d.MaxScale);
                spawnParams.Transform.Scale = new ksVector3(scale, scale, scale);

                // Check the asteroid will not spawn overlapping. If there is an overlap, move the spawn point away
                // from the origin and try again.
                sphere.Radius = 2f * scale;
                ksVector3 direction = spawnParams.Transform.Position.Normalized();
                if (direction == ksVector3.Zero)
                {
                    direction = ksVector3.Right;
                }
                overlapParams.Origin = spawnParams.Transform.Position;
                while (room.Physics.OverlapAny(overlapParams))
                {
                    overlapParams.Origin += direction * sphere.Radius;
                    spawnParams.Transform.Position = overlapParams.Origin;
                }

                ksIServerEntity entity = room.SpawnEntity(spawnParams);
                ksRigidBody rb = entity.Scripts.Get<ksRigidBody>();
                rb.AngularVelocity = rand.NextVector3() * d.MaxAngularVelocity;
                float speed = rand.NextFloat(d.MinSpeed, d.MaxSpeed);
                rb.Velocity = rand.NextUnitVector3() * speed;
                rb.Mass = scale * scale * scale;

                // The velocity bounds check script maintains the target speed and keeps the asteroids within the
                // sphere bounds.
                sVelocityBoundsCheck check = new sVelocityBoundsCheck();
                check.TargetSpeed = speed;
                entity.Scripts.Attach(check);
            }
        }
    }
}