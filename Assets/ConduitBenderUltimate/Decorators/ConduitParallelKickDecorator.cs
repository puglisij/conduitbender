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
        var cBend = m_Conduit.centerline;
        var cBendIndices = m_Conduit.centerlineBendIndices;

        float spacingM = (float) bend.GetInputParameter(BendParameter.Name.Spacing).value;
        float shiftM = (float) bend.GetOutputParameter(BendParameter.Name.Shift).value;
        float travelM = (float) bend.GetOutputParameter(BendParameter.Name.KickTravel).value;
        float firstMarkM = (float) bend.GetOutputParameter(BendParameter.Name.KickFirstMark).value;

        // Get Start and End Centerline Indices
        int cs90_i = cBendIndices[2].index;
        int ce90_i = cBendIndices[3].index;
        int csKick_i = cBendIndices[0].index;
        int ceKick_i = cBendIndices[1].index;
        Vector3 cs90 = cBend[ cs90_i ].point;

        // Have the Bends Changed?
        float angleDeg = (float) bend.inputParameters[0].value;
        //if(angleDeg != lastAngleDeg) 
        //{
            //UnityEngine.Debug.Log( "ConduitParallelKickDecorator: Re-Copying Mesh. Bend Name: " + m_Conduit.bend.modelName );

            // Re-Copy Split Mesh
            ConduitGenerator.CopyLastConduitPartial( bendKickMesh, 0, cs90_i );
            // @TODO - We Don't need to Re-Copy the 90 every time, we could just rotate a Clone as needed
            ConduitGenerator.CopyLastConduitPartial( bend90Mesh, cs90_i, cBend.Count - 1 ); 

            // Record Start Position of Kick
            bend90Adjust = cs90;
            bend90Forward = cBend[ cs90_i ].forwardDir;

            lastAngleDeg = angleDeg;
        //}

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
        Vector3 staShiftLine = cs90;
        Vector3 endShiftLine = cs90 + shiftM * bend90Forward;
        Vector3 tStart = cBend[ csKick_i ].point;
        Vector3 fmStart = cBend[ ceKick_i ].point + cBend[ ceKick_i ].radialDir * m_Conduit.conduitDiameterM;
        m_ShiftLine.Draw( staShiftLine, endShiftLine );
        m_FirstMarkLine.Draw( fmStart, fmStart + cBend[ ceKick_i ].forwardDir * firstMarkM );
        m_TravelLine.Draw( tStart, tStart + cBend[ csKick_i ].forwardDir * travelM );
    }

    public override void OnRemove()
    {
        // Undo any changes made to m_Conduit Transform
        m_Conduit.transform.localPosition = Vector3.zero;
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

        m_ShiftLine = FlagRenderer.NewLine( transform );
        m_ShiftLine.SetWidth( lineWidth );

        m_FirstMarkLine = FlagRenderer.NewLine( m_Conduit.transform );
        m_FirstMarkLine.SetWidth( lineWidth );

        m_TravelLine = FlagRenderer.NewLine( m_Conduit.transform );
        m_TravelLine.SetWidth( Engine.conduitDiameterM * 0.5f );

        // Set Marker Colors
        var bend = m_Conduit.bend;
        m_ShiftLine.SetColor( bend.GetOutputParameter(BendParameter.Name.Shift).color );
        m_FirstMarkLine.SetColor( bend.GetOutputParameter( BendParameter.Name.KickFirstMark ).color );
        m_TravelLine.SetColor( bend.GetOutputParameter( BendParameter.Name.KickTravel ).color );

        if (m_Conduit.centerlineBendIndices.Count == 4) {
            UnityEngine.Debug.Log( "ConduitParallelKickDecorator: Set() Decorating..." );
            lastAngleDeg = -1;
            Decorate();
        }
    }

    public override void Highlight()
    {
        throw new NotImplementedException();
    }
}
