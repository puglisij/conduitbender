using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ShowRectTransformInfo : MonoBehaviour
{
    public Vector2 anchoredPosition;
    public Vector3 localPosition;
    public Vector2 offsetMin;
    public Vector2 offsetMax;
    public Rect rect;

    RectTransform thisTrans;

    void Awake()
    {
        thisTrans = (RectTransform)transform;

    }

	void Update ()
    {
        anchoredPosition = thisTrans.anchoredPosition;
        localPosition = thisTrans.localPosition;
        offsetMin = thisTrans.offsetMin;
        offsetMax = thisTrans.offsetMax;
        rect = thisTrans.rect;
	}
}
