using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class DarkLord : MonoBehaviour
{

    Camera cam;

    public float deathBallFlyHeight = 5f;
    public DarkState currentState;

    public Sprite idleSprite;
    public Sprite pickSprite;
    public Sprite attackSprite;

    SpriteRenderer spriteRenderer;

    public GameObject explosionPrefab;
    public GameObject spellPrefab;

    public Vector3 spellSpawnDelta;

    GameObject currentSpell;

    void Start()
    {
        currentState = DarkState.Waiting;
        cam = FindObjectOfType<Camera>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        transform.position = new Vector3(cam.transform.position.x, transform.position.y, transform.position.z);
        switch (currentState)
        {
            case DarkState.Waiting:
                if (UpventureGameManager.instance.character.transform.position.y > 165f && UpventureGameManager.instance.character.CharacterHealth.CurrentHealth > 0)
                {
                    currentState = DarkState.Idle;
                }
                break;
            case DarkState.Idle:
                Invoke("IdleSprite", 1f);
                Invoke("PickAction", 2f);
                currentState = DarkState.Cooldown;
                break;
            case DarkState.Cooldown:
                spriteRenderer.sprite = idleSprite;
                break;
            case DarkState.Pick:
                Pick();
                break;
            case DarkState.Attack:
                Attack();
                break;
            case DarkState.Stomp:
                Stomp();
                break;
        }
    }

    private void Pick()
    {
        spriteRenderer.sprite = pickSprite;
        if (currentSpell == null)
        {
            currentSpell = Instantiate(spellPrefab, transform.position + spellSpawnDelta, Quaternion.identity) as GameObject;
            Invoke("Attack", 1.5f);
        }
    }

    private void Attack()
    {
        currentSpell.transform.position = transform.position + spellSpawnDelta;
        spriteRenderer.sprite = attackSprite;
        if (currentSpell != null)
        {
            var launchedSpell = currentSpell;
            var sequence = DOTween.Sequence();
            sequence.Append(currentSpell.transform.DOLocalJump(UpventureGameManager.instance.character.transform.position, deathBallFlyHeight, 1, 1.5f).SetEase(Ease.Linear));
            sequence.AppendCallback(() => UpventureGameManager.instance.ShakeScreen(0));
            sequence.AppendCallback(() => Destroy(launchedSpell));
            sequence.AppendCallback(() => Instantiate(explosionPrefab, launchedSpell.transform.position, Quaternion.identity));
            sequence.AppendCallback(() => currentState = DarkState.Idle);
            currentSpell = null;
        }
        currentState = DarkState.DoNothing;
    }

    private void Stomp()
    {
        currentState = DarkState.DoNothing;
        spriteRenderer.sprite = pickSprite;
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOMoveY(6f, 1f).SetEase(Ease.OutQuad).SetRelative(true));
        sequence.Append(transform.DOMoveY(-6f, 0.5f).SetEase(Ease.InExpo).SetRelative(true));
        sequence.AppendCallback(() => spriteRenderer.sprite = attackSprite);
        sequence.AppendCallback(() => UpventureGameManager.instance.ShakeScreen(1));
        sequence.AppendCallback(() => currentState = DarkState.Idle);
        sequence.AppendCallback(
            () => {
                if (UpventureGameManager.instance.character.Grounded)
                {
                    UpventureGameManager.instance.character.InputEnabled = false;
                }
            }
        );
        sequence.AppendInterval(3f);
        sequence.AppendCallback(
            () => {
                UpventureGameManager.instance.character.InputEnabled = true;
            }
        );
        //Disable player input for a while if its grounded
    }

    void IdleSprite()
    {
        spriteRenderer.sprite = idleSprite;
    }

    void PickAction()
    {
        switch (UnityEngine.Random.Range(0, 3))
        {
            case 0: currentState = DarkState.Pick; break;
            case 1: currentState = DarkState.Stomp; break;
            default: currentState = DarkState.Idle; break;
        }
    }

    public void OnCharacterDeath()
    {
        currentState = DarkState.Waiting;
    }

}


public enum DarkState { Waiting, Idle, Pick, Attack,
    Stomp,
    Cooldown,
    DoNothing
}