using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public abstract class AConduitDecorator : MonoBehaviour
{
    public Conduit conduit {
        get { return m_Conduit; }
    }
    protected Conduit m_Conduit = null;
    protected bool m_isHighlighted = false;

    public abstract void Decorate();
    public abstract void Highlight();
    public abstract void OnRemove();
    // NOTE: Do NOT call Decorate() from Set()
    public abstract void Set( Conduit conduit );

}
public class ConduitDefaultDecorator : AConduitDecorator
{
    public override void Decorate()  { }
    public override void Highlight() { }
    public override void OnRemove() { }
    public override void Set( Conduit conduit ) { }
    
}

