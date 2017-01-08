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
}
