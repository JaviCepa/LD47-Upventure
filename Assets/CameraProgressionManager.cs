using Com.LuisPedroFonseca.ProCamera2D;
using PlatformerPro;
using Sirenix.OdinInspector;
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

    public float startOffsetY = -0.5f;
    public float endOffsetY = 0.5f;

    public float startOffsetX = 0.25f;
    public float endOffsetX = 0.0f;

    public Camera cam;
    public ProCamera2D procamera;

    float timeSinceStart => UpventureGameManager.instance.timeSinceStart;

    void Start()
    {
        if (Application.isEditor)
        {
            var respawnPosition = FindObjectOfType<RespawnPoint>();
            if (procamera != null && respawnPosition != null)
            {
                procamera.MoveCameraInstantlyToPosition(new Vector2(respawnPosition.transform.position.x, respawnPosition.transform.position.y));
            }
        }
    }

    void Update()
    {
        var normalizedHeight = Mathf.Clamp01((transform.position.y - startHeight) / (endHeight - startHeight));
        transform.localEulerAngles = new Vector3(Mathf.Lerp(startRotation, endRotation, normalizedHeight), transform.localEulerAngles.y, transform.localEulerAngles.z);
        cam.fieldOfView = Mathf.Lerp(startFieldOfView, endFieldOfView, normalizedHeight);
        procamera.OffsetY = Mathf.Lerp(startOffsetY, endOffsetY, normalizedHeight);
        if (Application.isPlaying)
        {
            procamera.OffsetX = Mathf.Lerp(startOffsetX, endOffsetX,  Mathf.Clamp01(timeSinceStart/10f));
        }
        else
        {
            procamera.OffsetX = startOffsetX;
        }
    }

}
