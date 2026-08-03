using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nanover.Core.Math;
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
        private Transform calibratedParent;

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

        public void Reparent(Transform child, string parent)
        {
            child.SetParent(GetTransform(parent), worldPositionStays: false);
        }

        private void Refresh()
        {
            var m = application.CalibratedSpace.LocalToWorldMatrix;
            calibratedParent.localPosition = m.GetPosition();
            calibratedParent.localRotation = m.GetRotation();

            id2transform.Clear();

            transforms.MapConfig(nanover.Multiplayer.Transforms.Values, UpdateTransformMatrix);
            transforms.MapConfig(nanover.Multiplayer.Transforms.Values, UpdateTransformParent);

            // can't decide what this is called
            id2transform["root"] = calibratedParent;
            id2transform["shared"] = calibratedParent;
            id2transform["calibrated"] = calibratedParent;

            // overwrite with local so box movement is fluid
            id2transform["simulation"] = simulationParent;

            void UpdateTransformMatrix(MultiplayerTransform transform, GameObject go)
            {
                go.name = $"Transform {transform.ID}";
                id2transform[transform.ID] = go.transform; 
                transform.Transformation.CopyToTransformRelativeToParent(go.transform);
            }

            void UpdateTransformParent(MultiplayerTransform transform, GameObject go)
            {
                Reparent(go.transform, transform.Parent);
            }
        }
    }
}