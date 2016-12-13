using UnityEngine;
using System.Collections;
using System;

public class ConduitSegmentedDecorator : AConduitDecorator
{
    public Transform      sectorObject;

    private MeshFilter     sectorMeshFilter;
    private Mesh           sectorMesh;

    private bool           m_SectorEnabled = false;

    void Awake()
    {
        sectorMeshFilter = sectorObject.GetComponent<MeshFilter>();
        sectorMesh = sectorMeshFilter.mesh;
        EnableSector( false );
        MeshRenderer sectorRenderer = sectorObject.GetComponent<MeshRenderer>();
        Color color = Colors.instance.flagBlue;
        sectorRenderer.material.color = color;
        sectorRenderer.material.SetColor( "_EmissionColor", color );

        // Rotate Towards Camera
        sectorObject.localRotation = Quaternion.Euler( 0f, 90f, 180f );
    }

    private void EnableSector(bool enable)
    {
        sectorObject.gameObject.SetActive( enable );
        m_SectorEnabled = enable;
    }

    public override void Decorate()
    {
        Bend bend = m_Conduit.bend;
        // Get Segmented Radius
        float radiusM = (float) bend.GetInputParameter(EBendParameterName.SegmentedRadius).value;
        float angleDeg = (float) bend.GetInputParameter(EBendParameterName.SegmentedAngle).value;
        var centerline = m_Conduit.centerline;
        var indices = m_Conduit.centerlineBendIndices;

        if (radiusM == 0f || angleDeg == 0f || indices.Count == 0 || bend.alert != null) {
            EnableSector( false );
            return;
        }
        if(!m_SectorEnabled) {
            EnableSector( true );
        }

        // Get Bend Start Positions
        int start_i = indices[0].index;

        // Generate Circle Sector Mesh
        ConduitGenerator.GenerateSector( sectorMesh, radiusM, angleDeg, 5f );

        // Set Position
        
        Vector3     center = centerline[ start_i ].point + centerline[ start_i ].radialDir * radiusM;
        sectorObject.localPosition = center;
    }

    public override void OnRemove()
    {
        
    }

    public override void Set( Conduit conduit )
    {
        m_Conduit = conduit;

        Decorate();
    }

    public override void Highlight()
    {
        var bend = m_Conduit.bend;
        var highlight = bend.GetHighlight();
        var highlightColor = highlight.color;

        m_Conduit.SetHighlightColor( highlightColor );

        Debug.Assert( highlight.enabled );

        // Which parameter to highlight?
        if (highlight.name == EBendParameterName.DistanceBetween) {
            var start = m_Conduit.centerlineBendIndices[2];
            var end = m_Conduit.centerlineBendIndices[4];

            ConduitGenerator.ColorConduit( m_Conduit, highlightColor, start.index, end.index );
        } else if(highlight.name == EBendParameterName.DistanceTo2nd) {
            var start = m_Conduit.centerlineBendIndices[0];
            var end = m_Conduit.centerlineBendIndices[2];

            ConduitGenerator.ColorConduit( m_Conduit, highlightColor, start.index, end.index );
        }
    }
}
