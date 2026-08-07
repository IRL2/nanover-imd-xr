using Nanover.Frontend.Utility;
using Nanover.Network.Multiplayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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

        [SerializeField]
        private NanoverImdRemoteTexturesManager textures;

        [Header("Shapes")]
        [SerializeField]
        private Renderer shapeTemplate;

        [SerializeField]
        private ShapeMesh[] shapeMeshes;

        [Header("Lines")]
        [SerializeField]
        private ParticleRibbonRenderer lineTemplate;

        [SerializeField]
        private LineMaterial[] lineMaterials;

        [Header("Labels")]
        [SerializeField]
        private Text labelTemplate;

        [Header("Sprites")]
        [SerializeField]
        private Canvas spriteTemplate;
#pragma warning restore 0649

        private IndexedPool<Renderer> shapeObjects;
        private IndexedPool<ParticleRibbonRenderer> lineObjects;
        private IndexedPool<Text> labelObjects;
        private IndexedPool<Canvas> spriteObjects;

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

            lineObjects = new IndexedPool<ParticleRibbonRenderer>(
                () => Instantiate(lineTemplate, parent: lineTemplate.transform.parent),
                transform => transform.gameObject.SetActive(true),
                transform => transform.gameObject.SetActive(false)
            );

            labelObjects = new IndexedPool<Text>(
                () => Instantiate(labelTemplate, parent: labelTemplate.transform.parent),
                transform => transform.gameObject.SetActive(true),
                transform => transform.gameObject.SetActive(false)
            );

            spriteObjects = new IndexedPool<Canvas>(
                () => Instantiate(spriteTemplate, parent: spriteTemplate.transform.parent),
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
            spriteObjects.MapConfig(application.Simulation.Multiplayer.Sprites.Values, UpdateSprite);

            void UpdateShape(MultiplayerObjectShape shape, Renderer model)
            {
                var template = GetShapeTemplate(shape.Shape);
                model.sharedMaterial = template.material;
                model.GetComponent<MeshFilter>().sharedMesh = template.mesh;
                model.transform.localPosition = shape.Position;
                model.transform.localScale = Vector3.one * shape.Size;
                model.material.color = shape.Color;

                transforms.Reparent(model.transform, shape.Parent);
            }

            void UpdateLine(MultiplayerObjectLine line, ParticleRibbonRenderer model)
            {
                var template = GetLineTemplate(line.Type);
                model.GetComponent<ParticleSystemRenderer>().sharedMaterial = template.material;
                model.SetData(
                    line.Positions, 
                    line.Colors,
                    line.Sizes,
                    color: line.Color, 
                    size: line.Size
                );
                transforms.Reparent(model.transform, line.Parent);
            }

            void UpdateLabel(MultiplayerObjectLabel label, Text model)
            {
                model.text = label.Text;
                model.transform.localPosition = label.Position;
                model.transform.localScale = Vector3.one * label.Size / scale;
                model.color = label.Color;

                transforms.Reparent(model.transform, label.Parent);
                model.transform.LookAt(camera.transform);
            }

            void UpdateSprite(MultiplayerObjectSprite sprite, Canvas model)
            {
                var image = model.GetComponentInChildren<RawImage>();

                image.texture = textures.GetTexture(sprite.Texture);
                image.transform.localPosition = sprite.Position;
                image.transform.localScale = Vector3.one * sprite.Size;
                image.color = sprite.Color;
                image.SetNativeSize();

                transforms.Reparent(model.transform, sprite.Parent);
            }
        }
    }
}