using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;

public class DebugToScreen : MonoBehaviour
{
    static FixedStack<string>  logStack = new FixedStack<string>(50);

    public bool isStopped
    {
        get { return m_Stopped; }
    }

    public GameObject outView;
    public Text  outText;

    public bool displayAppLogs = false;

    private bool m_Stopped = true;

    private static bool m_VisualsDirty = false;

	void  Awake()
    {

        // Print ALL Log Messages to this view
        if(displayAppLogs) {
            Application.logMessageReceived += LogCallback;
        }
    }

    void Update()
    {
        if (m_Stopped) { return; }

        if(m_VisualsDirty) {
            // Loop through Log Stack and Write to Text
            StringBuilder sb = new StringBuilder();
            for(int m = 0; m < logStack.Count; ++m) {
                sb.Append( Time.time + " " );
                sb.Append( logStack.At( m ) );
                sb.Append( "\n" );
            }
            outText.text = sb.ToString();

            m_VisualsDirty = false;
        }
    }
	
    void LogCallback( string condition, string stackTrace, LogType type )
    {
        Log( "Condition: " + condition + " stackTrace: " + stackTrace );
    }

    public void Toggle()
    {
        if (m_Stopped) {
            m_Stopped = false;
        } else {
            m_Stopped = true;
            outText.text = "";
        }
    }

    public void ToggleActive()
    {
        if (outView.activeSelf) {
            outView.SetActive( false );
        } else {
            outView.SetActive( true );
        }
    }

    public static void Log(string message)
    {
        logStack.Push( message );
        m_VisualsDirty = true;
    }
	
}
