using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlatformerPro;

public class RandomPatrolDirection : MonoBehaviour
{
    void Start()
    {
        GetComponent<EnemyMovement_Patrol>().speed = Random.value > 0.5f ? 1f : -1f;
    }

}
