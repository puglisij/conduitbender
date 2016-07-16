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
        float radiusM = (float) bend.GetInputParameter(BendParameter.Name.SegmentedRadius).value;
        float angleDeg = (float) bend.GetInputParameter(BendParameter.Name.SegmentedAngle).value;
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
}
