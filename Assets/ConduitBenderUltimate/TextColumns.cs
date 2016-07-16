using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Simple Text Controller which writes lines to Text components representing Columns
/// Each column can have a specified number of lines/rows.
/// 
/// Expects the Column Text components to use Anchors to size their RectTransform
/// </summary>
public class TextColumns : MonoBehaviour {

    public struct TextPair
    {
        public static TextPair empty
        {
            get { return new TextPair( "", "" ); }
        }
        public string left;
        public string right;

        public TextPair(string left, string right)
        {
            this.left = left; this.right = right;
        }
    }

    [Serializable]
	public class Column
    {
        [SerializeField]
        public Text text = null;
        [SerializeField]
        public int  maxLines = 1;
        [SerializeField, HideInInspector]
        public int  maxCharPerLine = 0;
        [HideInInspector]
        public bool visualsDirty = false;

    }

    public int maxCharPerLineTotal
    {
        get { return m_MaxCharPerLineTotal; }
        set
        {
            m_MaxCharPerLineTotal = value;
            CalculateTextSizes();
            CalculateColumnCharPerLine();
        }
    }
    public int lineCount
    {
        get { return m_Lines.Count; }
    }
    [SerializeField]
    public List<Column>     columns;
    [SerializeField]
    public TextExtra        textMetrics;


    private List<TextPair>  m_Lines = new List<TextPair>();
    [SerializeField, Tooltip("Maximum Characters on one line to fit across all columns horizontally")]
    private int             m_MaxCharPerLineTotal = 10;
    private int             m_CurrentWriteColumn = 0;

    private bool            m_VisualsDirty = false;

    void Awake()
    {
#if UNITY_EDITOR
        CheckNulls();
#endif
    }
    void Start()
    {
        CalculateTextSizes();
    }
    void Update()
    {
        if(m_VisualsDirty) {
            RefreshColumns();
            m_VisualsDirty = false;
        }
    }

    void CalculateColumnCharPerLine()
    {
        RectTransform columnTransform;
        for(int c = 0; c < columns.Count; ++c) {
            columnTransform = columns[ c ].text.rectTransform;
            columns[ c ].maxCharPerLine = (int) (columnTransform.anchorMax.x - columnTransform.anchorMin.x) * maxCharPerLineTotal;
        }
    }
    void CheckNulls()
    {
        if(columns.Count < 1) {
            Debug.LogWarning( "TextColumns: No Columns Set." );
            return;
        }
        for(int c = 0; c < columns.Count; ++c) {
            if(columns[c].text == null) {
                Debug.LogError( "TextColumns: Column(" + c + ") null." );
                return;
            }
        }
    }
    void CalculateTextSizes()
    {
        int bestSize = textMetrics.CalculateBestFontSize( columns[ 0 ].text, (RectTransform) transform, maxCharPerLineTotal );

        for (int c = 1; c < columns.Count; ++c) {
            columns[ c ].text.fontSize = bestSize;
        }
        
    }
    int GetColumnIndex(int line)
    {
        int range = 0;
        for(int c = 0; c < columns.Count; ++c) {
            range += columns[ c ].maxLines;
            if(line <= range) { return c; }
        }
        return -1;
    }

    void RefreshColumns()
    {
        int startLine = 0;
        for(int c = 0; c < columns.Count; ++c) {
            if(columns[c].visualsDirty) {
                RefreshColumn( c, startLine );
            }
            startLine += columns[ c ].maxLines;
        }
    }
    void RefreshColumn(int col, int startLine)
    {
        var column = columns[col];
        StringBuilder text = new StringBuilder();
        for(int l = startLine; l < startLine + column.maxLines && l < m_Lines.Count; ++l) {
            text.Append( m_Lines[ l ].left );
            text.Append( m_Lines[ l ].right );
            text.Append( "\n" );
        }
        column.text.text = text.ToString();
        column.visualsDirty = false;
    }


    /*##################################

            Public Functions

    ##################################*/
    /// <summary>
    /// Remove all TextPairs from internal array and set
    /// Columns Texts to empty string.
    /// </summary>
    public void Clear()
    {
        m_Lines.Clear();
        m_CurrentWriteColumn = 0;
        for(int c = 0; c < columns.Count; ++c) {
            columns[ c ].text.text = "";
        }
    }
    public List<TextPair> GetLines()
    {
        return m_Lines;
    }

    /// <summary>
    /// Returns the combined text displayed in the specified column.
    /// Returns empty string if invalid column
    /// </summary>
    public string GetColumnText(int col)
    {
        if(col < 0 || col >= columns.Count) {
            return "";
        }
        return columns[ col ].text.text;
    }

    public bool GetLine(int line, out TextPair pair)
    {
        if(line < m_Lines.Count) {
            pair = m_Lines[ line ];
        }
        pair = TextPair.empty;
        return false;
    }
    /// <summary>
    /// Sets internal Line list to 'lines' and Refreshes Output view.
    /// </summary>
    public void WriteLines( List<TextPair> lines)
    {
        if(lines.Count != m_Lines.Count) {
            throw new ArgumentException( "TextColumn: WriteLines() Parameter list length must match current line count." );
        }
        m_Lines = lines;
        m_VisualsDirty = true;
        for(int c = 0; c < columns.Count; ++c) {
            columns[ c ].visualsDirty = true;
        }
    }
    /// <summary>
    /// Writes the TextPair to the specified line. If 'autoAppend', then 'line' must be equal to current line count, 
    /// or no appendage takes place.
    /// </summary>
    public void WriteLine( int line, TextPair text, bool autoAppend = false)
    {
        if (line < m_Lines.Count) {
            m_Lines[ line ] = text;
            // Mark Visuals Dirty
            columns[ GetColumnIndex( line ) ].visualsDirty = true;
            m_VisualsDirty = true;
        } else if(autoAppend && line == m_Lines.Count) {
            Write( text );
        }
    }
    /// <summary>
    /// Append a new TextPair to internal lines.
    /// Returns true if successfully appended. False if line count is at 'maxLines'
    /// </summary>
    public bool Write(TextPair text)
    {
        // @TODO - Currently no Checking for text overflow (maxCharPerLine)
        int totalLines = m_Lines.Count + 1;
        int column = GetColumnIndex(totalLines);
        if( column == -1 ) {
            return false;
        }
        m_CurrentWriteColumn = column;
        m_Lines.Add( text );
        // Mark Visuals Dirty
        columns[ m_CurrentWriteColumn ].visualsDirty = true;
        m_VisualsDirty = true;

        return true;
    }


}
