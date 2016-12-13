using UnityEngine;
using System.Collections;
using System;

public class ConduitStubUpDecorator : AConduitDecorator
{
    private LineFlag    m_TakeUpLine;

    private float       m_LineWidth;

    public override void Decorate()
    {
        var centerline = m_Conduit.centerline;
        var start90_i = m_Conduit.centerlineBendIndices[ 0 ].index;
        var end90_i = m_Conduit.centerlineBendIndices[ 1 ].index;

        var endTakeUp = centerline[ end90_i ].point + transform.forward * (Engine.conduitDiameterM + m_LineWidth) * 0.5f;
        var startTakeUp = endTakeUp;
            startTakeUp.y = centerline[ start90_i ].point.y;
        m_TakeUpLine.Draw( startTakeUp, endTakeUp );
    }

    public override void Highlight()
    {
        var bend = m_Conduit.bend;
        var highlight = bend.GetHighlight();
        var highlightColor = highlight.color;

        m_Conduit.SetHighlightColor( highlightColor );

        Debug.Assert( highlight.enabled );

        // Which parameter to highlight?
        if (highlight.name == EBendParameterName.DistanceFromEnd) {
            var start = m_Conduit.centerlineBendIndices[1];

            ConduitGenerator.ColorConduit( m_Conduit, highlightColor, start.index, m_Conduit.centerline.Count - 1 );
        } else if(highlight.name == EBendParameterName.LengthOfBend) {
            var start = m_Conduit.centerlineBendIndices[0];
            var end = m_Conduit.centerlineBendIndices[1];

            ConduitGenerator.ColorConduit( m_Conduit, highlightColor, start.index, end.index );
        }
    }

    public override void OnRemove()
    {
 
    }

    public override void Set( Conduit conduit )
    {
        m_Conduit = conduit;

        var bend = m_Conduit.bend;
        //----------------------
        // Create Line Flags
        //----------------------
        m_LineWidth = Engine.conduitDiameterM * Mathf.Sin( Mathf.PI * 0.25f );

        m_TakeUpLine = FlagRenderer.NewLine( transform );
        m_TakeUpLine.SetWidth( m_LineWidth );
        m_TakeUpLine.SetColor( bend.GetOutputParameter( EBendParameterName.StubTakeUp ).color );
    }
}
