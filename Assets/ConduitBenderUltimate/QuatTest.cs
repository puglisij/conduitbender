using UnityEngine;
using System.Collections;

public class QuatTest : MonoBehaviour {

    public enum CrossProductType { Normal, ReverseForward, ReverseUp, ReverseBoth }

    public CrossProductType crossProductType;
    public Vector3 crossProductDir;
    public Vector3 rotateRayDir;
    public float degPerSec = 90.0f;

    Vector3 currUp;
    Vector3 currForward;
    Vector3 crossProduct;

    Color cpRayColor;
    Color rotRayColor;

    GameObject crossProductRayObj;
    GameObject rotateRayObj;

    Ray cpRay;
    Ray rotRay;

	// Use this for initialization
	void Start () {

        rotRayColor = Color.yellow;
        cpRay = new Ray();
        rotRay = new Ray();

        SetCrossProduct();

        crossProductRayObj = FlagRenderer.NewRay( transform, cpRayColor );
        rotateRayObj = FlagRenderer.NewRay( transform, rotRayColor );
        rotateRayDir = transform.up;
    }

    void SetCrossProduct()
    {
        currForward = transform.forward;
        currUp = transform.up;
        switch (crossProductType) {
            case CrossProductType.Normal:
                cpRayColor = Color.green;
                crossProduct = Vector3.Cross( currUp, currForward );
                break;
            case CrossProductType.ReverseBoth:
                cpRayColor = Color.black;
                crossProduct = Vector3.Cross( -currUp, -currForward );
                break;
            case CrossProductType.ReverseForward:
                cpRayColor = Color.cyan;
                crossProduct = Vector3.Cross( currUp, -currForward );
                break;
            case CrossProductType.ReverseUp:
                cpRayColor = Color.magenta;
                crossProduct = Vector3.Cross( -currUp, currForward );
                break;
        }
        cpRay.origin = transform.position;
        cpRay.direction = crossProduct;
    }
    // Update is called once per frame
    void Update()
    {
        rotateRayDir = Quaternion.AngleAxis( degPerSec * Time.deltaTime, crossProduct ) * rotateRayDir;
        rotRay.origin = transform.position;
        rotRay.direction = rotateRayDir;

        SetCrossProduct();
        
        FlagRenderer.DrawRay( crossProductRayObj, cpRay, cpRayColor );
        FlagRenderer.DrawRay( rotateRayObj, rotRay );
    }
        
}
