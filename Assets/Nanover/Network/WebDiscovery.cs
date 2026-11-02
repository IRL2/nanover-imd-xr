using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace WebDiscovery
{
    [Serializable]
    public class DiscoveryInfo
    {
        public string name;
        public string https;
        public string ws;
        public string wss;
    }

    [Serializable]
    public class DiscoveryEntry
    {
        public string code;
        public DiscoveryInfo info;
    }

    [Serializable]
    public class DiscoveryListing
    {
        public List<DiscoveryEntry> list;
    }

    public class Discovery
    {
        public string Host { get; set; }

        public async UniTask<List<DiscoveryEntry>> GetListing()
        {
            var request = UnityWebRequest.Get($"https://{Host}/list");
            await request.SendWebRequest();

            var json = request.downloadHandler.text;
            json = "{\"list\":" + json + "}";

            var listing = JsonUtility.FromJson<DiscoveryListing>(json);
            return listing.list;
        }

        public async UniTask<DiscoveryEntry> GetEntry(string id)
        {
            var request = UnityWebRequest.Get($"https://{Host}/item/{id}");
            await request.SendWebRequest();

            var json = request.downloadHandler.text;
            json = $"{{\"code\":{id}, \"info\": {request.downloadHandler.text}}}";
            var entry = JsonUtility.FromJson<DiscoveryEntry>(json);
            return entry;
        }
    }
}
