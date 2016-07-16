using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class UIBendParameter : MonoBehaviour {

    //public Button.ButtonClickedEvent onClick
    //{
    //    get { return m_OnClick; }
    //    set { m_OnClick = value; }
    //}
    public OnClickDelegate onClick
    {
        set { m_OnClick = value; }
    }
    public delegate void OnClickDelegate( int id);


    public Button button;

    public Sprite angleSprite;
    public Sprite rulerSprite;
    public Sprite integerSprite;
    public Sprite enumSprite;

    public Image  spriteIcon;
    public Text   nameText;

    private BendParameter.Type  type;
    private int                 id;
    //private Button.ButtonClickedEvent m_OnClick = new Button.ButtonClickedEvent();
    private OnClickDelegate m_OnClick = null;


    void Awake()
    {
        button.onClick.AddListener( ButtonListener );
    }

    public void Set(BendParameter.Type type, string name, int id)
    {
        this.id = id;
        this.type = type;
        switch (type) 
        {
            case BendParameter.Type.FloatAngle:
                spriteIcon.sprite = angleSprite;
                break;
            case BendParameter.Type.Float:
                spriteIcon.sprite = rulerSprite;
                break;
            case BendParameter.Type.Integer:
                spriteIcon.sprite = integerSprite;
                break;
            case BendParameter.Type.StringEnum:
                spriteIcon.sprite = enumSprite;
                break;
            default:
                throw new ArgumentException( "UIBendParameter: Set() Invalid type." );
        }
        nameText.text = name;
    }
    public void SetName(string name)
    {
        nameText.text = name;
    }
    
    private void ButtonListener()
    {
        m_OnClick( id );
    }
}
