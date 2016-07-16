using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class EnumDropdown : Widget
{
    [Serializable]
    public class EnumOptionData : Dropdown.OptionData
    {
        public string description { get { return m_Description; } set { m_Description = value; } }
        [SerializeField]
        private string m_Description;

        public EnumOptionData(string text, Sprite image, string description)
        {
            this.text = text;
            this.image = image;
            this.description = description;
        }
    }

    //--------------------------
    //      Public Data
    //--------------------------
    public int value
    {
        get { return dropdown.value; }
        set { dropdown.value = value; }
    }
    public Dropdown.DropdownEvent onValueChanged
    {
        get { return m_OnValueChanged; }
        set { m_OnValueChanged = value; }
    }
    public List<Dropdown.OptionData> options
    {
        get {
            return dropdown.options;
        }
        set {
            dropdown.options =  value;
            Refresh();
        }
    }

    public Text     displayText;

    public Dropdown dropdown;

    //--------------------------
    //      Private Data
    //--------------------------
    [SerializeField]
    private Dropdown.DropdownEvent m_OnValueChanged;

    private bool              m_VisualsDirty = false;

    void Awake()
    {
#if UNITY_EDITOR

#endif
        m_OnValueChanged = GetComponentInChildren<Dropdown>().onValueChanged;
        m_OnValueChanged.AddListener( ( val ) => {
            m_VisualsDirty = true;
        } );
    }
    void Update()
    {
        if (m_VisualsDirty) {
            UpdateVisuals();
            m_VisualsDirty = false;
        }

    }
    void UpdateVisuals()
    {
        EnumOptionData data = options[value] as EnumOptionData;
        if (data != null) {
            displayText.text = data.description;
        }
    }
    /// <summary>
    /// Refresh the Visuals
    /// </summary>
    public void Refresh()
    {
        dropdown.captionText.text = options[ value ].text;
        m_VisualsDirty = true;
    }
	
}
