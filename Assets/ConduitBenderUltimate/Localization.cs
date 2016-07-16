using UnityEngine;

using System;
using System.Collections.Generic;


public class Localization
{
    [Serializable]
    public class Translation
    {
        public Dictionary<string, string>  nameToTranslation = new Dictionary<string, string>();
    }
    [Serializable]
    public class Language
    {
        string      language;
        TextAsset   textAsset;
        Translation translation;
    }

    private Dictionary<string, Language> languages = new Dictionary<string, Language>();


    private void LoadLanguage( string language )
    {
        //nameToTranslation.Clear();
        //TextAsset textAsset = Resources.Load("I18N/" + language);
        //string allTexts="";
        //if (textAsset == null) {
        //    textAsset = (TextAsset)UnityEditor.AssetDatabase.LoadAssetAtPath( path + language, typeof( TextAsset ) );
        //}
        //allTexts = textAsset.text;
        //string[] lines=allTexts.Split(new string[] { "\r\n", "\n" },
        //StringSplitOptions.None);
        //string key, value;
        //for (int i = 0; i < lines.Length; i++) {
        //    if (lines[ i ].IndexOf( "=" ) >= 0 && !lines[ i ].StartsWith( "#" )) {
        //        key = lines[ i ].Substring( 0, lines[ i ].IndexOf( "=" ) );
        //        value = lines[ i ].Substring( lines[ i ].IndexOf( "=" ) + 1,
        //                lines[ i ].Length - lines[ i ].IndexOf( "=" ) - 1 ).Replace( "\\n", Environment.NewLine );
        //        nameToTranslation.Add( key, value );
        //    }
        //}
    }


}
