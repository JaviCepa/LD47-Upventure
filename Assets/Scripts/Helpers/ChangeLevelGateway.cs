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


}
