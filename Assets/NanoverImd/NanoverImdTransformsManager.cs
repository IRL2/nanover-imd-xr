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

        [SerializeField]
        private Transform defaultParent;

        [SerializeField]
        private Transform simulationParent;

        private IndexedPool<GameObject> transforms;

        private Dictionary<string, Transform> id2transform = new Dictionary<string, Transform>();

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

        public Transform GetTransform(string id)
        {
            return id2transform.TryGetValue(id ?? "", out var transform) ? transform : defaultParent;
        }

        private void Refresh()
        {
            id2transform.Clear();
            transforms.MapConfig(nanover.Multiplayer.Transforms.Values, UpdateTransformMatrix);
            transforms.MapConfig(nanover.Multiplayer.Transforms.Values, UpdateTransformParent);

            void UpdateTransformMatrix(MultiplayerTransform transform, GameObject go)
            {
                id2transform[transform.ID] = go.transform; 
                transform.Transformation.CopyToTransformRelativeToParent(go.transform);
            }

            void UpdateTransformParent(MultiplayerTransform transform, GameObject go)
            {
                if (transform.Parent == "simulation")
                    go.transform.SetParent(simulationParent, worldPositionStays: false);
                else
                    go.transform.SetParent(defaultParent, worldPositionStays: false);
            }
        }
    }
}