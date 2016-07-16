using UnityEngine;
using System.Collections;
using System.Timers;
using System;

public class ValueThrottle<T>
{
    public delegate void ValueThrottleDelegate( T value );

    public event ValueThrottleDelegate onValue;


    T   value = default(T);

    //Timer timer = new Timer();
    float throttleMs;
    float throttleSec;
    bool  elapsed = true;

    public ValueThrottle( float throttleMs )
    {
        this.throttleMs = throttleMs;
        this.throttleSec = throttleMs / 1000f;
        //timer.Interval = throttleMs;
        //timer.AutoReset = false;
        //timer.Elapsed += new ElapsedEventHandler( Elapsed );
    }

    public void Set(T value)
    {
        this.value = value;

        if(elapsed) {
            elapsed = false;
            //timer.Enabled = true;
            //timer.Start();
            CoroutineManager.Start( Elapsed() );
        }
    }

    IEnumerator Elapsed()
    {
        yield return new WaitForSeconds( throttleSec ) ;

        if(onValue != null) {
            onValue( value );
            elapsed = true;
        }
    }

    //private void Elapsed( object sender, ElapsedEventArgs args )
    //{
    //    onValue( value );
    //    elapsed = true;
    //}

}
