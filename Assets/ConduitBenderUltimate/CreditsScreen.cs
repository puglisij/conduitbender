using UnityEngine;
using System.Collections;

public class CreditsScreen : AnimScreen
{
    //-------------------
    // Public Data
    //-------------------
    public RectTransform thankYouText;

    //-------------------
    // Private Data
    //-------------------

    string email = "support@nicaeastudios.com";


    public override void Open()
    {
        base.Open();

        // Wobble Text

    }

    public void OnEmail()
    {
        
        //subject of the mail
        string subject = MyEscapeURL("Feedback/Suggestion");
        //body of the mail which consists of Device Model and its Operating System
        string body = MyEscapeURL("Enter your message here.\n\n\n\n" +
   "________" +
   "\n\nPlease Do Not Modify This\n\n" +
   "Model: "+SystemInfo.deviceModel+"\n\n"+
      "OS: "+SystemInfo.operatingSystem+"\n\n" +
   "________");

        //Open the Default Mail App
        Application.OpenURL( "mailto:" + email + "?subject=" + subject + "&body=" + body );
    }

    public void OnRate()
    {

        #if UNITY_ANDROID
                    Application.OpenURL( "market://details?id=YOUR_APP_ID" );
        #elif UNITY_IPHONE
             Application.OpenURL("itms-apps://itunes.apple.com/app/idYOUR_APP_ID");
        #endif

    }

    private string MyEscapeURL( string url )
    {
        return WWW.EscapeURL( url ).Replace( "+", "%20" );
    }

}
