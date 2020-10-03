using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteDepthFaker : MonoBehaviour
{
    public int copies = 10;
    public float separation = 0.1f;

    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        for (int i = 0; i < copies; i++)
        {
            var newObject = new GameObject($"SpriteClone {i}");
            newObject.transform.SetParent(transform);
            var spRenderer = newObject.AddComponent<SpriteRenderer>();
            spRenderer.sharedMaterial = spriteRenderer.sharedMaterial;
            spRenderer.sprite = spriteRenderer.sprite;
            spRenderer.sortingOrder = spriteRenderer.sortingOrder;
            newObject.transform.position = transform.position + Vector3.forward * i * separation;
        }
    }

}
