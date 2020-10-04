using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlatformerPro;

public class RestoreOnPlayerDeath : MonoBehaviour
{

    Vector3 startPosition;
    FallingPlatform platform;
    Rigidbody2D rb2d;

    public GameObject prefab;

    void Start()
    {
        UpventureGameManager.instance.restorables.Add(this);
        platform = GetComponent<FallingPlatform>();
        rb2d = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    public void OnPlayerDeath()
    {
        Instantiate(prefab, startPosition, Quaternion.identity, transform.parent);
        Destroy(gameObject);
    }

}
