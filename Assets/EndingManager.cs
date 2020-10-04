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
        var sequence = DOTween.Sequence();
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
            SceneManager.LoadScene(0);
        }
    }
}
