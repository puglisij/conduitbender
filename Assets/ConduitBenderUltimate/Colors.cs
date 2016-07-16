using UnityEngine;
using System.Collections;

public class Colors : MonoBehaviour
{
    public Color inputParameterDefault;
    public Color outputParameterDefault;
    public Color flagRed;
    public Color flagOrange;
    public Color flagYellow;
    public Color flagGreen;
    public Color flagLightBlue;
    public Color flagBlue;
    public Color flagConduitBlack;
    public Color flagPurple;

    public static Colors instance = null;

    void Awake()
    {
        // Singleton
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
#if UNITY_EDITOR
            Debug.LogError( "Colors: Awake() Only one instance of Colors should exist in the scene." );
#endif
            Destroy( gameObject );
            return;
        }
    }

}
