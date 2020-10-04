using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fader : MonoBehaviour
{
    public float fadeTime = 0.5f;
    public Ease fadeEaseIn;
    public Ease fadeEaseOut;
    public static Fader instance;

    private void Awake()
    {
        instance = this;
    }

    public void HideInstantly()
    {
        transform.localPosition = new Vector3(0, transform.localPosition.y, transform.localPosition.z);
    }

    public void ShowInstantly()
    {
        transform.localPosition = new Vector3(10f, transform.localPosition.y, transform.localPosition.z);
    }


    [Button]
    public void HideScreen()
    {
        transform.localPosition = new Vector3(10f, transform.localPosition.y, transform.localPosition.z);
        transform.DOLocalMoveX(0, fadeTime).SetEase(fadeEaseIn);
    }

    [Button]
    public void ShowScreen()
    {
        transform.localPosition = new Vector3(0, transform.localPosition.y, transform.localPosition.z);
        transform.DOLocalMoveX(-10f, fadeTime).SetEase(fadeEaseOut);
    }
}
