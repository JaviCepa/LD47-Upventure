using PlatformerPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSoundEffects : MonoBehaviour
{

    public List<SpriteToSoundBindings> soundBindings;

    SpriteRenderer spriteRenderer;
    Sprite previousSprite;

    AudioSource audioSource;

    Character character;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        character = GetComponentInParent<Character>();

        character.ChangeAnimationState += HandleChangeAnimationState;
    }

    void HandleChangeAnimationState(object sender, AnimationEventArgs e)
    {
        
    }

    void Update()
    {
        if (spriteRenderer.sprite != previousSprite)
        {
            OnSpriteEnter(spriteRenderer.sprite, previousSprite);
            previousSprite = spriteRenderer.sprite;
        }
    }

    private void OnSpriteEnter(Sprite sprite, Sprite previousSprite)
    {
        foreach (var soundBinding in soundBindings)
        {
            if ((soundBinding.sprite == sprite || soundBinding.sprite == null) && (soundBinding.previousSprite == previousSprite || soundBinding.previousSprite == null))
            {
                var targetClip = soundBinding.GetRandomClip();
                if (targetClip != null)
                {
                    audioSource.clip = targetClip;
                    audioSource.Play();
                }
                else
                {
                    Debug.LogWarning("AudioClip is null!");
                }
                break;
            }
        }
    }
}
