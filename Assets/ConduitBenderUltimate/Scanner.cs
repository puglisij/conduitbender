using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CB
{
    public enum Associativity
    {
        LeftToRight,
        RightToLeft,
        None
    }

    /*
        IMPORTANT:
    
        To prevent cases where operands would be associated with two operators, or no operator at all, 
        operators with the same precedence MUST have the same associativity.
    */
    public enum TokenType
    {
        UNDEFINED,

        // Functions
        FUNCTION,
        //UNIT,

        // Symbols
        //AND,
        COMMA,
        L_BRACKET,
        R_BRACKET,

        // Arithmetic Operators
        ADD,
        DIVIDE,
        EXPONENT,
        MULTIPLY,
        SUBTRACT,

        // Literals
        NUMBER
    }

    public struct Token
    {
        public TokenType        type;

        public Associativity    associativity;

        public int              precedence;     // lower is higher

        public object           value;

        public Token(TokenType type, Associativity associativity, int precedence, object value)
        {
            this.type = type;
            this.associativity = associativity;
            this.precedence = precedence;
            this.value = value;
        }
    }

    /// <summary> 
    /// Parse input string and break into internal list of Tokens.
    /// </summary>
    public class Scanner
    {


        //------------------
        //  Private Data
        //------------------
        private Regex ftInRegex = new Regex( @"(((\d*\.?\d+)|(\d+&\d+\/\d+)|(\d+\/\d+))ft)(((\d*\.?\d+)|(\d+&\d+\/\d+)|(\d+\/\d+))in)", RegexOptions.ECMAScript);
        private Regex ftRegex = new Regex( @"(((\d*\.?\d+)|(\d+&\d+\/\d+)|(\d+\/\d+))ft)", RegexOptions.ECMAScript);  // Use ECMAScript option for English only
        private Regex inRegex = new Regex( @"(((\d*\.?\d+)|(\d+&\d+\/\d+)|(\d+\/\d+))in)", RegexOptions.ECMAScript);
        //private Regex ftInRegex = new Regex( @"(((\d*\.?\d+)|(\d+&\d+\/\d+)|(\d+\/\d+)|(\d+&\(\d+\/\d+\))|(\(\d+\/\d+\)))ft)(((\d*\.?\d+)|(\d+&\d+\/\d+)|(\d+\/\d+)|(\d+&\(\d+\/\d+\))|(\(\d+\/\d+\)))in)", RegexOptions.ECMAScript);
        //private Regex ftRegex = new Regex( @"(((\d*\.?\d+)|(\d+&\d+\/\d+)|(\d+\/\d+)|(\d+&\(\d+\/\d+\))|(\(\d+\/\d+\)))ft)", RegexOptions.ECMAScript);  // Use ECMAScript option for English only
        //private Regex inRegex = new Regex( @"(((\d*\.?\d+)|(\d+&\d+\/\d+)|(\d+\/\d+)|(\d+&\(\d+\/\d+\))|(\(\d+\/\d+\)))in)", RegexOptions.ECMAScript);

        private List<Token> m_Tokens = new List<Token>();

        private Token m_CurrentToken = default(Token);

        private int m_NextToken = 0;

        private bool m_HaveUnits = false;

        public Token CurrentToken()
        {
            return m_CurrentToken;
        }
        public bool HasNextToken()
        {
            if(m_NextToken < m_Tokens.Count) {
                return true;
            }
            return false;
        }
        public bool HaveUnits()
        {
            return m_HaveUnits;
        }
        /// <summary>
        /// Returns and consumes next Token. 
        /// May throw IndexOutOfRange Exception.
        /// </summary>
        public Token NextToken()
        {
            m_CurrentToken = m_Tokens[ m_NextToken++ ];
            return m_CurrentToken;
        }

        public bool Tokenize(string inputString)
        {
            // Clear current Tokens
            m_Tokens.Clear();
            m_NextToken = 0;
            m_HaveUnits = false;

            // Create new Tokens
            char[] input = inputString.ToCharArray();
            char nextChar;

            for (int c = 0; c < input.Length; ++c) 
            {
                nextChar = input[ c ];

                if (char.IsDigit( nextChar ) || nextChar == '.' ) 
                {
                    //-----------------------
                    // Literal (Operand)
                    //-----------------------
                    float  number = 0;
                    string  numberStr1;
                    string  numberStr2;

                    // Match Regex
                    Match ftInMatch = ftInRegex.Match( inputString, c );
                    Match ftMatch = ftRegex.Match( inputString, c );
                    Match inMatch = inRegex.Match( inputString, c );

                    if (ftInMatch.Success && ftInMatch.Index == c) {
                        numberStr1 = ftInMatch.Groups[ 1 ].Value;
                        numberStr2 = ftInMatch.Groups[ 6 ].Value;

                        number = ParseUnitString( numberStr1, "ft", 1f );
                        number += ParseUnitString( numberStr2, "in", Units.k_InToFt );

                        m_HaveUnits = true;
                        c += ftInMatch.Length - 1;
                    } else if (ftMatch.Success && ftMatch.Index == c) {

                        number = ParseUnitString( ftMatch.Value, "ft", 1f );

                        m_HaveUnits = true;
                        c += ftMatch.Length - 1;
                    } else if (inMatch.Success && inMatch.Index == c) {

                        number = ParseUnitString( inMatch.Value, "in", Units.k_InToFt );

                        m_HaveUnits = true;
                        c += inMatch.Length - 1;
                    } else {
                        // Is it a regular number (without units)?
                        string  numberStr = char.ToString( nextChar );
                        int     nc = c + 1;

                        while (nc < input.Length) {
                            if (char.IsDigit( input[ nc ] ) || input[ nc ] == '.') {
                                nextChar = input[ nc ];
                                numberStr += nextChar;
                                c = nc;
                                nc += 1;
                            } else {
                                break;
                            }
                        }
                        if (!float.TryParse( numberStr, out number )) {
                            // Error!
                            return false;
                        }
                    }
                    
                    // Check if last token was ')' which indicates Multiply
                    if( m_Tokens.Count > 0 && m_Tokens.Last().type == TokenType.R_BRACKET ) {
                        m_Tokens.Add( new Token( TokenType.MULTIPLY, Associativity.LeftToRight, 3, "*" ) );
                    }

                    m_Tokens.Add( new Token( TokenType.NUMBER, Associativity.None, 0, number ) );

                } else if (char.IsLetter( nextChar )) 
                {
                    //---------------
                    // Get Name
                    //---------------
                    string name = "" + nextChar;
                    int nc = c + 1;

                    while (nc < input.Length && char.IsLetter(input[nc])) 
                    {
                        nextChar = input[ nc ];
                        name += nextChar;
                        c = nc;
                        nc += 1;
                    }
                    // Is it a Function, Constant, or Unit Name?
                    if( ArrayContains( Units.k_UnitAbbreviations, name ) >= 0 ) {
                        // Error?
                        return false;

                        // TODO - Allow Unit Tokens, and use them for parenthesized groups
                        //m_Tokens.Add( new Token( TokenType.UNIT, Associativity.RightToLeft, 9, name ) );
                    } else if(name == "pi") {
                        m_Tokens.Add( new Token( TokenType.NUMBER, Associativity.None, 0, 3.141592654f ) );
                    } else {
                        // Create Function Token
                        m_Tokens.Add( new Token( TokenType.FUNCTION, Associativity.None, 2, name ) );
                    }
                } else if (nextChar == '(') {
                    // Check if last token was NUMBER which indicates Multiply
                    if ( m_Tokens.Count > 0 && m_Tokens.Last().type == TokenType.NUMBER) {
                        m_Tokens.Add( new Token( TokenType.MULTIPLY, Associativity.LeftToRight, 3, "*" ) );
                    }

                    m_Tokens.Add( new Token( TokenType.L_BRACKET, Associativity.None, 2, "(" ) );
                } else if (nextChar == ')') {
                    m_Tokens.Add( new Token( TokenType.R_BRACKET, Associativity.None, 2, ")" ) );
                } else if (nextChar == ',') {
                    m_Tokens.Add( new Token( TokenType.COMMA, Associativity.LeftToRight, 10, "," ) );
                } 
                else if (nextChar == '&') {
                    //m_Tokens.Add( new Token( TokenType.AND, Associativity.LeftToRight, 8, "&" ) );
                    // Error
                    return false;
                } 
                else if (nextChar == '^') {
                    m_Tokens.Add( new Token( TokenType.EXPONENT, Associativity.RightToLeft, 3, "^" ) );
                } else if (nextChar == '/') {
                    m_Tokens.Add( new Token( TokenType.DIVIDE, Associativity.LeftToRight, 3, "/" ) );
                } else if (nextChar == '*') {
                    m_Tokens.Add( new Token( TokenType.MULTIPLY, Associativity.LeftToRight, 3, "*" ) );
                } else if (nextChar == '-') {
                    // Check if last token was a non-NUMBER which indicates Multiply by -1
                    if (m_Tokens.Count == 0 || m_Tokens.Last().type != TokenType.NUMBER) {
                        m_Tokens.Add( new Token( TokenType.NUMBER, Associativity.None, 0, -1f ) );
                        m_Tokens.Add( new Token( TokenType.MULTIPLY, Associativity.LeftToRight, 3, "*" ) );
                    } else {
                        m_Tokens.Add( new Token( TokenType.SUBTRACT, Associativity.LeftToRight, 4, "-" ) );
                    }
                } else if (nextChar == '+') {
                    m_Tokens.Add( new Token( TokenType.ADD, Associativity.LeftToRight, 4, "+" ) );
                } else {
                    Debug.LogError( "Scanner: Tokenize() Token not Recognized." );
                    return false;
                }

            }

            return true;
        } // Tokenize()

        /// <summary>
        /// Test if string array contains the given string. Return matched index, or -1 if not found.
        /// </summary>
        private int ArrayContains( string[] matches, string matchee )
        {
            for (int i = 0; i < matches.Length; ++i) {
                if (matches[ i ] == matchee) {
                    return i;
                }
            }
            return -1;
        }

        private float ParseUnitString(string unitStr, string unit, float unitConversionFactor )
        {
            unitStr = unitStr.Substring( 0, unitStr.Length - unit.Length );

            float number = 0;
            if (unitStr.Contains( '/' )) {
                var andParts = unitStr.Split('&');
                if (andParts.Length > 1) {
                    // Whole Unit, & Fraction
                    string inWholePart = andParts[0];
                    string inFracPart = andParts[1];
                    var fracParts = inFracPart.Split('/');
                    //var fracParts = inFracPart.Replace("(","").Replace(")","").Split('/');
                    //var fracParts = Regex.Replace(inFracPart, @"[\(,\)]", "").Split('/');

                    number += float.Parse( inWholePart ) * unitConversionFactor;
                    number += (float.Parse( fracParts[ 0 ] ) / float.Parse( fracParts[ 1 ] )) * unitConversionFactor;
                } else {
                    // Unit Fraction
                    var fracParts = unitStr.Split('/');
                    //var fracParts = unitStr.Replace("(","").Replace(")","").Split('/');
                    //var fracParts = Regex.Replace(unitStr, @"[\(,\)]", "").Split('/');

                    number += (float.Parse( fracParts[ 0 ] ) / float.Parse( fracParts[ 1 ] )) * unitConversionFactor;
                }
            } else {
                // Unit Decimal 
                number += float.Parse( unitStr ) * unitConversionFactor;
            }

            return number;
        }

    } // Scanner

    

}


