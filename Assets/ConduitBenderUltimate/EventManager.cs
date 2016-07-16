using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


/// <summary>
///  TODO  - Lots to Do Here... This was taken as sample code from Unity3d.com
/// </summary>
public class EventManager : MonoBehaviour
{

    private Dictionary<string, UnityEvent>   m_EventDictionary;

    void Awake()
    {
        if (m_EventDictionary == null) {
            m_EventDictionary = new Dictionary<string, UnityEvent>();
        }
    }
     
    void Initialize()
    {
        
    }

    public void StartListening( string eventName, UnityAction listener )
    {
        UnityEvent thisEvent = null;
        if (m_EventDictionary.TryGetValue( eventName, out thisEvent )) {
            thisEvent.AddListener( listener );
        } else {
            thisEvent = new UnityEvent();
            thisEvent.AddListener( listener );
            m_EventDictionary.Add( eventName, thisEvent );
        }
    }

    public void StopListening( string eventName, UnityAction listener )
    {

        UnityEvent thisEvent = null;
        if (m_EventDictionary.TryGetValue( eventName, out thisEvent )) {
            thisEvent.RemoveListener( listener );
        }
    }

    public void TriggerEvent( string eventName )
    {
        UnityEvent thisEvent = null;
        if (m_EventDictionary.TryGetValue( eventName, out thisEvent )) {
            thisEvent.Invoke();
        }
    }

}
