using Com.LuisPedroFonseca.ProCamera2D;
using PlatformerPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLevelGateway : MonoBehaviour
{

    public ChangeLevelGateway exitGateway;

    static float lastUseTime = 0;

    const float cooldown = 0.5f;

    private void OnDrawGizmos()
    {
        if (exitGateway != null)
        {
            Debug.DrawLine(transform.position, exitGateway.transform.position, Color.yellow);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var character = collision.gameObject.GetComponentInParent<Character>();
        if (character != null && Time.time - lastUseTime > cooldown)
        {
            TeleportToOther(character);
        }
    }

    void TeleportToOther(Character character)
    {
        var proCamera = Camera.main.GetComponent<ProCamera2D>();

        var cameraDelta = proCamera.transform.position - character.transform.position;
        var characterDelta = character.transform.position - transform.position;

        var targetPosition = exitGateway.transform.position + characterDelta;
        character.transform.position = targetPosition;
        proCamera.MoveCameraInstantlyToPosition(targetPosition + cameraDelta);
        lastUseTime = Time.time;
    }


}
