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
    /// Accelerates or decelerates the rigid body to maintain a target speed. if it is outside the spherical bounds,
    /// accelerates it back towards the origin.
    /// </summary>
    public class sVelocityBoundsCheck : ksServerEntityScript
    {
        private const float ACCEL = 5f;
        private const float BOUNDS_ACCEL = 10f;

        public float TargetSpeed;
        public float Bounds;
        private ksRigidBody m_rigidBody;

        public override void Initialize()
        {
            m_rigidBody = Scripts.Get<ksRigidBody>();
            if (m_rigidBody == null)
            {
                return;
            }
            Room.OnUpdate[0] += Update;
        }
        
        public override void Detached()
        {
            Room.OnUpdate[0] -= Update;
        }
        
        private void Update()
        {
            if (Transform.Position.MagnitudeSquared() > Bounds * Bounds)
            {
                m_rigidBody.Velocity -= m_rigidBody.Transform.Position.Normalized() * BOUNDS_ACCEL * Time.Delta;
            }
            else
            {
                float speedSq = m_rigidBody.Velocity.MagnitudeSquared();
                if (ksMath.Abs(speedSq - TargetSpeed * TargetSpeed) > .001f)
                {
                    float speed = ksMath.Sqrt(speedSq);
                    if (speed < TargetSpeed)
                    {
                        speed += ACCEL * Time.Delta;
                        speed = Math.Min(speed, TargetSpeed);
                    }
                    else
                    {
                        speed -= ACCEL * Time.Delta;
                        speed = Math.Max(speed, TargetSpeed);
                    }
                    m_rigidBody.Velocity = m_rigidBody.Velocity.Normalized() * speed;
                }
            }
        }
    }
}