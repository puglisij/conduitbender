///*
// *
// *  Modified version of code written by Chris Burton (Adventure Creator)
// * 
// */
//#if UNITY_EDITOR

//using UnityEngine;
//using UnityEditor;
//using System.IO;

//namespace CB
//{

//    /// <summary>
//    /// A class that assists with asset file creation.
//    /// </summary>
//    public static class AssetUtility
//    {
//        //[MenuItem( "Assets/Build AssetBundles" )]
//        //static void BuildAssets()
//        //{
//        //    BuildPipeline.BuildAssetBundles( "AssetBundles/", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64 );
//        //}

//        [MenuItem( "Assets/Print AssetBundle Names" )]
//        static void PrintAssetBundleNames()
//        {
//            var names = AssetDatabase.GetAllAssetBundleNames();
//            foreach (var name in names)
//                Debug.Log( "AssetBundle: " + name );
//        }

//        //public class MyPostprocessor : AssetPostprocessor
//        //{

//        //    void OnPostprocessAssetbundleNameChanged( string path,
//        //            string previous, string next )
//        //    {
//        //        Debug.Log( "AB: " + path + " old: " + previous + " new: " + next );
//        //    }
//        //}


//        private static string GetUniqueAssetPathNameOrFallback( string filename )
//        {
//            string path;
//            //try {
//            //    // Private implementation of a filenaming function which puts the file at the selected path.
//            //    System.Type assetdatabase = typeof (AssetDatabase);
//            //    path = (string)assetdatabase.GetMethod( "GetUniquePathNameAtSelectedPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static ).Invoke( assetdatabase, new object[] { filename } );
//            //} catch {
//                // Protection against implementation changes.
//                path = AssetDatabase.GenerateUniqueAssetPath( "Assets/" + filename );
//            //}
//            return path;
//        }


//        /**
//		 * <summary>Creates a ScriptableObject asset file.</summary>
//		 * <param name = "filename">The filename of the new asset</param>
//		 * <param name = "path">Where to save the new asset</param>
//		 * <returns>The created asset</returns>
//		 */
//        public static T CreateAsset<T>( string filename, string path = "" ) where T : ScriptableObject
//        {
//            T asset = ScriptableObject.CreateInstance<T> ();

//            string assetPathAndName = "";
//            if (path == "") {
//                assetPathAndName = GetUniqueAssetPathNameOrFallback( filename + ".asset" );
//            } else {
//                assetPathAndName = AssetDatabase.GenerateUniqueAssetPath( "Assets" + Path.DirectorySeparatorChar.ToString() + path + Path.DirectorySeparatorChar.ToString() + filename + ".asset" );
//            }
//            AssetDatabase.CreateAsset( asset, assetPathAndName );

//            AssetDatabase.SaveAssets();
//            EditorUtility.FocusProjectWindow();

//            return asset;
//        }


//        /**
//		 * <summary>Creates a ScriptableObject asset file.</summary>
//		 * <param name = "path">Where to save the new asset</param>
//		 * <returns>The created asset</returns>
//		 */
//        public static T CreateAndReturnAsset<T>( string path ) where T : ScriptableObject
//        {
//            T asset = ScriptableObject.CreateInstance<T> ();
//            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath ("Assets" + Path.DirectorySeparatorChar.ToString () + path + Path.DirectorySeparatorChar.ToString () + typeof(T).ToString() + ".asset");
//            AssetDatabase.CreateAsset( asset, assetPathAndName );

//            AssetDatabase.SaveAssets();
//            EditorUtility.FocusProjectWindow();

//            return asset;
//        }

//    }
//}

//#endif
