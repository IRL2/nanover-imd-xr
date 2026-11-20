using Nanover.Core;
using System;
using System.Collections.Generic;
using System.IO;
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
        private AudioClip fallback;

        private Dictionary<string, AudioClip> resource2audioclip = new Dictionary<string, AudioClip>();

        public void Clear()
        {
            foreach (var audio in resource2audioclip.Values)
                Destroy(audio);

            resource2audioclip.Clear();
        }

        public AudioClip GetAudioClip(string resourceId)
        {
            if (resource2audioclip.TryGetValue(resourceId, out AudioClip audio))
                return audio;

            FetchAudio(resourceId);

            return fallback;
        }

        private async void FetchAudio(string resourceId)
        {
            // placeholder to mark this audio as already fetching
            resource2audioclip[resourceId] = fallback;

            var response = await nanover.RunCommand("resources/fetch", new Dictionary<string, object> { { "key", resourceId } });
            var path = $"{Application.temporaryCachePath}/{resourceId}.ogg";

            if (response.TryGetValue("data", out byte[] bytes))
            {
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
            else
            {
                resource2audioclip.Remove(resourceId);
            }
        }
    }
}