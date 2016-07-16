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

    public MeshFilter       meshFilter;

    [HideInInspector]
    public float            conduitDiameterM = 0f;
    [HideInInspector]
    public Mesh             mesh;
    [HideInInspector]
    public List<CenterlineMarker> centerline = new List<CenterlineMarker>();
    // Indices into conduit centerline list indicating vertices at beginning and end of bends
    [HideInInspector]
    public List<CenterlineIndice> centerlineBendIndices = new List<CenterlineIndice>();

    [HideInInspector]
    private Bend         m_Bend = null;


    // Use this for initialization
    void Start () {


        mesh = meshFilter.mesh;
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

    public void Clone()
    {

    }


    public void SetMesh(Vector3[] vertices, Vector2[] uvs, int[] triangles)
    {
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
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
        }
    }

}
