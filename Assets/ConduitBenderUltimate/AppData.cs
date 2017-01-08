using UnityEngine;
using LitJson;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class AppData
{
    public const string k_SettingsFilePath = "/Settings.json";

    // Path to the Game Data Folder (e.g. points directly at APK on Android, or <path to player app>/<AppName.app> on iOS)
    static string dataPath             = Application.dataPath;
    // Where data that is expected to be kept between runs can be stored. (Files wont be erased with each update of App)
    // persistentDataPath is device specific
    static string persistentDataPath   = Application.persistentDataPath;
   
    public static void Initialize()
    {
        Environment.SetEnvironmentVariable( "MONO_REFLECTION_SERIALIZER", "yes" );

    }

    /// <summary>
    /// Loads a file from Resources (e.g. MyDefaults.bytes). File must be in Resources directory. Don't use file extension.
    /// Example: LoadResource( "MyDefaults", out defaults )
    /// Returns default(T) if unsuccessful
    /// </summary>
    public static void LoadResource<T>( string fullPath, out T data )
    {
        var asset = Resources.Load( fullPath ) as TextAsset;

        try {
            BinaryFormatter bf = new BinaryFormatter();
            var stream = new MemoryStream(asset.bytes);

            data = (T)bf.Deserialize( stream );
            stream.Close();
        } catch(Exception) {
            data = default( T );
        }
    }

    /// <summary>
    /// Loads a json file from Resources (e.g. MyDefaults.json). File must be in Resources directory. Don't use file extension.
    /// Example: LoadResource( "MyJSON", out myJSONObject )
    /// Returns default(T) if unsuccessful
    /// </summary>
    public static void LoadResourceJSON<T>( string fullPath, out T data)
    {
        var asset = Resources.Load( fullPath ) as TextAsset;

        try {
            data = JsonUtility.FromJson<T>( asset.text );
        } catch(Exception) {
            data = default( T );
        }
    }

    /// <summary>
    /// Loads given filename (from persistentDataPath) into 'object' data parameter. 
    /// null if file does not exist.
    /// Returns default(T) if unsuccessful
    /// </summary>
    public static void LoadPersistent<T>( string fileName, out T data )
    {
        Load( fileName, out data, Application.persistentDataPath );
    }
    /// <summary>
    /// Loads given filename (from given path) into 'object' data parameter. 
    /// null if file does not exist.
    /// Returns default(T) if unsuccessful
    /// </summary>
    public static void Load<T>( string fileName, out T data, string path )
    {
        if (File.Exists( path + "/" + fileName )) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(path + "/" + fileName, FileMode.Open);

            // Load from File
            try {
                data = (T) bf.Deserialize( file );
            } catch(Exception) {
                data = default( T );
            }
            
            file.Close();
        } else {
            data = default( T );
        }
    }

    /// <summary>
    /// Save 'object' data parameter to Binary File at Application persistentDataPath
    /// The given data object should be [Serializable]
    /// </summary>
    public static void SavePersistent( string fileName, object data ) 
    {
        Save( fileName, data, Application.persistentDataPath );
    }
    /// <summary>
    /// Save 'object' data parameter to Binary File at given path
    /// The given data object should be [Serializable]
    /// </summary>
    public static void Save( string fileName, object data, string path ) 
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create( path + "/" + fileName);

        // Write to file
        try {
            bf.Serialize( file, data );
        } catch(Exception) {
            data = null;
        }
        
        file.Close();
    }

}
