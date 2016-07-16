using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AlertText : MonoBehaviour {

    public Text text;

    /// <summary>
    /// Deactivate GameObject
    /// </summary>
    public void Off()
    {
        gameObject.SetActive( false );
        text.text = "";
    }
    /// <summary>
    /// Activate GameObject and set Text
    /// </summary>
	public void Set(string value)
    {
        gameObject.SetActive( true );
        text.text = value;
    }
}
