using UnityEngine;

namespace KS.Benchmark.SmartFox2X
{
    public class sfNetId : MonoBehaviour
    {
        public int Id
        {
            get { return m_id; }
            set { m_id = value; }
        }
        private int m_id;
    }
}