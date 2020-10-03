using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpriteToSoundBindings
{
    public Sprite previousSprite;
    public Sprite sprite;
    public List<AudioClip> clips;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Count == 0)
        {
            return null;
        }

        var randomClip = clips[Random.Range(0, clips.Count)];
        return randomClip;
    }
}