using System.Collections.Generic;
using UnityEngine;

public class ParticleRibbonRenderer : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem particleSystem;

    private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[0];

    private void Awake()
    {
        Resize(128);
    }

    private void Resize(int minimum)
    {
        int count = minimum;

        while (count < minimum)
            count *= 2;

        particles = new ParticleSystem.Particle[count];

        float factor = 1f / count;

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].remainingLifetime = i * factor;
        }
    }

    public void SetData(
        IList<Vector3> positions,
        IList<Color32> colors = null,
        IList<float> sizes = null,
        Color32 color = default,
        float size = .1f
    )
    {
        if (positions.Count > particles.Length)
            Resize(positions.Count);

        for (int i = 0; i < positions.Count; ++i)
        {
            particles[i].position = positions[i];
            particles[i].startColor = colors != null ? colors[i] : color;
            particles[i].startSize = sizes != null ? sizes[i] : size;
        }

        particleSystem.SetParticles(particles, positions.Count);
    }
}
