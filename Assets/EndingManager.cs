using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{

    public DOTweenPath heroAnimation;
    public DOTweenPath princessAnimation;
    public DOTweenAnimation theEndAnimation;

    bool replayAvailable = false;

    void Start()
    {
        Fader.instance.HideInstantly();
        var sequence = DOTween.Sequence();
        sequence.AppendInterval(1f);
        sequence.AppendCallback(() => Fader.instance.ShowScreen());
        sequence.AppendInterval(2f);
        sequence.AppendCallback( () => heroAnimation.DOPlay());
        sequence.AppendInterval(4f);
        sequence.AppendCallback(() => princessAnimation.DOPlay());
        sequence.AppendInterval(4f);
        sequence.AppendCallback(() => theEndAnimation.DOPlay());
        sequence.AppendInterval(4f);
        sequence.AppendCallback(() => replayAvailable = true);
    }

    void Update()
    {
        if (replayAvailable && Input.anyKeyDown)
        {
            replayAvailable = false;
            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => Fader.instance.HideScreen());
            sequence.AppendInterval(2f);
            sequence.AppendCallback(() => SceneManager.LoadScene(0));
        }
    }
}
