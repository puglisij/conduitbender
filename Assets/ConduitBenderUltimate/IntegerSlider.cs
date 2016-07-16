using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IntegerSlider : Widget
{
    public delegate void IntegerSliderAction( int value );

    public int value
    {
        get { return (int) slider.value; }
        set { slider.value = value; }
    }
    public IntegerSliderAction onValueChanged
    {
        get { return m_OnValueChanged; }
        set { m_OnValueChanged = value; }
    }

    [SerializeField]
    public Text             displayText;
    [SerializeField]
    public Text             displayName;
    [SerializeField]
    public Slider           slider;

    [SerializeField]
    private IntegerSliderAction  m_OnValueChanged = null;

    private bool                m_VisualsDirty = false;

    void Awake()
    {
        var ovc = GetComponentInChildren<Slider>().onValueChanged;
        ovc.AddListener( ValueChanged );
        
    }
    void Start()
    {
        displayText.text = "0";
    }

    void Update()
    {
        if (m_VisualsDirty) {
            UpdateVisuals();
            m_VisualsDirty = false;
        }

    }

    void ValueChanged(float value)
    {
        if(m_OnValueChanged != null) {
            m_OnValueChanged( (int)value );
        }
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        displayText.text = value.ToString();
    }

    public void Set(string name, float min, float max)
    {
        displayName.text = name;
        slider.minValue = min;
        slider.maxValue = max;
    }
    public void SetName(string name)
    {
        displayName.text = name;
    }
    public void SetRange(int min, int max)
    {
        slider.minValue = min;
        slider.maxValue = max;

        m_VisualsDirty = true;
    }
}
