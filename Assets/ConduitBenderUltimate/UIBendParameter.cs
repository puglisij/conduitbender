using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class UIBendParameter : MonoBehaviour
{

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
    public Sprite helpSprite;

    public Image  spriteIcon;
    public Text   nameText;

    private int                 id;

    private OnClickDelegate m_OnClick = null;


    void Awake()
    {
        button.onClick.AddListener( ButtonListener );
    }

    /// <summary>
    /// IMPORTANT: Currently this just sets the parameter to "Help"
    /// </summary>
    public void SetAsHelp()
    {
        this.id = -1;
        spriteIcon.sprite = helpSprite;
        nameText.text = "Help";
    }

    public void Set(BendParameter.Type type, string name, int id)
    {
        this.id = id;
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

    private void ButtonListener()
    {
        m_OnClick( id );
    }
}
