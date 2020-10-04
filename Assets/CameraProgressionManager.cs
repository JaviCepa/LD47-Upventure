using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CameraProgressionManager : MonoBehaviour
{

    public float startHeight = -0.5f;
    public float endHeight = 300;

    public float startRotation = -20;
    public float endRotation = 20;

    public float startFieldOfView = 120;
    public float endFieldOfView = 80;

    public Camera cam;

    void Start()
    {
        
    }

    void Update()
    {
        var normalizedHeight = Mathf.Clamp01((transform.position.y - startHeight) / (endHeight - startHeight));
        transform.localEulerAngles = new Vector3(Mathf.Lerp(startRotation, endRotation, normalizedHeight), transform.localEulerAngles.y, transform.localEulerAngles.z);
        cam.fieldOfView = Mathf.Lerp(startFieldOfView, endFieldOfView, normalizedHeight);
    }
}
