using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteDepthFaker : MonoBehaviour
{
    public int copies = 10;
    public float separation = 0.1f;

    SpriteRenderer spriteRenderer;

    GameObject parentFaker;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        parentFaker = new GameObject($"SpriteFaker");
        parentFaker.transform.SetParent(transform);

        for (int i = 0; i < copies; i++)
        {
            var newObject = new GameObject($"SpriteClone {i}");
            newObject.transform.SetParent(parentFaker.transform);
            var spRenderer = newObject.AddComponent<SpriteRenderer>();
            spRenderer.sharedMaterial = spriteRenderer.sharedMaterial;
            spRenderer.sprite = spriteRenderer.sprite;
            spRenderer.color = spriteRenderer.color;
            spRenderer.sortingOrder = spriteRenderer.sortingOrder;
            newObject.transform.position = transform.position + Vector3.forward * i * separation;
        }
        parentFaker.SetActive(false);
    }

    private void OnBecameVisible()
    {
        parentFaker.SetActive(true);
    }

    private void OnBecameInvisible()
    {
        parentFaker.SetActive(false);
    }

}
