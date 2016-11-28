using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class BendHelp : Widget
{
    private const string k_AvailableMessage = "Select a Parameter to Highlight.";
    private const string k_UnAvailableMessage = "Sorry. No Parameters can be Highlighted on this Bend.";

    public delegate void BendHelpEvent( BendParameter value );

    //--------------------------
    //      Public Data
    //--------------------------
    public BendHelpEvent onValueChanged
    {
        get { return m_OnValueChanged; }
        set { m_OnValueChanged = value; }
    }

    public Text message;

    public ScrollRect parameterScroller;

    public RectTransform parameterPrefab;
    //-------------------------------
    //      Private Data
    //-------------------------------
    [SerializeField]
    private BendHelpEvent m_OnValueChanged = null;

    /// <summary> The Bend parameters which can be highlighted. </summary>
    private List<BendParameter> m_highlightables;

    /// <summary> The original color of the current selected parameter </summary>
    private Color m_selectedOrigColor;

    /// <summary> The current Enumerator </summary>
    private int m_selectedIndex = -1;

    public void UnSelect()
    {
        if(m_selectedIndex == -1) {
            return;
        }
        var paramPrefab = parameterScroller.content.GetChild(m_selectedIndex);
            paramPrefab.GetComponent<Text>().color = m_selectedOrigColor;

        m_selectedIndex = -1;
    }

    public void SetHighlightables(List<BendParameter> highlightables)
    {
        m_highlightables = highlightables;

        // Clear previous Parameters
        var content = parameterScroller.content;
        for (int i = 0; i < content.childCount; ++i) {
            Destroy( content.GetChild( i ).gameObject );
        }

        if (highlightables.Count == 0) {
            message.text = k_UnAvailableMessage;
            return;
        } else {
            message.text = k_AvailableMessage;
        }

        // Populate Parameter view
        RectTransform scrollerContent = parameterScroller.content;
        RectTransform paramPrefab;
        Button        paramButton;
        Text          paramText;
        Vector2       nextPos = parameterPrefab.anchoredPosition;
        Vector2       size = parameterPrefab.rect.size;

        for (var i = 0; i < m_highlightables.Count; ++i) 
        {
            var param = m_highlightables[i];
            var index = i;
            // Instantiate next 
            paramPrefab = Instantiate( parameterPrefab );

            // Position, Format and Listen
            paramButton = paramPrefab.GetComponent<Button>();
            paramButton.onClick.AddListener( () => { OnParameterClick( index ); } );

            paramText = paramPrefab.GetComponent<Text>();
            paramText.text = BendParameter.GetStringValue( param.name );
            paramText.color = Color.black;

            paramPrefab.SetParent( scrollerContent, false );
            paramPrefab.anchoredPosition = nextPos;
            paramPrefab.gameObject.SetActive( true );

            nextPos.y -= size.y;
        }
    }

    private void OnParameterClick(int index)
    {
        //if (m_selectedIndex == index) { return; }
        UnSelect();

        var selected    = m_highlightables[ index ];
        var paramPrefab = parameterScroller.content.GetChild(index);
        var paramText   = paramPrefab.GetComponent<Text>();

        m_selectedIndex     = index;
        m_selectedOrigColor = paramText.color; 
        paramText.color     = selected.color;

        if (m_OnValueChanged != null) {
            m_OnValueChanged( selected );
        }
        Debug.Log( "BendHelp: OnParameterClick() index: " + index );
    }

}
