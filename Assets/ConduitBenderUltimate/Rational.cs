using System;
using System.Collections.Generic;
using System.Globalization;

public static class Algorithms
{
    public static long Gcd(long a, long b)
    {
        bool aneg = a < 0, bneg = b < 0;
        if (aneg) a = -a;
        if (bneg) b = -b;

        var gcd = Gcd( (ulong)a, (ulong)b );
        return aneg == bneg ? (long)gcd : -((long)gcd);
    }

    public static ulong Gcd(ulong a, ulong b)
    {
        while (a != 0 && b != 0) {
            if (a > b)
                a %= b;
            else
                b %= a;
        }
        return a == 0 ? b : a;
    }

    public static bool IsNullOrWhiteSpace(string value)
    {
        if (value != null) {
            for (int i = 0; i < value.Length; i++) {
                if (!char.IsWhiteSpace( value[i] )) {
                    return false;
                }
            }
        }
        return true;
    }
}


public partial struct Rational : IEquatable<Rational>, IFormattable
{
    public Rational(long numerator, long denominator = 1) : this()
    {
        if (denominator == 0) throw new ArgumentOutOfRangeException( "denominator", "The denominator cannot be 0." );

        Initialise( numerator, denominator );
    }

    private void Initialise(long numerator, long denominator)
    {
        if (denominator == 1) {
            Numerator = numerator;
            Denominator = 1;
        } else if (numerator == denominator) {
            Numerator = 1;
            Denominator = 1;
        } else if (numerator == 0) {
            Numerator = 0;
            Denominator = 1;
        } else {
            var gcd = Algorithms.Gcd( numerator, denominator );
            numerator /= gcd;
            denominator /= gcd;
            Numerator = denominator < 0 ? -numerator : numerator;
            Denominator = denominator < 0 ? -denominator : denominator;
        }
    }

    public long Numerator { get; private set; }
    public long Denominator { get; private set; }

    public bool IsValid
    {
        get { return Denominator != 0; }
    }

    public static Rational Empty { get; private set; }
    public static Rational Zero { get; private set; }
    public static Rational One { get; private set; }

    static Rational()
    {
        Empty = new Rational();
        Zero = new Rational( 0 );
        One = new Rational( 1 );
    }

    public bool Equals(Rational other)
    {
        return Numerator == other.Numerator && Denominator == other.Denominator;
    }

    public override bool Equals(object obj)
    {
        return obj is Rational && Equals( (Rational)obj );
    }

    public override int GetHashCode()
    {
        unchecked {
            return (Numerator.GetHashCode() * 397) ^ Denominator.GetHashCode();
        }
    }

    public override string ToString()
    {
        return ToString( "L", null );
    }

    public string ToString(string format, IFormatProvider provider)
    {
        if (Algorithms.IsNullOrWhiteSpace( format ))
            format = "L";

        switch (format[0]) {
            case 'S':
            case 's':
                return string.Concat( Numerator, '/', Denominator );

            case 'L':
            case 'l':
                if (Denominator == 1)
                    return Numerator.ToString( CultureInfo.InvariantCulture );

                if ((Numerator >= 0 && Numerator < Denominator) || (Numerator < 0 && -Numerator < Denominator))
                    return string.Concat( Numerator, '/', Denominator );

                if (Numerator < 0)
                    return string.Concat( Numerator / Denominator, ' ', -Numerator % Denominator, '/', Denominator );

                return string.Concat( Numerator / Denominator, ' ', Numerator % Denominator, '/', Denominator );

            case 'D':
            case 'd':
            case 'X':
            case 'x':
            case 'N':
            case 'n':
                return (Numerator / Denominator).ToString( format, provider );

            case 'C':
            case 'c':
            case 'E':
            case 'e':
            case 'F':
            case 'f':
            case 'G':
            case 'g':
            case 'P':
            case 'p':
            case 'R':
            case 'r':
                return ((double)Numerator / Denominator).ToString( format, provider );

            default:
                throw new FormatException( string.Concat( "Unknown format string: \"", format, "\"." ) );
        }
    }
}

public partial struct Rational
{
    public Rational Negate()
    {
        return new Rational( -Numerator, Denominator );
    }

    public Rational Invert()
    {
        return new Rational( Denominator, Numerator );
    }

    public Rational Add(Rational rational)
    {
        return Add( rational.Numerator, rational.Denominator );
    }

    public Rational Add(long value)
    {
        checked {
            return Add( value * Denominator, Denominator );
        }
    }

    public Rational Subtract(Rational rational)
    {
        return Add( -rational.Numerator, rational.Denominator );
    }

    public Rational Subtract(long value)
    {
        checked {
            return Add( -value * Denominator, Denominator );
        }
    }

    public Rational Multiply(Rational rational)
    {
        return Multiply( rational.Numerator, rational.Denominator );
    }

    public Rational Multiply(long value)
    {
        checked {
            return Multiply( value * Denominator, Denominator );
        }
    }

    public Rational Divide(Rational rational)
    {
        return Multiply( rational.Denominator, rational.Numerator );
    }

    public Rational Divide(long value)
    {
        checked {
            return Multiply( Denominator, value * Denominator );
        }
    }

    private Rational Add(long numerator, long denominator)
    {
        checked {
            return
                Denominator == denominator
                    ? new Rational( Numerator + numerator, denominator )
                    : new Rational( Numerator * denominator + numerator * Denominator, Denominator * denominator );
        }
    }

    private Rational Multiply(long numerator, long denominator)
    {
        checked {
            return new Rational( Numerator * numerator, Denominator * denominator );
        }
    }

    public Rational Abs()
    {
        return Numerator < 0 ? new Rational( -Numerator, Denominator ) : this;
    }

    public Rational Round(long targetDenominator)
    {
        var sign = Numerator < 0 ? -1 : 1;
        var numerator = Numerator * sign;
        var whole = numerator / Denominator;
        numerator %= Denominator;
        double local = numerator / (double) Denominator;

        return new Rational( (whole + (long) Math.Round( local * targetDenominator )) * sign, targetDenominator );
    }
    public Rational Round(IList<long> targetDenominators)
    {
        var sign = Numerator < 0 ? -1 : 1;
        var numerator = Numerator * sign;       // Absolute Value
        var whole = numerator / Denominator;
            numerator %= Denominator;
        double local = numerator / (double)Denominator;
        double bestNumerator = numerator;
        double bestDenominator = Denominator;
        double bestError = 1;

        double rounded;
        double error;
        foreach (var denominator in targetDenominators) {
            rounded = Math.Round(local * denominator);
            error = Math.Abs((rounded / denominator) - local);
            if (error < bestError) {
                bestNumerator = rounded;
                bestDenominator = denominator;
                bestError = error;
            }
        }

        return new Rational( (whole + (long) bestNumerator ) * sign, (long) bestDenominator );
    }
}
public partial struct Rational : IComparable<Rational>, IComparable
{
    public int CompareTo(Rational other)
    {
        long diff;

        if (Denominator == other.Denominator)
            diff = Numerator - other.Numerator;
        else {
            var whole1 = Numerator / Denominator;
            var whole2 = other.Numerator / other.Denominator;
            diff = whole1 - whole2;

            if (diff == 0) {
                var n1 = Numerator % Denominator;
                var n2 = other.Numerator % other.Denominator;

                checked {
                    diff = n1 * other.Denominator - n2 * Denominator;
                }
            }
        }

        return diff == 0 ? 0 : diff > 0 ? 1 : -1;
    }

    public int CompareTo(object value)
    {
        if (value == null)
            return 1;
        if (!(value is Rational))
            throw new ArgumentException( "must be rational" );

        return CompareTo( (Rational)value );
    }
}
public partial struct Rational : IConvertible
{
    public TypeCode GetTypeCode()
    {
        throw new InvalidOperationException();
    }

    public bool ToBoolean(IFormatProvider provider)
    {
        return Numerator != 0;
    }

    public char ToChar(IFormatProvider provider)
    {
        throw new InvalidCastException( "no valid cast to char" );
    }

    public sbyte ToSByte(IFormatProvider provider)
    {
        return (sbyte)(Numerator / Denominator);
    }

    public byte ToByte(IFormatProvider provider)
    {
        return (byte)(Numerator / Denominator);
    }

    public short ToInt16(IFormatProvider provider)
    {
        return (short)(Numerator / Denominator);
    }

    public ushort ToUInt16(IFormatProvider provider)
    {
        return (ushort)(Numerator / Denominator);
    }

    public int ToInt32(IFormatProvider provider)
    {
        return (int)(Numerator / Denominator);
    }

    public uint ToUInt32(IFormatProvider provider)
    {
        return (uint)(Numerator / Denominator);
    }

    public long ToInt64(IFormatProvider provider)
    {
        return Numerator / Denominator;
    }

    public ulong ToUInt64(IFormatProvider provider)
    {
        return (ulong)(Numerator / Denominator);
    }

    public float ToSingle(IFormatProvider provider)
    {
        return (float)((double)Numerator / Denominator);
    }

    public double ToDouble(IFormatProvider provider)
    {
        return (double)Numerator / Denominator;
    }

    public decimal ToDecimal(IFormatProvider provider)
    {
        return (decimal)Numerator / Denominator;
    }

    public DateTime ToDateTime(IFormatProvider provider)
    {
        throw new InvalidCastException( "no valid cast to DateTime" );
    }

    public string ToString(IFormatProvider provider)
    {
        return ToString( "L", provider );
    }

    public object ToType(Type type, IFormatProvider provider)
    {
        switch (Type.GetTypeCode( type )) {
            case TypeCode.Boolean:
                return ToBoolean( provider );
            case TypeCode.Char:
                return ToChar( provider );
            case TypeCode.SByte:
                return ToSByte( provider );
            case TypeCode.Byte:
                return ToByte( provider );
            case TypeCode.Int16:
                return ToInt16( provider );
            case TypeCode.UInt16:
                return ToUInt16( provider );
            case TypeCode.Int32:
                return ToInt32( provider );
            case TypeCode.UInt32:
                return ToUInt32( provider );
            case TypeCode.Int64:
                return ToInt64( provider );
            case TypeCode.UInt64:
                return ToUInt64( provider );
            case TypeCode.Single:
                return ToSingle( provider );
            case TypeCode.Double:
                return ToDouble( provider );
            case TypeCode.Decimal:
                return ToDecimal( provider );
            case TypeCode.DateTime:
                return ToDateTime( provider );
            case TypeCode.String:
                return ToString( provider );
            default:
                throw new InvalidCastException( "no valid cast to " + type.Name );
        }
    }
}
public partial struct Rational
{
    public static Rational operator -(Rational a)
    {
        return a.Negate();
    }

    public static Rational operator +(Rational a, Rational b)
    {
        return a.Add( b );
    }

    public static Rational operator +(Rational a, long b)
    {
        return a.Add( b );
    }

    public static Rational operator +(long a, Rational b)
    {
        return b.Add( a );
    }

    public static Rational operator ++(Rational a)
    {
        a.Initialise( a.Numerator + a.Denominator, a.Denominator );
        return a;
    }

    public static Rational operator -(Rational a, Rational b)
    {
        return a.Subtract( b );
    }

    public static Rational operator -(Rational a, long b)
    {
        return a.Subtract( b );
    }

    public static Rational operator -(long a, Rational b)
    {
        return b.Negate().Add( a );
    }

    public static Rational operator --(Rational a)
    {
        a.Initialise( a.Numerator - a.Denominator, a.Denominator );
        return a;
    }

    public static Rational operator *(Rational a, Rational b)
    {
        return a.Multiply( b );
    }

    public static Rational operator *(Rational a, long b)
    {
        return a.Multiply( b );
    }

    public static Rational operator *(long a, Rational b)
    {
        return b.Multiply( a );
    }

    public static Rational operator /(Rational a, Rational b)
    {
        return a.Divide( b );
    }

    public static Rational operator /(Rational a, long b)
    {
        return a.Divide( b );
    }

    public static Rational operator /(long a, Rational b)
    {
        return b.Invert().Multiply( a );
    }

    public static bool operator ==(Rational a, Rational b)
    {
        return a.Equals( b );
    }

    public static bool operator !=(Rational a, Rational b)
    {
        return !a.Equals( b );
    }

    public static bool operator >(Rational a, Rational b)
    {
        return a.CompareTo( b ) > 0;
    }

    public static bool operator >=(Rational a, Rational b)
    {
        return a.CompareTo( b ) >= 0;
    }

    public static bool operator <(Rational a, Rational b)
    {
        return a.CompareTo( b ) < 0;
    }

    public static bool operator <=(Rational a, Rational b)
    {
        return a.CompareTo( b ) <= 0;
    }

    public static implicit operator Rational(long value)
    {
        return new Rational( value );
    }

    public static explicit operator long (Rational value)
    {
        return value.Numerator / value.Denominator;
    }

    public static explicit operator double (Rational value)
    {
        return (double)value.Numerator / value.Denominator;
    }

    public static explicit operator Rational(double value)
    {
        return FromDouble( value, 8 );
    }
}
public partial struct Rational
{
    public static Rational Parse(string s)
    {
        if (s == null) throw new ArgumentNullException();

        Rational rational;
        if (!TryParse( s, out rational )) throw new FormatException();
        return rational;
    }

    public static bool TryParse(string s, out Rational rational)
    {
        if (s != null) {
            s = s.Trim();
            var slashIndex = s.IndexOf( '/' );
            if (slashIndex == -1) {
                long whole;
                if (long.TryParse( s, out whole )) {
                    rational = new Rational( whole );
                    return true;
                }
            } else {
                var denominatorString = s.Substring( slashIndex + 1, s.Length - (slashIndex + 1) ).TrimStart();
                long denominator;
                if (long.TryParse( denominatorString, out denominator )) {
                    long numerator;
                    var numeratorString = s.Substring( 0, slashIndex ).TrimEnd();
                    var spaceIndex = numeratorString.IndexOf( ' ' );
                    if (spaceIndex == -1) {
                        if (long.TryParse( numeratorString, out numerator )) {
                            rational = new Rational( numerator, denominator );
                            return true;
                        }
                    } else {
                        long whole;
                        if (long.TryParse( numeratorString.Substring( 0, spaceIndex ).TrimEnd(), out whole )
                            && long.TryParse( numeratorString.Substring( spaceIndex + 1, numeratorString.Length - spaceIndex - 1 ).TrimStart(), out numerator )
                            && numerator >= 0) {
                            rational = new Rational( whole * denominator + numerator, denominator );
                            return true;
                        }
                    }
                }
            }
        }
        rational = default( Rational );
        return false;
    }
}
public partial struct Rational
{
    public static Rational FromDouble(double value, int maxIterations = int.MaxValue, double threshold = double.Epsilon)
    {
        var whole = (long)Math.Floor( value );
        value -= whole;

        if (value < threshold)
            return new Rational( whole );

        if (1 - threshold < value)
            return new Rational( whole + 1 );

        var low = new Rational( 0 );
        var high = new Rational( 1 );

        for (var i = 0; i < maxIterations; ++i) {
            var mid = new Rational( low.Numerator + high.Numerator, low.Denominator + high.Denominator );
            if (mid.Numerator > mid.Denominator * (value + threshold))
                high = mid;
            else if (mid.Numerator < mid.Denominator * (value - threshold))
                low = mid;
            else
                return new Rational( whole * mid.Denominator + mid.Numerator, mid.Denominator );
        }

        throw new ArithmeticException( "Failed to solve." );
    }
}