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

        private Coroutine m_activeRoutine = null;
        private GameObject m_ParticleObj;
        private ParticleSystem m_Particles;

        private float m_FramesPerSec = 30f;
        private float m_SecPerFrame = 1f / 30f;

        public override void Run()
        {
            base.Run();
            m_activeRoutine = StartCoroutine( DelayedStartParticles() );
        }

        public override void Kill() 
        {
            base.Kill();
            StopCoroutine( m_activeRoutine );
        }

        IEnumerator DelayedDestroyParticles()
        {
            float startTime = Time.time;
            float endTime = startTime + particleDestroyDelay;
            // Fade Lamp Post
            while(Time.time < endTime) { 
                yield return new WaitForFixedUpdate();
            }

            m_Particles.Stop();

            while (m_Particles.IsAlive()) {
                yield return new WaitForFixedUpdate();
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

            yield return new WaitForSeconds( particleStartDelay );

            // Start the Particle System
            m_Particles.Play();

            // Disable particles after t seconds
            m_activeRoutine = StartCoroutine( DelayedDestroyParticles() );
        }
    }
}

