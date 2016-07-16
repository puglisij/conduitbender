using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class ConduitDecoratorItem
{
    public string               bendName;
    public AConduitDecorator    decoratorPrefab;
}
public class ConduitDecoratorFactory : MonoBehaviour
{
    public List<ConduitDecoratorItem>   decorators = new List<ConduitDecoratorItem>();

    private static Dictionary<string, AConduitDecorator> m_Decorators = new Dictionary<string, AConduitDecorator>();

    void Awake()
    {
        for(int i = 0; i < decorators.Count; ++i) {
            m_Decorators.Add( decorators[ i ].bendName, decorators[ i ].decoratorPrefab );
        }
    }

    /// <summary>
    /// Returns prefab associated with Bend Name, Else null
    /// </summary>
    public static AConduitDecorator Get(string bendName)
    {
        AConduitDecorator decorator;
        if(m_Decorators.TryGetValue(bendName, out decorator )) {
            return decorator;
        }
        return null;
    }
	
}
