using System;
using KS.Reactor;
using UnityEngine;

namespace KS.Benchmark
{
    /// <summary>
    /// Accelerates or decelerates the rigid body to maintain a target speed. if it is outside the spherical bounds,
    /// accelerates it back towards the origin.
    /// </summary>
    public class VelocityBoundsCheck : MonoBehaviour
    {
        private const float ACCEL = 5f;
        private const float BOUNDS_ACCEL = 10f;

        public float TargetSpeed;
        public float Bounds;
        private Rigidbody m_rigidBody;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            m_rigidBody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            if (transform.position.sqrMagnitude > Bounds * Bounds)
            {
                m_rigidBody.linearVelocity -= transform.position.normalized * BOUNDS_ACCEL * Time.deltaTime;
            }
            else
            {
                float speedSq = m_rigidBody.linearVelocity.sqrMagnitude;
                if (ksMath.Abs(speedSq - TargetSpeed * TargetSpeed) > .001f)
                {
                    float speed = ksMath.Sqrt(speedSq);
                    if (speed < TargetSpeed)
                    {
                        speed += ACCEL * Time.deltaTime;
                        speed = Math.Min(speed, TargetSpeed);
                    }
                    else
                    {
                        speed -= ACCEL * Time.deltaTime;
                        speed = Math.Max(speed, TargetSpeed);
                    }
                    m_rigidBody.linearVelocity = m_rigidBody.linearVelocity.normalized * speed;
                }
            }
        }
    }
}