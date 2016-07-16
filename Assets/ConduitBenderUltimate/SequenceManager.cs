using UnityEngine;
using System;
using System.Collections.Generic;



namespace CB
{
    public class SequenceManager : MonoBehaviour
    {
        [Serializable]
        public class SequenceMeta
        {
            public string sequenceName;
            public Sequence sequence;
            public SequenceMeta(string sequenceName, Sequence sequence)
            {
                this.sequenceName = sequenceName;
                this.sequence = sequence;
            }
        }

        [SerializeField]
        public List<SequenceMeta> sequences = new List<SequenceMeta>();

        private SequenceMeta m_ActiveSequence = null;


        void Awake()
        {

        }
        void Start()
        {

        }

        private void DestroySequence()
        {
            if (m_ActiveSequence != null) {
                Destroy( m_ActiveSequence.sequence.gameObject );
                m_ActiveSequence = null;
            }
        }

        /// <summary>
        /// Sequence Events Management  e.g. Cleanup when Sequence Finishes.
        /// </summary>
        private void OnSequenceEvent( Sequence.Event e )
        {
            if (e == Sequence.Event.Finished) {
                DestroySequence();
            }
            Debug.Log( "SequenceManager: OnSequenceEvent() Event: " + e );
        }

        /// <summary>
        /// Runs the Sequence by the given name if found.
        /// </summary>
        public void RunSequence(string name)
        {
            int i = sequences.FindIndex( ( x ) => { return x.sequenceName == name; } );
            if(i != -1) {
                var seq = Instantiate( sequences[i].sequence );
                    seq.transform.SetParent( transform, false );
                    seq.onEvent += OnSequenceEvent;
                m_ActiveSequence = new SequenceMeta( name, seq );
                m_ActiveSequence.sequence.Run();
            }
        }
        /// <summary>
        /// Prematurely kill the currently active Sequence.
        /// </summary>
        public void KillSequence(string name)
        {
            if(m_ActiveSequence != null) {
                m_ActiveSequence.sequence.Kill();
            }
            DestroySequence();
        }


    }
}
