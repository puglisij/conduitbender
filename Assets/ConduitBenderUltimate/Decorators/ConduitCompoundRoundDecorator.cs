using UnityEngine;
using System.Collections;

public class ConduitCompoundRoundDecorator : AConduitDecorator
{
    const float k_objstacleLength = 0.5f;

    public Material  material;

    GameObject  m_obstacle;
    LineFlag    m_travelLine1;
    LineFlag    m_travelLine2;

    public override void Decorate()
    {
        var bend = m_Conduit.bend;
        var centerline = m_Conduit.centerline;
        var indices = m_Conduit.centerlineBendIndices;
        var conduitRadius = Engine.conduitDiameterM * 0.5f;

        var travel = (float) bend.GetOutputParameter(EBendParameterName.Travel).value;
        var oDiam = (float) bend.GetInputParameter(EBendParameterName.Diameter).value;
        var oRadius = 0.5f * oDiam;

        // Set Obstacle Position
        Vector3 oPos;
                oPos.x = -k_objstacleLength;
                oPos.y = oRadius - conduitRadius;
                oPos.z = centerline[ indices[ 3 ].index ].point.z - oRadius + conduitRadius;
        m_obstacle.transform.localScale = new Vector3(oDiam, k_objstacleLength, oDiam);
        m_obstacle.transform.localPosition = oPos;

        // Set Flag Positions
        Vector3 startLine1 = centerline[ indices[0].index ].point;
                startLine1.y -= Engine.conduitDiameterM;
        Vector3 startLine2 = centerline[ indices[3].index ].point;
                startLine2.z += Engine.conduitDiameterM;
        Vector3 endLine1 = startLine1;
                endLine1.z += travel;
        Vector3 endLine2 = startLine2;
                endLine2.y -= travel;
        m_travelLine1.Draw( startLine1, endLine1 );
        m_travelLine2.Draw( startLine2, endLine2 );
    }

    public override void Highlight()
    {
        var bend = m_Conduit.bend;
        var highlight = bend.GetHighlight();
        var highlightColor = highlight.color;

        m_Conduit.SetHighlightColor( highlightColor );

        // Which parameter to highlight?
        if (highlight.name == EBendParameterName.DistanceBetween) {
            var start = m_Conduit.centerlineBendIndices[0];
            var end   = m_Conduit.centerlineBendIndices[2];

            ConduitGenerator.ColorConduit( m_Conduit, highlightColor, start.index, end.index );
        } else if(highlight.name == EBendParameterName.LengthOfBend) {
            var start = m_Conduit.centerlineBendIndices[0];
            var end   = m_Conduit.centerlineBendIndices[1];

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
        var travelColor = bend.GetOutputParameter( EBendParameterName.Travel ).color;

        // Create Flags 
        float lineWidth = Engine.conduitDiameterM * Mathf.Sin(Mathf.PI * 0.25f);

        m_travelLine1 = FlagRenderer.NewLine( m_Conduit.transform );
        m_travelLine1.SetWidth( lineWidth );
        m_travelLine1.SetColor( travelColor );
        m_travelLine2 = FlagRenderer.NewLine( m_Conduit.transform );
        m_travelLine2.SetWidth( lineWidth );
        m_travelLine2.SetColor( travelColor );

        // Create Obstacle
        m_obstacle = GameObject.CreatePrimitive( PrimitiveType.Cylinder );
        var renderer = m_obstacle.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.material = material;
            renderer.material.SetColor( "_Color", new Color( 0.2f, 0.2f, 0.2f, 1f ) );
        GameObject.DestroyObject( m_obstacle.GetComponent<Collider>() );

        m_obstacle.transform.SetParent( transform );
        m_obstacle.transform.localPosition = new Vector3( -k_objstacleLength, 0f, 0f );
        m_obstacle.transform.localRotation = Quaternion.Euler( 0f, 0f, 90f );
    }
}
