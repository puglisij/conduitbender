using UnityEngine;
using System.Collections;


public static class GlobalEnum
{
    public enum ESegmentedBendMethod
    {
        [DescriptionValue( "The Accurate Method requires different bend angles for the First and Last bend but more closely follows the Segmented Radius." )]
        Accurate = 0,
        [DescriptionValue( "The Simple Method uses equidistant marks and equal angles for all bends but does not follow the Segmented Radius as accurately on fewer numbered bends." )]
        Simple = 1
    }
    public static StringEnum SegmentedBendMethod = new StringEnum(typeof(ESegmentedBendMethod));
    public enum ESaddle3BendMethod
    {
        [DescriptionValue( "Used with 45 degree center bends. The center mark is aligned with typically a notch or tear drop on the bender." )]
        Notch,
        [DescriptionValue( "Uses non-equidistant marks between 1st mark to center mark, and center mark to 3rd mark, but marks are aligned with bender arrow for all bends." )]
        Arrow
    }
    public static StringEnum Saddle3BendMethod = new StringEnum(typeof(ESaddle3BendMethod));

}
