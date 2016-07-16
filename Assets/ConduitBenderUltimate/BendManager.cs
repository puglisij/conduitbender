using UnityEngine;
using System.Collections;
using System.Collections.Generic;




public class BendManager: ILinker
{
    // Bend Name to Bend Model Instances
    private static Dictionary<string, Bend> m_Bends = new Dictionary<string, Bend>();


    public static void Initialize()
    {
        // TODO - Instead of Mapping all of these on Initialize and holding them in memory
        //      we could just Ask BendFactory for the Bend in Announce() and check if returned null
        //      To remedy saving, we could store the Bend Parameters in a File
        // Iterate through BendFactoryDelegates and Get/Map Bend Instances
        var names = BendFactory.GetBendNames();
        for(int i = 0; i < names.Count; ++i) {
            var bend = BendFactory.New( names[i] );
            if(bend != null) {
                m_Bends.Add( names[ i ], bend );
            }
        }
    }
    
    /// <summary>
    /// Returns list of all Bend Instances currently held by BendManager
    /// </summary>
    public static List<Bend> GetBends()
    {
        return new List<Bend>( m_Bends.Values );
    }

    public void Announce( ILinkable linkable )
    {
        Bend bend;
        if( m_Bends.TryGetValue(linkable.modelName, out bend )) {
            // Link to Active Conduit
            ConduitManager.LinkActiveConduit( bend );
            // Establish Link to Screen
            linkable.Link( bend );
        } else {
            // We Don't Handle a Linkable by this Name
        }
    }

}
