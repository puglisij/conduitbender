using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Stack collection which automatically removes oldest item when reaching its size limit
/// </summary>
/// <typeparam name="T"></typeparam>
public class FixedStack<T> : IEnumerable<T>, IEnumerator<T>
{
    protected T[]   items;
    protected int   count;
    protected int   size;
    protected int   startIndex;     // Start of circular array
    protected int   index;          // Index of top of stack

    protected int   enumIndex;

    public int Count { get { return count; } }

    public FixedStack( int size )
    {
        this.size = size;

        Clear();

        enumIndex = -1;
    }

    /// <summary>
    /// Clears the stack, Reallocating a new internal Array
    /// </summary>
    public void Clear()
    {
        // items.Initialize();  // Faster?
        items = new T[ size ];
        startIndex = 0;
        index = -1;
        count = 0;
    }

    public T Current
    {
        get
        {
            return items[ enumIndex ];
        }
    }

    ///// <summary>
    ///// Gets/sets the value at the specified index.
    ///// </summary>
    //public T this[ int index ]
    //{
    //    get
    //    {
    //        RangeCheck( index );
    //        return items[ index ];
    //    }
    //    set
    //    {
    //        RangeCheck( index );
    //        items[ index ] = value;
    //    }
    //}

    /// <summary>
    /// Returns element stored at given index, Where index 0 indicates bottom of stack.
    /// Throws Exception if invalid index.
    /// </summary>
    public T At( int index )
    {
        // Convert index
        int internalIndex = startIndex + index ;
        if (internalIndex >= size) {
            internalIndex = internalIndex - size;
        }
        // Return item
        return items[ internalIndex ];
    }

    object IEnumerator.Current
    {
        get
        {
            return items[ enumIndex ];
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return this;
    }

    public bool MoveNext()
    {
        enumIndex += 1;
        if (enumIndex >= size) {
            enumIndex = size - 1;
            return false;
        }
        return true;
    }

    public T Peek()
    {
        return items[ index ];
    }
    public void Push( T item )
    {
        index = (index + 1) % size;
        count += 1;
        if (count > size) {
            count = size;
            startIndex = (startIndex + 1) % size;
        }
        items[ index ] = item;
    }
    public T Pop()
    {
        if (count == 0) { throw new InvalidOperationException( "FixedStack: Pop() No Items on Stack." ); }

        T item = items[ index ];
        items[ index ] = default( T );
        if (index == 0) {
            index = size - 1;
        } else {
            index -= 1;
        }
        count -= 1;
        return item;
    }

    public void Reset()
    {
        enumIndex = -1;
    }


    /// <summary>
    /// Internal indexer range check helper. Throws
    /// ArgumentOutOfRange exception if the index is not valid.
    /// </summary>
    protected void RangeCheck( int index )
    {
        if (index < 0) {
            throw new ArgumentOutOfRangeException(
               "Indexer cannot be less than 0." );
        }

        if (index >= size) {
            throw new ArgumentOutOfRangeException(
               "Indexer cannot be greater than or equal to the number of items in the collection." );
        }
    }

}
