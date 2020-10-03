using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeSprite : MonoBehaviour
{

    public List<Sprite> sprites;

    private void Start()
    {
        transform.position += Vector3.forward;
    }

    [Button]
    void Randomize()
    {
        GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Count)];
    }

}
