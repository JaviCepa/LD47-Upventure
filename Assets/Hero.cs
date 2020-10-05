using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    void OnHeroDeath()
    {
        UpventureGameManager.instance.OnHeroDeath(this);
    }
}
