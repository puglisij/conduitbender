using UnityEngine;
using System.Collections;
using System;

public class ConduitSaddle4Decorator : AConduitDecorator
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
        if (highlight.name == BendParameter.Name.DistanceBetween) {
            var start = m_Conduit.centerlineBendIndices[0];
            var end   = m_Conduit.centerlineBendIndices[3];

            Debug.Assert( start.type == BendMarkType.Start );
            Debug.Assert( end.type == BendMarkType.End );

            ConduitGenerator.ColorConduit( m_Conduit, highlightColor, start.index, end.index );
        } else if(highlight.name == BendParameter.Name.Distance2ndTo3rd) {
            var start = m_Conduit.centerlineBendIndices[2];
            var end = m_Conduit.centerlineBendIndices[4];

            Debug.Assert( start.type == BendMarkType.Start );
            Debug.Assert( end.type == BendMarkType.Start );

            ConduitGenerator.ColorConduit( m_Conduit, highlightColor, start.index, end.index );
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
