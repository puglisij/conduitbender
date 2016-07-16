using UnityEngine;
using System.Collections;
using System;

public class ConduitRolledOffsetDecorator : AConduitDecorator
{
    public Transform      sectorObject;

    private MeshFilter     sectorMeshFilter;
    private Mesh           sectorMesh;

    private LineFlag       m_RollLine;
    private LineFlag       m_RiseLine;

    private bool           m_LinesEnabled = false;
    private float          m_LineWidth;

    void Awake()
    {
        sectorMeshFilter = sectorObject.GetComponent<MeshFilter>();
        sectorMesh = sectorMeshFilter.mesh;

        // Rotate Towards Camera
        sectorObject.localRotation = Quaternion.Euler( 0f, 180f, 0f );
        
    }

    private void EnableLines(bool enable)
    {
        m_RollLine.gameObject.SetActive( enable );
        m_RiseLine.gameObject.SetActive( enable );
        m_LinesEnabled = enable;
    }

    public override void Decorate()
    {
        Bend bend = m_Conduit.bend;

        // Get Roll Angle
        float rollAngleDeg = (float) bend.GetOutputParameter(BendParameter.Name.RollAngleDegrees).value;
        float riseM = (float) bend.GetInputParameter(BendParameter.Name.Rise).value;
        float rollM = (float) bend.GetInputParameter(BendParameter.Name.Roll).value;
        var centerline = m_Conduit.centerline;
        var indices = m_Conduit.centerlineBendIndices;

        if (rollAngleDeg == 0f || indices.Count == 0 || bend.alert != null) {
            // Disable Lines
            EnableLines( false );
            return;
        }
        if(!m_LinesEnabled) {
            EnableLines( true );
        }

        // Get Points
        Vector3 csFirst = centerline[ indices[0].index ].point;
        Vector3 ceSecond = centerline[ indices[3].index ].point;
        Vector3 cEnd = centerline[ centerline.Count - 1 ].point;

        Vector3 origin = Vector3.zero;
                origin.z = cEnd.z;
        Vector3 riseOrigin = origin;
                riseOrigin.y -= m_LineWidth * 0.5f;
        // Generate Circle Sector Mesh
        //ConduitGenerator.GenerateSector( sectorMesh, riseM, rollAngleDeg, 5f );
        m_RiseLine.Draw( riseOrigin, riseOrigin + Vector3.up * riseM );
        m_RollLine.Draw( origin, origin + Vector3.left * rollM );

        // Positions
        //sectorObject.localPosition = origin;
        m_Conduit.transform.rotation = Quaternion.Euler( 0f, 0f, rollAngleDeg );
        transform.localRotation = Quaternion.Euler( 0f, 0f, 360f - rollAngleDeg ); // Undo Rotation (since we're child of Conduit)
    }
    public override void OnRemove()
    {
        // Reset Rotation
        m_Conduit.transform.rotation = Quaternion.Euler( 0f, 0f, 0f );
    }
    public override void Set( Conduit conduit )
    {
        m_Conduit = conduit;

        //----------------------
        // Create Line Flags
        //----------------------
        var bend = m_Conduit.bend;
        m_LineWidth = Engine.conduitDiameterM * Mathf.Sin( Mathf.PI * 0.25f );

        m_RollLine = FlagRenderer.NewLine( transform );
        m_RollLine.SetWidth( m_LineWidth * 0.7f );
        m_RollLine.SetColor( bend.GetInputParameter( BendParameter.Name.Roll ).color );

        m_RiseLine = FlagRenderer.NewLine( transform );
        m_RiseLine.SetWidth( m_LineWidth * 0.75f );
        m_RiseLine.SetColor( bend.GetInputParameter( BendParameter.Name.Rise ).color );
        EnableLines( false );

        // Rotate Conduit
        Decorate();
    }
}
