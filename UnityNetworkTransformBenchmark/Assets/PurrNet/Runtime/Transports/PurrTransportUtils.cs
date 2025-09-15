// #define USE_LOCAL_MASTER

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#if UNITY_WEB
using UnityEngine.Networking;
#endif

namespace PurrNet.Transports
{
    [UsedImplicitly]
    [Serializable]
    public struct RelayServer
    {
        public string host;
        public int restPort;
        public int udpPort;
        public int webSocketsPort;
        public string region;
    }

    [UsedImplicitly]
    [Serializable]
    public struct Relayers
    {
        public RelayServer[] servers;
    }

    [UsedImplicitly]
    [Serializable]
    public struct HostJoinInfo
    {
        public bool ssl;
        public string secret;
        public int port;
    }

    [UsedImplicitly]
    [Serializable]
    public struct ClientJoinInfo
    {
        public bool ssl;
        public string secret;
        public string host;
        public int port;
    }

    public static class PurrTransportUtils
    {
        static async Task<string> Get([UsedImplicitly] string url)
        {
#if UNITY_WEB
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Cache-Control", "no-cache");
            request.useHttpContinue = false;
            var response = await request.SendWebRequest();
            return response.webRequest.downloadHandler.text;

#else
            throw new NotSupportedException("You need the `com.unity.modules.unitywebrequest` package to use this.");
#endif
        }

        public static async Task<T> Retry<T>(int count, Func<Task<T>> action, CancellationTokenSource cts = null)
        {
            Exception lastException = null;
            for (var i = 0; i < count; i++)
            {
                if (cts is { IsCancellationRequested: true })
                    return Task.FromCanceled<T>(cts.Token).Result;

                if (i > 0)
                    await UnityLatestUpdate.WaitSeconds(1f);
                try
                {
                    return await action();
                }
                catch (Exception e)
                {
                    lastException = e;
                }
            }

            if (lastException == null)
                throw new Exception("Failed to retry.");
            throw lastException;
        }

        internal static async Task<ClientJoinInfo> Join(string server, string roomName, CancellationTokenSource cts)
        {
            return await Retry<ClientJoinInfo>(10, () => ActualClientJoinInfo(server, roomName), cts);
        }

        private static async Task<ClientJoinInfo> ActualClientJoinInfo(string server, string roomName)
        {
#if UNITY_WEB
            var url = $"{server}join";
            var request = UnityWebRequest.Get(url);
            request.useHttpContinue = false;
            request.SetRequestHeader("name", roomName);
            request.SetRequestHeader("Cache-Control", "no-cache");
            var response = await request.SendWebRequest();

            if (response.webRequest.result != UnityWebRequest.Result.Success)
                throw new Exception($"Failed to allocate room: {response.webRequest.downloadHandler.text}");

            var text = response.webRequest.downloadHandler.text;
            var res = JsonUtility.FromJson<ClientJoinInfo>(text);
#if USE_LOCAL_MASTER
            res.ssl = false;
#else
            res.ssl = true;
#endif
            return res;
#else
            throw new NotSupportedException("You need the `com.unity.modules.unitywebrequest` package to use this.");
#endif
        }

        internal static async Task<HostJoinInfo> Alloc(string server, string region, string roomName, CancellationTokenSource cts)
        {
            return await Retry<HostJoinInfo>(10, () => ActualAlloc(server, region, roomName), cts);
        }

        private static async Task<HostJoinInfo> ActualAlloc(string server, string region, string roomName)
        {
#if UNITY_WEB
            var url = $"{server}allocate_ws";

            var request = UnityWebRequest.Get(url);
            request.useHttpContinue = false;
            request.SetRequestHeader("Cache-Control", "no-cache");
            request.SetRequestHeader("region", region);
            request.SetRequestHeader("name", roomName);
            var response = await request.SendWebRequest();

            if (response.webRequest.result != UnityWebRequest.Result.Success)
                throw new Exception($"Failed to allocate room: {response.webRequest.downloadHandler.text}");

            var text = response.webRequest.downloadHandler.text;
            var res = JsonUtility.FromJson<HostJoinInfo>(text);
#if USE_LOCAL_MASTER
            res.ssl = false;
#else
            res.ssl = true;
#endif
            return res;
#else
            throw new NotSupportedException("You need the `com.unity.modules.unitywebrequest` package to use this.");
#endif
        }

        static async Task<float> PingInMS([UsedImplicitly] string url)
        {
            return await Retry<float>(10, () => ActualPing(url));
        }

        private static async Task<float> ActualPing(string url)
        {
#if UNITY_WEB
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Cache-Control", "no-cache");
            request.useHttpContinue = false;
            var sent = DateTime.Now;
            await request.SendWebRequest();

            var received = DateTime.Now;
            return (float)(received - sent).TotalSeconds;
#else
            throw new NotSupportedException("You need the `com.unity.modules.unitywebrequest` package to use this.");
#endif
        }

        public static async Task<Relayers> GetRelayServersAsync(string server)
        {
            return await Retry<Relayers>(10, () => ActualGetRelayServersAsync(server));
        }

        private static async Task<Relayers> ActualGetRelayServersAsync(string server)
        {
            string master = $"{server}servers";
            var response = await Get(master);
            return JsonUtility.FromJson<Relayers>(response);
        }

        public static async Task<RelayServer> GetRelayServerAsync(string masterServer, CancellationTokenSource cts)
        {
            return await Retry<RelayServer>(10, () => ActualGetRelayServerAsync(masterServer), cts);
        }

        private static async Task<RelayServer> ActualGetRelayServerAsync(string masterServer)
        {
            var servers = await GetRelayServersAsync(masterServer);
            float minPing = float.MaxValue;
            RelayServer result = default;

            var pings = new List<Task<float>>();

            foreach (var server in servers.servers)
            {
#if USE_LOCAL_MASTER
                var pingUrl = $"http://{server.host}:{server.restPort}/ping";
#else
                var pingUrl = $"https://{server.host}:{server.restPort}/ping";
#endif
                pings.Add(PingInMS(pingUrl));
            }

            await Task.WhenAny(pings);

            for (var i = 0; i < pings.Count; i++)
            {
                var ping = pings[i];

                if (ping.Status != TaskStatus.RanToCompletion)
                    continue;

                var resultPing = ping.Result;

                if (resultPing < minPing)
                {
                    minPing = resultPing;
                    result = servers.servers[i];
                }
            }

            return result;
        }
    }
}
