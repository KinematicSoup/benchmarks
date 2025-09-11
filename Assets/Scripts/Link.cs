using UnityEngine;
using UnityEngine.UI;

namespace KS.Benchmark
{
    /// <summary>Opens a URL when a button on the same object is pressed.</summary>
    [RequireComponent(typeof(Button))]
    public class Link : MonoBehaviour
    {
        public string URL;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(OpenURL);
        }

        private void OpenURL()
        {
            Application.OpenURL(URL);
        }
    }
}