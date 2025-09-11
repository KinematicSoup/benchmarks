using UnityEngine;
using UnityEditor;
using KS.Reactor.Client.Unity.Editor;

namespace KS.Benchmark.Reactor.Editor
{
    /// <summary>Excludes scenes from Reactor config builds that aren't in the PATH.</summary>
    [InitializeOnLoad]
    public class BuildFilter
    {
        private const string PATH = "Assets/Benchmarks/Reactor";

        static BuildFilter()
        {
            ksBuildEvents.IncludeSceneCheck = (string sceneName) => sceneName.StartsWith(PATH);
        }
    }
}
