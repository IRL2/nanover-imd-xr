using Cysharp.Threading.Tasks;
using Nanover.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NanoverImd
{
    public class NanoverImdRemoteDataManager : MonoBehaviour
    {
        [SerializeField]
        private NanoverImdApplication application;

        [SerializeField]
        private NanoverImdSimulation nanover;

        public async UniTask<byte[]> FetchData(string resourceId)
        {
            var response = await nanover.RunCommand("resources/fetch", new Dictionary<string, object> { { "key", resourceId } });

            if (response.TryGetValue("data", out byte[] bytes))
                return bytes;

            return null;
        }
    }
}