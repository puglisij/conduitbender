using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

public enum BendFlagType { Arrow, Star, Notch, Ignore };
public enum BendMarkType { Start, End };

public struct CenterlineIndice
{
    public BendMarkType type;
    public int          index;
    public CenterlineIndice(BendMarkType type, int index) {
        this.type = type;
        this.index = index;
    }
}

public struct CenterlineMarker
{
    public Vector3  point;
    public Vector3  forwardDir;
    public Vector3  radialDir;
    public float    distFromStartM;
    public CenterlineMarker(Vector3 point, Vector3 forwardDir, Vector3 radialDir, float distFromStartM)
    {
        this.point      = point;
        this.forwardDir = forwardDir;
        this.radialDir  = radialDir;
        this.distFromStartM = distFromStartM;
    }
    public void Set(Vector3 point, Vector3 forwardDir, Vector3 radialDir, float distFromStartM)
    {
        this.point      = point;
        this.forwardDir = forwardDir;
        this.radialDir  = radialDir;
        this.distFromStartM = distFromStartM;
    }
}

/*##########################################

            Conduit  Class

//#########################################*/
public delegate void ConduitHandler( Conduit conduit );

/// <summary>
/// A Simple Data Class which stores a Mesh, Bend, and Centerline marks for a Conduit
/// </summary>
public class Conduit : MonoBehaviour {

    public Bend bend
    {
        get { return m_Bend; }
    }

    /// <summary>
    /// Callback which is called when Bend has been ReCalculated
    /// </summary>
    public ConduitHandler   calculateHandler;

    /// <summary>
    /// Callback which is called when the Bend has set a parameter Highlight in order to color the mesh via vertice colors.
    /// NOTE: Listener (e.g. ConduitDecorator) should NOT attempt to maintain a highlight state post event as the highlighting may be removed internally.
    /// </summary>
    public ConduitHandler   highlightHandler;

    public MeshFilter       meshFilter;

    [HideInInspector]
    public float            conduitDiameterM = 0f;
    [HideInInspector]
    public Mesh             mesh;
    /// <summary> Cartesian data, at each vertice centerline point along the conduit mesh (e.g. forward and radial directions of bender). </summary>
    [HideInInspector]
    public List<CenterlineMarker> centerline = new List<CenterlineMarker>();
    /// <summary> Indices into conduit centerline list indicating vertices at beginning and end of bends </summary>
    [HideInInspector]
    public List<CenterlineIndice> centerlineBendIndices = new List<CenterlineIndice>();

    [HideInInspector]
    private Bend         m_Bend = null;

    private bool         m_isHighlighted = false;

    // Use this for initialization
    void Start () {


        mesh = meshFilter.mesh;
    }

    private void SetHighlightOff()
    {
        if (m_isHighlighted) {
            mesh.colors32 = null;
            m_isHighlighted = false;
            SetHighlightColor( Color.black );
        }
    }

    /// <summary>
    /// Clears the contents of the Mesh and Centerline Vertice List.
    /// </summary>
    public void Clear()
    {
        centerline.Clear();
        centerlineBendIndices.Clear();
        mesh.Clear();
    }

    //public void Clone()
    //{
    //    throw new NotImplementedException();
    //}

    /// <summary>
    /// Set the Highlight color used in the Conduit shader.
    /// </summary>
    public void SetHighlightColor( Color color )
    {
        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null) {
            renderer.material.SetColor( "_Highlight", color );
        }
    }

    public void SetMesh(Vector3[] vertices, Vector2[] uvs, int[] triangles)
    {
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // Ensure that vertice Highlighting is Off (i.e. Remove vertice colors since vertice count may have changed)
        SetHighlightOff();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public void Link( Bend model )
    {
        if (m_Bend != null) {
            m_Bend.onEvent.RemoveListener( ListenerBend );
        }
        m_Bend = model;
        m_Bend.onEvent.AddListener( ListenerBend );
    }
    public void ListenerBend( Bend.EventType type )
    {
        if (type == Bend.EventType.Calculated) {
            calculateHandler( this );
        } else if(type == Bend.EventType.HighlightOn) {
            m_isHighlighted = true; 
            highlightHandler( this );
        } else if(type == Bend.EventType.HighlightOff) {
            SetHighlightOff();
        }
    }

}
