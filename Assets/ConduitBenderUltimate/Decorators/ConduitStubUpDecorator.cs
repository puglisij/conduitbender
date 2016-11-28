using UnityEngine;
using System.Collections;
using System;

public class ConduitStubUpDecorator : AConduitDecorator
{
    public override void Decorate()
    {

    }

    public override void Highlight()
    {
 
    }

    public override void OnRemove()
    {
 
    }

    public override void Set( Conduit conduit )
    {
        m_Conduit = conduit;

    }
}
