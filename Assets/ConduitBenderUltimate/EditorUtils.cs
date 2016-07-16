using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

#if UNITY_EDITOR 
namespace CB
{
    public static class FlagsHelper
    {
        public static bool IsSet<T>( T flags, T flag ) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            return (flagsValue & flagValue) != 0;
        }

        public static void Set<T>( ref T flags, T flag ) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue | flagValue);
        }

        public static void Unset<T>( ref T flags, T flag ) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue & (~flagValue));
        }
    }

    [InitializeOnLoad]
    public class EditorUtils
    {
        public struct LayerMaskData
        {
            public string[] layerNames;
            public int[] layerNumbers;
        }
        /// <summary> Returns true if item is displayed. False if hidden. </summary>
        public delegate bool ShowItem<in T>( T item, int index );
        /// <summary> Returns true if item is displayed. False if hidden. </summary>
        public delegate bool ShowItemSerialized( SerializedProperty item, int index );

        [Flags]
        public enum ShowListOptions { None = 0, Reorderable = 1, Resizeable = 2, ShowListSize = 4, UseCogMenu = 8 }

        //----------------
        // Texture2d
        //----------------
        public static Texture2D blackQuarterAlphaTexture2D
        {
            get
            {
                if (m_blackQuarterAlpha2D == null) {
                    m_blackQuarterAlpha2D = new Texture2D( 1, 1 );
                    m_blackQuarterAlpha2D.SetPixel( 0, 0, new Color( 0f, 0f, 0f, 0.25f ) );
                    m_blackQuarterAlpha2D.Apply();
                }
                return m_blackQuarterAlpha2D;
            }
        }
        public static Texture2D blackTexture2D
        {
            get {
                if (m_blackTexture2D == null) {
                    m_blackTexture2D = new Texture2D( 1, 1 );
                    m_blackTexture2D.SetPixel( 0, 0, new Color( 0f, 0f, 0f, 1f ) );
                    m_blackTexture2D.Apply();
                }
                return m_blackTexture2D;
            }
        }
        public static Texture2D whiteTexture2D
        {
            get
            {
                if (m_whiteTexture2D == null) {
                    m_whiteTexture2D = new Texture2D( 1, 1 );
                    m_whiteTexture2D.SetPixel( 0, 0, new Color( 1f, 1f, 1f, 1f ) );
                    m_whiteTexture2D.Apply();
                }
                return m_whiteTexture2D;
            }
        }
        public static Texture2D inspectorCog2D
        {
            get { return m_inspectorCog2D; }
        }

        //----------------
        // Private
        //----------------
        private enum ListItemAction { Delete, InsertAfter, MoveDown, MoveUp, None }

        private struct ListItemActionData {
            ListItemAction action;
            int index;
            public ListItemActionData(ListItemAction action, int index) { this.action = action;  this.index = index; }
        }

        private static Texture2D m_whiteTexture2D = null;
        private static Texture2D m_blackTexture2D = null;
        private static Texture2D m_blackQuarterAlpha2D = null;
        private static Texture2D m_inspectorCog2D = null;

        //----------------
        // Styles
        //----------------
        public static GUIStyle boldLabelStyle { get { return m_boldLabelStyle; } }
        public static GUIStyle boxStyle { get { return m_boxStyle; } }

        private static GUIStyle m_boldLabelStyle;
        private static GUIStyle m_boxStyle;
        private static GUIStyle m_foldoutStyle;
        private static GUIStyle m_foldoutToggleStyle;

        private static bool m_initialized = false;

        /*=====================================================

                        Initialize all Statics

        =====================================================*/
        // A static constructor is used to initialize any static data, or to perform a particular action that needs to be performed once only. 
        // It is called automatically before the first instance is created or any static members are referenced.
        static EditorUtils()
        {
            // Initialize on Load
            Init();
        }

        public static void Init()
        {
            if (m_initialized) {
                return;
            }
            m_initialized = true;

            //---------------------
            // GUI Styles
            //---------------------
            m_foldoutStyle = new GUIStyle {
                normal =
                {
                        background = whiteTexture2D,
                        textColor = Color.black
                },
                border = new RectOffset( 0, 0, 0, 0 ),
                padding = new RectOffset( 10, 0, 0, 0 ),
                margin = new RectOffset( 0, 0, 0, 0 ),
                contentOffset = new Vector2( 0, 0 ),
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 20,
                fontStyle = FontStyle.Bold,
            };

            m_foldoutToggleStyle = new GUIStyle();
            m_foldoutToggleStyle.fixedHeight = 20;
            m_foldoutToggleStyle.fixedWidth = 20;
            m_foldoutToggleStyle.fontSize = 0;
            m_foldoutToggleStyle.border = new RectOffset( 0, 10, 0, 0 );
            m_foldoutToggleStyle.margin = new RectOffset( 0, 10, 0, 0 );
            m_foldoutToggleStyle.padding = new RectOffset( 0, 10, 0, 0 );
            m_foldoutToggleStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>( "Assets/ConduitBenderUltimate/EditorResources/foldoutDown20px.png" );
            m_foldoutToggleStyle.onNormal.background = AssetDatabase.LoadAssetAtPath<Texture2D>( "Assets/ConduitBenderUltimate/EditorResources/foldoutUp20px.png" );

            // NOTE: Cannot call GUI in static constructor. Must be called from within context of an OnGUI related function.
            // GUI.skin.box 
            m_boldLabelStyle = new GUIStyle();
            m_boldLabelStyle.fontStyle = FontStyle.Bold;

            m_boxStyle = new GUIStyle();
            m_boxStyle.alignment = TextAnchor.MiddleCenter;
            m_boxStyle.clipping = TextClipping.Clip;
            m_boxStyle.contentOffset = Vector2.zero;
            m_boxStyle.imagePosition = ImagePosition.ImageLeft;
            m_boxStyle.margin = new RectOffset( 0, 0, 1, 0 );
            m_boxStyle.normal = new GUIStyleState() {
                background = whiteTexture2D,
                textColor = Color.black
            };
            m_boxStyle.padding = new RectOffset( 0, 0, 0, 0 );
            m_boxStyle.richText = false;
            m_boxStyle.stretchHeight = false;
            m_boxStyle.stretchWidth = false;
            m_boxStyle.wordWrap = true;

            //---------------------
            // Textures
            //---------------------
            m_inspectorCog2D = AssetDatabase.LoadAssetAtPath<Texture2D>( "Assets/ConduitBenderUltimate/EditorResources/inspector_cog.png" );
        }


        /*=====================================================

                        Private Functions

        =====================================================*/
        //void ListItemCogCallback(object obj)
        //{
        //    var itemSelected = (ListItemActionData) obj;
        //    // Perform Action on List
        // 
        //}

        //void ShowListItemCog(int listSize, int itemIndex)
        //{
        //    GenericMenu menu = new GenericMenu ();

        //    menu.AddItem( new GUIContent( "Insert after" ), false, ListItemCogCallback, new ListItemActionData(ListItemAction.InsertAfter, itemIndex) );
        //    if (listSize > 0) 
        //        menu.AddItem( new GUIContent( "Delete" ), false, ListItemCogCallback, new ListItemActionData( ListItemAction.Delete, itemIndex ) );
        //    if (itemIndex > 0 || itemIndex < listSize - 1) 
        //        menu.AddSeparator( "" );
        //    if (itemIndex > 0) 
        //        menu.AddItem( new GUIContent( "Move up" ), false, ListItemCogCallback, new ListItemActionData( ListItemAction.MoveUp, itemIndex ) );
        //    if (itemIndex < listSize - 1) 
        //        menu.AddItem( new GUIContent( "Move down" ), false, ListItemCogCallback, new ListItemActionData( ListItemAction.MoveDown, itemIndex ) );

        //    menu.ShowAsContext();
        //}

        /*=====================================================

                        Static Functions

        =====================================================*/

        /// <summary>
        /// Draws a Sprite within the given Rect draw area. Default is to preserve aspect ratio of sprite. 
        /// Handles both packed sprites and non-packed.
        /// </summary>
        public static void DrawSprite( Sprite sprite, Rect drawArea, bool center = true, bool preserveAspect = true )
        {
            // TODO - Allow centering
            if (sprite) {
                var st  = sprite.texture;
                var str = sprite.textureRect;

                Rect str_normalized = new Rect( str.x / st.width, str.y / st.height, str.width / st.width, str.height / st.height);

                float width = 0, height = 0,
                      x = drawArea.x, y = drawArea.y;
                if (preserveAspect) {
                    var aspect = str.width / str.height;
                    var drawAreaAspect = drawArea.width / drawArea.height;

                    // Maximize draw size
                    if (drawAreaAspect > aspect) {
                        // Limited by height
                        height = drawArea.height;
                        width = aspect * height;
                        x = Mathf.Abs( drawArea.width - width ) * 0.5f;
                    } else {
                        // Limited by width
                        width = drawArea.width;
                        height = width / aspect;
                        y = Mathf.Abs( drawArea.height - height ) * 0.5f;
                    }
                    // Debug.Log( "EditorUtils: DrawSprite() Sprite aspect: " + aspect + " width: " + width + " height: " + height + " str_normalized: " + str_normalized);
                }
                GUI.DrawTextureWithTexCoords( new Rect( x, y, width, height ), st, str_normalized );
            }
        }

        public static string GetTooltip( Type type, string fieldName, bool inherit )
        {
            var field = type.GetField(fieldName);
            TooltipAttribute[] attributes = field.GetCustomAttributes(typeof(TooltipAttribute), inherit) as TooltipAttribute[];

            return attributes.Length > 0 ? attributes[ 0 ].tooltip : null;
        }

        public static LayerMaskData LayerMaskToNamesAndNumbers( LayerMask layerMask )
        {
            var layerNames = new List<string>();
            var layerNums = new List<int>();
            var layerName = string.Empty;

            for (int i = 0; i < 32; ++i) {
                layerName = LayerMask.LayerToName( i );
                if (!string.IsNullOrEmpty( layerName )) {
                    layerNames.Add( layerName );
                    layerNums.Add( i );
                }
            }
            return new LayerMaskData {
                layerNames = layerNames.ToArray(),
                layerNumbers = layerNums.ToArray()
            };
        }

        public static LayerMask LayerMaskField( string label, LayerMask layerMask )
        {
            //var layers = InternalEditorUtility.layers;
            var layerData = LayerMaskToNamesAndNumbers( layerMask );
            var layerNames = layerData.layerNames;
            var layerNums = layerData.layerNumbers;

            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNums.Length; i++) {
                // If this layer is On
                if (((1 << layerNums[ i ]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }

            maskWithoutEmpty = EditorGUILayout.MaskField( label, maskWithoutEmpty, layerNames );

            int mask = 0;
            for (int i = 0; i < layerNums.Length; i++) {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNums[ i ]);
            }
            layerMask.value = mask;

            return layerMask;
        }

        /// <summary>
        /// Recommend calling SerializedObject.Update() before.
        /// Returns true if any changes were made to the list. Automatically handles registering Undo, and saving changes to Serialized object.
        /// Recommend calling SerializedObject.ApplyModifiedProperties() afterwards.
        /// 
        /// Alternative:  Use the ReorderableList class in UnityEditorInternal namespace. See this excellent post: http://va.lent.in/unity-make-your-lists-functional-with-reorderablelist/
        /// </summary>
        public static bool ShowList<T>( SerializedProperty list, ShowItemSerialized showItemDelegate, 
            float viewWidth,
            ShowListOptions options = ShowListOptions.Reorderable | ShowListOptions.Resizeable | ShowListOptions.Resizeable | ShowListOptions.ShowListSize,
            int minListSize = 0, int maxListSize = int.MaxValue )
        {
            //if (list.isArray) {
            //    throw new ArgumentException( "SaveSystem: ShowList() SerializedProperty must be a List." );
            //}

            // To avoid clipping by vertical scrollbar
            viewWidth -= 20f;

            int indexToDelete = -1;
            int indexToMoveUp = -1;
            int indexToMoveDown = -1;
            int listCount = list.arraySize;
            var oddStyle = new GUIStyle();
            oddStyle.normal.background = blackQuarterAlphaTexture2D;

            EditorGUI.indentLevel++;
            var listStyle = new GUIStyle();
            listStyle.margin = new RectOffset( 0, 0, 10, 0 );
            listStyle.imagePosition = ImagePosition.ImageLeft;
            EditorGUILayout.BeginVertical( listStyle );

            if ((options & ShowListOptions.ShowListSize) == ShowListOptions.ShowListSize) {
                EditorGUILayout.LabelField( new GUIContent( "Size: " + listCount ), EditorUtils.boldLabelStyle );
                EditorGUILayout.Separator();
            }

            GUI.changed = false;
            for (int i = 0; i < listCount; ++i) {

                var item = list.GetArrayElementAtIndex(i);
                //------------
                // Show Item
                //------------
                if (i % 2 == 0) {
                    EditorGUILayout.BeginHorizontal();
                } else {
                    EditorGUILayout.BeginHorizontal( oddStyle );
                }

                EditorGUILayout.BeginVertical( GUILayout.Width( viewWidth * 0.94f ) );

                var visible = showItemDelegate( item, i );

                EditorGUILayout.EndVertical();
                //-----------------------
                // List Manipulation
                EditorGUILayout.BeginVertical( GUILayout.Width( viewWidth * 0.05f ) );

                if(visible) 
                {
                    if ((options & ShowListOptions.Resizeable) == ShowListOptions.Resizeable) {
                        GUI.enabled = (listCount > minListSize);
                        if (GUILayout.Button( "X", "toolbarbutton", GUILayout.ExpandWidth( false ) )) {
                            indexToDelete = i;
                        }
                    }
                    if ((options & ShowListOptions.Reorderable) == ShowListOptions.Reorderable) {
                        GUI.enabled = (i < listCount - 1);
                        if (GUILayout.Button( "\u25BC", EditorStyles.toolbarButton, GUILayout.Width( 18 ) )) {
                            indexToMoveDown = i;
                        }
                        GUI.enabled = i > 0;
                        if (GUILayout.Button( "\u25B2", EditorStyles.toolbarButton, GUILayout.Width( 18 ) )) {
                            indexToMoveUp = i;
                        }
                    }
                    
                }
                GUI.enabled = true;

                EditorGUILayout.EndVertical();
                // End List Manipulation
                //-----------------------
                EditorGUILayout.EndHorizontal();
            }

            if (listCount < maxListSize && (options & ShowListOptions.Resizeable) == ShowListOptions.Resizeable) {
                if (GUILayout.Button( "Add", EditorStyles.toolbarButton, GUILayout.Width( 50 ) )) {
                    list.InsertArrayElementAtIndex( listCount );
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;

            if (indexToDelete >= 0 && listCount > minListSize) {
                list.DeleteArrayElementAtIndex( indexToDelete );
            }
            if (indexToMoveUp >= 0) {
                list.MoveArrayElement( indexToMoveUp, indexToMoveUp - 1 );
            }
            if (indexToMoveDown >= 0) {
                list.MoveArrayElement( indexToMoveDown, indexToMoveDown + 1 );
            }

            return GUI.changed;
        }

        /// <summary>
        /// Returns true if any changes were made to the list.
        /// </summary>
        public static bool ShowList<T>( IList<T> list, 
            ShowItem<T> showItemDelegate, 
            float viewWidth, 
            ShowListOptions options = ShowListOptions.Reorderable | ShowListOptions.Resizeable | ShowListOptions.Resizeable | ShowListOptions.ShowListSize, 
            int minListSize = 0, int maxListSize = int.MaxValue) where T : new()
        {
            // To avoid clipping by vertical scrollbar
            viewWidth -= 20f;

            int indexToDelete = -1;
            int indexToMoveUp = -1;
            int indexToMoveDown = -1;
            int listCount = list.Count;
            var oddStyle = new GUIStyle();
                oddStyle.normal.background = blackQuarterAlphaTexture2D;

            EditorGUI.indentLevel++;
            var listStyle = new GUIStyle();
                listStyle.margin = new RectOffset( 0, 0, 10, 0 );
                listStyle.imagePosition = ImagePosition.ImageLeft;
            EditorGUILayout.BeginVertical( listStyle );

            if((options & ShowListOptions.ShowListSize) == ShowListOptions.ShowListSize) {
                EditorGUILayout.LabelField( new GUIContent( "Size: " + listCount ), EditorUtils.boldLabelStyle );
                EditorGUILayout.Separator();
            }
            
            GUI.changed = false;
            for (int i = 0; i < listCount; ++i) {

                var item = list[i];
                //------------
                // Show Item
                //------------
                if(i%2 == 0) {
                    EditorGUILayout.BeginHorizontal();
                } else {
                    EditorGUILayout.BeginHorizontal( oddStyle );
                }
                
                EditorGUILayout.BeginVertical(GUILayout.Width( viewWidth * 0.94f) );
                
                var visible = showItemDelegate( item, i );

                EditorGUILayout.EndVertical();
                //-----------------------
                // List Manipulation
                EditorGUILayout.BeginVertical( GUILayout.Width( viewWidth * 0.05f ) );

                if(visible) 
                {
                    if ((options & ShowListOptions.Resizeable) == ShowListOptions.Resizeable) {
                        GUI.enabled = (listCount > minListSize);
                        if (GUILayout.Button( "X", "toolbarbutton", GUILayout.ExpandWidth( false ) )) {
                            indexToDelete = i;
                        }
                    }
                    if ((options & ShowListOptions.Reorderable) == ShowListOptions.Reorderable) {
                        GUI.enabled = (i < listCount - 1);
                        if (GUILayout.Button( "\u25BC", EditorStyles.toolbarButton, GUILayout.Width( 18 ) )) {
                            indexToMoveDown = i;
                        }
                        GUI.enabled = i > 0;
                        if (GUILayout.Button( "\u25B2", EditorStyles.toolbarButton, GUILayout.Width( 18 ) )) {
                            indexToMoveUp = i;
                        }
                    }
                }
                
                GUI.enabled = true;

                EditorGUILayout.EndVertical();
                // End List Manipulation
                //-----------------------
                EditorGUILayout.EndHorizontal();
            }
            
            bool itemsChanged = false;

            if (listCount < maxListSize && (options & ShowListOptions.Resizeable) == ShowListOptions.Resizeable) {
                if (GUILayout.Button( "Add", EditorStyles.toolbarButton, GUILayout.Width( 50 ) )) {
                    var newItem = default(T);
                    if(newItem == null) {
                        newItem = new T();
                    }
                    list.Add( newItem );
                    itemsChanged = true;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;

            if (indexToDelete >= 0 && listCount > minListSize) {
                list.RemoveAt( indexToDelete );
                itemsChanged = true;
            }
            if (indexToMoveUp >= 0) {
                Swap( list, indexToMoveUp, indexToMoveUp - 1 );
                itemsChanged = true;
            }
            if (indexToMoveDown >= 0) {
                Swap( list, indexToMoveDown, indexToMoveDown + 1 );
                itemsChanged = true;
            }

            return itemsChanged || GUI.changed;
        } // ShowList<T>()


        public static bool Toggle( bool value, float viewWidth, string name, string tooltip = "" )
        {
            Init();
            //var content = new GUIContent(name, tooltip);
            //var toggleStyle = new GUIStyle();
            //    toggleStyle.imagePosition = ImagePosition.ImageLeft;

            //EditorGUILayout.BeginHorizontal( toggleStyle );

            // LabelField width are just tweaked values to account for Editor Scrollbar
            //var toggle = EditorGUILayout.Toggle( value, m_foldoutToggleStyle );

            //EditorGUILayout.LabelField( content, m_foldoutStyle, (value) ? GUILayout.Width( viewWidth - 40) : GUILayout.Width( viewWidth - 25) );
            //EditorGUILayout.EndHorizontal();

            var text = "<b><size=11>" + name + "</size></b>";
            if (value) {
                text = "\u25BC " + text;
            } else {
                text = "\u25BA " + text;
            }
            var toggle = GUILayout.Toggle( value, text, "dragtab" );

            return toggle;
        }


        /*=====================================================

                        Utility Functions

        =====================================================*/
        public static void Swap<T>( IList<T> list, int indexA, int indexB )
        {
            T tmp = list[indexA];
            list[ indexA ] = list[ indexB ];
            list[ indexB ] = tmp;
        }
    }

    
}
#endif
