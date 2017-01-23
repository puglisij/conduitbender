using UnityEngine;
using System.Collections;

public static class Extensions
{
    public static void DestroyChildren( this Transform transform )
    {
        foreach (Transform child in transform) {
            GameObject.Destroy( child.gameObject );
        }
    }

    /// <summary>
    /// Raycasts from given screen position to the given Plane and returns the resulting World Position.
    /// </summary>
    public static bool ScreenPointToWorldPosition( this Camera cam, Plane plane, Vector2 screenPos, out Vector3 worldPos )
    {
        var ray = cam.ScreenPointToRay(screenPos);

        float rayDist;
        if (plane.Raycast( ray, out rayDist )) {
            worldPos = ray.GetPoint( rayDist );
            return true;
        }
        worldPos = Vector3.zero;
        return false;
    }
}
