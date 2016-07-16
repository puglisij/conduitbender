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
    public static void AddBendFactoryDelegate( string type, BendFactoryDelegate method )
    {
        if (m_BendMakers.ContainsKey( type )) {
            m_BendMakers.Remove( type );
        }
        m_BendMakers.Add( type, method );
    }
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
    /// <param name="type"></param>
    /// <returns></returns>
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
        inputs.Add( new BendParameter( BendParameter.Name.AngleDegrees, BendParameter.Type.FloatAngle, colors.inputParameterDefault, 0f ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( BendParameter.Name.DistanceBetween, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );

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
        inputs.Add( new BendParameter( BendParameter.Name.AngleDegrees, BendParameter.Type.FloatAngle, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( BendParameter.Name.OffsetHeight, BendParameter.Type.Float, colors.inputParameterDefault, 0f ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( BendParameter.Name.DistanceBetween, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.TotalShrink, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );

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
        inputs.Add( new BendParameter( BendParameter.Name.AngleDegrees, BendParameter.Type.FloatAngle, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( BendParameter.Name.Spacing, BendParameter.Type.Float, colors.inputParameterDefault, 0f ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( BendParameter.Name.Shift, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );

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
        inputs.Add( new BendParameter( BendParameter.Name.AngleDegrees, BendParameter.Type.FloatAngle, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( BendParameter.Name.Rise, BendParameter.Type.Float, colors.flagBlue, 0f ) );
        inputs.Add( new BendParameter( BendParameter.Name.Roll, BendParameter.Type.Float, colors.flagPurple, 0f ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( BendParameter.Name.DistanceBetween, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.TotalShrink, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.RollAngleDegrees, BendParameter.Type.FloatAngle, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.OffsetHeight, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );

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
        inputs.Add( new BendParameter( BendParameter.Name.CenterAngleDegrees, BendParameter.Type.FloatAngle, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( BendParameter.Name.SaddleHeight, BendParameter.Type.Float, colors.inputParameterDefault, 0f ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( BendParameter.Name.LengthOfCenterBend, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.DistanceBetween, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.ShrinkToCenter, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.TotalShrink, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );

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
        inputs.Add( new BendParameter( BendParameter.Name.AngleDegrees, BendParameter.Type.FloatAngle, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( BendParameter.Name.SaddleHeight, BendParameter.Type.Float, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( BendParameter.Name.SaddleLength, BendParameter.Type.Float, colors.inputParameterDefault, 0f ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( BendParameter.Name.DistanceBetween, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.Distance2ndTo3rd, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.TotalShrink, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.ShrinkToCenter, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );

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
        inputs.Add( new BendParameter( BendParameter.Name.SegmentedAngle, BendParameter.Type.FloatAngle, colors.flagBlue, 0f ) );
        inputs.Add( new BendParameter( BendParameter.Name.SegmentedCount, BendParameter.Type.Integer, colors.inputParameterDefault, 3 ) );
        inputs.Add( new BendParameter( BendParameter.Name.SegmentedRadius, BendParameter.Type.Float, colors.flagBlue, 0f ) );
        inputs.Add( new BendParameter( BendParameter.Name.SegmentedMethod, BendParameter.Type.StringEnum, colors.inputParameterDefault,
            GlobalEnum.SegmentedBendMethod.Last(), GlobalEnum.SegmentedBendMethod ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( BendParameter.Name.AngleFirstDegrees, BendParameter.Type.FloatAngle, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.AngleDegrees, BendParameter.Type.FloatAngle, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.AngleLastDegrees, BendParameter.Type.FloatAngle, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.DistanceBetween, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.DistanceTo2nd, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.DevelopedLength, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );

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
        inputs.Add( new BendParameter( BendParameter.Name.StubLength, BendParameter.Type.Float, colors.inputParameterDefault, 0f ) );
        //inputs.Add( new BendParameter( BendParameter.Name.StubMethod, BendParameter.Type.StringEnum,
        //    GlobalEnum.StubUpMethod.First(), GlobalEnum.StubUpMethod ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( BendParameter.Name.DistanceFromEnd, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.StubTakeUp, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );

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
        inputs.Add( new BendParameter( BendParameter.Name.AngleDegrees, BendParameter.Type.FloatAngle, colors.inputParameterDefault, 0f ) );
        inputs.Add( new BendParameter( BendParameter.Name.Spacing, 
            BendParameter.Type.Float, colors.inputParameterDefault, Engine.conduitDiameterM * 2f ) );
        inputs.Add( new BendParameter( BendParameter.Name.KickOffset,
            BendParameter.Type.Float, colors.inputParameterDefault, Engine.conduitDiameterM * 2f ) );

        List<BendParameter> outputs = new List<BendParameter>();
        outputs.Add( new BendParameter( BendParameter.Name.KickTravel, BendParameter.Type.Float, colors.flagBlue, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.KickSpread, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.KickFirstMark, BendParameter.Type.Float, colors.flagGreen, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.Shift, BendParameter.Type.Float, colors.flagPurple, 0f ) );
        outputs.Add( new BendParameter( BendParameter.Name.DevelopedLength, BendParameter.Type.Float, colors.outputParameterDefault, 0f ) );

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
        if(Vs < 0f) {
            Lhs = 0f;
            message = BendMessages.k_BendsTooClose;
        }
        bend.alert = message;

        // Set Output Parameters
        bend.outputParameters[ 0 ].value = 2f * Lhb;  // Length Of Center Bend
        bend.outputParameters[ 1 ].value = distBetween;  // Distance Between
        bend.outputParameters[ 2 ].value = halfShrink;       // Shrink To Center
        bend.outputParameters[ 3 ].value = halfShrink * 2f;  // Shrink
        

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

        //// Start Conduit
        //bend.conduitOrder.Add( new Marker( 0f, Vector3.right, Vector3.up ) );

        //// Calculate Bend Marks (Arbitrary Start point)
        //float mark_1 = 0.3048f;  // 12 in
        //float mark_2 = mark_1 + Lb_90 + (firstMarkM - Hb_90); 

        //Vector3 rdl_1   = Calculator.RotateCCW( 90f - angleDeg, Vector3.right, Vector3.forward );
        //Vector3 fwd_1   = Vector3.right;
        //Vector3 axis    = Vector3.Cross( rdl_1, fwd_1 ).normalized;
        //Vector3 rdl_2   = Calculator.RotateCCW( 90f, Vector3.right, rdl_1 );
        //Vector3 fwd_2   = rdl_1;
        //Vector3 rdl_3   = Vector3.forward;
        //Vector3 fwd_3   = Vector3.up;

        //bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_1, fwd_1, rdl_1, 90f, benderRadiusM ) );
        //bend.conduitOrder.Add( new BendMarker( BendFlagType.Arrow, mark_2, fwd_2, rdl_2, angleDeg, benderRadiusM ) );

        //// End Conduit
        //bend.conduitOrder.Add( new Marker( mark_2 + Lb + 0.3048f, fwd_3, rdl_3 ) );
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

        // Check Values
        string message = null;
        if(stubLengthM < Vb) {
            message = BendMessages.k_StubLengthTooSmall;
            distFromEnd = 0f;
        }
        bend.alert = message;

        // Set Output Parameters
        bend.outputParameters[ 0 ].value = distFromEnd;  // Distance from End
        bend.outputParameters[ 1 ].value = stubTakeUp;  // Stub TakeUp

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