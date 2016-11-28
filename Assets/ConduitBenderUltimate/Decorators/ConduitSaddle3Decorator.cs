using UnityEngine;
using System.Collections.Generic;
using System;

public class ConduitSaddle3Decorator : AConduitDecorator
{
    int [] bendIndices = new int[6];
    Vector3[] bendPoints = new Vector3[3];
    GameObject[] rays = new GameObject[3];


    private void ToggleRays(bool on)
    {
        for (int r = 0; r < rays.Length; ++r) {
            rays[ r ].gameObject.SetActive( on );
        }
    }
    public override void Decorate()
    {
        // Find Centerline Bend Marks
        var centerline = m_Conduit.centerline;
        var indices = m_Conduit.centerlineBendIndices;
        int bi = 0;
        for (int i = 0; i < indices.Count; ++i) {
            if (indices[ i ].type == BendMarkType.Start || indices[ i ].type == BendMarkType.End) {
                bendIndices[ bi++ ] = indices[ i ].index;
            }
        }

        if (bi < 6) {
            // Rays Off
            ToggleRays( false );
            return;
        }
        ToggleRays( true );

        // Arrow locations are dependent on Bend Method
        var bend = m_Conduit.bend;
        var bendMethod = (GlobalEnum.ESaddle3BendMethod) bend.GetInputParameter(BendParameter.Name.Saddle3Method).value;

        if(bendMethod == GlobalEnum.ESaddle3BendMethod.Notch) 
        {
            // Determine Points
            int centerOfCenterBend = (bendIndices[3] - bendIndices[2]) / 2 + bendIndices[2];
            Vector3 bend1Dir = centerline[ bendIndices[ 1 ] ].radialDir;
            Vector3 bend2Dir = transform.up;
            Vector3 bend3Dir = centerline[ bendIndices[ 4 ] ].radialDir;

            bendPoints[ 0 ] = m_Conduit.transform.TransformPoint( centerline[ bendIndices[ 1 ] ].point ) + bend1Dir * Engine.conduitDiameterM * 0.5f;
            bendPoints[ 1 ] = m_Conduit.transform.TransformPoint( centerline[ centerOfCenterBend ].point ) + bend2Dir * Engine.conduitDiameterM * 0.5f;
            bendPoints[ 2 ] = m_Conduit.transform.TransformPoint( centerline[ bendIndices[ 4 ] ].point ) + bend3Dir * Engine.conduitDiameterM * 0.5f;

            // Draw Arrows
            FlagRenderer.DrawRay( rays[ 0 ], -bend1Dir, bendPoints[ 0 ] );
            FlagRenderer.DrawRay( rays[ 1 ], -bend2Dir, bendPoints[ 1 ] );
            FlagRenderer.DrawRay( rays[ 2 ], -bend3Dir, bendPoints[ 2 ] );
        } 
        else if(bendMethod == GlobalEnum.ESaddle3BendMethod.Arrow)
        {

        }
    }

    public override void Highlight()
    {
        var bend = m_Conduit.bend;
        var highlight = bend.GetHighlight();
        var highlightColor = highlight.color;

        m_Conduit.SetHighlightColor( highlightColor );

        Debug.Assert( highlight.enabled );

        // Which parameter to highlight?
        if (highlight.name == BendParameter.Name.DistanceBetween) {
            var start = m_Conduit.centerlineBendIndices[0];
            var end   = m_Conduit.centerlineBendIndices[2];

            ConduitGenerator.ColorConduit( m_Conduit, highlightColor, start.index, end.index );
        } else if (highlight.name == BendParameter.Name.Distance1stTo2nd) {
            var start = m_Conduit.centerlineBendIndices[1];
            var end   = m_Conduit.centerlineBendIndices[2];

            ConduitGenerator.ColorConduit( m_Conduit, highlightColor, start.index, end.index );
        } else if(highlight.name == BendParameter.Name.Distance2ndTo3rd) {
            var start = m_Conduit.centerlineBendIndices[2];
            var end   = m_Conduit.centerlineBendIndices[4];

            ConduitGenerator.ColorConduit( m_Conduit, highlightColor, start.index, end.index );
        } else if (highlight.name == BendParameter.Name.LengthOfCenterBend) {
            var start = m_Conduit.centerlineBendIndices[2];
            var end = m_Conduit.centerlineBendIndices[3];

            ConduitGenerator.ColorConduit( m_Conduit, highlightColor, start.index, end.index );
        }
    }

    public override void OnRemove()
    {

    }

    public override void Set( Conduit conduit )
    {
        m_Conduit = conduit;

        // Create Rays
        rays[ 0 ] = FlagRenderer.NewRay( transform );
        rays[ 1 ] = FlagRenderer.NewRay( transform );
        rays[ 2 ] = FlagRenderer.NewRay( transform );

        // Position Conduit
        Decorate();
    }
}
