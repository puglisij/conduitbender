using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections;


/// <summary>
/// Handles input for slider settings on Settings page. Places sliders in a vertical scroll rect.
/// </summary>
public class SettingSliders : MonoBehaviour
{
    public delegate void OnValue( float value );

    public RectTransform scrollContent;

    /// <summary> A slider prefab which has a single Slider, and single Text component in its hierarchy </summary>
    public RectTransform sliderPrefab;

    public Text descriptionText;

    public void RemoveAllSliders()
    {
        scrollContent.DestroyChildren();
    }

    public void AddSlider(string sliderName, float value, UnityAction<float> valueHandler )
    {
        // Add Slider Prefab to Scroll Content      
        RectTransform prefab = (RectTransform)Instantiate( sliderPrefab, scrollContent, false );
        Vector2 size = prefab.rect.size;

        float oldScrollContentSize = scrollContent.childCount > 0 ? scrollContent.rect.size.y : 0f;

        scrollContent.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, oldScrollContentSize + size.y );
        prefab.SetInsetAndSizeFromParentEdge( RectTransform.Edge.Top, oldScrollContentSize, size.y );
        prefab.GetComponentInChildren<Text>( true ).text = sliderName;
        prefab.GetComponentInChildren<Slider>(true).onValueChanged.AddListener( valueHandler ); 
    }

    /// <summary>
    /// Sets the description text for this Setting widget and removes all previous sliders.
    /// </summary>
    public void Setup(string description)
    {
        RemoveAllSliders();

        descriptionText.text = description;
    }
}
