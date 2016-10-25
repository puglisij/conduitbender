using UnityEngine;
using System.Collections;

public class BenderInfo
{
    /*
        Center Radius for Greenlee Benders:

        Sizes:                  1/2"        3/4"        1"          1 1/4"      1 1/2"      2"
        Site-Rite:              4 3/16"     5 1/8"      6 1/2"      11"         -           -
        Site-Rite 2:            4 3/16"     5 1/8"      6 1/2"      9 5/8"      -           -
        555 EMT:                4 1/4"      5 3/8"      6 3/4"      8 3/4"      8 9/32"     9 3/16"
        555 IMC:                4 3/8"      4 1/2"      5 3/4"      7 1/4"      8 1/4"      9 1/2"
        555 Rigid:              4 3/8"      4 1/2"      5 3/4"      7 1/4"      8 9/32"     9 3/16"
        854/855 EMT:            4 5/16"     5 1/2"      7"          8 13/16"    8 3/8"      9 1/4"
        854/855 IMC:            4 1/4"      5 7/16"     6 15/16"    8 3/4"      8 1/4"      9"
        880:                    4"          4 1/2"      5 3/4"      7 1/4"      8 1/4"      9 1/2"
        882 EMT:                 -             -           -        7 7/32"     8 1/16"     9 5/16"
        882 IMC/Rigid:           -             -           -        7 1/4"      8 1/4"      8 7/8"
        1800/1801 Rigid & IMC:  2 5/8"      4 5/8"      5 7/8"      8 1/16"     9 11/16"       -

        Sizes:                  1 1/4"      1 1/2"      2"          2 1/2"      3"          3 1/2"      4"          5"
        777:                    7 1/4"      8 1/4"      9 1/2"      11 7/16"    13 3/4"     16"         18 1/4"     -
        881:                    -           -           -           13 1/2"     16"         18 5/8"     20 7/8"     -
        884 / 885:              7 1/4"      8 1/4"      9 1/2"      12 1/2"     15"         17 1/2"     20"         25"
    */
    public static class Metric
    {
        // Metric
        public static readonly float k_BenderRadiusRange     = 0.9f;        // Meters
        public static readonly float k_ConduitDiameterRange  = 150f;        // Millimeter

        public static readonly string[] k_BenderRadiusPresets = {
            ""
        };
        public static readonly string[] k_ConduitDiameterPresets = {
            ""
        };
    }

    public static class Standard
    {
        // Standard
        public static readonly float k_BenderRadiusRange    = 3f;           // Feet
        public static readonly float k_ConduitDiameterRange = 4f * 16f;     // Sixteenths

        public static readonly string[] k_BenderRadiusPresets = {
            "1/2\"- Common - 4 1/4\"",
        };
        public static readonly string[] k_ConduitDiameterPresets = {

        };
    }

}
