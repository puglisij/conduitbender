using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


[Serializable]
public class BendManagerSaveData
{
    // One to One Mapping bendNames -> bendData
    public string[] bendNames;
    public BendSaveData[] bendData;
}

public class BendManager: ILinker, ISaveable
{
    private const string k_defaultsFileName = "BendDefaults";
    private const string k_saveFileName = "savedbends.dat";

    // Bend Name to Bend Model Instances
    private static Dictionary<string, Bend> m_Bends = new Dictionary<string, Bend>();

    private static void Load()
    {
        // Load Serialized Save Data from file
        BendManagerSaveData saveData;
        BendSaveData bendSaveData;
        AppData.LoadPersistent<BendManagerSaveData>( k_saveFileName, out saveData );

        // If no saved data file, load defaults from assets
        if(saveData == null) {
            //Debug.Log( "BendManager: Load() Attempting to load default Bends..." );
            AppData.LoadResource( k_defaultsFileName, out saveData );
        }

        if (saveData == null) {
            //Debug.Log( "BendManager: Load() Failed to load defaults." );
            return;
        }

        // Build dictionary from saved Bends
        var savedBends = new Dictionary<string, BendSaveData>();
        for (var i = 0; i < saveData.bendNames.Length; ++i) {
            savedBends.Add( saveData.bendNames[ i ], saveData.bendData[ i ] );
        }

        Bend    bend;
        string  name;
        foreach(var entry in m_Bends) 
        {
            name = entry.Key;
            bend = entry.Value;

            // Load saved data into Bend here
            if (savedBends.TryGetValue( name, out bendSaveData )) 
            {
                if (bendSaveData.inputValues != null) 
                {
                    // NOTE: Assume BendParameters are in same order as when saved
                    try {
                        for (var j = 0; j < bendSaveData.inputValues.Length; ++j) {
                            if(bend.inputParameters[ j ].type != (EBendParameterType) bendSaveData.inputTypes[ j ]) {
                                throw new Exception( "BendManager: Mismatching types with saved bend inputs." );
                            }
                            bend.inputParameters[ j ].value = bendSaveData.inputValues[ j ];
                        }
                    } catch(Exception e) {
                        //Debug\.LogError( "BendManager: Exception occurred loading saved bends." );
                    }
                }
            }
        }    
    }

    public static void Initialize()
    {
        // TODO: Instead of Mapping all of these on Initialize and holding them in memory
        //      we could just Ask BendFactory for the Bend in Announce() and check if returned null
        //      To remedy saving, we could store the Bend Parameters in a File

        // Iterate through BendFactoryDelegates and Get/Map Bend Instances
        var names = BendFactory.GetBendNames();
        for(int i = 0; i < names.Count; ++i) 
        {
            var bend = BendFactory.New( names[i] );
            if(bend != null) {
                m_Bends.Add( names[ i ], bend );
            }
        }

        // Load saved bends
        Load();
    }

    public void Save()
    {
        var data = new BendManagerSaveData();
        var bendNames = new List<string>(m_Bends.Count);
        var bendDatas = new List<BendSaveData>(m_Bends.Count);

        Bend bend;
        BendSaveData bendSaveData;
        // Loop through all Bends and map data to Serializable Object
        foreach (var entry in m_Bends) 
        {
            // Gather data from Bend
            bend = entry.Value;
            bendSaveData = new BendSaveData();
            bendSaveData.modelName = bend.modelName;
            bendSaveData.inputTypes = new object[ bend.inputParameters.Count ];
            bendSaveData.inputValues = new object[ bend.inputParameters.Count ];
  
            for (var i = 0; i < bend.inputParameters.Count; ++i) {
                bendSaveData.inputTypes[ i ] = bend.inputParameters[ i ].type;
                bendSaveData.inputValues[ i ] = bend.inputParameters[ i ].value;
            }

            bendNames.Add( entry.Key );
            bendDatas.Add( bendSaveData );
        }
        data.bendNames = bendNames.ToArray();
        data.bendData = bendDatas.ToArray();

        // Write Serialiable Object to storage
        AppData.SavePersistent( k_saveFileName, data );
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
            //Debug\.Log( "BendManager: Announce() No Bend found for model name: " + linkable.modelName );
        }
    }

}
