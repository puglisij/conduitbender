using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;


public class ConduitGenerator : MonoBehaviour
{

    [SerializeField]
    public MeshFilter   circleMeshFilter;
    public static Mesh  circleMesh;

    public static float conduitDiameterM
    {
        get { return s_ConduitDiameterM; }
        set
        {
            if(s_ConduitDiameterM == value) { return; }

            s_ConduitDiameterM = value;
            ReCalculate();
        }
    }
    public static float degreesPerVerticeSet
    {
        get { return s_DegreesPerVerticeSet; }
        set
        {
            s_DegreesPerVerticeSet = value;
        }
    }
    public static int numberOfSides
    {
        get { return s_NumberOfSides; }
        set
        {
            s_NumberOfSides = value;
        }
    }

    [HideInInspector]
    public ConduitGenerator        instance;
    /*----------------------------------------
                    Private
    ----------------------------------------*/

    [SerializeField]
    private static float            s_ConduitDiameterM;
    [SerializeField]
    private static float            s_DegreesPerVerticeSet;
    [SerializeField]
    private static int              s_NumberOfSides;

    private static Vector3[]        s_CircleVertices = null;
    private static Vector3[]        s_LastConduitVerts;         // Vertices of last generated conduit
    private static Vector2[]        s_LastConduitUVs;
    private static int[]            s_LastConduitTris;

    /*----------------------------------------
                    NOTES
    Conduit Bender Marks:
        Arrow: Start of Bends
        Star:  Back of Bends (i.e. from here to the other side of the notch of the bender is the diameter of the EMT)
        Tear drop / Notch: The exact center of a 45 degree bend  (NOT the center of other bends)

    Circle:
        Circumference = 2(pi)r
        Area = (pi)(r^2)
        Arc Length (angle in Radians) = angle * r

    Circular Segment:
        Let A be the angle of the sector, in radians.
        The arc length is:
        s = R * A
        Let h be the height of the arced portion, and r be the height of the triangular portion. 
        R, the radius, is then:
        R = h + r
        Let a be the chord length:
        a = 2Rsin(0.5 * A)
          = 2rtan(0.5 * A)
          = 2sqrt(R^2 - r^2)
          = 2sqrt(h(2R - h))
        r = Rcos(0.5 * A)
          = 0.5 * a * cot(0.5 * A)
          = 0.5 * sqrt(4R^2 - a^2)

    Cylinder:
        Volumes = (pi)(r^2)(h)
        Surface Area = 2(pi)(r)(h) + 2(pi)(r^2)

    Bend Geometry:
        Vb = R - (R)Cos(A)
        Es = [(R)Sin(A/2)] / [Cos(A/2)]
        Hb = (R)Sin(A)
        Hs = (Ls)Cos(A)
        Ls = (Vs) / Sin(A)
        Lb = [(Pi)(R)A] / 180
                                            .   
                                           /'   ' . Pipe End                        
                                                 /:     ^
                    ........C           Ls      / :
                    ^       |' .               /  :     Vs
                (R)Cos(A)   | A  ' . /        /   :
                    v       |        ' .     /    :     v
                    ......R |............'../ N........... 
                    ^       |            .*       :
                    Vb      |        . *  /       :
  Pipe Start________v_______|.   *......./ M      :
                            |<    Es    >|<  Hs  >:
                            P

            C - Center of Circle forming the Pipe Bender
            R - Centerline Radius (Radius of Bender)
            P - Start of bend
            N - End of bend

            If you draw a line between C and M, you bisect angle A resulting in angle (A/2) on either side
            Lb - The length of the Arc(PN) of the Bend 
            Vs - The straight segment part of the offset of the Bend (Offset being = Vb + Vs + height of next bend if there is one)
            Vb - Essentially the vertical height of the actual bend


            Vector3.Cross()
            ---------------
            To determine the direction, use the left hand rule, where
            the 1st parameter to Vector3.Cross() would be the thumb (1), 
            and the 2nd parameter would be the index finger (2).
            Alternatively, using the Left hand Rule, if the 1st parameter is the 12 o'clock position,
            and the 2nd parameter is the 3 o'clock position, the direction of
            the Cross Product would point 'out of' the Clock. 
            Alternatively, using the Right hand Rule, if the 1st parameter is the 12 o'clock position,
            and the 2nd parameter is the 9 o'clock position, the direction of 
            the Cross Product woudl point 'into' the Clock.
            You can think of this as "Left Loosy" and "Righty Tighty"

            Quaternion.AngleAxis(angle, axis)
            ---------------------------------
            If looking in the opposite direction of the axis vector:
            Negative angle results in counterclockwise rotation 
            Positive angle results in clockwise rotation

    ----------------------------------------*/

    void Awake()
    {
        // Singleton
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Debug.LogError( "ConduitGenerator: Awake() Only one instance of ConduitGenerator should exist in the scene." );
            Destroy( gameObject );
            return;
        }

        circleMesh = circleMeshFilter.mesh;

    }


    /*------------------------------

            Private Functions

    ------------------------------*/
    /// <summary>
    /// Should be called after changing any member variables of this class.
    /// </summary>
    public static void Initialize()
    {
        ReCalculate();
    }
    /// <summary>
    /// Forces a Re-Calculation on internal Circle Mesh
    /// Should be called when Conduit Diameter, or Degrees per Vertice Settings Change
    /// </summary>
    private static void ReCalculate()
    {
        circleMesh.Clear();

        // Make Circle Mesh
        Vector3[]   circleVerts = new Vector3[s_NumberOfSides + 2];
        Vector2[]   circleUVs   = new Vector2[s_NumberOfSides + 2];
        int[]       circleTriangles = new int[s_NumberOfSides * 3]; // Indices
        float       radPerVert  = (Mathf.PI * 2f) / s_NumberOfSides;
        float       radius      = s_ConduitDiameterM * 0.5f;

        // We can calculate the verts clockwise by using Sin for x and Cos for y
        // @TODO - To create an inner mesh for our conduit, so we don't see through the ends
        // We can simply reverse Sin, Cos to Cos, Sin for the X and Y coordinates of the vertices 
        // and use the same algorithm below for calculating indices and such
        circleVerts[ 0 ] = Vector3.zero;
        for (int s = 0; s < circleVerts.Length - 1; ++s) {
            circleVerts[ s + 1 ].x = Mathf.Sin( radPerVert * s ) * radius;
            circleVerts[ s + 1 ].y = Mathf.Cos( radPerVert * s ) * radius;
        }
        for (int i = 0, c; i < s_NumberOfSides; ++i) {
            c = i * 3;
            circleTriangles[ c ] = 0;            // The Center of the Circle 
            circleTriangles[ c + 1 ] = i + 2;
            circleTriangles[ c + 2 ] = i + 1;
        }
        for (int i = 0; i < circleUVs.Length; ++i) {
            circleUVs[ i ] = new Vector2( circleVerts[ i ].x, circleVerts[ i ].y );
        }

        s_CircleVertices = circleVerts;
        circleMesh.vertices = circleVerts;
        circleMesh.uv = circleUVs;
        circleMesh.triangles = circleTriangles;
        circleMesh.RecalculateNormals();
        circleMesh.RecalculateBounds();
    }


    /*------------------------------

            Public Functions

    ------------------------------*/

    /// <summary>
    /// Copies the given mesh data (vertices, UVs, triangles) into the given Mesh
    /// Start and End indices are inclusive.
    /// </summary>
    public static void CopyConduitPartial( Vector3[] verts, Vector2[] uvs, int[] tris, Mesh toMesh, int startCenterlineIndice, int endCenterlineIndice )
    {
        // Important to Clear (To Avoid Out of Bounds Errors)
        toMesh.Clear();

        // Get Vertice, UV, Triangle Counts
        int sides = Engine.conduitSideCount;
        int pointCount = (endCenterlineIndice - startCenterlineIndice) + 1;
        int vertCount = sides * pointCount;
        int uvCount = sides * pointCount;
        int triCount = sides * 6 * (pointCount - 1);  // 3 per triangle

        int vertStart = sides * startCenterlineIndice;
        int uvStart = vertStart;
        int triStart = vertStart * 6;
        int triOffset = startCenterlineIndice * 6;

        // Allocate Vertices, UVs, Triangles
        Vector3[]   c_verts = new Vector3[vertCount];
        Vector2[]   c_uvs   = new Vector2[uvCount];
        int[]       c_tris  = new int[triCount];

        // Copy Vertices, UVs
        //System.Array.Copy( norms, vertStart, c_norms, 0, vertCount );
        System.Array.Copy( verts, vertStart, c_verts, 0, vertCount );
        System.Array.Copy( uvs, uvStart, c_uvs, 0, uvCount );
        // Copy Triangles (Adjust Triangle Indice)
        for (int t = 0; t < triCount; ++t) {
            c_tris[ t ] = tris[ triStart + t ] - triOffset;
        }

        // NOTES:
        // The number of vertices in the mesh is changed by assigning a vertex array with a different number of vertices. 
        // Note that if you resize the vertex array then all other vertex attributes( normals, colors, tangents, UVs ) 
        // will be automatically resized too.

        // Set Mesh
        //toMesh.normals = c_norms;
        toMesh.vertices = c_verts;
        toMesh.uv = c_uvs;
        toMesh.triangles = c_tris;
        toMesh.RecalculateNormals();
    }
    /// <summary>
    /// @TODO - Currently SLOW! Getting copies of the Meshes current vertices, uv, triangles takes Forever!
    /// Should definitely Not be called every frame!
    /// </summary>
    public static void CopyConduitPartial( Conduit conduit, Mesh toMesh, int startCenterlineIndice, int endCenterlineIndice )
    {
        CopyConduitPartial( conduit.mesh.vertices, conduit.mesh.uv, conduit.mesh.triangles, toMesh, startCenterlineIndice, endCenterlineIndice );
    }
    /// <summary>
    /// Copies the last Generated conduit data into the given Mesh
    /// </summary>
    public static void CopyLastConduitPartial( Mesh toMesh, int startCenterlineIndice, int endCenterlineIndice )
    {
        CopyConduitPartial( s_LastConduitVerts, s_LastConduitUVs, s_LastConduitTris, toMesh, startCenterlineIndice, endCenterlineIndice );
    }

    /// <summary>
    /// Generate a Circular Sector Mesh
    /// Sector Normals will point in the Positive Z-Axis Direction
    /// </summary>
    public static void GenerateSector( Mesh toMesh, float radiusM, float sectorAngleDeg, float degPerVertice )
    {
        toMesh.Clear();

        // Make Circle Mesh
        int         vertCount = (int) Mathf.Ceil( sectorAngleDeg / degPerVertice ) + 2;
        int         sideCount = vertCount - 2;
        Vector3[]   circleVerts = new Vector3[vertCount];
        Vector2[]   circleUVs   = new Vector2[vertCount];
        int[]       circleTriangles = new int[sideCount * 3]; // Indices
        float       radPerVert  = (sectorAngleDeg * Mathf.Deg2Rad) / sideCount;
        float       radius      = radiusM;

        circleVerts[ 0 ] = Vector3.zero;
        for (int s = 0; s < vertCount - 1; ++s) {
            circleVerts[ s + 1 ].x = Mathf.Sin( radPerVert * s ) * radius;
            circleVerts[ s + 1 ].y = Mathf.Cos( radPerVert * s ) * radius;
        }
        for (int i = 0, c; i < sideCount; ++i) {
            c = i * 3;
            circleTriangles[ c ] = 0;            // The Center of the Circle 
            circleTriangles[ c + 1 ] = i + 2;
            circleTriangles[ c + 2 ] = i + 1;
        }
        for (int i = 0; i < vertCount; ++i) {
            circleUVs[ i ] = new Vector2( circleVerts[ i ].x, circleVerts[ i ].y );
        }

        toMesh.vertices = circleVerts;
        toMesh.uv = circleUVs;
        toMesh.triangles = circleTriangles;
        toMesh.RecalculateNormals();
        toMesh.RecalculateBounds();
    }

    /// <summary>
    /// @TODO - Could split this function up to run over multiple frames by turning it into a Coroutine
    /// and calling 'yield return null'
    ///     Could also use Threading
    /// @TODO - Could also set the # of vertices on each bend to a constant value so that we can
    /// avoid reallocating new vertices each time.
    /// </summary>
    public static void GenerateConduit( Conduit conduit )
    {
#if UNITY_EDITOR
        if (conduit.bend.conduitOrder.Count < 2) {
            Debug.LogError( "ConduitGenerator: GenerateConduit() Invalid Conduit Order." );
            return;
        }
#endif
        // Start Fresh
        conduit.Clear();

        var conduitOrder = conduit.bend.conduitOrder;
        var bentCenterline = conduit.centerline;
        var bentCenterlineBendStartIndices = conduit.centerlineBendIndices;
        //var straightCenterline = conduit.centerline;
        //var straightCenterlineBendStartIndices = conduit.centerlineBendIndices;

        Vector3 startPosition = Vector3.zero;
        Vector3 radialCenter;
        Vector3 reverseRadial;

        Quaternion          rot;
        Marker              currConduitMark = null;
        CenterlineMarker    prevCenterMark = new CenterlineMarker( startPosition, conduitOrder[0].forwardDir, conduitOrder[0].radialDir, 0f );
        CenterlineMarker    currCenterMark = new CenterlineMarker();

        float distSoFar = 0f;
        float degPerRotate;
        int numRotations;

        // Record starting centerline point 
        bentCenterline.Add( prevCenterMark );

        //----------------------------------------------------
        // Build the 'Bent' and 'Straight' Conduit Centerline 
        //----------------------------------------------------
        for (int i = 1; i < conduitOrder.Count; ++i) 
        {
            currConduitMark = conduitOrder[ i ];

            // ConduitMarker (Calculate next 'straight segment' centerline vertice)
            currCenterMark.Set(
                prevCenterMark.point + (currConduitMark.distFromStartM - distSoFar) * prevCenterMark.forwardDir,
                currConduitMark.forwardDir,
                currConduitMark.radialDir,
                currConduitMark.distFromStartM
                );
            prevCenterMark.Set( currCenterMark.point, currCenterMark.forwardDir, currCenterMark.radialDir, currCenterMark.distFromStartM );
            distSoFar = currConduitMark.distFromStartM; 


            if (currConduitMark is BendMarker ) 
            {
                // BendMarker
                BendMarker currBendMark = (BendMarker)currConduitMark;

                // Add new Centerline Marker
                bentCenterline.Add( currCenterMark );
                bentCenterlineBendStartIndices.Add( new CenterlineIndice( BendMarkType.Start, bentCenterline.Count - 1 ) );

                radialCenter = (currCenterMark.point + currBendMark.radialDir * currBendMark.radiusM);
                reverseRadial = Vector3.Normalize( currCenterMark.point - radialCenter );
                numRotations = (int)Mathf.Ceil( currBendMark.angleDeg / s_DegreesPerVerticeSet );
                degPerRotate = currBendMark.angleDeg / numRotations;

                // Negative angle results in counterclockwise rotation (which is what we want for our Bender)
                // Positive angle results in clockwise rotation
                rot = Quaternion.AngleAxis( -degPerRotate, Vector3.Cross( currBendMark.radialDir, currBendMark.forwardDir ) ); 
                float lbPerRotate = Lb( currBendMark.radiusM, degPerRotate * Mathf.Deg2Rad );

                // Iterate through entire rotation
                for (int r = 0; r < numRotations; ++r) 
                {
                    reverseRadial = rot * reverseRadial;

                    currCenterMark.Set(
                        radialCenter + reverseRadial * currBendMark.radiusM,
                        rot * prevCenterMark.forwardDir, 
                        rot * prevCenterMark.radialDir,  
                        (distSoFar + lbPerRotate * (r + 1))
                        );
                    prevCenterMark.Set( currCenterMark.point, currCenterMark.forwardDir, currCenterMark.radialDir, currCenterMark.distFromStartM );

                    // Record Centerline Vertices
                    bentCenterline.Add( currCenterMark );

                }
                bentCenterlineBendStartIndices.Add( new CenterlineIndice( BendMarkType.End, bentCenterline.Count - 1 ) );

                // Update centerline distance so far
                distSoFar += Lb( currBendMark.radiusM, currBendMark.angleDeg * Mathf.Deg2Rad );
            } else {
                // Add new Centerline Marker
                bentCenterline.Add( currCenterMark );
            }
        }

        //-------------------------------------------
        // Build the bent 'Mesh'
        //-------------------------------------------
        // To Create SubMeshes:
        // Use Mesh.subMeshCount to set the number of submeshes, then SetTriangles() to set the triangle list for each submesh. 
        // There isn't a "main" mesh in this case. Then, make a Material array (where each material corresponds to each submesh), 
        // and set renderer.materials to that Material array.

        // Make Conduit Vertices
        Vector3[] conduitVerts  = new Vector3[s_NumberOfSides * bentCenterline.Count];
        Vector2[] conduitUVs    = new Vector2[s_NumberOfSides * bentCenterline.Count];
        int[] conduitTriangles  = new int[s_NumberOfSides * 6 * (bentCenterline.Count - 1)]; // Indices

        // Get Reference to Circle Mesh Vertices
        var circleVerts = s_CircleVertices;

        // Move Circle Mesh Along Centerline, Rotate, and Record Vertice Positions
        Vector3 prevForwardDir = Vector3.forward;
        for (int v = 0; v < bentCenterline.Count; ++v) {
            // @TODO - Could make this more efficient by caching the rotation from the centerline calculations
            // @TODO - Could also use the SetFromToRotation instead of allocating new Quaternion each time
            rot = Quaternion.FromToRotation( prevForwardDir, bentCenterline[ v ].forwardDir );

            for (int s = 0; s < s_NumberOfSides; ++s) {
                conduitVerts[ v * s_NumberOfSides + s ] = (rot * circleVerts[ s + 1 ]) + (bentCenterline[ v ].point - bentCenterline[ 0 ].point);
            }
        }
        // B   D
        // |\``|
        // | \ |
        // |__\|
        // A   C
        int C;
        int totalSides = s_NumberOfSides * (bentCenterline.Count - 1);
        for (int s = 0, t; s < totalSides; ++s) {
            //int sideStartI = (s % m_NumberOfSides) + (s / m_NumberOfSides) * m_NumberOfSides; // 4 vertices per side
            t = s * 6;

            if ((s + 1) % s_NumberOfSides == 0) {
                C = (s / s_NumberOfSides) * s_NumberOfSides;
            } else {
                C = s + 1;
            }
            // Bottom Triangle
            conduitTriangles[ t ] = s;                                // A
            conduitTriangles[ t + 1 ] = s + s_NumberOfSides;          // B
            conduitTriangles[ t + 2 ] = C;                            // C 

            // Top Triangle
            conduitTriangles[ t + 3 ] = C;                            // C
            conduitTriangles[ t + 4 ] = s + s_NumberOfSides;          // B
            conduitTriangles[ t + 5 ] = C + s_NumberOfSides;          // D
        }
        // Calculate UVs 
        // (Currently these UVs are stretched/compressed along the bent areas - i.e. inaccurate)
        for (int i = 0; i < conduitUVs.Length; ++i) {
            conduitUVs[ i ] = new Vector2( circleVerts[ i % s_NumberOfSides ].x, (i % s_NumberOfSides) / (float)bentCenterline.Count );
        }

        conduit.SetMesh( conduitVerts, conduitUVs, conduitTriangles );
        conduit.conduitDiameterM = conduitDiameterM;

        // Remember Conduit Data
        s_LastConduitVerts = conduitVerts;
        s_LastConduitUVs = conduitUVs;
        s_LastConduitTris = conduitTriangles;

        //Debug.Log( "ConduitGenerator: GenerateConduit() Model Name: " + conduit.bend.modelName );
    }  // End GenConduitMesh()


    /*------------------------------

            Public Utilities

    ------------------------------*/
    public static bool PointsTowards( Vector3 dir, Vector3 point )
    {
        return Vector3.Dot( dir, point ) > 0f;
    }

    public static float Chord( float radius, float angleRad )
    {
        return 2f * radius * Mathf.Sin( 0.5f * angleRad );
    }
    public static float Vb( float radius, float angleRad )
    {
        return radius - (radius * Mathf.Cos( angleRad ));
    }
    public static float Es( float radius, float angleRad )
    {
        return (radius * Mathf.Sin( angleRad / 2f )) / Mathf.Cos( angleRad / 2f );
    }
    public static float Hb( float radius, float angleRad )
    {
        return radius * Mathf.Sin( angleRad );
    }
    public static float Hs( float Ls, float angleRad )
    {
        return Ls * Mathf.Cos( angleRad );
    }
    public static float Lb( float radius, float angleRad )
    {
        return radius * angleRad;
    }
    public static float Ls( float Vs, float angleRad )
    {
        return Vs / Mathf.Sin( angleRad );
    }

    /// <summary>
    /// If facing the opposite direction of the axis vector, 
    /// Rotation is of given vector is CounterClockwise around given axis by given angle (in degrees)
    /// </summary>
    public static Vector3 RotateCCW( float angleDeg, Vector3 bendAxis, Vector3 vec )
    {
        return Quaternion.AngleAxis( -angleDeg, bendAxis ) * vec;
    }
    public static Vector3 RotateCW( float angleDeg, Vector3 bendAxis, Vector3 vec )
    {
        return Quaternion.AngleAxis( angleDeg, bendAxis ) * vec;
    }




}
