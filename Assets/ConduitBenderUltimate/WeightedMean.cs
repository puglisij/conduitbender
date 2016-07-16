using UnityEngine;
using System.Collections;
using System;

namespace CB
{
    /// <summary>
    /// Uses FixedStack<T> internally
    /// Weights are determined by the number (n) of values (vi) stored in this Weighted Mean:
    /// v0, v1, ... v(n-1) where the weight (w) is w = n - i.  
    /// For example, storing 3 values would give i = 0 through 2, giving n = 3, and weights = (3 - 0), (3 - 1), (3 - 2) = 3, 2, 1
    /// T must have following operators defined:
    /// T += T
    /// T * float
    /// </summary>
    public class WeightedMean<T>
    {
        public delegate T MultiplyDelegate( T next, float weight );
        public delegate T AddDelegate( T sum, T next );

        private FixedStack<T> m_values;

        private MultiplyDelegate multiplyFn;
        private AddDelegate addFn;

        private T m_mean;

        private bool m_isMeanDirty = false;

        //Type[] definedTypes = {
        //    typeof(Vector2),
        //    typeof(Vector3),
        //    typeof(Vector4) ...
        //};

        /// <summary>
        /// Construct Weighted Mean class.
        /// </summary>
        /// <param name="valueCount">Max number of values to find the average of. Oldest added values are automatically removed.</param>
        /// <param name="md">Delegate which multiplies value type T by a float and returns the result as type T</param>
        /// <param name="ad">Delegate which adds two values of type T and returns the result as type T</param>
        public WeightedMean(int valueCount, MultiplyDelegate md, AddDelegate ad)
        {
            // We could instead check if typeof(T) is in list of definedTypes. Which is faster?
            //if(T is Vector2 || T is Vector3 || T is Vector4 || T is int || T is float || T is double || T is long) {
            //}

            multiplyFn = md;
            addFn = ad;
            m_values = new FixedStack<T>(valueCount);
        }

        /// <summary>
        /// Add a new value to internal sequence. The new value / most recent is given the highest weight.
        /// </summary>
        public WeightedMean<T> Add(T newValue)
        {
            m_isMeanDirty = true;
            m_values.Push( newValue );

            return this;
        }
        public void Clear()
        {
            m_mean = default( T );
            m_isMeanDirty = false;
            m_values.Clear();
        }
        public T Mean()
        {
            if(m_isMeanDirty) 
            {
                m_isMeanDirty = false;

                try {
                    T       mean = default(T);
                    int     n = m_values.Count;
                    float   w;
                    float   totalW = (n * (n + 1)) / 2f;  // Gauss Formula
                    // Calculate new mean
                    for (int i = 0; i < n; ++i) {
                        // Get Normalized weight (0 to 1)
                        w = (n - i) / totalW;
                        mean = addFn(mean, multiplyFn(m_values.At( n - i - 1 ), w ));
                    }
                    m_mean = mean;
                } catch(Exception e) {
                    // Catch any errors with (dynamic)
                    Debug.LogError( "WeightedMean: Mean() Exception: " + e.ToString() );
                }
            }
            return m_mean;
        }
    }
}
