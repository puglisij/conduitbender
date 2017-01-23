using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

namespace CB
{
    public class Button2nd : MonoBehaviour
    {
        public CalculatorScreen calculatorScreen;

        public Button btn;
        public Text   btnText;

        public string primaryText;
        public string secondaryText;
        public string primaryValue;
        public string secondaryValue;

        private string m_CurrentValue;

        void Awake()
        {
            m_CurrentValue = primaryValue;

            // Register Listener for 'this' Button 
            btn.onClick.AddListener( OnClick );

            // Register Listener for Secondary Button
            calculatorScreen.onSecondary += Secondary;

            // Cache Delegates if primary/secondary values indicate functions (start with '_')
            //Type type = typeof(CalculatorScreen);
            //MethodInfo handler = type.GetMethod(primaryValue.Substring(1));
            //if (handler != null) {
            //    handler.Invoke( calculatorScreen, new object[] { } );
            //}      
        }

        public void OnClick()
        {
            if(m_CurrentValue[0] == '_') {
                calculatorScreen.SendMessage( m_CurrentValue.Substring(1) );
            } else {
                calculatorScreen.AddInput( m_CurrentValue );
            }  
        }

        public void Secondary(bool isOn)
        {
            if(isOn) {
                btnText.text = secondaryText;
                m_CurrentValue = secondaryValue;
            } else {
                btnText.text = primaryText;
                m_CurrentValue = primaryValue;
            }
        }
    }
}

