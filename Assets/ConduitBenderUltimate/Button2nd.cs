using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
        }

        public void OnClick()
        {
            calculatorScreen.AddInput( m_CurrentValue );
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

