using UnityEngine;
using System.Collections;
using System;

public class ConduitOffsetDecorator : AConduitDecorator
{
    public override void Decorate()
    {
  
    }

    public override void Highlight()
    {
        var bend = m_Conduit.bend;
        var highlight = bend.GetHighlight();
        var highlightColor = highlight.color;

        m_Conduit.SetHighlightColor( highlightColor );

        // Which parameter to highlight?
        if(highlight.name == BendParameter.Name.DistanceBetween) {
            var start = m_Conduit.centerlineBendIndices[0];
            var end   = m_Conduit.centerlineBendIndices[3];

            Debug.Assert( start.type == BendMarkType.Start );
            Debug.Assert( end.type == BendMarkType.End );
            
            ConduitGenerator.ColorConduit( m_Conduit, highlightColor, start.index, end.index );

            //Debug.Log( "ConduitOffsetDecorator: Highlight() Distance Between parameter." );
        }

    }

    public override void OnRemove()
    {
     
    }

    public override void Set( Conduit conduit )
    {
        m_Conduit = conduit;

    }
}