using UnityEngine;
using System.Collections;


public static class GlobalEnum
{
    public enum ESegmentedBendMethod
    {
        [DescriptionValue( "The Accurate Method requires different bend angles for the 1st and Last bend but more closely follows the Segmented Radius." )]
        Accurate = 0,
        [DescriptionValue( "The Simple Method uses equidistant marks and equal angles for all bends but does not follow the Segmented Radius as accurately on fewer numbered bends." )]
        Simple = 1
    }
    public static StringEnum SegmentedBendMethod = new StringEnum(typeof(ESegmentedBendMethod));
    public enum EStubUpMethod
    {
        [DescriptionValue( "Determine Mark by Total Tail Length." )]
        TailLength,
        [DescriptionValue( "Determine Mark by Total Stub Height." )]
        StubLength
    }
    public static StringEnum StubUpMethod = new StringEnum(typeof(EStubUpMethod));
    //public enum ERolledOffsetMethod
    //{
    //    [DescriptionValue("Input the angle of the bends...")]
    //    InputBendAngle,
    //    InputRolledAngle
    //}
    //public static StringEnum RolledOffsetMethod = new StringEnum(typeof(ERolledOffsetMethod));
}
