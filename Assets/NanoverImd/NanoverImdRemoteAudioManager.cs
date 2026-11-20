using Cysharp.Threading.Tasks;
using Nanover.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace NanoverImd
{
    public class NanoverImdRemoteAudioManager : MonoBehaviour
    {
        [SerializeField]
        private NanoverImdApplication application;

        [SerializeField]
        private NanoverImdSimulation nanover;

        [SerializeField]
        private NanoverImdRemoteDataManager remoteDataManager;

        [SerializeField]
        private AudioClip fallback;

        private Dictionary<string, AudioClip> resource2audioclip = new Dictionary<string, AudioClip>();

        private void Update()
        {
            const string preloadAudioKey = "resources.preload.audio";

            if (nanover.Multiplayer.GetSharedState(preloadAudioKey) is IList<object> resourceIds)
                foreach (var resourceId in resourceIds.Cast<string>())
                    PreloadAudio(resourceId);
        }

        public void Clear()
        {
            foreach (var audio in resource2audioclip.Values)
                Destroy(audio);

            resource2audioclip.Clear();
        }

        public AudioClip GetAudioClip(string resourceId)
        {
            PreloadAudio(resourceId);

            if (resource2audioclip.TryGetValue(resourceId, out AudioClip audio))
                return audio;

            return fallback;
        }

        public void PreloadAudio(string resourceId)
        {
            if (!resource2audioclip.TryGetValue(resourceId, out var audio))
                FetchAudio(resourceId);
        }

        private async void FetchAudio(string resourceId)
        {
            // placeholder to mark this audio as already fetching
            resource2audioclip[resourceId] = fallback;

            var bytes = await remoteDataManager.FetchData(resourceId);
            var path = $"{Application.temporaryCachePath}/{resourceId}.ogg";

            await File.WriteAllBytesAsync(path, bytes);
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($"file://{path}", AudioType.OGGVORBIS))
            {
                await www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                    resource2audioclip[resourceId] = DownloadHandlerAudioClip.GetContent(www);
                else
                    resource2audioclip.Remove(resourceId);
            }
        }
    }
}