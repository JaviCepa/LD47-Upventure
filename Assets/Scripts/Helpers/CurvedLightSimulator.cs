using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurvedLightSimulator : MonoBehaviour
{

    public GameObject cam;
    public float towerPerimeter = 120f;

    float startRotation;

    void Start()
    {
        startRotation = transform.localEulerAngles.y;
    }

    void Update()
    {
        var deltaX = cam.transform.position.x - transform.position.x;
        var normalizedDeltaX = deltaX / towerPerimeter;

        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, startRotation + normalizedDeltaX * 360f, transform.localEulerAngles.z);
    }
}
