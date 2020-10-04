using Com.LuisPedroFonseca.ProCamera2D;
using PlatformerPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLevelGateway : MonoBehaviour
{

    public Levels newLevel;

    static float lastUseTime = 0;

    const float cooldown = 0.5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var character = collision.gameObject.GetComponentInParent<Character>();
        if (character != null && Time.time - lastUseTime > cooldown)
        {
            FindObjectOfType<UpventureGameManager>().ChangeLevel(newLevel);
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    var character = collision.gameObject.GetComponentInParent<Character>();
    //    if (character != null && Time.time - lastUseTime > cooldown)
    //    {
    //        TeleportToOther(character);
    //    }
    //}

    //void TeleportToOther(Character character)
    //{
    //    var proCamera = Camera.main.GetComponent<ProCamera2D>();

    //    var cameraDelta = proCamera.transform.position - character.transform.position;
    //    var characterDelta = character.transform.position - transform.position;

    //    character.transform.position = exitGateway.transform.position + characterDelta;
    //    proCamera.MoveCameraInstantlyToPosition(character.transform.position + cameraDelta);
    //    lastUseTime = Time.time;
    //}


}
