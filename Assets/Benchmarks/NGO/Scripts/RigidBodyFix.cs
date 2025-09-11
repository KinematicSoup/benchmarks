using UnityEngine;
using Unity.Netcode;

namespace KS.Benchmark.NGO
{
    /// <summary>Makes the rigid body kinematic when spawned on a client.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RigidBodyFix : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }
}