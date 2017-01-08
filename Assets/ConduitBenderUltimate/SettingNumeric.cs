using UnityEngine;
using UnityEngine.UI;
using System;


/// <summary>
/// Handles input for numeric settings on Settings page as well as Presets button.
/// </summary>
public class SettingNumeric : MonoBehaviour
{
    public delegate void OnValue( float value );
    public delegate void OnPresetClick();

    public InputField inputField;

    public Text formattedText;
    public Text descriptionText;
    public Text unitText;

    public Button incBigBtn;
    public Button incSmallBtn;
    public Button decBigBtn;
    public Button decSmallBtn;

    /// <summary> Number of digits of precision after the decimal point that will be displayed </summary>
    public int precision = 4;

    /// <summary> Event fires when value changes </summary>
    public OnValue onValue;
    /// <summary> Event fires preset button is clicked </summary>
    public OnPresetClick onPresetClick;

    public float value {
        get { return m_value; }
        set {
            if(value < m_minValue) {
                m_value = Units.Round( m_minValue, precision );
            } else if(value > m_maxValue) {
                m_value = Units.Round( m_maxValue, precision );
            } else {
                m_value = Units.Round( value, precision );
            }
            inputField.text = m_value.ToString();
            formattedText.text = Units.Format( Engine.unitType, m_unit, m_value );

            // Fire Value Changed event
            if (onValue != null) {
                onValue( m_value );
            }
        }
    }

    //---------------------
    //  Private
    //---------------------
    float m_value;

    float m_maxValue = float.PositiveInfinity;
    float m_minValue = 0f;

    Units.RulerUnit m_unit;

    void Awake()
    {
        inputField.onEndEdit.AddListener( OnEndEdit );
    }

    void OnEndEdit(string valueStr)
    {
        try {
            value = float.Parse( valueStr );
        } catch(Exception) {
            value = 0f;
        }
    }

    public void Adjust(float deltaValue)
    {
        value = m_value + deltaValue;
    }

    public void PresetClick()
    {
        if(onPresetClick != null) {
            onPresetClick();
        }
    }

    /// <summary>
    /// Set the Range, Increment & Decrement button deltas, the Description, and the Unit Text (e.g. in or cm)
    /// </summary>
    public void Setup( float min, float max, float bigIncDec, float smallIncDec, string description, Units.RulerUnit unit )
    {
        // Button Listeners
        incBigBtn.onClick.RemoveAllListeners();
        incSmallBtn.onClick.RemoveAllListeners();
        decBigBtn.onClick.RemoveAllListeners();
        decSmallBtn.onClick.RemoveAllListeners();

        incBigBtn.onClick.AddListener( () => { Adjust( bigIncDec ); } ); 
        incSmallBtn.onClick.AddListener( () => { Adjust( smallIncDec ); } );
        decBigBtn.onClick.AddListener( () => { Adjust( -bigIncDec ); } );
        decSmallBtn.onClick.AddListener( () => { Adjust( -smallIncDec ); } );

        // Range 
        m_minValue = min;
        m_maxValue = max;

        m_unit = unit;

        // Description & Unit Text
        descriptionText.text = description;
        unitText.text = Units.k_UnitAbbreviations[ (int)unit ];
    }
}
