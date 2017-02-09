using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ShowTransformInfo : MonoBehaviour
{
    public Vector3 worldPosition;
    public Vector3 localPosition;
    public Vector3 localScale;

    Transform thisTrans;

    void Awake()
    {
        thisTrans = (Transform)transform;

    }

    void Update()
    {
        worldPosition = thisTrans.position;
        localPosition = thisTrans.localPosition;
        localScale = thisTrans.localScale;
    }
}
