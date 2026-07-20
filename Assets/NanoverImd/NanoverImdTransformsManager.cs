using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nanover.Frontend.Utility;
using Nanover.Network.Multiplayer;
using UnityEngine;

namespace NanoverImd
{
    public class NanoverImdTransformsManager : MonoBehaviour
    {
        [SerializeField]
        private NanoverImdApplication application;
        
        [SerializeField]
        private NanoverImdSimulation nanover;

        private IndexedPool<GameObject> transforms;

        private void Awake()
        {
            transforms = new IndexedPool<GameObject>(
                () => new GameObject("Transform"),
                (go) => go.SetActive(true),
                (go) => go.SetActive(false)
            );
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            transforms.MapConfig(nanover.Multiplayer.Transforms.Values, UpdateTransformMatrix);

            void UpdateTransformMatrix(MultiplayerTransform transform, GameObject go)
            {
                transform.Transformation.CopyToTransformRelativeToParent(go.transform);
            }
        }
    }
}