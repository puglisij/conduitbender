using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

/// <summary>
/// One of these should be present in the Scene for any Font recipe (Font & Font Style combination) which 
/// you want to generate size Metrics for.
/// </summary>
//[ExecuteInEditMode]
public class TextExtra : MonoBehaviour {


    private struct MaxChar
    {
        public float x;
        public float y;
        public char  character;
    }

    public Font  font
    {
        get { return m_Font; }
        set {
            // Clear Dictionaries?
            if(value != m_Font) {
                Clear();
            }

            m_Font = value;
        }
    }
    public FontStyle fontStyle
    {
        get { return m_FontStyle; }
        set
        {
            // Clear Dictionaries?
            if(value != m_FontStyle) {
                Clear();
            }
            m_FontStyle = value;
        }
    }

    [Tooltip("A hidden Text element where measurements can be made.")]
    public Text  hiddenText;


    public int   minFontSize;
    public int   maxFontSize;
    public int   fontStepSize;

    //----------------------
    //    Private Data
    //----------------------
    private Dictionary<int, MaxChar>    m_FontSizeToMaxCharSize = new Dictionary<int, MaxChar>();
    private Dictionary<int, Dictionary<char, Vector2>>  m_FontSizeToCharSize = new Dictionary<int, Dictionary<char, Vector2>>();

    [SerializeField, Tooltip( "Font type on which to calculate Metrics." )]
    private Font m_Font;
    [SerializeField]
    private FontStyle m_FontStyle;

    private bool m_HasInitialized = false;

    void Awake()
    {

        if(hiddenText == null) {
            Debug.LogError( "TextExtra: Awake() Hidden Text is null." );
            return;
        }
        if (m_Font == null) {
            Debug.LogError( "TextExtra: Awake() Font is null." );
            return;
        }

        Calculate( );

        m_HasInitialized = true;
        //Debug.Log( "TextExtra: Awake() ");
    }
    // Use this for initialization
    void Start () {
        //TextGenerator tg = new TextGenerator();
        //Debug.Log( "TextMagic: Start() Text value: " + text.text );
        //text.text = "<color=#ffaa00>Blarrggg</color>";

    }
	
    void OnValidate()
    {
        if (!m_HasInitialized) { return; }
        
    }

    //public void GetAllSupportedCharacters(Font font, out List<char> characters )
    //{
    //    characters = new List<char>();
    //    //System.Text.Encoding.UTF8.GetBytes( chars )
    //    //System.Text.Encoding.UTF16.GetString( byte[] )
    //    Encoding UTF16Enc = Encoding.GetEncoding( "utf-16" );
    //    char[] nextChar = new char[1];
    //    char[] nextUTF16Chars;
    //    for (char c = char.MinValue; c < char.MaxValue; ++c) {
    //        nextChar[ 0 ] = c;
    //        nextUTF16Chars = UTF16Enc.GetChars( UTF16Enc.GetBytes( nextChar ) );

    //        if (nextUTF16Chars.Length > 0) {
    //            char UTF8Char = nextUTF16Chars[0];
    //            if (font.HasCharacter( UTF8Char )) {
    //                characters.Add( UTF8Char );
    //            }
    //        }
    //    }
    //}

    /// <summary>
    /// Calculate Font Size Metrics for Accurate Fitting
    /// Should be called if changing min/max font sizes, font step size, or font style, or when using new characters
    /// </summary>
    private void Calculate( )
    {
        // Get all possible characters for selected Language from Engine/Localization
        string allChars = Regex.Replace( Engine.GetLanguageCharacters(), "(<.*?>)|(\\s+)", string.Empty );
        for (int fs = minFontSize; fs <= maxFontSize; fs += fontStepSize) {
            CalculateFontMetrics( fs, allChars );
        }
    }
    /// <summary>
    /// Calculates and sets Text font size to maximum Font Size at which specified # characters per line will fit. 
    /// Returns best size
    /// </summary>
    public int CalculateBestFontSize(Text text, RectTransform bounds, int charPerLine)
    {
        if(charPerLine < 1) {
            Debug.LogError( "TextExtra: CalculateBestFontSize() Characters per line must be a natural number except 0." );
            return -1;
        }
        if(text.font != font || text.fontStyle != fontStyle) {
            Debug.LogError( "TextExtra: CalculateBestFontSize() Given Text style does not match." );
            return -1;
        }
        if(bounds == null) {
            bounds = text.GetComponentsInParent<RectTransform>( true )[0];
        }
        // Copy Text Component Values
        hiddenText.font = font;
        hiddenText.fontStyle = fontStyle;

        // Init Vars
        int bestFit = minFontSize;
        MaxChar maxCharSize;
        // Loop through Font Sizes
        for (int fs = minFontSize; fs <= maxFontSize; fs += fontStepSize) {
            if (m_FontSizeToMaxCharSize.TryGetValue( fs, out maxCharSize )) {
                // @TODO - Binary Search?
                // Measure Hidden Text Component
                hiddenText.text = new string( maxCharSize.character, charPerLine );
                hiddenText.fontSize = fs;
                float width = LayoutUtility.GetPreferredWidth( (RectTransform) hiddenText.transform );

                //Debug.Log( "Font Size: " + fs + " Preferred Width: " + width + " Bounds Width: " + bounds.rect.size.x );
                //if ((maxCharSize.x * charPerLine) / text.pixelsPerUnit < m_Bounds.rect.size.x) {
                if (width < bounds.rect.size.x) {
                    bestFit = fs;
                } else {
                    break;
                }
            }
        }
        // Set Text Component to Best Fit
        text.fontSize = bestFit;
        Debug.Log( "TextExtra: CalculateBestFontSize() BestFit: " + bestFit );

        return bestFit;
    }
    /// <summary>
    /// Clears the Font Metrics
    /// Should only be called when changing Font or Font style and before calling ReCalculate
    /// </summary>
    public void Clear()
    {
        m_FontSizeToMaxCharSize.Clear();
        m_FontSizeToCharSize.Clear();

        Debug.Log( "<color=#ff0000>TextExtra: Clear()</color>" );
    }
    

    private void CalculateFontMetrics(int fontSize, string allChars )
    {
        //StringBuilder sb = new StringBuilder();
        //for(int i = 32; i < 127; ++i ) {
        //    sb.Append( Convert.ToUInt16( i ) );
        //}
        //string allChars = sb.ToString();

        // Look for Maximum Size
        MaxChar maxChar;
        maxChar.x = maxChar.y = 0f;
        maxChar.character = System.Char.MinValue;

        // Ensure our Font Texture contains the current characters
        font.RequestCharactersInTexture( allChars, fontSize );

        Dictionary<char, Vector2> charSizes;
        if(!m_FontSizeToCharSize.TryGetValue( fontSize, out charSizes)) {
            charSizes = new Dictionary<char, Vector2>();
            m_FontSizeToCharSize.Add( fontSize, charSizes );
        }


        //Loop through all characters
        CharacterInfo charInfo;
        for (int i = 0; i < allChars.Length; i++) {

            //Make sure character exists in font texture
            if (font.GetCharacterInfo( allChars[ i ], out charInfo, fontSize ) ) {
                Vector2 charSize;
                charSize.x = charInfo.maxX;
                charSize.y = charInfo.maxY;

                if(charInfo.maxX > maxChar.x) {
                    maxChar.character = allChars[ i ];
                    maxChar.x = charInfo.maxX;
                }
                if(charInfo.maxY > maxChar.y) {
                    maxChar.y = charInfo.maxY;
                }
                if(!charSizes.ContainsKey(allChars[i])) {
                    charSizes.Add( allChars[ i ], charSize );
                }

            }
        }
        MaxChar currMaxSize;
        bool containsMaxSize = m_FontSizeToMaxCharSize.TryGetValue( fontSize, out currMaxSize );

        // Update Maximum Detected Character Size for this Specific Font Size
        if(containsMaxSize) {
            if(maxChar.x > currMaxSize.x) {
                currMaxSize.character = maxChar.character;
                currMaxSize.x = maxChar.x;
            }
            if(maxChar.y > currMaxSize.y) {
                currMaxSize.y = maxChar.y;
            }
            m_FontSizeToMaxCharSize.Remove( fontSize );
            m_FontSizeToMaxCharSize.Add( fontSize, currMaxSize );
        } else {
            m_FontSizeToMaxCharSize.Add( fontSize, maxChar );
        }


        //Debug.Log( "TextExtra: PrintCharMetrics() All Chars: " + allChars );
        //Debug.Log( "TextExtra: PrintCharMetrics() Font Size: " + fontSize + " Max Char Size: " + maxChar );
    }


}
