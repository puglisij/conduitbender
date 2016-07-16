using UnityEngine;
using System.Collections;
using System;



namespace CB
{
    public delegate void SequenceEvent( Sequence.Event e );

    public abstract class Sequence : MonoBehaviour
    {
        public enum Event { Started, Killed, Finished }

        public event SequenceEvent onEvent;
        public virtual bool hasFinished
        {
            get { return m_HasFinished; }
        }

        protected bool m_HasFinished = false;

        public virtual void Run()
        {
            OnStarted();

        }
        public virtual void Kill()
        {
            OnKilled();

        }

        protected void OnKilled()
        {
            if (onEvent != null) {
                onEvent( Event.Killed );
            }
            m_HasFinished = true;
        }
        protected void OnFinished()
        {
            if (onEvent != null) {
                onEvent( Event.Finished );
            }
            m_HasFinished = true;
        }
        protected void OnStarted()
        {
            if (onEvent != null) {
                onEvent( Event.Started );
            }
            m_HasFinished = false;
        }
        
    } // Sequence
}



