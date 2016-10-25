using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestGravity : MonoBehaviour
{

    public GameObject ballPrefab;
    public int ballsPerSec = 2;
    public float ballLifespanSec = 4f;
    public bool usePhysicsGravity = false;

    float secPerBall;
    Queue<GameObject> activeBalls = new Queue<GameObject>();

    // Use this for initialization
    void Start()
    {
        secPerBall = 1f / ballsPerSec;

        Input.gyro.updateInterval = 0.2f;  // Max is 0.0167f ?
        Input.gyro.enabled = true;

        StartCoroutine( NewBall() );
        StartCoroutine( KillBall() );
    }

    void FixedUpdate()
    {

        Physics.gravity = new Vector3( Input.acceleration.z, Input.acceleration.y, Input.acceleration.x );
    }

    IEnumerator KillBall()
    {
        yield return new WaitForSeconds( ballLifespanSec );

        while (Time.realtimeSinceStartup < 60f * 10f) 
        {
            if(activeBalls.Count > 0) {
                DestroyObject( activeBalls.Dequeue() );
            }

            yield return new WaitForSeconds( secPerBall );
        }
    }

    IEnumerator NewBall()
    {
        while (Time.realtimeSinceStartup < 60f * 10f) {
            

            var ball = Instantiate( ballPrefab );
            var rb = ball.GetComponent<Rigidbody>();
            ball.transform.SetParent( transform, false );
            ball.transform.localPosition = Vector3.zero;
            
            if(usePhysicsGravity) {
                rb.useGravity = true;
            } else {
                var gravity = Input.gyro.gravity;
                DebugToScreen.Log( "Gravity: " + gravity );

                rb.useGravity = false;
                rb.velocity = gravity;
            }
            
            

            activeBalls.Enqueue( ball );

            yield return new WaitForSeconds( secPerBall );
        }

    }

}
