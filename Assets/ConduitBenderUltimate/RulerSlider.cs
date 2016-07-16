using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class RulerSlider : Slider {

    
    public Button minusButton;
    public Button plusButton;


    protected override void Awake()
    {
        base.Awake();

        // Register callbacks on buttons
        if(minusButton != null) {
            minusButton.onClick.AddListener( () => {
                value -= 1f;
            } );
        }
        if(plusButton != null) {
            plusButton.onClick.AddListener( () => {
                value += 1f;
            } );
        }
    }


    public void SetValue(float val, bool sendCallback)
    {
        Set( val, sendCallback );
        //OnValidate(); 
    }

}
