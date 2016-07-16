using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CoroutineManager : MonoBehaviour
{

    public static CoroutineManager instance = null;

    private static List< IEnumerator > Coroutines = new List< IEnumerator >();

    void Awake()
    {
        // Singleton
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
#if UNITY_EDITOR
            Debug.LogError( "Colors: Awake() Only one instance of CoroutineRunner should exist in the scene." );
#endif
            Destroy( gameObject );
            return;
        }
    }

    static void UpdateCoroutines()
    {
        foreach ( IEnumerator routine in Coroutines)
        {
            routine.MoveNext();
        }
    }

    public static void Start( IEnumerator coroutine )
    {
        instance.StartCoroutine( coroutine );
        //Coroutines.Add( coroutine );
    }


}
