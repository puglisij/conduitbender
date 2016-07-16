using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// ScreenControl is the MonoBehaviour component which stores a reference to the controls Button and Image.
/// </summary>
public class ScreenControl : MonoBehaviour {
    public enum EStartIcon { Icon1, Icon2 }

    public Button      button;

    public Image       controlIcon;

    private Sprite     m_icon1;
    private Sprite     m_icon2;

    void ToggleSprite()
    {
        if(controlIcon.sprite == m_icon1) {
            controlIcon.sprite = m_icon2;
        } else {
            controlIcon.sprite = m_icon1;
        }
    }
    /// <summary>
    /// Deactivates Control gameObject
    /// </summary>
    public void Off()
    {
        gameObject.SetActive( false );
    }
    /// <summary>
    /// Sets control button callback, icon, and Activates
    /// </summary>
    /// <param name="action"></param>
    /// <param name="icon"></param>
    public void Set(UnityAction action, Sprite icon )
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener( action );
        controlIcon.sprite = icon;
        m_icon1 = m_icon2 = null;

        gameObject.SetActive( true );
    }
    /// <summary>
    /// Sets the control button callback, Activates, and sets the icons to toggle between.
    /// </summary>
    public void Set(UnityAction action, Sprite icon1, Sprite icon2, EStartIcon startIcon )
    {
        m_icon1 = icon1;
        m_icon2 = icon2;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener( action );
        button.onClick.AddListener( ToggleSprite );
        if (startIcon == EStartIcon.Icon1)
            controlIcon.sprite = m_icon1;
        else
            controlIcon.sprite = m_icon2;

        gameObject.SetActive( true );
    }
    public void SetIcon(Sprite icon)
    {
        controlIcon.sprite = icon;
    }

}
