#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;

namespace CB
{
    public class ShowNormals : MonoBehaviour
    {
        MeshFilter m_meshFilter;
        Vector3[]  m_vertices;
        Vector3[]  m_normals;

        void Awake()
        {
            m_meshFilter = GetComponent<MeshFilter>();
            var mesh = m_meshFilter.mesh;

            m_vertices = mesh.vertices;
            m_normals = mesh.normals;
        }

        void OnDrawGizmosSelected()
        {

            var vertexCount = m_vertices.Length;
            for (int i = 0; i < vertexCount; i++) {
                Handles.matrix = transform.localToWorldMatrix;

                Handles.color = Color.yellow;
                Handles.DrawLine(
                    m_vertices[ i ],
                    m_vertices[ i ] + m_normals[ i ] );
            }
        }
    }

}

#endif
