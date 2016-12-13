using UnityEngine;
using System.Collections.Generic;
using System;

public class ConduitParallelOffsetDecorator : AConduitDecorator
{
    [Range(1, 10), Tooltip("Number of parallel conduits")]
    public int parallelCount;

    public UnityEngine.Rendering.ShadowCastingMode  castShadows;
    public bool receiveShadows = false;
    public Material material;
    public bool useLightProbes = false;

    private List<MeshFilter> m_Meshes = new List<MeshFilter>();

    void Awake()
    {
        GameObject      pConduit;
        MeshRenderer    pMeshRenderer;
        MeshFilter      pMeshFilter;
        Mesh            pMesh;
        // Create Parallel Conduits
        for (int i = 0; i < parallelCount; ++i) {
            pConduit = new GameObject( "ParallelConduit" + i );
            pConduit.transform.SetParent( transform, false );

            pMeshRenderer = pConduit.AddComponent<MeshRenderer>();
            pMeshRenderer.shadowCastingMode = castShadows;
            pMeshRenderer.receiveShadows = receiveShadows;
            pMeshRenderer.material = material;
            pMeshRenderer.useLightProbes = useLightProbes;

            pMeshFilter = pConduit.AddComponent<MeshFilter>();
            pMesh = pMeshFilter.mesh;

            m_Meshes.Add( pMeshFilter );
        }
    }

    private void SetConduitMirrors( Conduit conduit )
    {
        for (int i = 0; i < m_Meshes.Count; ++i) {
            m_Meshes[ i ].sharedMesh = conduit.mesh;
        }
    }
    public override void Decorate()
    {
        var bend = m_Conduit.bend;
        // Get amount of Shift
        float shiftM = (float) bend.GetOutputParameter(EBendParameterName.Shift).value;
        float spacingM = (float) bend.GetInputParameter(EBendParameterName.Spacing).value;

        Vector3 position = m_Conduit.transform.position;
        for (int i = 0; i < m_Meshes.Count; ++i) {
            position += shiftM * -conduit.transform.forward;
            position += spacingM * conduit.transform.up;
            m_Meshes[ i ].transform.position = position;
        }
    }
    public override void OnRemove()
    {  
    }
    public override void Set( Conduit conduit )
    {
        m_Conduit = conduit;

        // Reference same vertice meshes
        SetConduitMirrors( conduit );

        // Position Conduit
        Decorate();
    }

    public override void Highlight()
    {
        throw new NotImplementedException();
    }
}
