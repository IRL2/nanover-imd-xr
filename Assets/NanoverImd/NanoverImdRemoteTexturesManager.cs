using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nanover.Core;
using UnityEngine;

namespace NanoverImd
{
    public class NanoverImdRemoteTexturesManager : MonoBehaviour
    {
        [SerializeField]
        private NanoverImdApplication application;

        [SerializeField]
        private NanoverImdSimulation nanover;

        private Dictionary<string, Texture2D> resource2texture = new Dictionary<string, Texture2D>();

        public void Clear()
        {
            foreach (var texture in resource2texture.Values)
                Destroy(texture);

            resource2texture.Clear();
        }

        public Texture2D GetTexture(string resourceId)
        {
            if (resource2texture.TryGetValue(resourceId, out Texture2D texture))
                return texture;

            texture = new Texture2D(2, 2);
            resource2texture[resourceId] = texture;

            FetchTexture(resourceId, texture);

            return texture;
        }

        private async void FetchTexture(string resourceId, Texture2D texture)
        {
            var response = await nanover.RunCommand("resources/fetch", new Dictionary<string, object> { { "key", resourceId } });

            if (response.TryGetValue("data", out byte[] bytes))
            {
                ImageConversion.LoadImage(texture, bytes);
            }
        }
    }
}