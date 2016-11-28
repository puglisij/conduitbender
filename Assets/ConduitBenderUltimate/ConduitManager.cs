using UnityEngine;
using System.Collections.Generic;
using System;





public class ConduitManager : MonoBehaviour
{
    public Conduit              conduitPrefab;

    //-----------------
    // Static Data
    //-----------------
    private static GameObject        m_ConduitRoot = null;
    private static Conduit           m_ActiveConduit = null;
    private static AConduitDecorator m_ActiveDecorator = null;

    private static int          m_ConduitId = 0;
    private static bool         m_HasInitialized = false;

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
        if (m_HasInitialized) { return; } 

        // Check Nulls
        if (m_ConduitRoot == null) {
            m_ConduitRoot = GameObject.Find( "_Conduit" );
            if(m_ConduitRoot == null) {
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
        if(m_ActiveConduit == null) {
            m_ActiveConduit = New();
        }
        m_ActiveConduit.calculateHandler = ConduitCalculate;
        m_ActiveConduit.highlightHandler = ConduitHighlight;

        m_HasInitialized = true; 
    }

    private static void ConduitCalculate(Conduit conduit)
    {
        //Debug.Log( "ConduitManager: ConduitCalculate()" );
        // Generate
        ConduitGenerator.GenerateConduit( conduit );
        // Decorate
        if(m_ActiveDecorator != null) {
            m_ActiveDecorator.Decorate();
        }
    }
    private static void ConduitHighlight(Conduit conduit)
    {
        Debug.Log( "ConduitManager: ConduitHighlight() " + conduit.bend.modelName );
        if (m_ActiveDecorator != null) {
            m_ActiveDecorator.Highlight();
        }
    }
    private static Conduit New()
    {
        Conduit conduit = Instantiate( instance.conduitPrefab ); 
        conduit.name = "Conduit" + m_ConduitId;
        conduit.transform.SetParent( m_ConduitRoot.transform, false );
        m_ConduitId += 1;

        return conduit;
    }

    //public static Bounds GetActiveConduitBounds()
    //{
    //    return m_ActiveConduit.mesh.bounds;
    //}

    public static void LinkActiveConduit(Bend bend)
    {
        Debug.Log( "ConduitManager: LinkActiveConduit()" );
        // Remove any current Decorator component objects from Active Conduit
        AConduitDecorator decorator = m_ActiveConduit.GetComponentInChildren<AConduitDecorator>();
        if (decorator != null) {
            decorator.OnRemove();
            Destroy( decorator.gameObject );
        }

        m_ActiveConduit.Link( bend );

        // Set Decorator (if available)
        decorator = ConduitDecoratorFactory.Get( bend.type );
        if ( decorator != null) {
            m_ActiveDecorator = Instantiate( decorator );
            m_ActiveDecorator.transform.SetParent( m_ActiveConduit.transform, false );
            m_ActiveDecorator.transform.localPosition = Vector3.zero; // @TODO - Redundant?
            m_ActiveDecorator.Set( m_ActiveConduit );
        } else {
            m_ActiveDecorator = null;
        }
    }

}
