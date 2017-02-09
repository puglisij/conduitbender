using UnityEngine;
using System.Collections.Generic;
using System;





public class ConduitManager : MonoBehaviour
{
    public Conduit              conduitPrefab;

    //-----------------
    // Static Data
    //-----------------
    private static GameObject        m_conduitRoot = null;
    private static Conduit           m_activeConduit = null;
    private static AConduitDecorator m_activeDecorator = null;

    private static int          m_conduitId = 0;
    private static bool         m_hasInitialized = false;

    public static ConduitManager instance = null;
    

    void Awake()
    { 
        // Ensure one instance of this 
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy( this );
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
        if (m_hasInitialized) { return; } 

        // Check Nulls
        if (m_conduitRoot == null) {
            m_conduitRoot = GameObject.Find( "_Conduit" );
            if(m_conduitRoot == null) {
                Debug.LogError( "ConduitManager: Initialize() Unable to find _Conduit Root Object." );
                return;
            }
        }
#if UNITY_EDITOR
        if(conduitPrefab == null) {
            Debug.LogError( "ConduitManager: Initialize() No Conduit Prefab Set." );
            return;
        } 
#endif
        if(m_activeConduit == null) {
            m_activeConduit = New();
        }
        m_activeConduit.calculateHandler = ConduitCalculate;
        m_activeConduit.highlightHandler = ConduitHighlight;

        m_hasInitialized = true; 
    }

    private static void ConduitCalculate(Conduit conduit)
    {
        //Debug.Log( "ConduitManager: ConduitCalculate()" );
        // Generate
        ConduitGenerator.GenerateConduit( conduit );
        // Decorate
        if(m_activeDecorator != null) {
            m_activeDecorator.Decorate();
        }
    }
    private static void ConduitHighlight(Conduit conduit)
    {
        Debug.Log( "ConduitManager: ConduitHighlight() " + conduit.bend.modelName );
        if (m_activeDecorator != null) {
            m_activeDecorator.Highlight();
        }
    }
    private static Conduit New()
    {
        Conduit conduit = Instantiate( instance.conduitPrefab ); 
        conduit.name = "Conduit" + m_conduitId;
        conduit.transform.SetParent( m_conduitRoot.transform, false );
        m_conduitId += 1;

        return conduit;
    }

    //public static Bounds GetActiveConduitBounds()
    //{
    //    return m_activeConduit.mesh.bounds;
    //}

    public static void LinkActiveConduit(Bend bend)
    {
        Debug.Log( "ConduitManager: LinkActiveConduit()" );
        // Remove any current Decorator component objects from Active Conduit
        AConduitDecorator decorator = m_activeConduit.GetComponentInChildren<AConduitDecorator>();
        if (decorator != null) {
            decorator.OnRemove();
            //Destroy( decorator.gameObject );
            m_activeConduit.transform.DestroyChildren();
        }

        m_activeConduit.Link( bend );

        // Set Decorator (if available)
        decorator = ConduitDecoratorFactory.Get( bend.type );
        if ( decorator != null) {
            m_activeDecorator = Instantiate( decorator );
            m_activeDecorator.transform.SetParent( m_activeConduit.transform, false );
            m_activeDecorator.transform.localPosition = Vector3.zero; // @TODO - Redundant?
            m_activeDecorator.Set( m_activeConduit );
        } else {
            m_activeDecorator = null;
        }
    }

}
