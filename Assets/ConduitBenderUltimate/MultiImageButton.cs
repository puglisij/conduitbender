using UnityEngine;
using UnityEngine.UI;
using System;

public class MultiImageButton : UnityEngine.UI.Button
{
    private Graphic[] m_graphics;
    protected Graphic[] Graphics
    {
        get
        {
            if (m_graphics == null) {
                m_graphics = targetGraphic.transform.GetComponentsInChildren<Graphic>();
            }
            return m_graphics;
        }
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        Color color;
        switch (state) {
            case SelectionState.Normal:
                color = this.colors.normalColor;
                break;
            case SelectionState.Highlighted:
                color = this.colors.highlightedColor;
                break;
            case SelectionState.Pressed:
                color = this.colors.pressedColor;
                break;
            case SelectionState.Disabled:
                color = this.colors.disabledColor;
                break;
            default:
                color = Color.black;
                break;
        }
        if (base.gameObject.activeInHierarchy) {
            switch (this.transition) {
                case Transition.ColorTint:
                    ColorTween( color * this.colors.colorMultiplier, instant );
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    private void ColorTween(Color targetColor, bool instant)
    {
        if (targetGraphic == null) {
            return;
        }

        foreach (Graphic g in Graphics) {
            g.CrossFadeColor( targetColor, (!instant) ? this.colors.fadeDuration : 0f, true, true );
        }
    }
}
