using Nanover.Core;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
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

        private Dictionary<string, AudioClip> resource2audio = new Dictionary<string, AudioClip>();

        public void Clear()
        {
            foreach (var audio in resource2audio.Values)
                Destroy(audio);

            resource2audio.Clear();
        }

        public AudioClip GetAudio(string resourceId)
        {
            if (resource2audio.TryGetValue(resourceId, out AudioClip audio))
                return audio;

            FetchAudio(resourceId);

            return fallback;
        }

        private async void FetchAudio(string resourceId)
        {
            var response = await nanover.RunCommand("resources/fetch", new Dictionary<string, object> { { "key", resourceId } });
            var path = $"{Application.temporaryCachePath}/{resourceId}.ogg";

            if (response.TryGetValue("data", out byte[] bytes))
            {
                await File.WriteAllBytesAsync(path, bytes);
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($"file://{path}", AudioType.OGGVORBIS))
                {
                    await www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError("Error: " + www.error);
                    }
                    else
                    {
                        resource2audio[resourceId] = DownloadHandlerAudioClip.GetContent(www);
                        Debug.LogError($"{resourceId} is {resource2audio[resourceId]}");
                    }
                }
            }
        }
    }
}