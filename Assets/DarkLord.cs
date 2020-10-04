using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkLord : MonoBehaviour
{

    Camera cam;

    void Start()
    {
        cam = FindObjectOfType<Camera>();
    }

    void Update()
    {
        transform.position = new Vector3(cam.transform.position.x, transform.position.y, transform.position.z);
    }
}
