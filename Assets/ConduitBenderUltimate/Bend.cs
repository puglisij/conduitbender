using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;
using System.Collections.Generic;




/*##########################################

            Delegates

//#########################################*/
public delegate Bend BendFactoryDelegate();
public delegate void BendDelegate( Bend bend );

/*##########################################

         Bend Dependency Classes

//#########################################*/
public class Marker
{
    public Vector3  forwardDir;         // (Normalized) Vector pointing in current direction of Conduit 
    public Vector3  radialDir;          // (Normalized) Conduit bender handle direction (or direction toward center of bend circle)
    public float    distFromStartM;     // Straightline distance (in meters) where Mark is made from beginning of conduit

    public Marker( float distFromStartM, Vector3 forwardDir, Vector3 radialDir )
    {
        this.distFromStartM = distFromStartM;
        this.forwardDir = forwardDir;
        this.radialDir = radialDir;
    }
}
public class BendMarker : Marker
{
    public float        angleDeg;         // Amount of Bend (in Degrees)
    public float        radiusM;          // Centerline Radius (in meters)
    public BendFlagType flag;

    public BendMarker( BendFlagType flag, float distFromStartM, Vector3 forwardDir, Vector3 radialDir, float angleDeg, float radiusM )
        : base( distFromStartM, forwardDir, radialDir )
    {
        this.angleDeg = angleDeg;
        this.radiusM = radiusM;
        this.flag = flag;
    }
}
/*##########################################

            Bend Class

//#########################################*/
/// <summary>
/// A Data class which stores info about a Bend, including input parameters,
/// and calculated output values. Values are expected to be Metric.
/// </summary>
public class Bend : IModel
{
    public enum EventType { Calculated, HighlightOn, HighlightOff }
    public class Event : UnityEvent<EventType> { }
    
    public string modelName {
        get { return m_Type; }    
        set { throw new Exception( "Bend: modelName only settable on construction." ); }
    }
    public string type
    {
        get { return m_Type; }
    }
    public string alert
    {
        get { return m_Alert; }
        set { m_Alert = value; }
    }
    /// <summary>
    /// New parameters should NOT be Added OR Removed from the returned list.
    /// </summary>
    public List<BendParameter> inputParameters
    {
        get { return m_InputParameters; }
    }
    /// <summary>
    /// New parameters should NOT be Added OR Removed from the returned list.
    /// </summary>
    public List<BendParameter> outputParameters
    {
        get { return m_OutputParameters; }
    }

    //-----------------------------
    //      Public Data
    //-----------------------------
    // Delegate for calculating output parameters for this Type of bend
    public BendDelegate Calculate;
    /// <summary> Event which is fired when bend is Calculated </summary>
    public Event        onEvent = new Event();
    /// <summary> A series of data points indicating how the conduit should be bent. Calculated in BendFactory </summary>
    public List<Marker> conduitOrder = new List<Marker>();

    //-----------------------------
    //      Private Data
    //-----------------------------
    private List<BendParameter>    m_InputParameters = null;
    private List<BendParameter>    m_OutputParameters = null;

    /// <summary> The current BendParameter which is to be highlighted (input or output), if any. </summary>
    private BendParameter m_Highlight = null;
    /// <summary> The name/type of the Bend (e.g. Saddle3) </summary>
    private string   m_Type = "Undefined";
    /// <summary> An alert message to display for this Bend, if any. </summary>
    private string   m_Alert = null;

    public void Initialize(string type)
    {
        // Allow initialization once
        if(!m_Type.Equals("Undefined")) {
            return;
        }
        m_Type = type;
    }
    /// <summary>
    /// Permanently Sets internal list of Input Parameters to given parameter list.
    /// This function is only callable once.
    /// </summary>
    public void EmbedInputParameters(List<BendParameter> inputParams)
    {
        if(m_InputParameters != null) {
            throw new Exception( "Bend: EmbedInputParameters() only callable once." );
        }
        m_InputParameters = inputParams;
    }
    /// <summary>
    /// Permanently Sets internal list of Output Parameters to given parameter list.
    /// This function is only callable once.
    /// </summary>
    public void EmbedOutputParameters(List<BendParameter> outputParams)
    {
        if (m_OutputParameters != null) {
            throw new Exception( "Bend: EmbedOutputParameters() only callable once." );
        }
        m_OutputParameters = outputParams;
    }
    /// <summary>
    /// Returns the current BendParameter being highlighted. Else null.
    /// </summary>
    public BendParameter GetHighlight()
    {
        return m_Highlight;
    }
    /// <summary>
    /// Returns list of BendParameters that can be highlighted.
    /// </summary>
    public List<BendParameter> GetHighlightables()
    {
        var ins = m_InputParameters.Where(param => param.canHighlight && param.enabled);
        var outs = m_OutputParameters.Where(param => param.canHighlight && param.enabled);

        return ins.Concat( outs ).ToList();
    }
    public BendParameter GetInputParameter(BendParameter.Name name)
    {
        return m_InputParameters.Find( ( bp ) => bp.name == name );
    }
    public BendParameter GetOutputParameter( BendParameter.Name name )
    {
        return m_OutputParameters.Find( ( bp ) => bp.name == name );
    }

    /// <summary>
    /// Sets the current BendParameter to be highlighted. 
    /// Accepts an encoding of the form CI  where C is either "i" for Inputs or "o" for Outputs
    /// and I is the index of the parameter in the parameter collection. (e.g. i0, or o3)
    /// </summary>
//    public void SetHighlight( string highlightable )
//    {
//        try {
//            if (highlightable[ 0 ] == 'i') {
//                m_Highlight = m_InputParameters[ int.Parse( highlightable.Substring( 1 ) ) ];
//            } else if (highlightable[ 0 ] == 'o') {
//                m_Highlight = m_OutputParameters[ int.Parse( highlightable.Substring( 1 ) ) ];
//            }
//        } catch(Exception e) {
//#if UNITY_EDITOR 
//            Debug.Log( "Bend: SetHighlight() Exception setting highlightable. " + e.StackTrace);
//#endif
//        }
        
//    }
    /// <summary>
    /// Sets the current BendParameter to be highlighted. 
    /// ASSUMPTION: The BendParameter is a highlightable type
    /// </summary>
    public void SetHighlight( BendParameter highlightable )
    {
        m_Highlight = highlightable;

        if (highlightable == null) {
            onEvent.Invoke( EventType.HighlightOff );
        } else {
            onEvent.Invoke( EventType.HighlightOn );
        }
    }
    /// <summary>
    /// Sets the input parameter at specified index to given value.
    /// This should be called (Fires Calculated Event) to set values instead of setting them directly on the List elements.
    /// </summary>
    public void SetInputParameter( int index, object value, bool reCalculate = true )
    {
        SetHighlight( null );

        m_InputParameters[ index ].value = value;
        if(reCalculate) {
            Calculate( this );
            onEvent.Invoke( EventType.Calculated );
        }
    }
    public void SetOutputParameter( int index, object value )
    {
        m_OutputParameters[ index ].value = value;
    }
}
