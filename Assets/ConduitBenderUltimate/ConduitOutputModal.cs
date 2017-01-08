using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

namespace CB
{
    /// <summary>
    /// Not using the Animator for this Modal as it immediately appears / disappears.
    /// </summary>
    public class ConduitOutputModal : AModal
    {
        //-------------------------
        //      Public Data
        //-------------------------
        public Text headerText;
        public Text outputText;

        public RectTransform outputTextBounds;

        //public TextExtra textMetrics;

        //[Tooltip("Used to calculate maximum font size")]
        //public int maxCharPerLine;

        public override string modalTitle
        {
            get { return m_ModalTitle; }
            set { m_ModalTitle = value;
                headerText.text = m_ModalTitle;
            }
        }
        //-------------------------
        //      Private Data
        //-------------------------
        private int m_BestFontSize;

        private string m_OutputText = "";

        private bool m_VisualsDirty = false;

        protected override void Awake()
        {
            base.Awake();

        }
        protected override void OnEnable()
        {
            base.OnEnable();

        }
        protected override void OnDisable()
        {
            base.OnDisable();

        }
        protected virtual void Start()
        {
            // Figure best Font Size
            //CalculateTextSizes();

        }


        void Update()
        {
            if (m_VisualsDirty) {
                outputText.text = m_OutputText;

                m_VisualsDirty = false;
            }
        }

        /*##########################################

                    Private Functions

        ###########################################*/

        //private void CalculateTextSizes()
        //{
        //    m_BestFontSize = textMetrics.CalculateBestFontSize( outputText, outputTextBounds, maxCharPerLine );
        //    outputText.fontSize = m_BestFontSize;

        //    Debug.Log( "ConduitOutputModal: CalculateTextSizes() Best size: " + m_BestFontSize );
        //}

        /*##########################################

                    Public Functions

        ###########################################*/

        public override void Close( bool doDisable )
        {
            m_IsOpen = false;
            gameObject.SetActive( false );
        }
        public override void Open()
        {
            m_IsOpen = true;
            gameObject.SetActive( true );
        }

        /// <summary>
        /// Writes the string to the output Text
        /// </summary>
        public void WriteLines( string lines )
        {
            m_OutputText = lines;
            m_VisualsDirty = true;
        }
    }
}

