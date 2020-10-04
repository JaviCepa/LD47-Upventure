using Com.LuisPedroFonseca.ProCamera2D;
using PlatformerPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopLoopGateway : MonoBehaviour
{
    public TopLoopGateway exitGateway;

    public int enterSide = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var character = collision.gameObject.GetComponentInParent<Character>();
        if (character != null)
        {
            enterSide = GetCharacterSide(character);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var character = collision.gameObject.GetComponentInParent<Character>();
        if (character != null)
        {
            var exitSide = GetCharacterSide(character);
            if (enterSide != exitSide && enterSide != 0)
            {
                TeleportToOther(character);
            }
            enterSide = 0;
        }
    }

    private int GetCharacterSide(Character character)
    {
        return (int)Mathf.Sign(character.transform.position.x - transform.position.x);
    }

    void TeleportToOther(Character character)
    {
        var proCamera = Camera.main.GetComponent<ProCamera2D>();

        var cameraDelta = proCamera.transform.position - character.transform.position;
        var characterDelta = character.transform.position - transform.position;

        character.transform.position = exitGateway.transform.position + characterDelta;
        proCamera.MoveCameraInstantlyToPosition(character.transform.position + cameraDelta);
    }
}
