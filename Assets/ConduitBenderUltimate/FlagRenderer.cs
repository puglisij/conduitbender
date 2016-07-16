using UnityEngine;
using System.Collections;

public class FlagRenderer : MonoBehaviour {

    //public static Transform s_rayContainer;
    public static GameObject s_rayPrefab;
    public static LineFlag s_linePrefab;
    public GameObject rayPrefab;
    public LineFlag linePrefab;

    private static Vector3 s_RaySize;
    void Awake()
    {
        //s_rayContainer = GameObject.Find( "_Rays" ).transform;
        s_rayPrefab = rayPrefab;
        s_linePrefab = linePrefab;
        
    }
    void Start()
    {
        // Get size of Ray
        GameObject rayObj = Instantiate( s_rayPrefab );
        rayObj.transform.SetParent( transform, false );

        StartCoroutine( WaitForInitialize( rayObj ) );

    }

    IEnumerator WaitForInitialize(GameObject rayObj)
    {
        // Nice: Wait until Next Frame (so Prefab has time to initialize)
        yield return new WaitForEndOfFrame();

        MeshRenderer rayRenderer = rayObj.GetComponentInChildren<MeshRenderer>();
        if(rayRenderer != null) {
            Bounds rayBounds = rayRenderer.bounds;
            s_RaySize = rayBounds.size;   // World size
        }
        Destroy( rayObj );

        //Debug.Log( "RayRenderer: Awake() Ray Size: " + s_RaySize );
    }
    //public static void ClearRays()
    //{
    //    if(!s_rayContainer) {
    //        Debug.LogError( "RayRenderer: ClearRays() Ray Container null" );
    //        return;
    //    }
    //    for (int i = 0; i < s_rayContainer.childCount; ++i) {
    //        Destroy( s_rayContainer.GetChild( i ).gameObject );
    //    }
    //}

    public static LineFlag NewLine(Transform parent)
    {
        LineFlag lineObj = Instantiate( s_linePrefab );
        lineObj.transform.SetParent( parent, false );

        return lineObj;
    }
    /// <summary>
    /// Create a Ray Mesh Object.
    /// Will be parented to the given parent.
    /// </summary>
    public static GameObject NewRay(Transform parent)
    {
        GameObject rayObj = Instantiate( s_rayPrefab );
        rayObj.transform.SetParent( parent, false );

        return rayObj;
    }
    /// <summary>
    /// Create a Ray Mesh Object, giving it a color.
    /// Will be parented to the given parent.
    /// </summary>
    public static GameObject NewRay( Transform parent, Color color)
    {
        GameObject rayObj = Instantiate( s_rayPrefab );
        rayObj.transform.SetParent( parent, false );
        rayObj.GetComponentInChildren<MeshRenderer>().material.color = color;

        return rayObj;
    }
    /// <summary>
    /// Point an already created Ray in a new direction
    /// Point and Direction specified in world space
    /// </summary>
    public static void DrawRay(GameObject rayObj, Ray ray)
    {
        rayObj.transform.position = ray.origin;
        rayObj.transform.LookAt( ray.origin + ray.direction );
    }
    /// <summary>
    /// Point an already created Ray in a new direction, giving it a new color
    /// Point and Direction specified in world space
    /// </summary>
    public static void DrawRay(GameObject rayObj, Ray ray, Color color)
    {
        rayObj.transform.position = ray.origin;
        rayObj.transform.LookAt( ray.origin + ray.direction );
        rayObj.GetComponentInChildren<MeshRenderer>().material.color = color;
    }
    /// <summary>
    /// Draw an already created Ray pointing towards the specified point 'pointAt' in world space
    /// 'direction' should be normalized.
    /// </summary>
    public static void DrawRay( GameObject rayObj, Vector3 directionNorm, Vector3 pointAt)
    {
        rayObj.transform.position = pointAt - (directionNorm * s_RaySize.z);
        rayObj.transform.LookAt( pointAt );

    }
}
