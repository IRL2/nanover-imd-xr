using Nanover.Frontend.Utility;
using Nanover.Network.Multiplayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;
using Text = TMPro.TextMeshPro;

namespace NanoverImd
{
    public class NanoverImdObjectsRenderer : MonoBehaviour
    {
        [Serializable]
        public class ShapeMesh
        {
            public string shape;
            public Mesh mesh;
            public Material material;
        }

        [Serializable]
        public class LineMaterial
        {
            public string type;
            public Material material;
        }


#pragma warning disable 0649
        [SerializeField]
        private NanoverImdApplication application;
        
        [SerializeField]
        private NanoverImdSimulation nanover;

        [SerializeField]
        private NanoverImdTransformsManager transforms;

        [Header("Shapes")]
        [SerializeField]
        private Renderer shapeTemplate;

        [SerializeField]
        private ShapeMesh[] shapeMeshes;

        [Header("Lines")]
        [SerializeField]
        private LineRenderer lineTemplate;

        [SerializeField]
        private LineMaterial[] lineMaterials;

        [Header("Labels")]
        [SerializeField]
        private Text labelTemplate;
#pragma warning restore 0649

        private IndexedPool<Renderer> shapeObjects;
        private IndexedPool<LineRenderer> lineObjects;
        private IndexedPool<Text> labelObjects;

        private void Update()
        {
            UpdateRendering();
        }

        private void Start()
        {
            shapeObjects = new IndexedPool<Renderer>(
                () => Instantiate(shapeTemplate, parent: shapeTemplate.transform.parent),
                transform => transform.gameObject.SetActive(true),
                transform => transform.gameObject.SetActive(false)
            );

            lineObjects = new IndexedPool<LineRenderer>(
                () => Instantiate(lineTemplate, parent: lineTemplate.transform.parent),
                transform => transform.gameObject.SetActive(true),
                transform => transform.gameObject.SetActive(false)
            );

            labelObjects = new IndexedPool<Text>(
                () => Instantiate(labelTemplate, parent: labelTemplate.transform.parent),
                transform => transform.gameObject.SetActive(true),
                transform => transform.gameObject.SetActive(false)
            );
        }

        private ShapeMesh GetShapeTemplate(string shape)
        {
            return shapeMeshes.FirstOrDefault(mesh => mesh.shape == shape) ?? shapeMeshes[0];
        }

        private LineMaterial GetLineTemplate(string type)
        {
            return lineMaterials.FirstOrDefault(template => template.type == type) ?? lineMaterials[0];
        }

        private void UpdateRendering()
        {
            var camera = Camera.main;
            var scale = Math.Abs(transform.lossyScale.x);

            shapeObjects.MapConfig(application.Simulation.Multiplayer.Shapes.Values, UpdateShape);
            lineObjects.MapConfig(application.Simulation.Multiplayer.Lines.Values, UpdateLine);
            labelObjects.MapConfig(application.Simulation.Multiplayer.Labels.Values, UpdateLabel);

            void UpdateShape(MultiplayerObjectShape shape, Renderer model)
            {
                var template = GetShapeTemplate(shape.Shape);
                model.sharedMaterial = template.material;
                model.GetComponent<MeshFilter>().sharedMesh = template.mesh;
                model.transform.localPosition = shape.Position;
                model.transform.localScale = Vector3.one * shape.Size;
                model.material.color = shape.Color;

                model.transform.SetParent(transforms.GetTransform(shape.Parent), worldPositionStays: false);
            }

            void UpdateLine(MultiplayerObjectLine line, LineRenderer model)
            {
                var template = GetLineTemplate(line.Type);
                model.positionCount = line.Positions.Length;
                model.SetPositions(line.Positions);
                model.widthMultiplier = line.Size * scale;
                model.sharedMaterial = template.material;
                model.startColor = line.Color;
                model.endColor = line.Color;

                model.transform.SetParent(transforms.GetTransform(line.Parent), worldPositionStays: false);
            }

            void UpdateLabel(MultiplayerObjectLabel label, Text model)
            {
                model.text = label.Text;
                model.transform.localPosition = label.Position;
                model.transform.localScale = Vector3.one * label.Size / scale;
                model.color = label.Color;

                model.transform.SetParent(transforms.GetTransform(label.Parent), worldPositionStays: false);
                model.transform.LookAt(camera.transform);
            }
        }
    }
}