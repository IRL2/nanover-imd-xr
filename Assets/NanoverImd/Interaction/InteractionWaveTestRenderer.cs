using System.Collections.Generic;
using Nanover.Frontend.Utility;
using UnityEngine;

namespace NanoverImd.Interaction
{
    /// <summary>
    /// Manage instances of InteractionWaveRenderer so that all known 
    /// interactions are rendered using Mike's pretty sine wave method from 
    /// Nanover 1
    /// </summary>
    public class InteractionWaveTestRenderer : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NanoverImdSimulation simulation;
        [SerializeField]
        private SineConnectorRenderer waveTemplate;

        private IndexedPool<SineConnectorRenderer> wavePool;
        
#pragma warning restore 0649

        private void Start()
        {
            wavePool = new IndexedPool<SineConnectorRenderer>(CreateInstanceCallback, ActivateInstanceCallback, DeactivateInstanceCallback);
        }

        private void DeactivateInstanceCallback(SineConnectorRenderer obj)
        {
            obj.gameObject.SetActive(false);
        }

        private void ActivateInstanceCallback(SineConnectorRenderer obj)
        {
            obj.gameObject.SetActive(true);
        }

        private SineConnectorRenderer CreateInstanceCallback()
        {
            var renderer = Instantiate(waveTemplate, transform, true);
            renderer.gameObject.SetActive(true);
            return renderer;
        }

        private void Update()
        {
            var interactions = simulation.Interactions;
            var frame = simulation.FrameSynchronizer.CurrentFrame;
            
            wavePool.MapConfig(interactions.Values, MapConfigToInstance);

            void MapConfigToInstance(ParticleInteraction interaction, 
                                     SineConnectorRenderer renderer)
            {
                var particlePositionSim = computeParticleCentroid(interaction.Particles);
                var particlePositionWorld = transform.TransformPoint(particlePositionSim);

                renderer.EndPosition = transform.TransformPoint(interaction.Position);
                renderer.StartPosition = particlePositionWorld;
                renderer.Refresh();
            }

            Vector3 computeParticleCentroid(IReadOnlyList<int> particleIds)
            {
                var centroid = Vector3.zero;

                for (int i = 0; i < particleIds.Count; ++i)
                    centroid += frame.ParticlePositions[particleIds[i]];

                return centroid / particleIds.Count;
            }
        }
    }
}
