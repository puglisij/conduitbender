using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class MainScreen : AnimScreen
{
    public  ScreenManager   screenManager;

    [Space]

    public Button           toggleButton;

   
    protected override void Awake()
    {
        base.Awake();

        CheckNulls();
        Initialize();
    }

    /*##################################

            Private Functions

    ###################################*/
    private void CheckNulls()
    {
        if(toggleButton == null) {
            Debug.LogError( "MainScreen: CheckNulls() Toggle Button not set for Main Screen." );
            return;
        }

    }
    private void Initialize()
    {
        toggleButton.onClick.AddListener( Toggle );
    }

    /*##################################

            Screen Functions

    ###################################*/
    public override void Link( IModel model )
    {
        throw new NotImplementedException();
    }


    public void Toggle()
    {
        if(m_IsOpen) {
            screenManager.CloseMain();
        } else {
            screenManager.OpenMain();
        }
    }

    public override void OnUnfocus()
    {
        if (m_IsOpen) {
            screenManager.CloseMain();
        }
    }
}
