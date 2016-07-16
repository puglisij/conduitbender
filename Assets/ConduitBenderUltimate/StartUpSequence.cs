using UnityEngine;
using System.Collections;
using System;

namespace CB
{
    public class StartUpSequence : Sequence
    {
        //public Animator animator; //?

        // Lamp Post
        public GameObject particlesPrefab;

        public float particleStartDelay = 1.0f;
        public float particleDestroyDelay = 2.0f;

        private GameObject m_ParticleObj;
        private ParticleSystem m_Particles;

        private MeshRenderer m_LampRenderer;
        private Color        m_LampColor;

        private float m_FramesPerSec = 30f;
        private float m_SecPerFrame = 1f / 30f;

        public override void Run()
        {
            StartCoroutine( DelayedStartParticles() );
        }

        public override void Kill()
        {
            throw new NotImplementedException();
        }

        IEnumerator DelayedDestroyParticles()
        {
            float startTime = Time.time;
            float endTime = startTime + particleDestroyDelay;
            // Fade Lamp Post
            while(Time.time < endTime) {
                yield return new WaitForFixedUpdate();

                // Set Lamp Post Alpha
                m_LampColor.a = (endTime - Time.time) / particleDestroyDelay;
                m_LampRenderer.material.color = m_LampColor;
            }

            Destroy( m_ParticleObj );
            OnFinished();
        }
        IEnumerator DelayedStartParticles()
        {
            // Create Particles
            m_ParticleObj = Instantiate( particlesPrefab );
            m_ParticleObj.transform.SetParent( transform, true );
            m_Particles = m_ParticleObj.GetComponentInChildren<ParticleSystem>();
            m_LampRenderer = m_ParticleObj.GetComponent<MeshRenderer>();
            m_LampColor = m_LampRenderer.material.color;

            yield return new WaitForSeconds( particleStartDelay );

            // Start the Particle System
            m_Particles.Play();

            // Disable particles after t seconds
            StartCoroutine( DelayedDestroyParticles() );

            OnStarted();
        }
    }
}

