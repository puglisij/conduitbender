using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class ScrollRectExtra : ScrollRect {

    //[ExposeProperty]
    public bool allowCentering
    {
        get { return m_AllowCentering; }
        set {
            m_AllowCentering = value;
            Layout();
        }
    }
    [Tooltip("As a percentage (0 to 1) of Viewport"), SerializeField, Range(0.01f, 1.0f)]
    public float itemSize;
    [SerializeField]
    public bool isVertical;

    /*------------------------------------

            Private Data

    ------------------------------------*/
    private RectTransform[] m_PosToItem;      // Maintain record of order of items at Start()

    private Vector2 m_ViewCenter;
    private Vector2 m_ViewportSize;
    //private Vector2 m_ZeroAnchoredPosition;

    //private float m_CanvasScale;
    private float m_ItemSize = 0.01f;

    [SerializeField]
    private bool m_AllowCentering;
    [Tooltip("Enlarge (scale) the item in center of the viewport.")]
    private bool m_AllowEnlarging;
    private bool m_VisualsDirty = false;

    private int  m_CachedChildCount;
    private int  m_FrameCountOnClear;

    protected override void Awake()
    {
        base.Awake();

    }
    protected override void OnEnable()
    {
        base.OnEnable();

        //if (!Application.isPlaying || !m_HasStarted || Mathf.Approximately( viewport.rect.width, 0f )) { return; }


    }
    protected override void Start () {
        base.Start();


        Initialize();

        //Debug.Log( "ScrollRectExtra: OnEnable() m_ViewportSize: " + viewport.rect.size + " m_ViewportSize (scaled): " + m_ViewportSize
        //    + " m_ItemSize: " + m_ItemSize + " ScreenSize: " + Screen.width + "," + Screen.height
        //    + " m_CanvasScale: " + m_CanvasScale );
    }
    void Update()
    {
        if(m_VisualsDirty && !(Time.frameCount == m_FrameCountOnClear)) {
            UpdateVisuals();
            m_VisualsDirty = false;
        }
    }

    public void ClearContent()
    {
        // IMPORTANT LINE: Since content.childCount is not accurate on the same Frame that this Function was Called, improper Layout will Result
        m_FrameCountOnClear = Time.frameCount;

        for (int i = 0; i < content.childCount; ++i) {
            Destroy( content.GetChild( i ).gameObject );
        }
    }
    public void Initialize()
    {
        /* Do Initializations that require us to know viewport size */
        //m_CanvasScale = GetComponentInParent<Canvas>().scaleFactor;
        m_ViewportSize = viewport.rect.size;
        m_ViewCenter.Set( m_ViewportSize.x / 2f, m_ViewportSize.y / 2f );
        if (isVertical) {
            m_ItemSize = m_ViewportSize.y * itemSize;
            vertical = true;
            horizontal = false;
        } else {
            m_ItemSize = m_ViewportSize.x * itemSize;
            vertical = false;
            horizontal = true;
        }

        // Initialize 
        m_PosToItem = new RectTransform[ content.childCount ];
        for (int c = 0; c < content.childCount; ++c) {
            m_PosToItem[ c ] = (RectTransform)content.GetChild( c );
        }

        Layout();
    }
    /// <summary>
    /// Resets the 
    /// </summary>
    public void Layout()
    {
        m_VisualsDirty = true;
    }
    /*#################################
        
            Private Functions

    ##################################*/
    private void LayoutContentChildren()
    {
        Vector2 ap;
        try {
            if(isVertical) 
            {
                for (int c = 0; c < m_CachedChildCount; ++c) {
                    RectTransform child = (RectTransform) content.GetChild( c );
                    child.pivot = new Vector2( 0.5f, 0.5f );
                    child.anchorMin = new Vector2( 0f, 1f );
                    child.anchorMax = Vector2.one;
                    child.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, m_ItemSize );
                    ap.x = 0f; 
                    ap.y = -(c * m_ItemSize) - ((m_AllowCentering) ? m_ViewCenter.y : m_ItemSize / 2f);
                    child.anchoredPosition = ap;
                }
            } else 
            {
                for( int c = 0; c < m_CachedChildCount; ++c) {
                    RectTransform child = (RectTransform) content.GetChild( c );
                    child.pivot = new Vector2( 0.5f, 0.5f );
                    child.anchorMin = Vector2.zero;
                    child.anchorMax = new Vector2( 0f, 1f );
                    child.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, m_ItemSize );
                    ap.x = c * m_ItemSize + ((m_AllowCentering) ? m_ViewCenter.x : m_ItemSize / 2f);
                    ap.y = 0f;
                    child.anchoredPosition = ap;
                }
            }
        } catch (Exception e) {
            Debug.LogError( "ScrollRectExtra: LayoutContentChildren() Exception: " + e.ToString() );
        }
    }
    private void LayoutContent()
    {
        Vector2 cap;
        float   size;

        content.anchorMin = Vector2.zero;
        content.anchorMax = Vector2.one;

        // Assume Item pivots are in center (0.5, 0.5)
        if (isVertical) {
            float prevHeight= content.rect.height;

            // Size content with extra space so we can scroll 'end' elements to center of view
            size = m_CachedChildCount * m_ItemSize + ((m_AllowCentering) ? (m_ViewCenter.y - m_ItemSize / 2f) * 2f : 0f);
            if(size < m_ViewportSize.y) {
                size = m_ViewportSize.y;
            }

            // Maintain verticalNormalizedPosition before resize
            cap = content.anchoredPosition;
            cap.y += (size - prevHeight) * content.pivot.y;

            verticalNormalizedPosition = 0f;
            content.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, size );
        } else {
            float prevWidth = content.rect.width;

            // Size content with extra space so we can scroll 'end' elements to center of view
            size = m_CachedChildCount * m_ItemSize + ((m_AllowCentering) ? (m_ViewCenter.x - m_ItemSize / 2f) * 2f : 0f);
            if(size < m_ViewportSize.x) {
                size = m_ViewportSize.x;
            }

            // Maintain horizontalNormalizedPosition before resize
            cap = content.anchoredPosition;
            cap.x += (size - prevWidth) * content.pivot.x;

            horizontalNormalizedPosition = 0f;
            content.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, size );
        }
        // Calculate Anchored Position at 0 h|vNormalizedPosition
        //m_ZeroAnchoredPosition = content.anchoredPosition;
        content.anchoredPosition = cap;

        //Debug.Log( "ScrollRectExtra: LayoutContent() size: " + size );
    }

    private void UpdateVisuals()
    {
        m_CachedChildCount = content.childCount;
        // Size Content
        LayoutContent();
        // Size Content Children
        LayoutContentChildren();
    }
}
