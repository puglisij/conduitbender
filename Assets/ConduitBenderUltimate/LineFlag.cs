using UnityEngine;
using System.Collections;

/// <summary>
/// IMPORTANT: Assume using Standard Shader
/// </summary>
public class LineFlag : MonoBehaviour
{
    Material m_Material;

    int      m_EmissiveColorId;

    void Awake()
    {

        MeshRenderer meshRenderer= GetComponent<MeshRenderer>();
#if UNITY_EDITOR
        if(meshRenderer == null) {
            Debug.LogError( "LineFlag: Awake() No Mesh Renderer on LineFlag Prefab!" );
            return;
        }
#endif
        m_Material = meshRenderer.material;
        m_EmissiveColorId = Shader.PropertyToID( "_EmissionColor" );
    }

    /// <summary>
    /// Draw Line between Start and End Position given in Local Coordinates
    /// </summary>
    public void Draw(Vector3 start, Vector3 end)
    {
        Vector3 delta = end - start;
        Vector3 scale = transform.localScale;
                scale.z = delta.magnitude;
        transform.localPosition = start + delta * 0.5f;
        transform.localScale = scale;
        transform.localRotation = Quaternion.FromToRotation( Vector3.forward, delta );
        //transform.LookAt( end ); // World Position
    }

    public void SetColor(Color color)
    {
        m_Material.color = color;
        m_Material.SetColor( m_EmissiveColorId, color ); // IMPORTANT: Assume using Standard Shader
    }

    public void SetWidth(float width)
    {
        Vector3 scale = transform.localScale;
        scale.Set( width, width, scale.z );
        transform.localScale = scale;
    }

}
