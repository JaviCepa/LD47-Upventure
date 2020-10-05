using Com.LuisPedroFonseca.ProCamera2D;
using PlatformerPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLevelGateway : MonoBehaviour
{

    public Levels newLevel;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var character = collision.gameObject.GetComponentInParent<Character>();
        if (character != null)
        {
            UpventureGameManager.instance.ChangeLevel(newLevel, this);
        }
    }


}
