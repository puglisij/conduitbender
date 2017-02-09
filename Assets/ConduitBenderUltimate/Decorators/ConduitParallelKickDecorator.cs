using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;

public class ConduitParallelKickDecorator : AConduitDecorator
{
    /// <summary>
    /// Notes:
    ///     1)  Assumes Parent Scale is 1,1,1
    /// </summary>
    public UnityEngine.Rendering.ShadowCastingMode  castShadows;

    public Material     material;

    public bool         receiveShadows = false;
    public bool         useLightProbes = false;
    

    //-------------------------
    //      Private Data
    //-------------------------
    GameObject  m_P90Conduit;
    GameObject  m_PKickConduit;
    LineFlag    m_SpreadLine;
    LineFlag    m_ShiftLine;
    LineFlag    m_FirstMarkLine;
    LineFlag    m_TravelLine;

    Mesh    bend90Mesh;
    Mesh    bendKickMesh;

    Vector3 bend90Adjust;
    Vector3 bend90Forward;

    float   lastAngleDeg = -1.0f;

    void Initialize()
    {
        // Ensure Decorator is Positioned at 0
        transform.localPosition = Vector3.zero;

        // Create Split Mesh Objects
        m_P90Conduit = new GameObject( "ParallelConduit90");
        m_P90Conduit.transform.SetParent( transform );
        m_P90Conduit.transform.localPosition = Vector3.zero;

        m_PKickConduit = new GameObject( "ParalleConduitKick");
        m_PKickConduit.transform.SetParent( transform );
        m_PKickConduit.transform.localPosition = Vector3.zero;

        MeshRenderer    p90Renderer = m_P90Conduit.AddComponent<MeshRenderer>();
            p90Renderer.shadowCastingMode = castShadows;
            p90Renderer.receiveShadows = receiveShadows;
            p90Renderer.material = material;
            p90Renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes; 

        MeshRenderer    pKickRenderer = m_PKickConduit.AddComponent<MeshRenderer>();
            pKickRenderer.shadowCastingMode = castShadows;
            pKickRenderer.receiveShadows = receiveShadows;
            pKickRenderer.material = material;
            pKickRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;

        MeshFilter      p90Filter = m_P90Conduit.AddComponent<MeshFilter>();
            bend90Mesh = p90Filter.mesh;
        MeshFilter      pKickFilter = m_PKickConduit.AddComponent<MeshFilter>();
            bendKickMesh = pKickFilter.mesh;


        UnityEngine.Debug.Log( "ConduitParallelKickDecorator: Initialize()" );
    }

    public override void Decorate()
    {
        Bend bend = m_Conduit.bend;

        // Get Values
        var centerline = m_Conduit.centerline;
        var bendIndices = m_Conduit.centerlineBendIndices;

        float spacingM = (float) bend.GetInputParameter(EBendParameterName.Spacing).value;
        float spreadM = (float) bend.GetOutputParameter(EBendParameterName.KickSpread).value;
        float shiftM = (float) bend.GetOutputParameter(EBendParameterName.Shift).value;
        float travelM = (float) bend.GetOutputParameter(EBendParameterName.KickTravel).value;
        float firstMarkM = (float) bend.GetOutputParameter(EBendParameterName.KickFirstMark).value;

        // Get Start and End Centerline Indices
        int cs90_i = bendIndices[2].index;
        int ce90_i = bendIndices[3].index;
        int csKick_i = bendIndices[0].index;
        int ceKick_i = bendIndices[1].index;
        Vector3 cs90 = centerline[ cs90_i ].point;

        // Have the Bends Changed?
        float angleDeg = (float) bend.inputParameters[0].value;

        // Re-Copy Split Mesh
        ConduitGenerator.CopyLastConduitPartial( bendKickMesh, 0, ceKick_i );
        // OPT: We Don't need to Re-Copy the 90 every time, we could just rotate a Clone as needed
        ConduitGenerator.CopyLastConduitPartial( bend90Mesh, ceKick_i, centerline.Count - 1 ); 

        // Record Start Position of Kick
        bend90Adjust = cs90;
        bend90Forward = centerline[ cs90_i ].forwardDir;

        lastAngleDeg = angleDeg;

        //----------------
        // Set Positions
        //----------------
        // Space & Slide Conduit
        Vector3 conduitOrig = m_Conduit.transform.localPosition;
                conduitOrig.y = spacingM;
        m_Conduit.transform.localPosition = conduitOrig;

        // Slide & Shift Parallel Conduit 
        float partialShift = (spacingM * Mathf.Tan( angleDeg * Mathf.Deg2Rad * 0.5f ));
        Vector3 parallelOrig = conduitOrig;
                parallelOrig.y = -spacingM;
                parallelOrig.z = partialShift;  // a 'part' of the total ShiftM
        transform.localPosition = parallelOrig;

        // Shift Parallel 90
        m_P90Conduit.transform.localPosition = (cs90 - bend90Adjust) + bend90Forward * (shiftM);

        //----------------
        // Draw Lines
        //----------------
        Vector3 startSpread = centerline[ ce90_i ].point;
                startSpread.y += Engine.conduitDiameterM * 0.5f;
        Vector3 endSpread = startSpread + m_Conduit.transform.forward * spreadM;
        Vector3 startShift = centerline[ ceKick_i ].point;
        Vector3 endShift = centerline[ ceKick_i ].point + shiftM * bend90Forward;
        Vector3 startTravel = centerline[ csKick_i ].point;
        Vector3 startFirstMark = centerline[ ceKick_i ].point + centerline[ ceKick_i ].radialDir * m_Conduit.conduitDiameterM;

        m_SpreadLine.Draw( startSpread, endSpread );
        m_ShiftLine.Draw( startShift, endShift );
        m_FirstMarkLine.Draw( startFirstMark, startFirstMark + centerline[ ceKick_i ].forwardDir * firstMarkM );
        m_TravelLine.Draw( startTravel, startTravel + centerline[ csKick_i ].forwardDir * travelM );
    }

    public override void OnRemove()
    {
        // Undo any changes made to m_Conduit Transform
        m_Conduit.transform.localPosition = Vector3.zero;

        // Destroy flags that were parented to m_Conduit
        Destroy( m_SpreadLine.gameObject );
        Destroy( m_FirstMarkLine.gameObject );
        Destroy( m_TravelLine.gameObject );
    }

    public override void Set( Conduit conduit )
    {
        Initialize();
        m_Conduit = conduit;

        //----------------------
        // Create Line Flags
        //----------------------
        float lineWidth = Engine.conduitDiameterM * Mathf.Sin(Mathf.PI * 0.25f);

        m_SpreadLine = FlagRenderer.NewLine( m_Conduit.transform );
        m_SpreadLine.SetWidth( lineWidth );

        m_ShiftLine = FlagRenderer.NewLine( transform );
        m_ShiftLine.SetWidth( lineWidth );

        m_FirstMarkLine = FlagRenderer.NewLine( m_Conduit.transform );
        m_FirstMarkLine.SetWidth( lineWidth );

        m_TravelLine = FlagRenderer.NewLine( m_Conduit.transform );
        m_TravelLine.SetWidth( Engine.conduitDiameterM * 0.5f );

        // Set Marker Colors
        var bend = m_Conduit.bend;
        m_SpreadLine.SetColor( bend.GetOutputParameter( EBendParameterName.KickSpread ).color );
        m_ShiftLine.SetColor( bend.GetOutputParameter(EBendParameterName.Shift).color );
        m_FirstMarkLine.SetColor( bend.GetOutputParameter( EBendParameterName.KickFirstMark ).color );
        m_TravelLine.SetColor( bend.GetOutputParameter( EBendParameterName.KickTravel ).color );

        if (m_Conduit.centerlineBendIndices.Count == 4) {
            UnityEngine.Debug.Log( "ConduitParallelKickDecorator: Set() Decorating..." );
            lastAngleDeg = -1;
            Decorate();
        }
    }

    public override void Highlight()
    {
        var bend = m_Conduit.bend;
        var highlight = bend.GetHighlight();
        var highlightColor = highlight.color;

        m_Conduit.SetHighlightColor( highlightColor );

        UnityEngine.Debug.Assert( highlight.enabled );

        // Which parameter to highlight?
        if (highlight.name == EBendParameterName.Alternate1stTo2nd) {
            var start = m_Conduit.centerlineBendIndices[0];
            var end   = m_Conduit.centerlineBendIndices[2];

            ConduitGenerator.ColorConduit( m_Conduit, highlightColor, start.index, end.index );
        } 
    }
}
