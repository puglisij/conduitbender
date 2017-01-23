using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CB;
using System.Collections.Generic;
using System;
using System.Text;



/// <summary>
/// Calculator View Class.   
/// A lot of functionality is crammed in here at the moment; Could use some housekeeping.
/// </summary>
public class CalculatorScreen : AnimScreen
{
    public delegate void SecondaryObserver(bool isOn);

    public enum OutputMode { Fraction, Decimal }

    public Text displayText;

    public AModal  diagramModal;

    [Space]
    public Toggle  secondaryBtn;
    public Toggle  fractionBtn;
    public Toggle  degreeBtn;

    /// <summary>
    /// Notify observers when 2nd button is pressed
    /// </summary>
    public event SecondaryObserver onSecondary;

    //------------------
    // Private Data
    //------------------
    private delegate float OperatorHandler();


    private Scanner         m_Scanner   = new Scanner();
    private Queue<Token>    m_OutputQ   = new Queue<Token>();
    private Stack<Token>    m_Stack     = new Stack<Token>();

    private Dictionary<string, OperatorHandler> m_OperatorHandlers = new Dictionary<string, OperatorHandler>();

    //private Units.Type m_Type       = Units.Type.Standard;
    //private OutputMode m_OutMode    = OutputMode.Fraction;

    private float   m_Result = 0;

    private string  m_EnteredText = "";
    private string  m_Input = "";
    private string  m_Signature = "";

    private bool    m_HaveResultUnits = false;      // Whether the previously calculated result specified Units
    private bool    m_HavePreviousResult = false;   // Whether we have a previously calculated result
    private bool    m_InFracState = true;           // Else in Decimal
    private bool    m_InDegState = true;            // Else in Radians
    private bool    m_In2ndState = false;           // 2nd Functionality
    private bool    m_InErrorState = false;


    private void AppendInput(string input)
    {
        // Clear any Signature
        m_Signature = "";

        if (m_InErrorState) {
            m_Input = "";
            m_InErrorState = false;
        }
        m_Input += input;

        // Update View
        RefreshDisplay();

        Debug.Log( "CalculatorScreen: AddInput() m_Input: " + m_Input );
    }
    /*##########################################

                Public Functions

    ###########################################*/
    protected override void Awake()
    {
        base.Awake();

        // Add Operator functions to Dictionary
        m_OperatorHandlers.Add( "+", Add );
        m_OperatorHandlers.Add( "-", Subtract );
        m_OperatorHandlers.Add( "*", Multiply );
        m_OperatorHandlers.Add( "/", Divide );
        m_OperatorHandlers.Add( "^", Exponent );
        m_OperatorHandlers.Add( "log", Log );
        m_OperatorHandlers.Add( "sin", Sin );
        m_OperatorHandlers.Add( "asin", ASin );
        m_OperatorHandlers.Add( "cos", Cos );
        m_OperatorHandlers.Add( "acos", ACos );
        m_OperatorHandlers.Add( "tan", Tan );
        m_OperatorHandlers.Add( "atan", ATan );
        m_OperatorHandlers.Add( "sqrt", Sqrt );

        m_OperatorHandlers.Add( "Bh", Bh );
        m_OperatorHandlers.Add( "Bv", Bv );
        m_OperatorHandlers.Add( "Ls", Ls );
        m_OperatorHandlers.Add( "Lb", Lb );
        m_OperatorHandlers.Add( "Es", Es );
        m_OperatorHandlers.Add( "Hb", Hb );
        m_OperatorHandlers.Add( "Hs", Hs );
        m_OperatorHandlers.Add( "Vb", Vb );
    }

    public override void Close( bool doDisable )
    {
        base.Close( doDisable );

        if(diagramModal.isOpen) {
            diagramModal.Close( true );
        }
    }
    public override void Open()
    {
        base.Open();

    }

    public void AddInput(string input)
    {
        // Assertion
        if(string.IsNullOrEmpty( input )) { return; }

        // NOTE: This is a bit of a hack
        // If this is a new line, auto add previous Answer
        if (string.IsNullOrEmpty( m_Input ) 
            && m_Result != 0f
            && (input == "+" || input == "-" || input == "*" || input == "/" || input == "^")) 
        {
            Ans();
        }

        AppendInput( input );
    }

    /// <summary>
    /// Adds a Function to the input stream, accompanied by a parameter signature which indicates parameters for the function.
    /// For example:  For the function Vb( Radius, Angle )
    ///     input =  "vb("
    ///     parameterSignature = "Radius, Angle"
    /// </summary>
    public void AddInputFunction(string input, string parameterSignature)
    {
        AppendInput( input );
        m_Signature = parameterSignature;

        // Update View
        RefreshDisplay();
    }

    //public void AddUnit(string input)
    //{
        // TODO - Same as AddInput() but set a bool 'expectUnits' which can be used by Scanner to avoid Regex matching when unnecessary.
    //}

    /// <summary>
    /// Add last result to current Input
    /// </summary>
    public void Ans()
    {
        if(m_HavePreviousResult) 
        {
            if (m_HaveResultUnits) {
                AppendInput( Units.FormatCalculator( Units.Type.Standard, Units.RulerUnit.Feet, m_Result ) );
            } else {
                AppendInput( m_Result.ToString() );
            }
        }
    }
    /// <summary>
    /// Delete the last character in current Input
    /// </summary>
    public void Back()
    {
        if(m_Input.Length == 0) { return; }

        m_Input = m_Input.Remove( m_Input.Length - 1 );
        m_Signature = "";
        RefreshDisplay();
    }
    /// <summary>
    /// Reset Calculator
    /// </summary>
    public void Clear()
    {
        m_EnteredText = string.Empty;
        m_Input = string.Empty;
        m_Signature = string.Empty;
        m_InErrorState = false;
        RefreshDisplay();
    }
    public void Diagram()
    {
        // Open Bend Diagram Modal
        diagramModal.Open();
    }
    public void Enter()
    {
        if(m_InErrorState || string.IsNullOrEmpty( m_Input ) ) { return; }

        //-------------------
        // Scan into Tokens 
        //-------------------
        bool success = m_Scanner.Tokenize( m_Input );

        if(!success) {
            Error();
            return;
        }

        //-------------------
        // Parse 
        //-------------------
        success = ShuntingYard();

        if (!success) {
            Error();
            return;
        }

        //-------------------
        //  Output Results
        //-------------------
        string resultStr = "";
        // Format as Fraction?
        if(m_InFracState) {
            if(m_HaveResultUnits) {
                resultStr = Units.FormatCalculator( Units.Type.Standard, Units.RulerUnit.Feet, m_Result );
            } else {
                resultStr = Rational.FromDouble( m_Result, 1000, 0.00001 ).ToString();
            }
        } else {
            //if (m_HaveResultUnits) {
            //  resultStr = Units.FormatCalculator( Units.Type.Standard, Units.RulerUnit.Feet, m_Result, Units.Format.Decimal );
            //} else {
                resultStr = m_Result.ToString();
            //}
        }

        m_EnteredText = m_Input + "\n\t" + resultStr + "\n";
        m_Input = "";
        RefreshDisplay();

        Debug.Log( "CalculatorScreen: Enter()" );
    }

    public void Secondary()
    {
        m_In2ndState = secondaryBtn.isOn;
        // Notify all Observers
        onSecondary( m_In2ndState );

        Debug.Log( "CalculatorScreen: Secondary( " + m_In2ndState + " )" );
    }

    public void ToggleFractionDecimal()
    {
        m_InFracState = (fractionBtn.isOn);

        Debug.Log( "Calculator: ToggleFractionDecimal() In Fraction State: " + m_InFracState );
    }
    public void ToggleDegreeRadian()
    {
        m_InDegState = (degreeBtn.isOn);

        Debug.Log( "Calculator: ToggleFractionDecimal() In Degree State: " + m_InDegState );
    }

    /*##########################################

                Public Other

    ###########################################*/
    public void AddBh()
    {
        AddInputFunction( "Bh(", "R, A)" );
        diagramModal.Close(true);
    }
    public void AddBv()
    {
        AddInputFunction( "Bv(", "R, A)" );
        diagramModal.Close( true );
    }
    public void AddLs()
    {
        AddInputFunction( "Ls(", "Vs, A)" );
        diagramModal.Close( true );
    }
    public void AddLb()
    {
        AddInputFunction( "Lb(", "R, A)" );
        diagramModal.Close( true );
    }
    public void AddHb()
    {
        AddInputFunction( "Hb(", "R, A)" );
        diagramModal.Close( true );
    }
    public void AddHs()
    {
        AddInputFunction( "Hs(", "Ls, A)" );
        diagramModal.Close( true );
    }
    public void AddEs()
    {
        AddInputFunction( "Es(", "R, A)" );
        diagramModal.Close( true );
    }
    public void AddVb()
    {
        AddInputFunction( "Vb(", "R, A)" );
        diagramModal.Close( true );
    }
    public void AddR() {
        var formattedR = Units.FormatCalculator( Units.Type.Standard, Units.RulerUnit.Feet, Engine.benderRadiusM * Units.k_MToFt );
        AddInput( formattedR );
        diagramModal.Close( true );
    }

    /*##########################################

                Private Functions

    ###########################################*/

    private void Error()
    {
        m_Input = "Error";
        m_InErrorState = true;
        RefreshDisplay();
    }

    private bool IsOperator(Token t)
    {
        return t.type == TokenType.ADD
                || t.type == TokenType.SUBTRACT
                || t.type == TokenType.MULTIPLY
                || t.type == TokenType.DIVIDE
                || t.type == TokenType.EXPONENT;
                //|| t.type == TokenType.AND;
    }

    private float PopAngleInDegrees()
    {
        // TODO - Add Try Catch?
        if (!m_InDegState) {
            return (float)m_Stack.Pop().value * Mathf.Rad2Deg;
        }
        return (float)m_Stack.Pop().value;
    }
    private float PopAngleInRadians()
    {
        // TODO - Add Try Catch?
        if (m_InDegState) {
            return (float)m_Stack.Pop().value * Mathf.Deg2Rad;
        }
        return (float)m_Stack.Pop().value;
    }
    private float PopNumber()
    {
        // TODO - Add Try Catch?
        return (float)m_Stack.Pop().value;
    }

    private string PrintQueue(Queue<Token> q)
    {
        StringBuilder sb = new StringBuilder();
        foreach(Token t in q) {
            sb.Append( t.value );
            sb.Append( "," );
        }
        return sb.ToString();
    }
    private void RefreshDisplay()
    {
        string text = m_EnteredText + m_Input;
        if ( m_Signature.Length != 0 ) {
            text += "<color=#FF1800>" + m_Signature + "</color>";
        } 
        displayText.text = text;
    }

    private bool ShuntingYard()
    {
        m_OutputQ.Clear();
        m_Stack.Clear();

        Token t;

        try 
        {
            //-----------------------------
            //  Convert to Reverse Polish
            //-----------------------------
            while (m_Scanner.HasNextToken()) {
                t = m_Scanner.NextToken();

                if (t.type == TokenType.NUMBER) {
                    //-------------
                    // Number
                    //-------------
                    m_OutputQ.Enqueue( t );
                } else if (t.type == TokenType.FUNCTION) {
                    //-------------
                    // Function
                    //-------------
                    m_Stack.Push( t );
                } else if (t.type == TokenType.COMMA) {
                    //-------------
                    // Comma
                    //-------------
                    bool missingBracket = true;

                    while (m_Stack.Count > 0) {
                        Token top = m_Stack.Peek();
                        if (top.type != TokenType.L_BRACKET) {
                            m_OutputQ.Enqueue( m_Stack.Pop() );
                        } else {
                            missingBracket = false;
                            break;
                        }
                    }

                    if (missingBracket) {
                        // Error (e.g. Mismatched parens)
                        Debug.LogError( "CalculatorScreen: ShuntingYard() Misplaced Bracket" );
                        return false;
                    }

                } else if (IsOperator( t )) {
                    //-------------
                    // Operator
                    //-------------
                    while (m_Stack.Count > 0) {
                        Token top = m_Stack.Peek();
                        if (IsOperator( top ) || top.type == TokenType.FUNCTION) {
                            if (top.type == TokenType.FUNCTION) {
                                m_OutputQ.Enqueue( m_Stack.Pop() );
                            } else if (t.associativity == Associativity.LeftToRight && t.precedence >= top.precedence
                                    || t.associativity == Associativity.RightToLeft && t.precedence > top.precedence) {
                                m_OutputQ.Enqueue( m_Stack.Pop() );
                            } else {
                                break;
                            }

                        } else {
                            break;
                        }
                    }

                    m_Stack.Push( t );

                } else if (t.type == TokenType.L_BRACKET) {
                    //-------------
                    // Left (
                    //-------------
                    m_Stack.Push( t );
                } else if (t.type == TokenType.R_BRACKET) {
                    //-------------
                    // Right )
                    //-------------
                    bool missingBracket = true;

                    while (m_Stack.Count > 0) {
                        Token top = m_Stack.Pop();
                        if (top.type != TokenType.L_BRACKET) {
                            m_OutputQ.Enqueue( top );
                        } else {
                            missingBracket = false;
                            break;
                        }
                    }

                    if (missingBracket) {
                        // Error (e.g. Mismatched parens)
                        Debug.LogError( "CalculatorScreen: ShuntingYard() Misplaced Bracket" );
                        return false;
                    }
                }

            } // while()


            // Handle remaining on stack
            while (m_Stack.Count > 0) {
                Token top = m_Stack.Pop();
                if (top.type == TokenType.L_BRACKET || top.type == TokenType.R_BRACKET) {
                    return false;
                }
                m_OutputQ.Enqueue( top );
            }

            Debug.Log( "Calculator: ShuntingYard() OutputQ: " + PrintQueue( m_OutputQ ) );
            //----------------------------
            //  Evaluate
            //----------------------------
            while (m_OutputQ.Count > 0) {
                t = m_OutputQ.Dequeue();

                if (t.type == TokenType.NUMBER) {
                    m_Stack.Push( t );
                } else if (t.type == TokenType.FUNCTION || IsOperator( t )) {
                    //--------------------
                    // Operator/Function
                    //--------------------
                    string op = (string) t.value;

                    OperatorHandler oh;
                    if (m_OperatorHandlers.TryGetValue( op, out oh )) {
                        m_Stack.Push( new Token( TokenType.NUMBER, Associativity.None, 0, oh() ) );
                    } else {
                        Debug.Log( "CalculatorScreen: ShuntingYard() Invalid Operator/Function: " + (string)t.value );
                        return false;
                    }

                } else {
                    // Error?
                }
            }


            if (m_Stack.Count == 1) {
                // Success!
                m_Result = PopNumber();
                m_HavePreviousResult = true;
                m_HaveResultUnits = m_Scanner.HaveUnits();
                return true;
            }

            return false;
        } 
        catch(Exception) 
        {

        }

        return false;
    } // ShuntingYard()


    private float Add()
    {
        // Pop Operands
        float num1 = PopNumber();
        float num2 = PopNumber();
        return num2 + num1;
    }
    private float Subtract()
    {
        // Pop Operands
        float num1 = PopNumber();
        float num2 = PopNumber();
        return num2 - num1;
    }
    private float Multiply()
    {
        // Pop Operands
        float num1 = PopNumber();
        float num2 = PopNumber();
        return num1 * num2;
    }
    private float Divide()
    {
        // Pop Operands
        float den = PopNumber();
        float num = PopNumber();
        return num / den;
    }
    private float Exponent()
    {
        // Pop Operands
        float exp = PopNumber();
        float bas = PopNumber();
        return Mathf.Pow(bas, exp );
    }
    private float Sqrt()
    {
        return Mathf.Sqrt( PopNumber() );
    }
    private float Sin()
    {
        return Mathf.Sin( PopAngleInRadians() );
    }
    private float ASin()
    {
        return Mathf.Asin( PopNumber() ) * ((m_InDegState) ? Mathf.Rad2Deg : 1f);
    }
    private float Cos()
    {
        return Mathf.Cos( PopAngleInRadians() );
    }
    private float ACos()
    {
        return Mathf.Acos( PopNumber() ) * ((m_InDegState) ? Mathf.Rad2Deg : 1f);
    }
    private float Tan()
    {
        return Mathf.Tan( PopAngleInRadians() );
    }
    private float ATan()
    {
        return Mathf.Atan( PopNumber() ) * ((m_InDegState) ? Mathf.Rad2Deg : 1f);
    }
    private float Log()
    {
        return Mathf.Log10( PopNumber() );
    }

    /*##########################################

                Diagram Functions

    ###########################################*/

    private float Bh()
    {
        // Pop Operands
        float a = PopAngleInRadians();
        float r = PopNumber();
        return Calculator.Bh( r, a );
    }
    private float Bv()
    {
        float a = PopAngleInRadians();
        float r = PopNumber();
        return Calculator.Bv( r, a );
    }
    private float Ls()
    {
        float a = PopAngleInRadians();
        float vs = PopNumber();
        return Calculator.Ls( vs, a );
    }
    private float Lb()
    {
        float a = PopAngleInRadians();
        float r = PopNumber();
        return Calculator.Lb( r, a );
    }
    private float Es()
    {
        float a = PopAngleInRadians();
        float r = PopNumber();
        return Calculator.Es( r, a );
    }
    private float Hb()
    {
        float a = PopAngleInRadians();
        float r = PopNumber();
        return Calculator.Hb( r, a );
    }
    private float Hs()
    {
        float a = PopAngleInRadians();
        float ls = PopNumber();
        return Calculator.Hs( ls, a );
    }
    private float Vb()
    {
        float a = PopAngleInRadians();
        float r = PopNumber();
        return Calculator.Vb( r, a );
    }

}
