using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MadStark.Wayfinder
{
    public class PathDisplay : MonoBehaviour
    {
        public new ParticleSystem particleSystem;
        public float spacing = 0.1f;
        public float size = 0.1f;
        public Color pathColor;

        private void Start()
        {
            var emissionModule = particleSystem.emission;
            emissionModule.enabled = false;
            particleSystem.Clear();
        }

        public void Display(Path path)
        {
            var particles = new ParticleSystem.Particle[(int)(path.calculatedLength / spacing)];

            int currentLinkIndex = 0;
            Link currentLink = path.links[currentLinkIndex];
            Transform a = path.start, b = currentLink.GetOtherNode(path.start);
            float currentLinkLength = currentLink.CalculateCost();
            float progressBetweenAandB = 0;

            for (int i = 0; i < particles.Length; i++)
            {
                progressBetweenAandB += spacing;

                while (progressBetweenAandB >= currentLinkLength)
                {
                    if (currentLinkIndex >= path.links.Count - 1)
                        break;
                    progressBetweenAandB -= currentLinkLength;
                    a = b;
                    currentLink = path.links[++currentLinkIndex];
                    b = currentLink.GetOtherNode(b);
                    currentLinkLength = currentLink.CalculateCost();
                }

                particles[i].position = Vector3.Lerp(a.position, b.position, progressBetweenAandB / currentLinkLength);
                particles[i].startColor = pathColor;
                particles[i].startSize = size;
            }

            particleSystem.SetParticles(particles.ToArray(), particles.Length);
        }

        public void Clear()
        {
            particleSystem.Clear();
        }
    }
}
