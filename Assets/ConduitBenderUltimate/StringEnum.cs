using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Helper class for working with 'extended' enums using <see cref="StringValueAttribute"/> attributes.
/// </summary>
public class StringEnum
{
    private int  last;        // Index of last Enum value
    private Type enumType;
    private Dictionary<int, string> descriptionValues = new Dictionary<int, string>();
    /// <summary>
    /// Creates a new <see cref="StringEnum"/> instance.
    /// </summary>
    /// <param name="enumType">Enum type.</param>
    public StringEnum( Type enumType )
    {
        if (!enumType.IsEnum)
            throw new ArgumentException( String.Format( "Supplied type must be an Enum.  Type was {0}", enumType.ToString() ) );

        this.enumType = enumType;

        // Map attributes
        DescriptionValueAttribute[] dvs;
        FieldInfo fi;
        last = -1;
        foreach (Enum val in Enum.GetValues( enumType )) {
            fi = enumType.GetField( val.ToString() );
            dvs = (DescriptionValueAttribute[])fi.GetCustomAttributes( typeof( DescriptionValueAttribute ), false );
            if (dvs.Length > 0) {
                descriptionValues.Add( Convert.ToInt32( val ), dvs[ 0 ].Value );
            }
            last += 1;
        }

    }


    /// <summary>
    /// Returns the 1st Enum value object
    /// </summary>
    public object First()
    {
        return ToEnum( 0 );
    }
    public List<string> GetDescriptionList()
    {
        return descriptionValues.Values.ToList();
    }
    public List<string> GetValueStringList()
    {
        return new List<string>( Enum.GetNames( enumType ) );  // About 20ms @ 100,000 iterations
    }

    /// <summary>
    /// Returns null if value does not map to a description.
    /// </summary>
    public string ToDescription( int val )
    {
        string des;
        if (descriptionValues.TryGetValue( val, out des )) {
            return des;
        }
        return null;
    }
    /// <summary>
    /// Returns description associated with the given string value name of an enum.
    /// If no description exists, null is returned.
    /// </summary>
    public string ToDescription( string name, bool ignoreCase = false )
    {
        object val;
        try {
            val = Enum.Parse( enumType, name, ignoreCase );
        } catch (Exception) {
            return null;
        }
        return ToDescription( (int)val );
    }
    public object ToEnum( int val )
    {
        if (!Enum.IsDefined( enumType, val ))
            throw new ArgumentOutOfRangeException( "StringEnum: ToEnum() Value not defined in this enumeration." );
        return Enum.ToObject( enumType, val );
    }
    public string ToStringValue(int val)
    {
        return ToEnum( val ).ToString();
    }
    /// <summary>
    /// Returns the 1st Enum value object
    /// </summary>
    public object Last()
    {
        return ToEnum( last );
    }
    public object Parse( string name, bool ignoreCase = false )
    {
        return Enum.Parse( enumType, name, ignoreCase );
    }
    
}



/// <summary>
/// Simple attribute class for storing String Values
/// </summary>
[AttributeUsage( AttributeTargets.Field )]
public class DescriptionValueAttribute : Attribute
{
    private string _value;
    /// <summary>
    /// Creates a new <see cref="DescriptionValueAttribute"/> instance.
    /// </summary>
    public DescriptionValueAttribute( string value )
    {
        _value = value;
    }
    /// <summary>
    /// Gets the value.
    /// </summary>
    public string Value
    {
        get { return _value; }
    }
}
