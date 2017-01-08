using System;

namespace CB
{
    [Serializable]
    public struct KeyFloatSet
    {
        /// <summary> The title of the set of key values </summary>
        public string title;
        /// <summary> The array of key values </summary>
        public KeyFloat[] set;

        static public explicit operator KeyValueSet<object>( KeyFloatSet set )
        {
            var s = new KeyValueSet<object>();  
            var s_set = Array.ConvertAll<KeyFloat, KeyValue<object>>( set.set, ( f ) => {
                return (KeyValue<object>)f;
            });
            s.title = set.title;
            s.set = s_set;

            return s;
        }
    }

    [Serializable]
    public struct KeyFloat
    {
        public string title;
        public float value;

        static public explicit operator KeyValue<object>( KeyFloat f )
        {
            var v = new KeyValue<object>();
                v.title = f.title;
                v.value = f.value;
            return v;
        }
    }

    [Serializable]
    public struct KeyStringSet
    {
        /// <summary> The title of the set of key values </summary>
        public string title;
        /// <summary> The array of key values </summary>
        public KeyString[] set;
    }

    [Serializable]
    public struct KeyString
    {
        public string title;
        public string value;
    }


    [Serializable]
    public struct KeyValueSet<T>
    {
        /// <summary> The title of the set of key values </summary>
        public string title;
        /// <summary> The array of key object values </summary>
        public KeyValue<T>[] set;
    }

    [Serializable]
    public struct KeyValue<T>
    {
        public string title;
        public T value;
    }

}
