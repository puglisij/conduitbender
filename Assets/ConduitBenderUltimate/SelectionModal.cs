using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

//using PresetType = SettingPresets.Preset;
//using SetType = SettingPresets.PresetSet;

namespace CB
{
    public class SelectionModal : AModal
    {
        public delegate void OnSelect( object value, int[] route );

        //-------------------------
        //      Public Data
        //-------------------------
        public Text headerText;

        public RectTransform scrollContent;
        public RectTransform selectionPrefab; // The Selection Prefab

        /// <summary>
        /// Callback to receive value final selection event. The delegate also receives an int[] array 
        /// representing the sequence of indexes to reach the selection.
        /// For example, [1, 3] for the 4th Selection within the 2nd Set.
        /// </summary>
        public OnSelect onSelect;

        public override string modalTitle
        {
            get { return m_ModalTitle; }
            set
            {
                m_ModalTitle = value;
                headerText.text = m_ModalTitle;
            }
        }
        //-------------------------
        //      Private Data
        //-------------------------
        bool m_isVisualsDirty = false;

        /// <summary>
        /// The current index selection route to get to the current set or leaf set value.
        /// </summary>
        int m_route = -1;

        /// <summary> The original key-value set array. </summary>
        KeyValueSet<object>[] m_set = null;


        void Update()
        {
            if (m_isVisualsDirty) {

                m_isVisualsDirty = false;
            }
        }

        void DrawSelections()
        {
            var options = CurrentSelections();

            if (options.Count == 0) {
                Debug.Log( "SettingPresetModal: DrawSelections() No options to draw in the current set." );
                return;
            }

            // Clear previous selections 
            scrollContent.DestroyChildren();

            // Loop through presets, create prefab, add to scroll content          
            RectTransform prefab = (RectTransform)Instantiate( selectionPrefab, scrollContent, false );
            Vector2 size = prefab.rect.size;
            Button btn;
            Text txt;

            scrollContent.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, options.Count * size.y );

            for (var p = 0; true; ++p) {
                int index = p;

                prefab.SetInsetAndSizeFromParentEdge( RectTransform.Edge.Top, size.y * p, size.y );
                txt = prefab.GetComponentInChildren<Text>( true );
                txt.text = options[ p ];
                btn = prefab.GetComponentInChildren<Button>( true );
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener( () => { OnSelection( index ); } );

                if (p + 1 < options.Count) {
                    prefab = (RectTransform)Instantiate( selectionPrefab, scrollContent, false );
                } else {
                    break;
                }
            }
        }

        List<string> CurrentSelections()
        {
            var options = new List<string>();

            if(m_set.Length == 1) {
                m_route = 0;
            }

            if (m_route == -1) {
                // Non-Leaf Level
                for (var s = 0; s < m_set.Length; ++s) {
                    options.Add( m_set[ s ].title );
                }
            } else {
                // Leaf Level
                var keyValues = m_set[m_route].set;
                for (var i = 0; i < keyValues.Length; ++i) {
                    options.Add( keyValues[ i ].title );
                }
            }

            return options;
        }

        void OnSelection( int index )
        {
            KeyValue<object> selected;

            if(m_route != -1) {
                // Leaf (final) selection
                selected = m_set[ m_route ].set[ index ];
                FireSelect( selected.value, new int[] { m_route, index } );
            } else {
                m_route = index;
                DrawSelections();
            }
        }

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

        public void OnBack()
        {
            m_route = -1;

            DrawSelections();
        }

        /// <summary>
        /// Fires selection event with the value at the given preset index
        /// </summary>
        public void FireSelect( object value, int[] route )
        {
            if (onSelect != null) {
                onSelect( value, route == null ? null : route);
            }
        }

        public void SetSelections( KeyValueSet<object>[] sets )
        {
            if (sets == null || sets.Length == 0) { return; }

            m_route = -1;
            m_set = sets;

            DrawSelections();
        }

    }
}
