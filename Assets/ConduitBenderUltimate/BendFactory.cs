using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BendFactory
{
    public const float k_SegmentedSimpleAccuracy = 0.001f;

    // Bend Name to Bend Factory Delegates \==========(Factory)==========/
    private static Dictionary<string, BendFactoryDelegate> m_BendMakers = new Dictionary<string, BendFactoryDelegate>();

    public static void Initialize()
    {
        // Iterate through Bend Names and Map BendFactoryDelegates 

        m_BendMakers.Add( "CompoundCircle", BendCompoundCircle );
        m_BendMakers.Add( "Offset", BendOffset );
        m_BendMakers.Add( "ParallelOffset", BendParallelOffset );
        m_BendMakers.Add( "RolledOffset", BendRolledOffset );
        m_BendMakers.Add( "Saddle3", BendSaddle3 );
        m_BendMakers.Add( "Saddle4", BendSaddle4 );
        m_BendMakers.Add( "Segmented", BendSegmented );
        m_BendMakers.Add( "StubUp", BendStubUp );
        m_BendMakers.Add( "ParallelKick", BendParallelKick );
        
    }

    /// <summary>
    /// Adds mapping of 'type' to 'method', replacing any previously mapping
    /// \==========(Factory)==========/
    /// </summary>
    //public static void AddBendFactoryDelegate( string type, BendFactoryDelegate method )
    //{
    //    if (m_BendMakers.ContainsKey( type )) {
    //        m_BendMakers.Remove( type );
    //    }
    //    m_BendMakers.Add( type, method );
    //}

    /// <summary>
    /// Return list of currently supported Bend Names/Types
    /// </summary>
    /// <returns></returns>
    public static List<string> GetBendNames()
    {
        return new List<string>(m_BendMakers.Keys);
    }
    /// <summary>
    /// Returns a new instance of the specified Bend type.
    /// If bend type does not exist, returns null.
    /// </summary>
    public static Bend New(string type)
    {
        BendFactoryDelegate bfd;
        if(m_BendMakers.TryGetValue(type, out bfd )) {
            return bfd();
        }
        return null;
    }
    /*##########################################

                Bend Functions

    ###########################################*/

    /// <summary>
    /// Check if bends are too close together.
    /// 'distBetweenM' is the distance from the start of the first bend to the start of the 2nd
    /// 'angle1Rad' is the angle of the first bend in Radians
    /// </summary>
    private static bool BendsTooClose(float distBetweenM, float angle1Rad, out string output)
    {
        bool tooClose = false;
        if ( (distBetweenM - Calculator.Lb(Engine.benderRadiusM, angle1Rad)) < 0f ) {
            tooClose = true;
            output = BendMessages.k_BendsTooClose;
        } else {
            output = null;
        }
        
        return tooClose;
    }

    private static Bend BendCompoundCircle()
    {
        var colors = Colors.instance;
        Bend bend = new Bend();
        bend.Initialize( "CompoundCircle" );

        List<BendParameter> inputs = new List<BendParameter>();
        inputs.Add( new BendParameter( EBendParameterName.AngleDegrees, EBendParameterType.FloatAngle, colors.inputParameterDefault, 0f ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( EBendParameterName.DistanceBetween, EBendParameterType.Float, colors.outputParameterDefault, 0f ) );

        bend.EmbedInputParameters( inputs );
        bend.EmbedOutputParameters( outputs );
        bend.Calculate = CalculateCompoundCircle;
        return bend;
    }

    private static Bend BendOffset()
    {
        var colors = Colors.instance;
        Bend bend = new Bend();
        bend.Initialize( "Offset" );

        List<BendParameter> inputs = new List<BendParameter>();
        inputs.Add( new BendParameter( EBendParameterName.AngleDegrees, EBendParameterType.FloatAngle, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( EBendParameterName.OffsetHeight, EBendParameterType.Float, colors.inputParameterDefault, 0f ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( EBendParameterName.DistanceBetween, EBendParameterType.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.TotalShrink, EBendParameterType.Float, colors.outputParameterDefault, 0f ) );

        // Highlightable 
        outputs[ 0 ].canHighlight = true;   // DistanceBetween 

        bend.EmbedInputParameters( inputs );
        bend.EmbedOutputParameters( outputs );
        bend.Calculate = CalculateOffset;
        return bend;
    }

    private static Bend BendParallelOffset()
    {
        var colors = Colors.instance;
        Bend bend = new Bend();
        bend.Initialize( "ParallelOffset" );

        List<BendParameter> inputs = new List<BendParameter>();
        inputs.Add( new BendParameter( EBendParameterName.AngleDegrees, EBendParameterType.FloatAngle, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( EBendParameterName.Spacing, EBendParameterType.Float, colors.inputParameterDefault, 0f ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( EBendParameterName.Shift, EBendParameterType.Float, colors.outputParameterDefault, 0f ) );

        bend.EmbedInputParameters( inputs );
        bend.EmbedOutputParameters( outputs );
        bend.Calculate = CalculateParallelOffset;
        return bend;
    }

    private static Bend BendRolledOffset()
    {
        var colors = Colors.instance;
        Bend bend = new Bend();
        bend.Initialize( "RolledOffset" );

        List<BendParameter> inputs = new List<BendParameter>();
        inputs.Add( new BendParameter( EBendParameterName.AngleDegrees, EBendParameterType.FloatAngle, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( EBendParameterName.Rise, EBendParameterType.Float, colors.flagBlue, 0f ) );
        inputs.Add( new BendParameter( EBendParameterName.Roll, EBendParameterType.Float, colors.flagPurple, 0f ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( EBendParameterName.DistanceBetween, EBendParameterType.Float, colors.flagGreen, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.TotalShrink, EBendParameterType.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.RollAngleDegrees, EBendParameterType.FloatAngle, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.OffsetHeight, EBendParameterType.Float, colors.outputParameterDefault, 0f ) );

        // Highlightable 
        outputs[ 0 ].canHighlight = true;   // DistanceBetween 

        bend.EmbedInputParameters( inputs );
        bend.EmbedOutputParameters( outputs );
        bend.Calculate = CalculateRolledOffset;
        return bend;
    }

    private static Bend BendSaddle3()
    {
        var colors = Colors.instance;
        Bend bend = new Bend();
        bend.Initialize( "Saddle3" );

        List<BendParameter> inputs = new List<BendParameter>();
        inputs.Add( new BendParameter( EBendParameterName.CenterAngleDegrees, EBendParameterType.FloatAngle, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( EBendParameterName.SaddleHeight, EBendParameterType.Float, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( EBendParameterName.Saddle3Method, EBendParameterType.StringEnum, colors.inputParameterDefault,
            GlobalEnum.Saddle3BendMethod.First(), GlobalEnum.Saddle3BendMethod ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( EBendParameterName.LengthOfCenterBend, EBendParameterType.Float, colors.flagBlue, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.DistanceBetween, EBendParameterType.Float, colors.flagPurple, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.Distance1stTo2nd, EBendParameterType.Float, colors.flagPurple, 0f, null, false ) );
        outputs.Add( new BendParameter( EBendParameterName.Distance2ndTo3rd, EBendParameterType.Float, colors.flagGreen, 0f, null, false ) );
        outputs.Add( new BendParameter( EBendParameterName.ShrinkTo2ndMark, EBendParameterType.Float, colors.outputParameterDefault, 0f, null, false ) );
        outputs.Add( new BendParameter( EBendParameterName.ShrinkToCenter, EBendParameterType.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.TotalShrink, EBendParameterType.Float, colors.outputParameterDefault, 0f ) );

        // Highlightable 
        outputs[ 0 ].canHighlight = true;   // LengthOfCenterBend 
        outputs[ 1 ].canHighlight = true;   // DistanceBetween
        outputs[ 2 ].canHighlight = true;   // Distance 1st to 2nd
        outputs[ 3 ].canHighlight = true;   // Distance 2nd to 3rd

        bend.EmbedInputParameters( inputs );
        bend.EmbedOutputParameters( outputs );
        bend.Calculate = Calculate3PointSaddle;
        return bend;
    }

    private static Bend BendSaddle4()
    {
        var colors = Colors.instance;
        Bend bend = new Bend();
        bend.Initialize( "Saddle4" );

        List<BendParameter> inputs = new List<BendParameter>();
        inputs.Add( new BendParameter( EBendParameterName.AngleDegrees, EBendParameterType.FloatAngle, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( EBendParameterName.SaddleHeight, EBendParameterType.Float, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( EBendParameterName.SaddleLength, EBendParameterType.Float, colors.inputParameterDefault, 0f ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( EBendParameterName.DistanceBetween, EBendParameterType.Float, colors.flagBlue, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.Distance2ndTo3rd, EBendParameterType.Float, colors.flagPurple, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.TotalShrink, EBendParameterType.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.ShrinkToCenter, EBendParameterType.Float, colors.outputParameterDefault, 0f ) );

        // Highlightable 
        outputs[ 0 ].canHighlight = true;   // DistanceBetween 
        outputs[ 1 ].canHighlight = true;   // Distance 2nd to 3rd

        bend.EmbedInputParameters( inputs );
        bend.EmbedOutputParameters( outputs );
        bend.Calculate = Calculate4PointSaddle;
        return bend;
    }

    private static Bend BendSegmented()
    {
        var colors = Colors.instance;
        Bend bend = new Bend();
        bend.Initialize( "Segmented" );

        List<BendParameter> inputs = new List<BendParameter>();
        inputs.Add( new BendParameter( EBendParameterName.SegmentedAngle, EBendParameterType.FloatAngle, colors.flagBlue, 0f ) );
        inputs.Add( new BendParameter( EBendParameterName.SegmentedCount, EBendParameterType.Integer, colors.inputParameterDefault, 3 ) );
        inputs.Add( new BendParameter( EBendParameterName.SegmentedRadius, EBendParameterType.Float, colors.flagBlue, 0f ) );
        inputs.Add( new BendParameter( EBendParameterName.SegmentedMethod, EBendParameterType.StringEnum, colors.inputParameterDefault,
            GlobalEnum.SegmentedBendMethod.Last(), GlobalEnum.SegmentedBendMethod ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( EBendParameterName.AngleFirstDegrees, EBendParameterType.FloatAngle, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.AngleDegrees, EBendParameterType.FloatAngle, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.AngleLastDegrees, EBendParameterType.FloatAngle, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.DistanceBetween, EBendParameterType.Float, colors.flagPurple, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.DistanceTo2nd, EBendParameterType.Float, colors.flagYellow, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.DevelopedLength, EBendParameterType.Float, colors.outputParameterDefault, 0f ) );

        // Highlightable 
        outputs[ 3 ].canHighlight = true;   // DistanceBetween 
        outputs[ 4 ].canHighlight = true;   // DistanceTo2nd 

        bend.EmbedInputParameters( inputs );
        bend.EmbedOutputParameters( outputs );
        bend.Calculate = CalculateSegmented;
        return bend;
    }

    private static Bend BendStubUp()
    {
        var colors = Colors.instance;
        Bend bend = new Bend();
        bend.Initialize( "StubUp" );

        List<BendParameter> inputs = new List<BendParameter>();
        inputs.Add( new BendParameter( EBendParameterName.StubLength, EBendParameterType.Float, colors.inputParameterDefault, 0f ) );
        //inputs.Add( new BendParameter( EBendParameterName.StubMethod, EBendParameterType.StringEnum,
        //    GlobalEnum.StubUpMethod.First(), GlobalEnum.StubUpMethod ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( EBendParameterName.DistanceFromEnd, EBendParameterType.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.StubTakeUp, EBendParameterType.Float, colors.flagPurple, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.LengthOfBend, EBendParameterType.Float, colors.flagBlue, 0f ) );

        // Highlightable 
        outputs[ 0 ].canHighlight = true; // DistanceFromEnd
        outputs[ 2 ].canHighlight = true; // LengthOfBend

        bend.EmbedInputParameters( inputs );
        bend.EmbedOutputParameters( outputs );
        bend.Calculate = CalculateStubUp;
        return bend;
    }

    private static Bend BendParallelKick()
    {
        var colors = Colors.instance;
        Bend bend = new Bend();
        bend.Initialize( "ParallelKick" );

        List<BendParameter> inputs = new List<BendParameter>();
        inputs.Add( new BendParameter( EBendParameterName.AngleDegrees, EBendParameterType.FloatAngle, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( EBendParameterName.Spacing, 
            EBendParameterType.Float, colors.inputParameterDefault, Engine.conduitDiameterM * 2f ) );
        inputs.Add( new BendParameter( EBendParameterName.KickOffset,
            EBendParameterType.Float, colors.inputParameterDefault, Engine.conduitDiameterM * 2f ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( EBendParameterName.KickTravel, EBendParameterType.Float, colors.flagBlue, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.KickSpread, EBendParameterType.Float, colors.flagYellow, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.KickFirstMark, EBendParameterType.Float, colors.flagGreen, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.Shift, EBendParameterType.Float, colors.flagPurple, 0f ) );
        outputs.Add( new BendParameter( EBendParameterName.DevelopedLength, EBendParameterType.Float, colors.outputParameterDefault, 0f ) );

        bend.EmbedInputParameters( inputs );
        bend.EmbedOutputParameters( outputs );
        bend.Calculate = CalculateParallelKick;
        return bend;
    }

    /*##########################################

                Bend Calculators

    ###########################################*/

    public static void CalculateCompoundCircle( Bend bend )
    {
        //float angleDeg;
        //float benderRadiusM;

    }

    public static void CalculateOffset( Bend bend )
    {
        // Write up a Conduit Order
        bend.conduitOrder.Clear();
        // Get Input Parameters
        float angleDeg  = (float) bend.inputParameters[0].value;
        float angleRad  = angleDeg * Mathf.Deg2Rad;
        float heightM   = (float) bend.inputParameters[1].value;
        float benderRadiusM = Engine.benderRadiusM;

        float Vs = heightM - 2f * Calculator.Vb( benderRadiusM, angleRad );
        float Lb = Calculator.Lb( benderRadiusM, angleRad );
        float Ls = Calculator.Ls( Vs, angleRad );
        float Hb = Calculator.Hb( benderRadiusM, angleRad );
        float distBetween = Lb + Ls;
        float shrink = (2f * Lb + Ls) - (2 * Hb + Calculator.Hs( Ls, angleRad ));

        // Check Distance Between Bends
        string message = null;
        if (BendsTooClose( distBetween, angleRad, out message )) {
            Vs = Lb = angleDeg = angleRad = distBetween = shrink = 0f;
        }
        bend.alert = message;

        // Set Output Parameters
        bend.outputParameters[ 0 ].value = distBetween;   // Distance between marks = Lb + Ls 
        bend.outputParameters[ 1 ].value = shrink; // Shrink

        // Start Conduit
        bend.conduitOrder.Add( new Marker( 0f, Vector3.forward, Vector3.up ) );

        // Calculate Bend Marks (Arbitrary Start point)
        float mark_1 = 0.3048f;  // 12 in
        float mark_2 = mark_1 + distBetween;

        Vector3 rdl_1   = Vector3.up;
        Vector3 fwd_1   = Vector3.forward;
        Vector3 axis    = Vector3.Cross( rdl_1, fwd_1 ).normalized;
        Vector3 rdl_2   = -Calculator.RotateCCW( angleDeg, axis, rdl_1 ).normalized;
        Vector3 fwd_2   = Calculator.RotateCCW( angleDeg, axis, fwd_1 ).normalized;

        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_1, fwd_1, rdl_1, angleDeg, benderRadiusM ) );
        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_2, fwd_2, rdl_2, angleDeg, benderRadiusM ) );

        // End Conduit
        bend.conduitOrder.Add( new Marker( mark_2 + Lb + 0.3048f, Vector3.zero, Vector3.zero ) );
    }

    public static void CalculateParallelOffset( Bend bend )
    {
        // Write up a Conduit Order
        bend.conduitOrder.Clear();

        // Get Input Parameters
        float angleDeg = (float) bend.inputParameters[0].value;
        float angleRad  = angleDeg * Mathf.Deg2Rad;
        float spacingM = (float) bend.inputParameters[1].value;
        float benderRadiusM = Engine.benderRadiusM;

        // Calculate Values
        float shiftM = Mathf.Tan( angleRad / 2f ) * spacingM;
        float Lb = Calculator.Lb( benderRadiusM, angleRad );

        // Set Output Parameters
        bend.outputParameters[ 0 ].value = shiftM; // Shift

        // Start Conduit
        bend.conduitOrder.Add( new Marker( 0f, Vector3.forward, Vector3.up ) );

        // Calculate Bend Marks (Arbitrary Start point)
        float mark_1 = 0.3048f;  // 12 in
        float mark_2 = mark_1 + Lb + 0.3048f;

        Vector3 rdl_1   = Vector3.up;
        Vector3 fwd_1   = Vector3.forward;
        Vector3 axis    = Vector3.Cross( rdl_1, fwd_1 ).normalized;
        Vector3 rdl_2   = -Calculator.RotateCCW( angleDeg, axis, rdl_1 ).normalized;
        Vector3 fwd_2   = Calculator.RotateCCW( angleDeg, axis, fwd_1 ).normalized;

        bend.conduitOrder.Add( new BendMarker( BendFlagType.Ignore, mark_1, fwd_1, rdl_1, angleDeg, benderRadiusM ) );
        bend.conduitOrder.Add( new BendMarker( BendFlagType.Ignore, mark_2, fwd_2, rdl_2, angleDeg, benderRadiusM ) );

        // End Conduit
        bend.conduitOrder.Add( new Marker( mark_2 + Lb + 0.3048f, Vector3.zero, Vector3.zero ) );
    }

    public static void CalculateRolledOffset( Bend bend )
    {
        // Write up a Conduit Order
        bend.conduitOrder.Clear();

        // Get Input Parameters
        float angleDeg = (float) bend.inputParameters[0].value;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float riseM = (float) bend.inputParameters[1].value;
        float rollM = (float) bend.inputParameters[2].value;
        float benderRadiusM = Engine.benderRadiusM;

        // Calculate Values
        float offsetM = 0f;
        float rollAngleDegrees = 0f;
        if( rollM <= 0f ) {
            rollAngleDegrees = 0f;
        } else if (riseM <= 0f) {
            rollAngleDegrees = 90f;
        } else {
            rollAngleDegrees = Mathf.Atan( rollM / riseM ) * Mathf.Rad2Deg;
        }
        offsetM = Mathf.Sqrt( riseM * riseM + rollM * rollM );

        float Vs = offsetM - 2f * Calculator.Vb( benderRadiusM, angleRad );
        float Lb = Calculator.Lb( benderRadiusM, angleRad );
        float Ls = Calculator.Ls( Vs, angleRad );
        float Hb = Calculator.Hb( benderRadiusM, angleRad );
        float distBetween = Lb + Ls;
        float shrink = (2f * Lb + Ls) - (2f * Hb + Calculator.Hs( Ls, angleRad ));

        // Check Distance Between Bends
        string message = null;
        if (BendsTooClose( distBetween, angleRad, out message )) {
            Vs = Lb = angleDeg = angleRad = distBetween = shrink = 0f;
        }
        bend.alert = message;

        // Set Output Parameters
        bend.outputParameters[ 0 ].value = distBetween;
        bend.outputParameters[ 1 ].value = shrink;
        bend.outputParameters[ 2 ].value = rollAngleDegrees;
        bend.outputParameters[ 3 ].value = offsetM;

        // Start Conduit
        bend.conduitOrder.Add( new Marker( 0f, Vector3.forward, Vector3.up ) );

        // Calculate Bend Marks (Arbitrary Start point)
        float mark_1 = 0.3048f;  // 12 in
        float mark_2 = mark_1 + distBetween;

        Vector3 rdl_1   = Vector3.up;
        Vector3 fwd_1   = Vector3.forward;
        Vector3 axis    = Vector3.Cross( rdl_1, fwd_1 ).normalized;
        Vector3 rdl_2   = -Calculator.RotateCCW( angleDeg, axis, rdl_1 ).normalized;
        Vector3 fwd_2   = Calculator.RotateCCW( angleDeg, axis, fwd_1 ).normalized;

        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_1, fwd_1, rdl_1, angleDeg, benderRadiusM ) );
        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_2, fwd_2, rdl_2, angleDeg, benderRadiusM ) );

        // End Conduit
        bend.conduitOrder.Add( new Marker( mark_2 + Lb + 0.3048f, Vector3.zero, Vector3.zero ) );
    }

    public static void Calculate3PointSaddle( Bend bend )
    {
        // Segmented Calculation Method
        GlobalEnum.ESaddle3BendMethod method = (GlobalEnum.ESaddle3BendMethod) bend.inputParameters[2].value;

        switch (method) {
            case GlobalEnum.ESaddle3BendMethod.Notch:
                Calculate3PointSaddleNotch( bend );
                break;
            case GlobalEnum.ESaddle3BendMethod.Arrow:
                Calculate3PointSaddleArrow( bend );
                break;
            default:
                Debug.LogError( "ConduitGenerator: Calculate3PointSaddle() Invalid Bend Type." );
                break;
        }
    }

    private static void Calculate3PointSaddleNotch( Bend bend )
    {
        // Write up a Conduit Order
        bend.conduitOrder.Clear();

        // Get Input Parameters
        float centerAngleDeg = (float) bend.inputParameters[0].value;
        float centerAngleRad = centerAngleDeg * Mathf.Deg2Rad;
        float halfAngleDeg = centerAngleDeg * 0.5f;
        float halfAngleRad = centerAngleRad * 0.5f;
        float saddleHeightM = (float) bend.inputParameters[1].value;
        float benderRadiusM = Engine.benderRadiusM;

        // Calculate Values
        float Vs = saddleHeightM - 2f * Calculator.Vb( benderRadiusM, halfAngleRad );
        float Lhb = Calculator.Lb( benderRadiusM, halfAngleRad );
        float Lhs = Calculator.Ls( Vs, halfAngleRad );
        float distBetween = Lhb + Lhs;
        float halfShrink = (2f * Lhb + Lhs) - (2f * Calculator.Hb(benderRadiusM, halfAngleRad) + Calculator.Hs( Lhs, halfAngleRad ));

        // Check Distance Between Bends
        string message = null;
        if (Vs < 0f) {
            Lhs = 0f;
            message = BendMessages.k_BendsTooClose;
        }
        bend.alert = message;

        // Set Output Parameters
        bend.outputParameters[ 0 ].value = 2f * Lhb;    // Length Of Center Bend
        bend.outputParameters[ 1 ].value = distBetween; // Distance Between
        bend.outputParameters[ 1 ].enabled = true;
        bend.outputParameters[ 2 ].enabled = false;     // Distance 1st to 2nd
        bend.outputParameters[ 3 ].enabled = false;     // Distance 2nd to 3rd
        bend.outputParameters[ 4 ].enabled = false;     // Shrink to 2nd Mark
        bend.outputParameters[ 5 ].value = halfShrink;       // Shrink To Center
        bend.outputParameters[ 5 ].enabled = true; 
        bend.outputParameters[ 6 ].value = halfShrink * 2f;  // Shrink

        // Start Conduit
        bend.conduitOrder.Add( new Marker( 0f, Vector3.forward, Vector3.up ) );

        // Calculate Bend Marks
        float mark_1 = 0.3048f;  // 12 in
        float mark_2 = mark_1 + Lhb + Lhs;
        float mark_3 = mark_2 + Lhb * 2f + Lhs;

        Vector3 rdl_1 = Vector3.up;
        Vector3 fwd_1 = Vector3.forward;
        Vector3 axis = Vector3.Cross( rdl_1, fwd_1 ).normalized;
        Vector3 rdl_2 = -Calculator.RotateCCW( halfAngleDeg, axis, rdl_1);
        rdl_2.Normalize();
        Vector3 fwd_2 = Calculator.RotateCCW( halfAngleDeg, axis, fwd_1 );
        fwd_2.Normalize();
        Vector3 rdl_3 = -Calculator.RotateCW( centerAngleDeg, axis, rdl_2 );
        rdl_3.Normalize();
        Vector3 fwd_3 = Calculator.RotateCW( centerAngleDeg, axis, fwd_2 );
        fwd_3.Normalize();

        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_1, fwd_1, rdl_1, halfAngleDeg, benderRadiusM ) );
        bend.conduitOrder.Add( new BendMarker( BendFlagType.Notch, mark_2, fwd_2, rdl_2, centerAngleDeg, benderRadiusM ) );
        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_3, fwd_3, rdl_3, halfAngleDeg, benderRadiusM ) );

        // End Conduit
        bend.conduitOrder.Add( new Marker( mark_3 + Calculator.Lb( benderRadiusM, halfAngleRad ) + 0.3048f, fwd_1, rdl_1 ) );
    }

    private static void Calculate3PointSaddleArrow( Bend bend )
    {
        // Write up a Conduit Order
        bend.conduitOrder.Clear();

        // Get Input Parameters
        float centerAngleDeg = (float) bend.inputParameters[0].value;
        float centerAngleRad = centerAngleDeg * Mathf.Deg2Rad;
        float halfAngleDeg = centerAngleDeg * 0.5f;
        float halfAngleRad = centerAngleRad * 0.5f;
        float saddleHeightM = (float) bend.inputParameters[1].value;
        float benderRadiusM = Engine.benderRadiusM;

        // Calculate Values
        float Vs = saddleHeightM - 2f * Calculator.Vb( benderRadiusM, halfAngleRad );
        float Lhb = Calculator.Lb( benderRadiusM, halfAngleRad );
        float Lhs = Calculator.Ls( Vs, halfAngleRad );
        float distFirstToSecond = Lhs;
        float distSecondToThird = 2f * Lhb + Lhs;
        float shrinkTo2ndMark = Lhs - (Calculator.Hs( Lhs, halfAngleRad ));
        float shrinkTotal = (4f * Lhb + 2f * Lhs) - (4f * Calculator.Hb( benderRadiusM, halfAngleRad ) + 2f * Calculator.Hs(Lhs, halfAngleRad));

        // Check Distance Between Bends
        string message = null;
        if (Vs < 0f) {
            Lhs = 0f;
            message = BendMessages.k_BendsTooClose;
        }
        bend.alert = message;

        // Set Output Parameters
        bend.outputParameters[ 0 ].value = 2f * Lhb;            // Length Of Center Bend
        bend.outputParameters[ 1 ].enabled = false;             // Distance Between
        bend.outputParameters[ 2 ].value = distFirstToSecond;   // Distance 1st to 2nd
        bend.outputParameters[ 2 ].enabled = true;
        bend.outputParameters[ 3 ].value = distSecondToThird;   // Distance 2nd to 3rd
        bend.outputParameters[ 3 ].enabled = true;
        bend.outputParameters[ 4 ].value = shrinkTo2ndMark;     // Shrink to 2nd Mark
        bend.outputParameters[ 4 ].enabled = true;
        bend.outputParameters[ 5 ].enabled = false;             // Shrink To Center
        bend.outputParameters[ 6 ].value = shrinkTotal;         // Total Shrink

        // Start Conduit
        bend.conduitOrder.Add( new Marker( 0f, Vector3.forward, Vector3.up ) );

        // Calculate Bend Marks
        float mark_1 = 0.3048f;  // 12 in
        float mark_2 = mark_1 + Lhb + Lhs;
        float mark_3 = mark_2 + Lhb * 2f + Lhs;

        Vector3 rdl_1 = Vector3.up;
        Vector3 fwd_1 = Vector3.forward;
        Vector3 axis = Vector3.Cross( rdl_1, fwd_1 ).normalized;
        Vector3 rdl_2 = -Calculator.RotateCCW( halfAngleDeg, axis, rdl_1);
        rdl_2.Normalize();
        Vector3 fwd_2 = Calculator.RotateCCW( halfAngleDeg, axis, fwd_1 );
        fwd_2.Normalize();
        Vector3 rdl_3 = -Calculator.RotateCW( centerAngleDeg, axis, rdl_2 );
        rdl_3.Normalize();
        Vector3 fwd_3 = Calculator.RotateCW( centerAngleDeg, axis, fwd_2 );
        fwd_3.Normalize();

        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_1, fwd_1, rdl_1, halfAngleDeg, benderRadiusM ) );
        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_2, fwd_2, rdl_2, centerAngleDeg, benderRadiusM ) );
        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_3, fwd_3, rdl_3, halfAngleDeg, benderRadiusM ) );

        // End Conduit
        bend.conduitOrder.Add( new Marker( mark_3 + Calculator.Lb( benderRadiusM, halfAngleRad ) + 0.3048f, fwd_1, rdl_1 ) );
    }

    public static void Calculate4PointSaddle( Bend bend )
    {
        // Write up a Conduit Order
        bend.conduitOrder.Clear();

        // Get Input Parameters
        float angleDeg = (float) bend.inputParameters[0].value;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float saddleHeightM = (float) bend.inputParameters[ 1 ].value;
        float saddleLengthM = (float) bend.inputParameters[ 2 ].value;
        float benderRadiusM = Engine.benderRadiusM;

        // Calculate Values
        float Vs = saddleHeightM - 2f * Calculator.Vb( benderRadiusM, angleRad );
        float Lb = Calculator.Lb( benderRadiusM, angleRad );
        float Ls = Calculator.Ls( Vs, angleRad );

        // Check Distance Between Bends
        string message = null;
        if (Ls < 0f) {
            Ls = 0f;
            message = BendMessages.k_BendsTooClose;
        }
        bend.alert = message;

        float Hb = Calculator.Hb( benderRadiusM, angleRad );
        float distBetween = Lb + Ls;
        float dist2ndTo3rd = Lb + saddleLengthM;
        float shrinkToCenter = (2f * Lb + Ls) - (2f * Hb + Calculator.Hs( Ls, angleRad ));


        // Set Output Parameters
        bend.outputParameters[ 0 ].value = distBetween;  // Distance Between
        bend.outputParameters[ 1 ].value = dist2ndTo3rd;  // Distance 2nd to 3rd
        bend.outputParameters[ 2 ].value = shrinkToCenter * 2f;  // Total Shrink
        bend.outputParameters[ 3 ].value = shrinkToCenter; // Shrink To Center

        // Start Conduit
        bend.conduitOrder.Add( new Marker( 0f, Vector3.forward, Vector3.up ) );

        // Calculate Bend Marks
        float mark_1 = 0.3048f;  // 12 in
        float mark_2 = mark_1 + distBetween;
        float mark_3 = mark_2 + dist2ndTo3rd;
        float mark_4 = mark_3 + distBetween;

        Vector3 rdl_1 = Vector3.up;
        Vector3 fwd_1 = Vector3.forward;
        Vector3 axis = Vector3.Cross( rdl_1, fwd_1 ).normalized;
        Vector3 rdl_2 = -Calculator.RotateCCW( angleDeg, axis, rdl_1 );
        rdl_2.Normalize();
        Vector3 fwd_2 = Calculator.RotateCCW( angleDeg, axis, fwd_1 );
        fwd_2.Normalize();
        Vector3 rdl_3 = Calculator.RotateCW( angleDeg, axis, rdl_2 );
        rdl_3.Normalize();
        Vector3 fwd_3 = Calculator.RotateCW( angleDeg, axis, fwd_2 );
        fwd_3.Normalize();
        Vector3 rdl_4 = -Calculator.RotateCW( angleDeg, axis, rdl_3);
        rdl_4.Normalize();
        Vector3 fwd_4 = Calculator.RotateCW( angleDeg, axis, fwd_3 );
        fwd_4.Normalize();

        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_1, fwd_1, rdl_1, angleDeg, benderRadiusM ) );
        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_2, fwd_2, rdl_2, angleDeg, benderRadiusM ) );
        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_3, fwd_3, rdl_3, angleDeg, benderRadiusM ) );
        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_4, fwd_4, rdl_4, angleDeg, benderRadiusM ) );

        // End Conduit
        bend.conduitOrder.Add( new Marker( mark_4 + Lb + 0.3048f, fwd_1, rdl_1 ) );
    }


    public static void CalculateParallelKick( Bend bend )
    {
        // Write up a Conduit Order
        bend.conduitOrder.Clear();

        // Get Input Parameters
        float angleDeg = (float) bend.inputParameters[0].value;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float spacingM = (float) bend.inputParameters[ 1 ].value;   // Center to center distance between the conduits 
        float kickDistM = (float) bend.inputParameters[ 2 ].value;  // Kick Offset
        float benderRadiusM = Engine.benderRadiusM;

        // Calculate Values
        float Lb = Calculator.Lb( benderRadiusM, angleRad );
        float Lb_90 = Calculator.Lb( benderRadiusM, Calculator.k_HalfPi );
        float Hb_90 = Calculator.Hb( benderRadiusM, Calculator.k_HalfPi );
        float shiftM = spacingM / Mathf.Tan(angleRad) + spacingM * Mathf.Tan(angleRad * 0.5f);
        float spreadM = spacingM / Mathf.Sin(angleRad);

        float firstMarkM = (kickDistM - Calculator.Bv( benderRadiusM, angleRad )) / Mathf.Sin(angleRad);
        float kickTravelM = Mathf.Max(0f, Calculator.Bh( benderRadiusM, angleRad ) + firstMarkM * Mathf.Cos( angleRad )); 
        float developedLengthM = Lb_90 + Lb;

        // Check Values
        string message = null;
        if(Hb_90 > firstMarkM) {
            firstMarkM = Hb_90;
            message = BendMessages.k_BendsTooClose;
        }
        bend.alert = message;

        // Set Output Parameters
        bend.outputParameters[ 0 ].value = kickTravelM;  // Kick Travel
        bend.outputParameters[ 1 ].value = spreadM;      // Kick Spread
        bend.outputParameters[ 2 ].value = firstMarkM;   // Kick First Mark
        bend.outputParameters[ 3 ].value = shiftM;       // Shift
        bend.outputParameters[ 4 ].value = developedLengthM;  // Developed Length

        // Start Conduit
        bend.conduitOrder.Add( new Marker( 0f, Vector3.forward, Vector3.up ) );

        // Calculate Bend Marks (Arbitrary Start point)
        float mark_1 = 0.3048f;  // 12 in
        float mark_2 = mark_1 + Lb + (firstMarkM - Hb_90);

        Vector3 rdl_1   = Vector3.up;
        Vector3 fwd_1   = Vector3.forward;
        Vector3 fwd_2   = Calculator.RotateCCW( angleDeg, Vector3.right, fwd_1 );
        Vector3 rdl_2   = Calculator.RotateCCW( angleDeg, Vector3.right, rdl_1 );
                rdl_2   = Calculator.RotateCW( 90f, fwd_2, rdl_2 );
        Vector3 axis    = Vector3.Cross( rdl_2, fwd_2 ).normalized;
        Vector3 rdl_3   = Calculator.RotateCCW( 90f, axis, rdl_2 );
        Vector3 fwd_3   = Calculator.RotateCCW( 90f, axis, fwd_2 );

        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_1, fwd_1, rdl_1, angleDeg, benderRadiusM ) );
        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_2, fwd_2, rdl_2, 90f, benderRadiusM ) );

        // End Conduit
        bend.conduitOrder.Add( new Marker( mark_2 + Lb_90 + 0.3048f, fwd_3, rdl_3 ) );
    }

    public static void CalculateStubUp( Bend bend )
    {
        // Write up a Conduit Order
        bend.conduitOrder.Clear();

        // Get Input Parameters
        float angleDeg = 90f;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float stubLengthM = (float) bend.inputParameters[0].value;
        float benderRadiusM = Engine.benderRadiusM;

        // Calculate Values
        float Lb = Calculator.Lb( benderRadiusM, angleRad );
        float Vb = Calculator.Vb( benderRadiusM, angleRad );
        //float Hb = Calculator.Hb( benderRadiusM, angleRad );
        float stubTakeUp = Vb + Engine.conduitDiameterM * 0.5f;
        float distFromEnd = stubLengthM - stubTakeUp;
        float lengthOfBend = Calculator.Lb( benderRadiusM, angleRad );

        // Check Values
        string message = null;
        if(stubLengthM < Vb) {
            message = BendMessages.k_StubLengthTooSmall;
            distFromEnd = 0f;
        }
        bend.alert = message;

        // Set Output Parameters
        bend.outputParameters[ 0 ].value = distFromEnd;  // Distance from End
        bend.outputParameters[ 1 ].value = stubTakeUp;   // Stub TakeUp
        bend.outputParameters[ 2 ].value = lengthOfBend; // Length of Bend

        // Start Conduit
        bend.conduitOrder.Add( new Marker( 0f, Vector3.forward, Vector3.up ) );

        // Calculate Bend Marks (Arbitrary Start point)
        float mark_1 = 0.3048f;  // 12 in

        Vector3 rdl_1   = Vector3.up;
        Vector3 fwd_1   = Vector3.forward;
        //Vector3 axis    = Vector3.Cross( rdl_1, fwd_1 ).normalized;

        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_1, fwd_1, rdl_1, angleDeg, benderRadiusM ) );

        // End Conduit
        bend.conduitOrder.Add( new Marker( mark_1 + Lb + distFromEnd, rdl_1, -fwd_1 ) );
    }

    /*#############################################

            Segmented Calculate Functions

    #############################################*/
    public static void CalculateSegmented( Bend bend )
    {
        // Segmented Calculation Method
        GlobalEnum.ESegmentedBendMethod method = (GlobalEnum.ESegmentedBendMethod) bend.inputParameters[3].value;

        switch (method) {
            case GlobalEnum.ESegmentedBendMethod.Simple:
                CalculateSegmentedSimple( bend );
                break;
            case GlobalEnum.ESegmentedBendMethod.Accurate:
                CalculateSegmentedAccurate( bend );
                break;
            default:
                Debug.LogError( "ConduitGenerator: GenSegmented() Invalid Segmented Bend Type." );
                break;
        }
    }

    private static float CalculateSegmentedAccurateHb( float M, float benderRadiusM, float anglePerHalfBendRad, int bendCount )
    {
        float currAngle = anglePerHalfBendRad;
        float totalHorizontalLength = 0f;
        for (int b = 0; b < bendCount - 1; ++b) {
            totalHorizontalLength += Calculator.Hs( M, currAngle );
            currAngle += anglePerHalfBendRad * 2f;
        }
        return totalHorizontalLength + Calculator.Hb( benderRadiusM, currAngle - anglePerHalfBendRad );
    }

    /// <summary>
    /// Calculate the Horizontal Length of a Simple Segmented Bend
    /// </summary>
    private static float CalculateSegmentedSimpleHb( float bendDistEndToStart, float benderRadiusM, float anglePerBendRad, int bendCount )
    {
        float currAngle = anglePerBendRad;
        float totalHorizontalLength = 0f;
        float Hs;
        for (int b = 0; b < bendCount - 1; ++b) {
            Hs = Calculator.Hs( bendDistEndToStart, currAngle );
            totalHorizontalLength += Hs;
            currAngle += anglePerBendRad;
        }
        return totalHorizontalLength + Calculator.Hb( benderRadiusM, anglePerBendRad * bendCount );
    }
    //private void CalcSegmentedHbAndM(List<CenterlineMarker> centerline, List<CenterlineIndice> indices) {
    //    CenterlineMarker start = centerline[ indices[0].index ];
    //    CenterlineMarker end = centerline[indices[indices.Count - 1].index];
    //    Vector3 projected = Vector3.Project( (end.point - start.point), start.forwardDir );
    //    Debug.Log( "CalcSegmentedHbAndM() Total Horizontal Length: " + projected.magnitude 
    //        + " Bend Dist End to Start: " + Vector3.Distance(centerline[indices[1].index].point, centerline[indices[2].index].point) );
    //}

    ///// <summary>
    ///// Optimizes the value of the distance between the end of a bend to the start of the next bend, in order to achieve the desired Radius.
    ///// </summary>
    private static float OptimizeSegmentedSimple( float startingDistBetweenBends, float desiredRadiusM, float benderRadiusM, float anglePerBendRad, int bendCount )
    {

        float totalSin = Mathf.Sin( anglePerBendRad * bendCount );
        float radius = CalculateSegmentedSimpleHb( startingDistBetweenBends, benderRadiusM, anglePerBendRad, bendCount ) / totalSin;
        float optValue = startingDistBetweenBends;
        float step = optValue;
        int maxIterations = 1000;
        while (Mathf.Abs( radius - desiredRadiusM ) > k_SegmentedSimpleAccuracy && --maxIterations > 0) {
            if (radius > desiredRadiusM) {
                step *= 0.5f;
                optValue -= step;
            } else {
                optValue += step;
            }
            radius = CalculateSegmentedSimpleHb( optValue, benderRadiusM, anglePerBendRad, bendCount ) / totalSin;
        }
        return optValue;
    }

    /// <summary>
    /// This method of segmented conduit bending uses the traditional method of calculating the legnth of the arc
    /// of the bend and dividing by the number of bends. This however is extremely inaccurate for smaller numbers
    /// of bends and thus we use a simple hill climbing algorithm below to optimize the distance between bends.
    /// This method is Simple, in that bends are equidistant from each other and are all at equal angles.
    /// Note: Could we use a Derivative?
    /// </summary>
    public static void CalculateSegmentedSimple( Bend bend )
    {
        // Write up a Conduit Order
        bend.conduitOrder.Clear();

        // Get Input Parameters
        float angleDeg = (float) bend.inputParameters[0].value;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        int   bendCount = (int) bend.inputParameters[1].value;
        float desiredRadiusM = (float) bend.inputParameters[2].value;
        float benderRadiusM = Engine.benderRadiusM;

        // Calculate Values
        float angleDeg_s = angleDeg / bendCount;
        float angleRad_s = angleRad / bendCount;
        float Lb_s = Calculator.Lb( benderRadiusM, angleRad_s );
        float lbTotal = desiredRadiusM * angleRad;
        float distBetweenBend = lbTotal / bendCount;

        // Check Values
        string message = null;
        if(desiredRadiusM <= benderRadiusM) {
            message = BendMessages.k_SegmentedRadiusTooSmall;
            distBetweenBend = Lb_s;
        } else if(distBetweenBend < Lb_s) {
            message = BendMessages.k_BendsTooClose;
            distBetweenBend = Lb_s;
        } else {
            distBetweenBend = OptimizeSegmentedSimple( distBetweenBend, desiredRadiusM, benderRadiusM, angleRad_s, bendCount ) + Lb_s;
        }
        bend.alert = message;
        float developedLength = distBetweenBend * (bendCount - 1) + Lb_s;

        // Set Output Parameters
        bend.outputParameters[ 0 ].enabled = false; // Angle 1st 
        bend.outputParameters[ 1 ].value = Units.Round(angleDeg_s, 2);  // Angle Degrees
        bend.outputParameters[ 2 ].enabled = false; // Angle Last
        bend.outputParameters[ 3 ].value = distBetweenBend;  // Distance Between
        bend.outputParameters[ 4 ].enabled = false; // Distance to 2nd
        bend.outputParameters[ 5 ].value = developedLength;  // Developed Length

        // Start Conduit
        bend.conduitOrder.Add( new Marker( 0f, Vector3.forward, Vector3.up ) );
        
        // Calculate Bend Marks 
        float mark_n = 0.3048f;  // 12 in

        Vector3 rdl_n = Vector3.up;
        Vector3 fwd_n = Vector3.forward;
        Vector3 axis = Vector3.Cross( rdl_n, fwd_n ).normalized;

        // In Between Bends
        for (int b = 0; b < bendCount; ++b) {
            bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_n, fwd_n, rdl_n, angleDeg_s, benderRadiusM ) );
            mark_n += distBetweenBend;
            fwd_n = Calculator.RotateCCW( angleDeg_s, axis, fwd_n );
            rdl_n = Calculator.RotateCCW( angleDeg_s, axis, rdl_n );
        }

        // End Conduit
        bend.conduitOrder.Add( new Marker( (mark_n - distBetweenBend) + Lb_s + 0.3048f, fwd_n, rdl_n ) );
    }

    ///// <summary>
    ///// This method of segmented conduit bending is the most accurate in that each bend is located on the 'actual'
    ///// circumference of the circle created by the desired radius. It does however, require a few extra calculations
    ///// and the first and last angles bent at half the angle of the in-between bends.
    ///// </summary>
    public static void CalculateSegmentedAccurate( Bend bend )
    {
        // Write up a Conduit Order
        bend.conduitOrder.Clear();

        // Get Input Parameters
        float angleDeg = (float) bend.inputParameters[0].value;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        int   bendCount = (int) bend.inputParameters[1].value;
        float desiredRadiusM = (float) bend.inputParameters[2].value;
        float benderRadiusM = Engine.benderRadiusM;

        // Calculate Values
        float angleDeg_b = angleDeg / (bendCount - 1);
        float angleRad_b = angleRad / (bendCount - 1);
        float angleDeg_s = angleDeg_b * 0.5f;
        float angleRad_s = angleRad_b * 0.5f;
        float lbPerBend_s = benderRadiusM * angleRad_s;
        float H_S = benderRadiusM * Mathf.Sin( angleRad_s );        // Horizontal Length of Bend
        float V_S = benderRadiusM * (1 - Mathf.Cos( angleRad_s ));  // Vertical Height of Bend
        float S = desiredRadiusM - V_S;
        float A_M = angleRad_b - Mathf.Atan( H_S / S ) * 2f;        // Angle of Chord
        float M = 2f * desiredRadiusM * Mathf.Sin( 0.5f * A_M );    // Chord Length

        float distBetweenBend = M + lbPerBend_s * 2f;
        float distTo2nd = distBetweenBend - lbPerBend_s;

        // Check Values
        string message = null;
        if (desiredRadiusM <= benderRadiusM) {
            message = BendMessages.k_SegmentedRadiusTooSmall;
            distBetweenBend = lbPerBend_s * 2f;
            distTo2nd = lbPerBend_s;
        } else if (M < 0f) {
            message = BendMessages.k_BendsTooClose;
            distBetweenBend = lbPerBend_s * 2f;
            distTo2nd = lbPerBend_s;
        } else if(bendCount == 2) {
            message = BendMessages.k_AtLeast3Bends;
        }
        bend.alert = message;
        float developedLength = distTo2nd + distBetweenBend * (bendCount - 2) + lbPerBend_s;

        // Set Output Parameters
        bend.outputParameters[ 0 ].enabled = true; 
        bend.outputParameters[ 0 ].value = Units.Round( angleDeg_s, 2 );  // Angle 1st 
        bend.outputParameters[ 1 ].value = Units.Round( angleDeg_b, 2 );  // Angle Degrees
        bend.outputParameters[ 2 ].enabled = true; 
        bend.outputParameters[ 2 ].value = Units.Round( angleDeg_s, 2 );  // Angle Last
        bend.outputParameters[ 3 ].value = distBetweenBend;  // Distance Between
        bend.outputParameters[ 4 ].enabled = true;
        bend.outputParameters[ 4 ].value = distTo2nd; // Distance to 2nd
        bend.outputParameters[ 5 ].value = developedLength;  // Developed Length


        // Start Conduit
        bend.conduitOrder.Add( new Marker( 0f, Vector3.forward, Vector3.up ) );

        // Calculate Bend Marks 
        float mark_n = 0.3048f;  // 12 in

        Vector3 rdl_n = Vector3.up;
        Vector3 fwd_n = Vector3.forward;
        Vector3 axis = Vector3.Cross( rdl_n, fwd_n ).normalized;

        // First Bend
        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_n, fwd_n, rdl_n, angleDeg_s, benderRadiusM ) );

        mark_n += distTo2nd;
        fwd_n = Calculator.RotateCCW( angleDeg_s, axis, fwd_n );
        rdl_n = Calculator.RotateCCW( angleDeg_s, axis, rdl_n );

        // In Between Bends
        for (int b = 1; b < bendCount - 1; ++b) {
            bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_n, fwd_n, rdl_n, angleDeg_b, benderRadiusM ) );
            mark_n += distBetweenBend;
            fwd_n = Calculator.RotateCCW( angleDeg_b, axis, fwd_n );
            rdl_n = Calculator.RotateCCW( angleDeg_b, axis, rdl_n );
        }

        // Last Bend
        bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_n, fwd_n, rdl_n, angleDeg_s, benderRadiusM ) );

        // End Conduit
        bend.conduitOrder.Add( new Marker( mark_n + lbPerBend_s + 0.3048f, fwd_n, rdl_n ) );
    }

}
